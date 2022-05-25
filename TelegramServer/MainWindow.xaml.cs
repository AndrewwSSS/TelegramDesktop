using CommonLibrary.Messages;
using CommonLibrary.Messages.Auth;
using CommonLibrary.Messages.Groups;
using CommonLibrary.Messages.Users;
using MessageLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace TelegramServer
{
    public delegate void UserDisconnectedeHandler(User user);

    public partial class MainWindow : Window
    {
        private ObservableCollection<User> UsersOnline;
        private ObservableCollection<User> UsersOffline;
        private ObservableCollection<GroupChat> Chats;
        private Dictionary<User, TcpClientWrap> Clients;
        private TelegramDb DbContext;
        private TcpServerWrap Server;
     

        public MainWindow()
        {
            InitializeComponent();


            DbContext = new TelegramDb();
            Clients = new Dictionary<User, TcpClientWrap>();
            UsersOnline = new ObservableCollection<User>();
            UsersOffline = new ObservableCollection<User>();
          

            DbContext.GroupChats.Load();
            Chats = DbContext.GroupChats.Local;

            Server = new TcpServerWrap();
            Server.Started += OnServerStarted;
            Server.Stopped += OnServerStopped;
            Server.MessageReceived += ClientMessageRecived;


            LB_UsersOffline.ItemsSource = UsersOffline;
            LB_UsersOnline.ItemsSource = UsersOnline;
            LB_Groups.ItemsSource = Chats;

            foreach (var user in DbContext.Users)
                UsersOffline.Add(user);
        }


        #region ServerEvents

        private void OnServerStarted(TcpServerWrap client)
        {
            Dispatcher.Invoke(() =>
            {
                BtnStopServer.IsEnabled = true;
            });
        }

        private void OnServerStopped(TcpServerWrap client)
        {
            Dispatcher.Invoke(() =>
            {
                BtnStopServer.IsEnabled = false;
                BtnStartServer.IsEnabled = true;
                TB_ListenerPort.IsEnabled = true;
            });
        }

        private void ClientMessageRecived(TcpClientWrap client, Message msg)
        {
            switch (msg.GetType().Name)
            {
                case "SignUpMessage":
                    {
                        SignUpMessage signUpMessage = (SignUpMessage)msg;


                        if (DbContext.Users.Any(u => u.Email == signUpMessage.Email || u.Login == signUpMessage.Login) != true)
                        {
                            User NewUser = new User()
                            {
                                Email = signUpMessage.Email,
                                Login = signUpMessage.Login,
                                Name = signUpMessage.Name,
                                Password = signUpMessage.Password,
                                RegistrationDate = DateTime.UtcNow,
                                LastVisitDate = default
                            };

                            DbContext.Users.Add(NewUser);
                            Dispatcher.Invoke(() => UsersOffline.Add(NewUser));
                            DbContext.SaveChanges();


                            client.SendAsync(new SignUpResultMessage(AuthenticationResult.Success));

                        }
                        else
                        {
                            client.SendAsync(new SignUpResultMessage(AuthenticationResult.Denied,
                                            "Email or login already used to create an account"));

                        }
                        break;

                    }
                case "LoginMessage":
                    {
                        LoginMessage loginMessage = (LoginMessage)msg;
                        User user = DbContext.Users.FirstOrDefault(u => u.Login == loginMessage.Login || u.Email == loginMessage.Login);

                        if (user != null && user.Password == loginMessage.Password)
                        {
                            PublicUserInfo UserInfo = new PublicUserInfo()
                            {
                                Id = user.Id,
                                Name = user.Name
                            };


                            UserInfo.Images.AddRange(user.Images);

                            if (user.Email == loginMessage.Login)
                                UserInfo.Login = user.Login;


                            client.SendAsync(new LoginResultMessage(AuthenticationResult.Success, UserInfo));
                            user.LastVisitDate = DateTime.UtcNow;

                            client.Disconnected += OnClientDisconnected;
                            Clients[user] = client;

                            DbContext.SaveChanges();

                            Dispatcher.Invoke(() =>
                            {
                                UsersOffline.Remove(user);
                                UsersOnline.Add(user);
                            });


                            if (user.MessagesToSend.Count > 0)
                            {

                                client.SendAsync(new ArrayMessage<BaseMessage>(user.MessagesToSend));

                                ClientMessageHandler OnMessageSent = null;
                                OnMessageSent = (c, m) =>
                                {
                                    user.MessagesToSend.Clear();
                                    client.MessageSent -= OnMessageSent;
                                };
                                client.MessageSent += OnMessageSent;
                            }
                        }
                        else
                            client.SendAsync(new LoginResultMessage(AuthenticationResult.Denied, "wrong login/email or password"));
                        break;
                    }
                case "ChatMessage":
                    {
                        ChatMessage chatMessage = (ChatMessage)msg;
                        GroupChat groupChat = DbContext.GroupChats.First(gc => gc.Id == chatMessage.GroupId);
                        User fromUser = DbContext.Users.First(u => u.Id == chatMessage.FromUserId);

                        groupChat.Messages.Add(chatMessage);

                        DbContext.SaveChanges();
                        SendMessageToUsers(chatMessage, fromUser.Id, groupChat.Members);

                        break;

                    }
                case "GroupLookupMessage":
                    {
                        GroupLookupMessage groupLookupMessage = (GroupLookupMessage)msg;
                        List<PublicGroupInfo> SuitableGroups = null;

                        foreach (var groupChat in DbContext.GroupChats)
                        {
                            if (groupChat.Name.ToLower().Contains(groupLookupMessage.GroupName.ToLower()))
                            {

                                if (SuitableGroups == null)
                                    SuitableGroups = new List<PublicGroupInfo>();

                                PublicGroupInfo SuitableGroup
                                        = new PublicGroupInfo(groupChat.Name, groupChat.Description, groupChat.Id);


                                SuitableGroup.Messages.AddRange(groupChat.Messages);
                                SuitableGroup.Images.AddRange(groupChat.Images);

                                foreach (var groupMember in groupChat.Members)
                                {
                                    PublicUserInfo publicUser = new PublicUserInfo()
                                    {
                                        Id = groupMember.Id,
                                        Name = groupMember.Name,
                                        Login = groupMember.Login,
                                        Description = groupMember.Description
                                    };

                                    SuitableGroup.Members.Add(publicUser);
                                    publicUser.Images.AddRange(groupMember.Images);
                                }

                                SuitableGroups.Add(SuitableGroup);
                            }
                        }

                        ArrayMessage<PublicGroupInfo> result
                            = new ArrayMessage<PublicGroupInfo>(SuitableGroups);
                        client.SendAsync(result);

                        break;
                    }
                case "CreateGroupMessage":
                    {
                        CreateGroupMessage createNewGroupMessage = (CreateGroupMessage)msg;
                        List<User> newGroupMembers = null;
                        User groupCreator = DbContext.Users.First(u => u.Id == createNewGroupMessage.FromUserId);

                        newGroupMembers = DbContext.Users.Where(u => createNewGroupMessage.MembersId.Equals(u.Id)).ToList();

                        if (newGroupMembers.Count > 0)
                        {

                            if (newGroupMembers.Any(newGroupMember
                                => newGroupMember.BlockedUsers.Any(blockedUser => blockedUser.Id == groupCreator.Id)))
                            {

                                client.SendAsync(new CreateGroupResultMessage(AuthenticationResult.Denied,
                                                 "One or more users blocked you"));
                                break;
                            }

                        }

                        GroupChat newGroup = new GroupChat()
                        {
                            Name = createNewGroupMessage.Name,
                            Members = new List<User>() { groupCreator },
                            DateCreated = DateTime.UtcNow
                        };

                        newGroup.Images.Add(createNewGroupMessage.Image);


                        Dispatcher.Invoke(() =>
                        {
                            DbContext.GroupChats.Add(newGroup);
                            DbContext.SaveChanges();
                            DbContext.GroupChats.Load();
                        });


                        client.SendAsync(new CreateGroupResultMessage(AuthenticationResult.Success, newGroup.Id));


                        if (newGroupMembers.Count > 0)
                        {
                            newGroup.Members.AddRange(newGroupMembers);

                            List<PublicUserInfo> PublicUsersInfo = new List<PublicUserInfo>();

                            foreach (var member in newGroupMembers)
                            {
                                member.Chats.Add(newGroup);

                                PublicUsersInfo.Add(new PublicUserInfo()
                                {
                                    Id = member.Id,
                                    Login = member.Login,
                                    Name = member.Name,
                                    Description = member.Description
                                });

                            }

                            DbContext.SaveChanges();


                            PublicGroupInfo GroupInfo = new PublicGroupInfo(newGroup.Name,
                                                                            newGroup.Description,
                                                                            newGroup.Id);

                            GroupInfo.Members = PublicUsersInfo;

                            SendMessageToUsers(new GroupInviteMessage(GroupInfo, groupCreator.Id),
                                                     groupCreator.Id,
                                                     newGroupMembers);
                        }

                        break;
                    }
                case "ClientDisconnectMessage":
                    {
                        ClientDisconnectMessage clientDisconnect = (ClientDisconnectMessage)msg;
                        User DisconnectedUser = DbContext.Users.First(u => u.Id == clientDisconnect.UserId);

                        UserDisconnect(DisconnectedUser);

                        break;
                    }
                case "GroupJoinMessage":
                    {
                        GroupJoinMessage groupJoinMessage = (GroupJoinMessage)msg;
                        GroupChat group = DbContext.GroupChats.First(g => g.Id == groupJoinMessage.GroupId);
                        User newGroupMember = DbContext.Users.First(u => u.Id == groupJoinMessage.UserId);

                        if (newGroupMember != null && group != null)
                        {

                            newGroupMember.Chats.Add(group);
                            group.AddMember(newGroupMember);

                            client.SendAsync(new GroupJoinResultMessage(AuthenticationResult.Success));

                            PublicUserInfo userInfo = new PublicUserInfo()
                            {
                                Id = newGroupMember.Id,
                                Name = newGroupMember.Name,
                                Description = newGroupMember.Description,
                                Login = newGroupMember.Login,
                            };

                            userInfo.Images.AddRange(newGroupMember.Images);


                            SendMessageToUsers(new GroupUpdateMessage(group.Id) { NewUser = userInfo },
                                                        newGroupMember.Id,
                                                        group.Members);

                        }
                        else
                            client.SendAsync(new GroupJoinResultMessage(AuthenticationResult.Denied));

                        break;
                    }
            }
        }

        private void OnClientDisconnected(TcpClientWrap client)
        {
            User DisconnectedUser = Clients.First((c) => c.Value == client).Key;

            if (Dispatcher.CheckAccess())
                UserDisconnect(DisconnectedUser);
            else
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                   new UserDisconnectedeHandler(UserDisconnect),
                   DisconnectedUser);
            }
        }

        #endregion ServerEvents


        #region WpfEvents

        private void BtnStartServer_Click(object sender, RoutedEventArgs e)
        {
            int port;
            if (int.TryParse(TB_ListenerPort.Text, out port) && port > 999 && port < 10000)
            {
                BtnStartServer.IsEnabled = false;
                TB_ListenerPort.IsEnabled = false;
                Server.Start(port, 1000);
            }
            else
                MessageBox.Show("Invalid port", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

        }

        private void BtnStopServer_Click(object sender, RoutedEventArgs e)
        {
            Server.Shutdown();

            UsersOnline.Clear();
            UsersOffline.Clear();

            foreach (var user in DbContext.Users)
                UsersOffline.Add(user);

        }


        private void BlockOfflineUser_Click(object sender, RoutedEventArgs e) {
            MessageBox.Show(LB_UsersOffline.SelectedItem.GetType().Name);
        }

        private void BlockOnlineUser_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion WpfEvents


        private void SendMessageToUsers(BaseMessage msg, int FromUserId, List<User> UsersToSend)
        {
            bool changesExist = false;
            foreach (var user in UsersToSend)
            {
                if (user.Id != FromUserId && !user.Banned)
                {
                    if (isUserOnline(user))
                        Clients[user].SendAsync(msg);
                    else
                    {
                        user.MessagesToSend.Add(msg);
                        changesExist = true;
                    }
                }
            }
            if(changesExist)
                DbContext.SaveChanges();
        }

        private bool isUserOnline(User user) => Clients.ContainsKey(user);

        private void UserDisconnect(User user)
        {
            Dispatcher.Invoke(() =>
            {
                UsersOnline.Remove(user);
                UsersOffline.Add(user);
                Clients.Remove(user);
                DbContext.SaveChanges();
            });
        }

     
        private void ClearGroups()
        {
            if(DbContext != null)
            {
                DbContext.GroupChats.RemoveRange(DbContext.GroupChats);
                DbContext.SaveChanges();
            }
        }



    
    }
}
