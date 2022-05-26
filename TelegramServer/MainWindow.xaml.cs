using CommonLibrary.Containers;
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
using System.Windows;
using System.Windows.Threading;

namespace TelegramServer
{
    public delegate void UserDisconnectedeHandler(User user);

    public partial class MainWindow : Window
    {
        private ObservableCollection<User> UsersOnline;
        private ObservableCollection<User> UsersOffline;
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
            

            Server = new TcpServerWrap();
            Server.Started += OnServerStarted;
            Server.Stopped += OnServerStopped;
            Server.MessageReceived += ClientMessageRecived;


            LB_UsersOffline.ItemsSource = UsersOffline;
            LB_UsersOnline.ItemsSource = UsersOnline;
            LB_Groups.ItemsSource = DbContext.GroupChats.Local;

            foreach (var user in DbContext.Users)
                UsersOffline.Add(user);

            //MailAddress from = new MailAddress("telegramdesktopbyadat@gmail.com");
            //MailAddress to = new MailAddress("");


            //MailMessage message = new MailMessage(from, to);
            //message.IsBodyHtml = false;
            //message.Subject = "test";
            //message.Body = "some code...";


            //SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587)
            //{
            //    Credentials = new NetworkCredential("telegramdesktopbyadat@gmail.com", ""),
            //    EnableSsl = true
            //};

            //smtp.Send(message);

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
                        SignUpStage1Message signUpMessage = (SignUpStage1Message)msg;


                        if (DbContext.Users.Any(u => u.Email == signUpMessage.Email || u.Login == signUpMessage.Login) != true)
                        {
                            User NewUser = new User()
                            {
                                Email = signUpMessage.Email,
                                Login = signUpMessage.Login,
                                Name = signUpMessage.Name,
                                Password = signUpMessage.Password,
                            };

                            DbContext.Users.Add(NewUser);
                            Dispatcher.Invoke(() => UsersOffline.Add(NewUser));
                            DbContext.SaveChanges();


                            client.SendAsync(new SingUpStage1Message(AuthenticationResult.Success));

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
                        User sender = DbContext.Users.FirstOrDefault(u => u.Login == loginMessage.Login || u.Email == loginMessage.Login);

                        if (sender != null && sender.Password == loginMessage.Password)
                        {
                            PublicUserInfo UserInfo = new PublicUserInfo()
                            {
                                Id = sender.Id,
                                Name = sender.Name
                            };


                            UserInfo.ImagesId.AddRange(sender.ImagesId);

                            if (sender.Email == loginMessage.Login)
                                UserInfo.Login = sender.Login;


                            client.SendAsync(new LoginResultMessage(AuthenticationResult.Success, UserInfo));
                            sender.VisitDate = DateTime.UtcNow;

                            client.Disconnected += OnClientDisconnected;
                            Clients[sender] = client;

                            DbContext.SaveChanges();

                            Dispatcher.Invoke(() =>
                            {
                                UsersOffline.Remove(sender);
                                UsersOnline.Add(sender);
                            });

                            if (sender.MessagesToSend.Count > 0)
                            {

                                client.SendAsync(new ArrayMessage<BaseMessage>(sender.MessagesToSend));

                                ClientMessageHandler OnMessageSent = null;
                                OnMessageSent = (c, m) =>
                                {
                                    sender.MessagesToSend.Clear();
                                    client.MessageSent -= OnMessageSent;
                                };
                                client.MessageSent += OnMessageSent;
                            }
                        }
                        else
                            client.SendAsync(new LoginResultMessage(AuthenticationResult.Denied,
                                                                    "wrong login/email or password"));
                        break;
                    }
                case "ChatMessage":
                    {
                        ChatMessage chatMessage = (ChatMessage)msg;
                        GroupChat chat = DbContext.GroupChats.First(gc => gc.Id == chatMessage.GroupId);
                        User sender = DbContext.Users.First(u => u.Id == chatMessage.FromUserId);

                        sender.Messages.Add(chatMessage);
                        chat.Messages.Add(chatMessage);

                        DbContext.SaveChanges();

                        SendMessageToUsers(chatMessage, sender.Id, chat.Members);

                        break;

                    }
                case "GroupLookupMessage":
                    {
                        GroupLookupMessage groupLookupMessage = (GroupLookupMessage)msg;
                        List<PublicGroupInfo> SuitableGroups = null;

                        foreach (var group in DbContext.GroupChats)
                        {
                            if (group.Name.ToLower().Contains(groupLookupMessage.GroupName.ToLower()))
                            {

                                if (SuitableGroups == null)
                                    SuitableGroups = new List<PublicGroupInfo>();

                                SuitableGroups.Add(PublicGroupInfoFromGroup(group));

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
                        User sender = DbContext.Users.First(u => u.Id == createNewGroupMessage.FromUserId);

                        newGroupMembers = DbContext.Users.Where(u => createNewGroupMessage.MembersId.Equals(u.Id)).ToList();

                        if (newGroupMembers.Count > 0)
                        {

                            if (newGroupMembers.Any(newGroupMember
                                => newGroupMember.BlockedUsers.Any(blockedUser => blockedUser.Id == sender.Id)))
                            {

                                client.SendAsync(new CreateGroupResultMessage(AuthenticationResult.Denied,
                                                 "One or more users blocked you"));
                                break;
                            }

                        }

                        GroupChat newGroup = new GroupChat()
                        {
                            Name = createNewGroupMessage.Name,
                            Members = new List<User>() { sender },
                            DateCreated = DateTime.UtcNow
                        };

                        if(createNewGroupMessage.Image != null)
                        {
                            ImageContainer newImage = new ImageContainer(createNewGroupMessage.Image);
                            DbContext.Images.Add(newImage);
                            
                            DbContext.SaveChanges();
                            DbContext.Images.Load();

                            newGroup.ImagesId.Add(newImage.Id);
                            
                        }
                        

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

                            List<int> UsersId = new List<int>();

                            foreach (var member in newGroupMembers)
                            {
                                member.Chats.Add(newGroup);
                                UsersId.Add(member.Id);
                            }

                            DbContext.SaveChanges();


                            PublicGroupInfo GroupInfo = new PublicGroupInfo(newGroup.Name,
                                                                            newGroup.Description,
                                                                            newGroup.Id);

                            GroupInfo.MembersId = UsersId;

                            SendMessageToUsers(new GroupInviteMessage(GroupInfo, sender.Id),
                                                     sender.Id,
                                                     newGroupMembers);
                        }

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

                            userInfo.ImagesId.AddRange(newGroupMember.ImagesId);


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
            User DisconnectedUser = Clients.FirstOrDefault((c) => c.Value == client).Key;

            Action Disconnect = () =>
            {
                Dispatcher.Invoke(() =>
                {
                    UsersOnline.Remove(DisconnectedUser);
                    UsersOffline.Add(DisconnectedUser);
                    Clients.Remove(DisconnectedUser);
                    DbContext.SaveChanges();
                });
            };

            if (Dispatcher.CheckAccess())
                Disconnect.Invoke();
            else
                Dispatcher.Invoke(() => { Disconnect.Invoke(); });
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

        //TODO
        private void BlockOfflineUser_Click(object sender, RoutedEventArgs e) {
            MessageBox.Show(LB_UsersOffline.SelectedItem.GetType().Name);
        }

        //TODO
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

        public PublicGroupInfo PublicGroupInfoFromGroup(GroupChat group, int MaxMessagesCount = 50)
        {
            PublicGroupInfo result = new PublicGroupInfo(group.Name,
                                                         group.Description,
                                                         group.Id);

            if (group.Messages.Count >= MaxMessagesCount)
                result.Messages.AddRange(group.Messages.GetRange(0, MaxMessagesCount-1));
            else
                result.Messages.AddRange(group.Messages);

            result.ImagesId.AddRange(group.ImagesId);

            foreach (var groupMember in group.Members)
                result.MembersId.Add(groupMember.Id);


            return result;
        }


        private bool isUserOnline(User user) => Clients.ContainsKey(user);

  


    
    }
}
