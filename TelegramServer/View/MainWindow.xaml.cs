using CommonLibrary.Containers;
using CommonLibrary.Messages;
using CommonLibrary.Messages.Auth;
using CommonLibrary.Messages.Auth.Login;
using CommonLibrary.Messages.Auth.SignUp;
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
   

    public partial class MainWindow : Window
    {
        private ObservableCollection<User> UsersOnline;
        private ObservableCollection<User> UsersOffline;
        private Dictionary<UserClient, TcpClientWrap> ClientsOnline;
        private TelegramDb DbTelegram;
        private TcpServerWrap Server;     

        public MainWindow()
        {
            InitializeComponent();


            DbTelegram = new TelegramDb();
            ClientsOnline = new Dictionary<UserClient, TcpClientWrap>();
            UsersOnline = new ObservableCollection<User>();
            UsersOffline = new ObservableCollection<User>();
          

            DbTelegram.GroupChats.Load();
            

            Server = new TcpServerWrap();
            Server.Started += OnServerStarted;
            Server.Stopped += OnServerStopped;
            Server.MessageReceived += ClientMessageRecived;


            LB_UsersOffline.ItemsSource = UsersOffline;
            LB_UsersOnline.ItemsSource = UsersOnline;
            LB_Groups.ItemsSource = DbTelegram.GroupChats.Local;

            foreach (var user in DbTelegram.Users)
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
            Console.WriteLine($"Message recieved. Type {msg.GetType().Name}");
            switch (msg.GetType().Name)
            {
                case "SignUpStage1Message":
                    {
                        SignUpStage1Message signUpMessage = (SignUpStage1Message)msg;


                        if (!DbTelegram.Users.Any(u => u.Email == signUpMessage.Email || u.Login == signUpMessage.Login))
                        {
                            User newUser = new User()
                            {
                                Email = signUpMessage.Email,
                                Login = signUpMessage.Login,
                                Name = signUpMessage.Name,
                                Password = signUpMessage.Password,
                                RegistrationDate = DateTime.UtcNow,
                                VisitDate = DateTime.UtcNow
                            };

                            DbTelegram.Users.Add(newUser);
                            Dispatcher.Invoke(() => UsersOffline.Add(newUser));
                            DbTelegram.SaveChanges();


                            client.SendAsync(new SignUpStage1ResultMessage(AuthenticationResult.Success));

                        }
                        else
                        {
                            client.SendAsync(new SignUpStage1ResultMessage(AuthenticationResult.Denied,
                                            "Email or login already used to create an account"));

                        }
                        break;

                    }
                case "LoginMessage":
                    {
                        LoginMessage loginMessage = (LoginMessage)msg;
                        User sender = DbTelegram.Users.FirstOrDefault(u => u.Login == loginMessage.Login || u.Email == loginMessage.Login);


                        if (sender != null && sender.Password == loginMessage.Password)
                        {
                            PublicUserInfo UserInfo = new PublicUserInfo()
                            {
                                Id = sender.Id,
                                Name = sender.Name
                            };

                            UserClient newUserClient
                                = new UserClient(loginMessage.MachineName, Guid.NewGuid().ToString());

                            newUserClient.User = sender;
                            sender.Clients.Add(newUserClient);
                           
                            UserInfo.ImagesId.AddRange(sender.ImagesId);

                            if(sender.Email == loginMessage.Login)
                                UserInfo.Login = sender.Login;


                            client.SendAsync(new LoginResultMessage(AuthenticationResult.Success, UserInfo, newUserClient.Guid));
                            

                            client.Disconnected += OnClientDisconnected;

                            ClientsOnline[newUserClient] = client;

                           
                            Dispatcher.Invoke(() =>
                            {
                                UsersOffline.Remove(sender);
                                UsersOnline.Add(sender);
                            });


                            DbTelegram.SaveChanges();

                            if(sender.Messages.Count != 0)
                                client.SendAsync(new ArrayMessage<ChatMessage>(sender.Messages));

                           

                            //if (sender.MessagesToSend.Count > 0)
                            //{

                            //    client.SendAsync(new ArrayMessage<BaseMessage>(sender.MessagesToSend));

                            //    ClientMessageHandler OnMessageSent = null;
                            //    OnMessageSent = (c, m) =>
                            //    {
                            //        sender.MessagesToSend.Clear();
                            //        client.MessageSent -= OnMessageSent;
                            //    };
                            //    client.MessageSent += OnMessageSent;
                            //}
                        }
                        else
                            client.SendAsync(new LoginResultMessage(AuthenticationResult.Denied,
                                                                    "wrong login/email or password"));
                        break;
                    }
                case "FastLoginMessage":
                    {
                        FastLoginMessage fastLoginMessage = (FastLoginMessage)msg;

                        User sender = DbTelegram.Users.FirstOrDefault(u => u.Id == fastLoginMessage.UserId);

                        if(sender != null)
                        {
                            UserClient userClient = sender.Clients.FirstOrDefault(c => c.MachineName == fastLoginMessage.MachineName);

                            if(userClient != null && userClient.Guid == fastLoginMessage.Guid)
                            {
                                client.SendAsync(new FastLoginResultMessage(AuthenticationResult.Success));
                                client.Disconnected += OnClientDisconnected;
                                ClientsOnline[userClient] = client;

                                Dispatcher.Invoke(() =>
                                {
                                    UsersOffline.Remove(sender);
                                    UsersOnline.Add(sender);
                                });
                            }    
                            else
                            {
                                userClient = sender.Clients.FirstOrDefault(c => c.Guid == fastLoginMessage.Guid);

                                if(userClient != null)
                                {
                                    //tmp
                                    userClient.MachineName = fastLoginMessage.MachineName;
                                    client.SendAsync(new FastLoginResultMessage(AuthenticationResult.Success));
                                    client.Disconnected += OnClientDisconnected;
                                    ClientsOnline[userClient] = client;


                                    Dispatcher.Invoke(() =>
                                    {
                                        UsersOffline.Remove(sender);
                                        UsersOnline.Add(sender);
                                    });

                                }
                                else
                                    client.SendAsync(new FastLoginResultMessage(AuthenticationResult.Denied));
                            }
                        }
                        break;
                    }
                case "ChatMessage":
                    {
                        ChatMessage newMessage = (ChatMessage)msg;
                        GroupChat chat = DbTelegram.GroupChats.First(gc => gc.Id == newMessage.GroupId);
                        
                        UserClient senderClient = ClientsOnline.FirstOrDefault(c => c.Value == client).Key;
                        User sender = senderClient.User;

                        sender.Messages.Add(newMessage);
                        chat.Messages.Add(newMessage);

                        DbTelegram.SaveChanges();

                        SendMessageToUsers(newMessage, sender.Id, senderClient.Id, chat.Members);

                        break;

                    }
                case "ChatLookupMessage":
                    {
                        ChatLookupMessage chatLookupMessage = (ChatLookupMessage)msg;
                    

                        UserClient senderClient = ClientsOnline.FirstOrDefault(c => c.Key.Guid == chatLookupMessage.UserGuid).Key;
                        User sender = senderClient.User;

                        ChatLookupResultMessage resultMessage =
                            new ChatLookupResultMessage();


                        foreach (var group in DbTelegram.GroupChats)
                        {
                            if (group.Name.ToLower().Contains(chatLookupMessage.Name.ToLower())
                                && !sender.Chats.Any(chat => chat.Id == group.Id)
                                && group.Type != GroupType.Personal)
                            {
                                resultMessage.Groups.Add(PublicGroupInfoFromGroup(group));

                            }
                        }

                        foreach(var user in DbTelegram.Users)
                        {
                            if(user.Name.Contains(chatLookupMessage.Name)
                               && user.Login.Contains(chatLookupMessage.Name))
                            {
                                resultMessage.UsersId.Add(user.Id); 
                            }
                        }

                        client.SendAsync(resultMessage);

                        break;
                    }
                case "CreateGroupMessage":
                    {
                        CreateGroupMessage createNewGroupMessage = (CreateGroupMessage)msg;
                        List<User> newGroupMembers = null;

                        UserClient senderClient = ClientsOnline.FirstOrDefault(c => c.Value == client).Key;
                        User sender = senderClient.User;  

                        newGroupMembers = DbTelegram.Users.Where(u => createNewGroupMessage.MembersId.Equals(u.Id)).ToList();

                        if (newGroupMembers.Count > 0)
                        {

                            if (newGroupMembers.Any(newGroupMember
                                => newGroupMember.BlockedUsersId.Any(blockedUserId => blockedUserId == sender.Id)))
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
                            DbTelegram.Images.Add(newImage);
                            
                            DbTelegram.SaveChanges();
                            DbTelegram.Images.Load();

                            newGroup.ImagesId.Add(newImage.Id);
                            
                        }
                        

                        Dispatcher.Invoke(() =>
                        {
                            DbTelegram.GroupChats.Add(newGroup);
                            DbTelegram.SaveChanges();
                            DbTelegram.GroupChats.Load();
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

                            DbTelegram.SaveChanges();


                            PublicGroupInfo GroupInfo = new PublicGroupInfo(newGroup.Name,
                                                                            newGroup.Description,
                                                                            newGroup.Id);

                            GroupInfo.MembersId = UsersId;

                            SendMessageToUsers(new GroupInviteMessage(GroupInfo, sender.Id),
                                                     sender.Id,
                                                     senderClient.Id,
                                                     newGroupMembers);
                        }

                        break;
                    }
                case "GroupJoinMessage":
                    {
                        GroupJoinMessage groupJoinMessage = (GroupJoinMessage)msg;
                        GroupChat group = DbTelegram.GroupChats.FirstOrDefault(g => g.Id == groupJoinMessage.GroupId);

                        User sender = DbTelegram.Users.FirstOrDefault(u => u.Id == groupJoinMessage.UserId);
                        UserClient senderClient = sender.Clients.FirstOrDefault(c => c.Guid == groupJoinMessage.Guid);


                        if (sender != null && group != null)
                        {

                            sender.Chats.Add(group);
                            group.AddMember(sender);

                            client.SendAsync(new GroupJoinResultMessage(AuthenticationResult.Success, group.Id));

                            PublicUserInfo userInfo = new PublicUserInfo()
                            {
                                Id = sender.Id,
                                Name = sender.Name,
                                Description = sender.Description,
                                Login = sender.Login,
                            };

                            userInfo.ImagesId.AddRange(sender.ImagesId);


                            SendMessageToUsers(new GroupUpdateMessage(group.Id) { NewUser = userInfo },
                                                        sender.Id,
                                                        senderClient.Id,
                                                        group.Members);

                        }
                        else
                            client.SendAsync(new GroupJoinResultMessage(AuthenticationResult.Denied));

                        break;
                    }
                case "DataRequestMessage":
                    {
                        DataRequestMessage message = (DataRequestMessage)msg;
                       

                        switch (message.Type)
                        {
                            case RequestType.File:
                            {
                               FileContainer[] results
                                        = DbTelegram.Files.Where(file => message.ItemsId.Contains(file.Id)).ToArray();

                                client.Send(new DataRequestResultMessage<FileContainer>(results));

                                break;
                            }
                            case RequestType.Image:
                            {
                                ImageContainer[] results
                                            = DbTelegram.Images.Where(image => message.ItemsId.Contains(image.Id)).ToArray();

                                client.Send(new DataRequestResultMessage<ImageContainer>(results));
                                break;
                            }
                            case RequestType.User:
                            {
                                List<UserContainer> results = new List<UserContainer>();
                                foreach(var user in DbTelegram.Users)
                                {
                                    if(message.ItemsId.Contains(user.Id))
                                    {
                                         UserContainer userItem = new UserContainer();
                                         userItem.User = new PublicUserInfo()
                                         {
                                             Id = user.Id,
                                             Login = user.Login,
                                             Description = user.Description,
                                             Name = user.Name
                                         };

                                         foreach(var image in DbTelegram.Images.Where(im => user.ImagesId.Contains(im.Id)))
                                             userItem.Images.Add(image);

                                         results.Add(userItem);
                                    }

                                }
                                
                                client.Send(new DataRequestResultMessage<UserContainer>(results));
                                break;
                            }


                        }


                        break;
                    }
            }
        }

        private void OnClientDisconnected(TcpClientWrap client)
        {
            UserClient DisconnectedClient
                = ClientsOnline.FirstOrDefault((c) => c.Value == client).Key;

            DisconnectedClient.User.VisitDate = DateTime.UtcNow;
            DbTelegram.SaveChanges();

            Dispatcher.Invoke(() =>
            {
                UsersOnline.Remove(DisconnectedClient.User);
                UsersOffline.Add(DisconnectedClient.User);

                ClientsOnline.Remove(DisconnectedClient);
            });
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

            foreach (var user in DbTelegram.Users)
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


        private void SendMessageToUsers(BaseMessage msg, int senderId, int senderClientId, List<User> usersToSend)
        {
            bool changesExist = false;

            foreach (var user in usersToSend)
            {
                if(user.Id == senderId)
                {
                    foreach(var userClient in user.Clients)
                    {
                        if(userClient.Id != senderClientId)
                            userClient.MessagesToSend.Add(msg);
                    }
                }
                else
                {
                    foreach (var userClient in user.Clients)
                    {
                        if (isUserOnline(userClient))
                            ClientsOnline[userClient].SendAsync(msg);
                        else
                            userClient.MessagesToSend.Add(msg);
                    }
                }
            }

            if(changesExist)
                DbTelegram.SaveChanges();
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

        private bool isUserOnline(UserClient userClient) => ClientsOnline.ContainsKey(userClient);
    }
}
