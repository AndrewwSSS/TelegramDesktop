using CommonLibrary.Containers;
using CommonLibrary.Messages;
using CommonLibrary.Messages.Auth;
using CommonLibrary.Messages.Auth.Login;
using CommonLibrary.Messages.Auth.SignUp;
using CommonLibrary.Messages.Files;
using CommonLibrary.Messages.Groups;
using CommonLibrary.Messages.Users;
using MessageLibrary;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;


namespace TelegramServer
{

    public partial class MainWindow : Window
    {
        private void ClientMessageRecived(TcpClientWrap client, Message msg)
        {
            //DEBUG
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
                            lock (DbTelegram)
                            {
                                DbTelegram.SaveChanges();
                            }
                            


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

                            UserClient senderClient
                                = new UserClient(loginMessage.MachineName, Guid.NewGuid().ToString());

                            senderClient.User = sender;
                            sender.Clients.Add(senderClient);

                            UserInfo.ImagesId.AddRange(sender.ImagesId);

                            if (sender.Email == loginMessage.Login)
                                UserInfo.Login = sender.Login;


                            client.SendAsync(new LoginResultMessage(AuthenticationResult.Success, UserInfo, senderClient.Guid));


                            client.Disconnected += OnClientDisconnected;

                            ClientsOnline[client] = senderClient;


                            Dispatcher.Invoke(() =>
                            {
                                UsersOffline.Remove(sender);
                                UsersOnline.Add(sender);
                            });

                            lock(DbTelegram)
                            {
                                DbTelegram.SaveChanges();
                            }
                            

                            if (senderClient.MessagesToSend.Count != 0)
                            {
                                ClientMessageEventHandler onMessagesSent = null;
                                onMessagesSent = (cl, message) =>
                                {
                                    senderClient.MessagesToSend.Clear();
                                    client.MessageSent -= onMessagesSent;
                                };

                                client.MessageSent += onMessagesSent;
                                client.SendAsync(new ArrayMessage<BaseMessage>(senderClient.MessagesToSend));
                            }
                               



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

                        ClientMessageEventHandler onMessagesSent = null;

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

                                    client.MessageSent += onMessagesSent;
                                    client.SendAsync(new ArrayMessage<BaseMessage>(userClient.MessagesToSend));
                                    
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

                        ChatMessageSendResult resultMessage
                            = new ChatMessageSendResult();

                        sender.Messages.Add(newMessage);
                        group.Messages.Add(newMessage);

                        lock (DbTelegram)
                        {
                            DbTelegram.SaveChanges();
                            DbTelegram.GroupChats.Load();
                        }
                      

                        client.SendAsync(new ChatMessageSendResult(toGroupMessage.LocalMessageId, newMessage.Id));

                        SendMessageToUsers(newMessage, sender.Id, senderClient.Id, group.Members.ToList());
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

                                lock(DbTelegram)
                                {
                                    DbTelegram.SaveChanges();
                                    DbTelegram.GroupChats.Load();
                                }
                               
                                
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
                                               newChat.Members.ToList());
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
                        sender.Chats.Add(newGroup);

                        Dispatcher.Invoke(() =>
                        {
                            DbTelegram.GroupChats.Add(newGroup);
                            lock(DbTelegram)
                            {
                                DbTelegram.SaveChanges();
                                DbTelegram.GroupChats.Load();
                            }
                        });


                        client.SendAsync(new CreateGroupResultMessage(AuthenticationResult.Success, newGroup.Id));


                        if (newGroupMembers.Count > 0)
                        {
                            foreach (var newGroupMember in newGroupMembers)
                                newGroup.Members.Add(newGroupMember);


                            foreach (var member in newGroupMembers)
                                member.Chats.Add(newGroup);


                            lock (DbTelegram) {
                                DbTelegram.SaveChanges();
                            }
                          

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

                            lock (DbTelegram){
                                DbTelegram.SaveChanges();
                            }

                            User test = DbTelegram.Users.FirstOrDefault(u => u.Id == sender.Id);
                            GroupChat testg = DbTelegram.GroupChats.FirstOrDefault(g => g.Id == group.Id);

                            client.SendAsync(new GroupJoinResultMessage(AuthenticationResult.Success, group.Id));

                   
                            SendMessageToUsers(new GroupUpdateMessage(group.Id) { NewUserId = sender.Id },
                                               sender.Id,
                                               senderClient.Id,
                                               group.Members.ToList());

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
                            case DataRequestType.FileData:
                                {
                                    List<FileContainer> Files = DbTelegram.Files.Where(file => dataRequestMessage.ItemsId.Contains(file.Id)).ToList();
                                    TcpFileClientWrap fileClient = FileClientsOnline[ClientsOnline[client]];


                                    foreach (var file in Files)
                                        fileClient.SendFileAsync(file);
                                  
                                    
                                    break;
                                }
                            case DataRequestType.ImageData:
                                {
                                    List<ImageContainer> Images = DbTelegram.Images.Where(image => dataRequestMessage.ItemsId.Contains(image.Id)).ToList();

                                    TcpFileClientWrap fileClient = FileClientsOnline[ClientsOnline[client]];


                                    foreach (var img in Images)
                                        fileClient.SendImageAsync(img);

                                    break;
                                }
                            case DataRequestType.ImageMetaData:
                                {
                                    ImageMetadata[] results = DbTelegram.Images.Where(image => dataRequestMessage.ItemsId.Contains(image.Id)).Select(r => r.Metadata).ToArray();

                                    client.Send(new DataRequestResultMessage<ImageMetadata>(results));
                                    break;
                                }
                            case DataRequestType.FileMetadata:
                                {
                                    FileMetadata[] results = DbTelegram.Files.Where(file => dataRequestMessage.ItemsId.Contains(file.Id)).Select(r => r.Metadata).ToArray();

                                    client.Send(new DataRequestResultMessage<FileMetadata>(results));
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
                                lock (DbTelegram)
                                {
                                    DbTelegram.SaveChanges();
                                }
                               

                                client.SendAsync(new DeleteChatMessageResultMessage(group.Id, deletedMessage.Id));

                                SendMessageToUsers(MsgDeleteMessage, 
                                                    sender.Id,
                                                    senderClient.Id,
                                                    group.Members.ToList());
                            }
                            else
                                client.SendAsync(new DeleteChatMessageResultMessage(AuthenticationResult.Denied));
                        }
                        else
                            client.SendAsync(new DeleteChatMessageResultMessage(AuthenticationResult.Denied));

                        break;
                    }
                case "SystemMessage":
                    {
                        SystemMessage messageType = (SystemMessage)msg;
                        UserClient senderClient = ClientsOnline[client];
                        User sender = senderClient.User;

                        switch (messageType.Type)
                        {
                            case SystemMessageType.Logout:
                                {
                                    sender.Clients.Remove(senderClient);
                                    lock (DbTelegram)
                                    {
                                        DbTelegram.SaveChanges();
                                    }
                                    
                                    break;
                                }
                        }


                        break;
                    }
                case "LeaveFromGroupMessage":
                    {
                        UserClient senderClient = ClientsOnline[client];
                        User sender = senderClient.User;

                        LeaveFromGroupMessage 
                            leaveFromGroup = (LeaveFromGroupMessage)msg;

                        GroupChat group
                            = DbTelegram.GroupChats.FirstOrDefault(gc => gc.Id == leaveFromGroup.Id);
                        

                        if(group != null)
                        {
                            
                            group.Members.Remove(sender);
                            lock (DbTelegram) {
                                DbTelegram.SaveChanges();
                            }
                            


                            GroupUpdateMessage groupUpdate = new GroupUpdateMessage()
                            {
                                GroupId = group.Id,
                                RemovedUserId = sender.Id
                            };


                            if(group.Members.Count == 0) {

                                DbTelegram.GroupChats.Remove(group);

                                lock (DbTelegram) {
                                    DbTelegram.SaveChanges();
                                }
                            }
                            else
                            {
                                SendMessageToUsers(groupUpdate, sender.Id,
                                              senderClient.Id,
                                              new List<User>(group.Members) { sender });
                            }
                        }
                        break;
                    }
                case "GroupUpdateMessage":
                    {
                        GroupUpdateMessage groupUpdateMessage = (GroupUpdateMessage)msg;

                        UserClient senderClient = ClientsOnline[client];
                        User sender = senderClient.User;
                        GroupChat updatedGroup = DbTelegram.GroupChats.FirstOrDefault(g => g.Id == groupUpdateMessage.Id);


                        GroupUpdateMessage messageToMembers
                            = new GroupUpdateMessage(sender.Id, groupUpdateMessage);
                        
                        if(updatedGroup != null)
                        {
                            bool ChangesExists = false;
                            if(groupUpdateMessage.NewDescription != null)
                            {
                                updatedGroup.Description = groupUpdateMessage.NewDescription;
                                ChangesExists = true;
                            }

                            if(groupUpdateMessage.NewName != null)
                            {
                                updatedGroup.Name = groupUpdateMessage.NewName;
                                ChangesExists = true;
                            }

                            if (ChangesExists)
                            {
                                lock (DbTelegram)
                                {
                                    DbTelegram.SaveChanges();
                                }
                            }
                                
                        }

                        break;
                    }
                case "MetadataMessage":
                    {
                        MetadataMessage metadataMessage = (MetadataMessage)msg;

                        UserClient senderClient = ClientsOnline[client];
    
                        UsersDownloads[senderClient] = new UserDownloads(metadataMessage.LocalMessageId, metadataMessage.Images, metadataMessage.Files);

                        client.SendAsync(new MetadataSyncMessage(metadataMessage.LocalReturnId));
                        break;
                    }
            }
        }

        private void FileServer_FileChunkReceived(TcpFileClientWrap client, FileChunk chunk)
        {
            UserClient senderClient = FileClientsOnline.First(fc => fc.Value == client).Key;
            UserDownloads downloads = UsersDownloads[senderClient];


            if (chunk.IsImage)
            {
                KeyValuePair<int, FileDownload> ChunksInfo;

                if (downloads.ImagesInProcess.Any(kv => kv.Key == chunk.FileId))
                    ChunksInfo = downloads.ImagesInProcess.First(kv => kv.Key == chunk.FileId);
                else
                {
                    ChunksInfo = new KeyValuePair<int, FileDownload>(chunk.FileId, new FileDownload());
                    downloads.ImagesInProcess.Add(ChunksInfo);
                }

                FileDownload fileDownload = ChunksInfo.Value;

                fileDownload.AddChunk(chunk);

                if (fileDownload.isCompleted)
                {
                    KeyValuePair<int, ImageMetadata> metadataInfo = downloads.RemainingImages.FirstOrDefault(ri => ri.Key == chunk.FileId);
                    ImageMetadata metaData = metadataInfo.Value;

                    MemoryStream stream = new MemoryStream();
                    foreach (var fileChunk in fileDownload.GetOrderedChanks())
                        stream.Write(fileChunk.Data, 0, fileChunk.Data.Length);

                    ImageContainer newImage = new ImageContainer(metaData.Name, stream.ToArray());

                    DbTelegram.Images.Add(newImage);
                    DbTelegram.SaveChanges();
                    DbTelegram.Images.Load();

                    downloads.ImageFinished(chunk.FileId, newImage.Id);
                }

            }
            else
            {
                KeyValuePair<int, FileDownload> ChunksInfo;

                if (downloads.FilesInProcess.Any(kv => kv.Key == chunk.FileId))
                    ChunksInfo = downloads.FilesInProcess.First(kv => kv.Key == chunk.FileId);
                else
                {
                    ChunksInfo = new KeyValuePair<int, FileDownload>(chunk.FileId, new FileDownload());
                    downloads.FilesInProcess.Add(ChunksInfo);
                }

                FileDownload fileDownload = ChunksInfo.Value;


                fileDownload.AddChunk(chunk);


                if (fileDownload.isCompleted)
                {
                    KeyValuePair<int, FileMetadata> metadataInfo = downloads.RemainingFiles.FirstOrDefault(ri => ri.Key == chunk.FileId);
                    FileMetadata metaData = metadataInfo.Value;

                    MemoryStream stream = new MemoryStream();
                    foreach (var fileChunk in fileDownload.GetOrderedChanks())
                        stream.Write(fileChunk.Data, 0, fileChunk.Data.Length);

                    FileContainer newFile = new FileContainer(metaData.Name, stream.ToArray());

                    lock (DbTelegram)
                    {
                        DbTelegram.Files.Add(newFile);
                        DbTelegram.SaveChanges();
                        DbTelegram.Files.Load();
                    }


                    downloads.FileFinished(chunk.FileId, newFile.Id);
                }

            }

            if (downloads.IsCompleted)
            {
                MetadataResultMessage resultMessage
                    = new MetadataResultMessage(downloads.ForMessageId,
                                                downloads.FinishedImages,
                                                downloads.FinishedFiles);

                TcpClientWrap TcpClient = ClientsOnline.FirstOrDefault(co => co.Value == senderClient).Key;

                TcpClient.SendAsync(resultMessage);
            }
        }

    }
}
