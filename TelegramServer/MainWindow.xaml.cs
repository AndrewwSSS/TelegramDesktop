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

namespace TelegramServer
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<User> UsersOnline;
        private ObservableCollection<User> UsersOffline;
        private ObservableCollection<GroupChat> Chats;
        private TelegramDb DbContext;

        private TcpServerWrap Server;

        public MainWindow()
        {
            InitializeComponent();

            DbContext = new TelegramDb();

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

            foreach (User user in DbContext.Users)
                UsersOffline.Add(user);
        }


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

                    if (DbContext.Users.Count(u => u.Email == signUpMessage.Email || u.Login == signUpMessage.Login) == 0)
                    {
                        User NewUser = new User()
                        {
                            Email = signUpMessage.Email,
                            Login = signUpMessage.Login,
                            Name = signUpMessage.Name,
                            Password = signUpMessage.Password,
                            RegistrationDate = DateTime.Now,
                            LastVisitDate = DateTime.Now
                        };

                        DbContext.Users.Add(NewUser);
                        Dispatcher.Invoke(() => UsersOffline.Add(NewUser));
                        DbContext.SaveChanges();

                        SignUpResultMessage ResultMessage
                                = new SignUpResultMessage(AuthenticationResult.Success).SetRegistrationDate(NewUser.RegistrationDate);

                        client.SendAsync(ResultMessage);

                    }
                    else
                    {
                        SignUpResultMessage ResultMessage
                            = new SignUpResultMessage(AuthenticationResult.Denied, "Email or login already used to create an account");
                        client.SendAsync(ResultMessage);
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
                            Name = user.Name,
                            RegistrationDate = user.RegistrationDate
                        };

                        if(user.Images != null)
                             UserInfo.Images = new List<ImageContainer>(user.Images);
                        
                         
                        if(user.Email == loginMessage.Login)
                           UserInfo.Login = user.Login;

                        LoginResultMessage ResultMessage = new LoginResultMessage(AuthenticationResult.Success, UserInfo);

                        client.SendAsync(ResultMessage);

                        user.client = client;
                        user.LastVisitDate = DateTime.Now;
                        user.isOnline = true;

                        DbContext.SaveChanges();

                        Dispatcher.Invoke(() =>
                        {
                            UsersOffline.Remove(user);
                            UsersOnline.Add(user);
                        });


                        if (user.MessagesToSend != null && user.MessagesToSend.Count > 0)
                        {
                            ArrayMessage<Message> Messages
                                    = new ArrayMessage<Message>(user.MessagesToSend);

                            client.SendAsync(Messages);

                            ClientMessageHandler OnSent = null;

                            OnSent = (c, m) =>
                            {
                                user.MessagesToSend.Clear();
                                client.MessageSent -= OnSent;
                            };

                                
                            client.MessageSent += OnSent;
                          
                            user.MessagesToSend.Clear();
                        }
                    }
                    else
                    {
                        LoginResultMessage ResultMessage = new LoginResultMessage(AuthenticationResult.Denied, "wrong login/email or password");
                        client.SendAsync(ResultMessage);
                    }
                    break;
                }
                case "ChatMessage":
                {
                    ChatMessage chatMessage = (ChatMessage)msg;
                    GroupChat groupChat = DbContext.GroupChats.First(gc => gc.Id == chatMessage.GroupId);
                    User fromUser = DbContext.Users.First(u => u.Id == chatMessage.FromUser.Id); 
                    groupChat.Messages.Add(chatMessage);
                    
                    DbContext.SaveChanges();

                    SendMessageToUsers(chatMessage, fromUser, groupChat.Members);

                    break;

                }
                case "GroupLookupMessage":
                {
                    GroupLookupMessage groupLookupMessage = (GroupLookupMessage)msg;
                    ArrayMessage<PublicGroupInfo> ResultMessage;
                    List<PublicGroupInfo> SuitableGroups = null;

                    foreach (var groupChat in DbContext.GroupChats)
                    {
                        // Поиск по имени группы
                        if (groupChat.Name.ToLower().Contains(groupLookupMessage.GroupName.ToLower()))
                        {

                            if (SuitableGroups == null)
                                SuitableGroups = new List<PublicGroupInfo>();

                            PublicGroupInfo SuitableGroup
                                    = new PublicGroupInfo(groupChat.Name, groupChat.Description, groupChat.Id);

                            if (SuitableGroup.Images != null)
                                SuitableGroup.Images.AddRange(groupChat.Images);

                            foreach (var groupMember in groupChat.Members)
                            {
                                PublicUserInfo publicUser
                                    = new PublicUserInfo(groupMember.Id, groupMember.Login, groupMember.Name, groupMember.Description, groupMember.LastVisitDate);

                                if (groupMember.Images != null)
                                    foreach (var image in groupMember.Images)
                                        publicUser.Images.Add(image);

                            }

                            SuitableGroups.Add(SuitableGroup);
                        }
                    }

                    ResultMessage = new ArrayMessage<PublicGroupInfo>(SuitableGroups);
                    client.SendAsync(ResultMessage);

                    break;
                }
                case "CreateGroupMessage":
                {
                    CreateGroupMessage createNewGroupMessage = (CreateGroupMessage)msg;
                    List<User> Members = null;
                    User GroupCreator = DbContext.Users.First(u => u.Id == createNewGroupMessage.FromUserId);

                    if (GroupCreator == null)
                    {
                        MessageBox.Show("Group creator not found");
                        break;
                    }

                    // если есть пользователи котрых добавили в группу
                    if (createNewGroupMessage.MembersId != null)
                    {
                        Members = DbContext.Users.Where(u => createNewGroupMessage.MembersId.Contains(u.Id)).ToList();

                        if (Members.Count > 0)
                        {
                            if (Members.Any(m => m.BlockedUsers.Any(bu => bu.Id == GroupCreator.Id)))
                            {
                                CreateGroupResultMessage DeniedMessage
                                        = new CreateGroupResultMessage(AuthenticationResult.Denied, "One or more users in list blocked you");
                                client.SendAsync(DeniedMessage);
                                break;
                            }

                        }
                    }

                    GroupChat NewGroupChat = new GroupChat();
                    NewGroupChat.Name = createNewGroupMessage.Name;
                    NewGroupChat.Members = new List<User>() { GroupCreator };
                    NewGroupChat.DateCreated = DateTime.Now;


                    if (createNewGroupMessage.Image != null)
                    {
                        NewGroupChat.Images = new List<ImageContainer>();
                        NewGroupChat.Images.Add(createNewGroupMessage.Image);
                    }

                    Dispatcher.Invoke(() =>
                    {
                        DbContext.GroupChats.Add(NewGroupChat);
                        DbContext.SaveChanges();
                        DbContext.GroupChats.Load();
                    });


                    CreateGroupResultMessage ResultMessage =
                            new CreateGroupResultMessage(AuthenticationResult.Success, NewGroupChat.Id);

                    client.SendAsync(ResultMessage);


                    if (Members != null)
                    {
                        NewGroupChat.Members.AddRange(Members);

                        List<PublicUserInfo> PublicUsersInfo = new List<PublicUserInfo>();

                        foreach (var member in Members)
                        {
                            if (member.Chats == null)
                                member.Chats = new List<GroupChat>();

                            member.Chats.Add(NewGroupChat);

                            PublicUsersInfo.Add(new PublicUserInfo()
                            {
                                Id = member.Id,
                                Login = member.Login,
                                Name = member.Name,
                                Description = member.Description 
                            });

                        }
                        DbContext.SaveChanges();


                        PublicGroupInfo GroupInfo
                                = new PublicGroupInfo(NewGroupChat.Name, NewGroupChat.Description, NewGroupChat.Id);

                        GroupInfo.Users = PublicUsersInfo;

                        SendMessageToUsers(new GroupJoinMessage(GroupInfo), GroupCreator, Members);
                    }

                    break;
                }
                case "ClientDisconnectMessage":
                {
                    ClientDisconnectMessage clientDisconnect = new ClientDisconnectMessage();

                    User DisconnectedUser = DbContext.Users.FirstOrDefault(u => u.Id == clientDisconnect.UserId);
                    DisconnectedUser.isOnline = false;

                    ClientHandler OnClientDisconnected = (c) =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            UsersOnline.Remove(DisconnectedUser);
                            UsersOffline.Add(DisconnectedUser);
                            DisconnectedUser.client = null;
                            DbContext.SaveChanges();
                        });
                    };


                    DisconnectedUser.client.Disconnected += OnClientDisconnected;
                    DisconnectedUser.client.DisconnectAsync();

                    

                
                    break;
                }

            }
        }

        private void SendMessageToUsers(BaseMessage msg, User FromUser, List<User> UsersToSend)
        {
            if (msg == null || UsersToSend == null)
                return;

            foreach (var user in UsersToSend)
                if (user.Id != FromUser.Id)
                {
                    if (user.isOnline)
                        user.client.SendAsync(msg);
                    else
                    {
                        if (user.MessagesToSend == null)
                            user.MessagesToSend = new List<BaseMessage>();
                        user.MessagesToSend.Add(msg);
                    }
                }
        }
    }
}
