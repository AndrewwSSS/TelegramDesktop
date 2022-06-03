using CommonLibrary.Containers;
using CommonLibrary.Messages;
using CommonLibrary.Messages.Auth;
using CommonLibrary.Messages.Auth.Login;
using CommonLibrary.Messages.Auth.Logout;
using CommonLibrary.Messages.Auth.SignUp;
using CommonLibrary.Messages.Groups;
using CommonLibrary.Messages.Users;
using MessageLibrary;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Threading;


namespace TelegramServer
{

    public partial class MainWindow : Window
    {
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

                            if (sender.Email == loginMessage.Login)
                                UserInfo.Login = sender.Login;


                            client.SendAsync(new LoginResultMessage(AuthenticationResult.Success, UserInfo, newUserClient.Guid));


                            client.Disconnected += OnClientDisconnected;

                            ClientsOnline[client] = newUserClient;


                            Dispatcher.Invoke(() =>
                            {
                                UsersOffline.Remove(sender);
                                UsersOnline.Add(sender);
                            });


                            DbTelegram.SaveChanges();

                            if (sender.Messages.Count != 0)
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

                        ClientMessageHandler onMessagesSent = null;

                        if (sender != null)
                        {
                            UserClient userClient = sender.Clients.FirstOrDefault(c => c.MachineName == fastLoginMessage.MachineName);

                            if (userClient != null && userClient.Guid == fastLoginMessage.Guid)
                            {
                                client.SendAsync(new FastLoginResultMessage(AuthenticationResult.Success));
                                client.Disconnected += OnClientDisconnected;
                                ClientsOnline[client] = userClient;




                                if (userClient.MessagesToSend.Count > 0)
                                {
                                    onMessagesSent = (cl, message) =>
                                    {
                                        userClient.MessagesToSend.Clear();
                                        client.MessageSent -= onMessagesSent;
                                    };

                                    client.SendAsync(new ArrayMessage<BaseMessage>(userClient.MessagesToSend));
                                    client.MessageSent += onMessagesSent;
                                }



                                Dispatcher.Invoke(() =>
                                {
                                    UsersOffline.Remove(sender);
                                    UsersOnline.Add(sender);
                                });
                            }
                            else
                            {
                                userClient = sender.Clients.FirstOrDefault(c => c.Guid == fastLoginMessage.Guid);

                                if (userClient != null)
                                {
                                    //tmp
                                    userClient.MachineName = fastLoginMessage.MachineName;
                                    client.SendAsync(new FastLoginResultMessage(AuthenticationResult.Success));
                                    client.Disconnected += OnClientDisconnected;
                                    ClientsOnline[client] = userClient;


                                    if (userClient.MessagesToSend.Count > 0)
                                    {
                                        onMessagesSent = (cl, message) =>
                                        {
                                            userClient.MessagesToSend.Clear();
                                            client.MessageSent -= onMessagesSent;
                                        };

                                        client.SendAsync(new ArrayMessage<BaseMessage>(userClient.MessagesToSend));
                                        client.MessageSent += onMessagesSent;
                                    }

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
                case "MessageToGroupMessage":
                    {
                        MessageToGroupMessage toGroupMessage =
                            (MessageToGroupMessage)msg;

                        ChatMessage newMessage = toGroupMessage.Message;
                        GroupChat group = DbTelegram.GroupChats.First(gc => gc.Id == newMessage.GroupId);

                        UserClient senderClient = ClientsOnline[client];
                        User sender = senderClient.User;


                  


                        sender.Messages.Add(newMessage);
                        group.Messages.Add(newMessage);


                        DbTelegram.SaveChanges();
                        DbTelegram.GroupChats.Load();

                        client.SendAsync(new ChatMessageSendResult(toGroupMessage.LocalMessageId, newMessage.Id));

                        SendMessageToUsers(newMessage, sender.Id, senderClient.Id, group.Members);
                        break;

                    }
                case "ChatLookupMessage":
                    {
                        ChatLookupMessage chatLookupMessage = (ChatLookupMessage)msg;
                        User sender = ClientsOnline[client].User;

                        ChatLookupResultMessage resultMessage =
                            new ChatLookupResultMessage();

                        foreach (var group in DbTelegram.GroupChats)
                        {
                            if (group.Type != GroupType.Personal
                                && group.Name.ToLower().Contains(chatLookupMessage.Name.ToLower())
                                && !sender.Chats.Any(chat => chat.Id == group.Id))
                            {
                                resultMessage.Groups.Add(new PublicGroupInfo(group));

                            }
                        }

                        foreach (var user in DbTelegram.Users)
                        {
                            if (user.Id != sender.Id &&
                                (user.Name.ToLower().Contains(chatLookupMessage.Name.ToLower())
                               || user.Login.ToLower().Contains(chatLookupMessage.Name.ToLower())))
                            {
                                resultMessage.UsersId.Add(user.Id);
                            }
                        }


                        client.SendAsync(resultMessage);

                        break;
                    }
                case "FirstPersonalMessage":
                    {
                        FirstPersonalMessage firstPersonalMessage
                                = (FirstPersonalMessage)msg;

                        UserClient senderClient =
                            ClientsOnline[client];

                        User sender = senderClient.User;

                        User toUser = DbTelegram.Users.FirstOrDefault(u => u.Id == firstPersonalMessage.ToUserId);

                        if (toUser != null && senderClient != null)
                        {
                            GroupChat newChat = new GroupChat()
                            {
                                DateCreated = DateTime.UtcNow,
                                Type = GroupType.Personal
                            };

                            newChat.Members.Add(sender);
                            newChat.Members.Add(toUser);


                            Dispatcher.Invoke(() =>
                            {
                                DbTelegram.GroupChats.Add(newChat);
                                DbTelegram.SaveChanges();
                                DbTelegram.GroupChats.Load();
                            });



                            client.SendAsync(new FirstPersonalResultMessage(newChat.Id, firstPersonalMessage.LocalId));


                            PublicGroupInfo newChatInfo = new PublicGroupInfo() {
                                Id = newChat.Id,
                            };

                            newChatInfo.Messages.Add(firstPersonalMessage.Message);
                            newChatInfo.MembersId.AddRange(new List<int>() { sender.Id, toUser.Id });

                            SendMessageToUsers(new PersonalChatCreatedMessage(newChatInfo),
                                               sender.Id,
                                               senderClient.Id,
                                               newChat.Members);
                        }



                        break;
                    }
                case "CreateGroupMessage":
                    {
                        CreateGroupMessage createNewGroupMessage = (CreateGroupMessage)msg;
                        UserClient senderClient = ClientsOnline[client];
                        User sender = senderClient.User;
                        List<User> newGroupMembers = DbTelegram.Users.Where(u => createNewGroupMessage.MembersId.Equals(u.Id)).ToList();

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
                            DateCreated = DateTime.UtcNow,
                            Type = GroupType.Public
                        };

                        newGroup.Members.Add(sender);
                        newGroup.Administrators.Add(sender);

                        if (createNewGroupMessage.Image != null)
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

                            foreach (var member in newGroupMembers)
                                member.Chats.Add(newGroup);

                            DbTelegram.SaveChanges();


                            PublicGroupInfo GroupInfo = new PublicGroupInfo(newGroup.Name,
                                                                            newGroup.Description,
                                                                            newGroup.Id);


                            GroupInfo.AdministratorsId.Add(sender.Id);
                            GroupInfo.MembersId = newGroup.Members.Select(m => m.Id).ToList();


                            List<User> DistributionList = new List<User>(newGroupMembers);
                            DistributionList.Add(sender);

                            SendMessageToUsers(new GroupInviteMessage(GroupInfo, sender.Id),
                                                     sender.Id,
                                                     senderClient.Id,
                                                     DistributionList);
                        }

                        break;
                    }
                case "GroupJoinMessage":
                    {
                        GroupJoinMessage groupJoinMessage = (GroupJoinMessage)msg;
                        GroupChat group = DbTelegram.GroupChats.FirstOrDefault(g => g.Id == groupJoinMessage.GroupId);
                        UserClient senderClient = ClientsOnline[client];
                        User sender = senderClient.User;
      

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
                        DataRequestMessage dataRequestMessage = (DataRequestMessage)msg;


                        switch (dataRequestMessage.Type)
                        {
                            case DataRequestType.File:
                                {
                                    FileContainer[] results
                                             = DbTelegram.Files.Where(file => dataRequestMessage.ItemsId.Contains(file.Id)).ToArray();

                                    client.Send(new DataRequestResultMessage<FileContainer>(results));

                                    break;
                                }
                            case DataRequestType.Image:
                                {
                                    ImageContainer[] results
                                                = DbTelegram.Images.Where(image => dataRequestMessage.ItemsId.Contains(image.Id)).ToArray();

                                    client.Send(new DataRequestResultMessage<ImageContainer>(results));
                                    break;
                                }
                            case DataRequestType.User:
                                {
                                    List<UserContainer> results = new List<UserContainer>();
                                    foreach (var user in DbTelegram.Users)
                                    {
                                        if (dataRequestMessage.ItemsId.Contains(user.Id))
                                        {
                                            UserContainer userItem = new UserContainer();
                                            userItem.User = new PublicUserInfo()
                                            {
                                                Id = user.Id,
                                                Login = user.Login,
                                                Description = user.Description,
                                                Name = user.Name
                                            };

                                            foreach (var image in DbTelegram.Images.Where(im => user.ImagesId.Contains(im.Id)))
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
                case "ChatMessageDeleteMessage":
                    {
                        ChatMessageDeleteMessage MsgDeleteMessage
                            = (ChatMessageDeleteMessage)msg;

                        UserClient senderClient = ClientsOnline[client];
                        User sender = senderClient.User;

                        GroupChat group
                            = DbTelegram.GroupChats.FirstOrDefault(gc => gc.Id == MsgDeleteMessage.GroupId);

                        if (group != null)
                        {
                            ChatMessage deletedMessage = group.Messages.FirstOrDefault(gc => gc.Id == MsgDeleteMessage.DeletedMessageId);

                            if (deletedMessage != null &&
                               (deletedMessage.FromUserId == deletedMessage.FromUserId ||
                               group.Administrators.Any(a => a.Id == sender.Id)))
                            {

                                group.Messages.Remove(deletedMessage);
                                DbTelegram.SaveChanges();

                                client.SendAsync(new DeleteChatMessageResultMessage(group.Id, deletedMessage.Id));

                                SendMessageToUsers(MsgDeleteMessage, sender.Id, senderClient.Id, group.Members);
                            }
                            else
                                client.SendAsync(new DeleteChatMessageResultMessage(AuthenticationResult.Denied));
                        }
                        else
                            client.SendAsync(new DeleteChatMessageResultMessage(AuthenticationResult.Denied));

                        break;
                    }
                case "LogoutMessage":
                    {
                        LogoutMessage logoutMessage = (LogoutMessage)msg;

                        break;
                    }
            }
        }
    }
}
