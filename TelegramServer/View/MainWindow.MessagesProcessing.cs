﻿using CommonLibrary.Containers;
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

                            lock (DbTelegram) {
                                DbTelegram.SaveChanges();
                            }



                            client.SendAsync(new SignUpStage1ResultMessage(AuthResult.Success));

                        }
                        else
                        {
                            client.SendAsync(new SignUpStage1ResultMessage(AuthResult.Denied,
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


                            client.SendAsync(new LoginResultMessage(AuthResult.Success, UserInfo, senderClient.Guid));


                            client.Disconnected += OnClientDisconnected;

                            ClientsOnline[client] = senderClient;


                            Dispatcher.Invoke(() =>
                            {
                                UsersOffline.Remove(sender);
                                UsersOnline.Add(sender);
                            });

                            lock (DbTelegram) {
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

                            SendMessageToUsers(new UserUpdateMessage() { OnlineStatus = true },
                                               sender.Id,
                                               senderClient.Id,
                                               sender.UniqueRelations);


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
                            client.SendAsync(new LoginResultMessage(AuthResult.Denied,
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
                                client.SendAsync(new FastLoginResultMessage(AuthResult.Success));
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
                                    client.SendAsync(new FastLoginResultMessage(AuthResult.Success));
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
                                    client.SendAsync(new FastLoginResultMessage(AuthResult.Denied));
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

                                lock (DbTelegram)
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

                                client.SendAsync(new CreateGroupResultMessage(AuthResult.Denied,
                                                 "One or more users blocked you"));
                                break;
                            }

                        }

                        GroupChat newGroup = new GroupChat()
                        {
                            Name = createNewGroupMessage.Name,
                            Description = createNewGroupMessage.Description,
                            DateCreated = DateTime.UtcNow,
                            Type = GroupType.Public
                        };

                        newGroup.Members.Add(sender);
                        newGroup.Administrators.Add(sender);


                        sender.Chats.Add(newGroup);

                        Dispatcher.Invoke(() =>
                        {
                            DbTelegram.GroupChats.Add(newGroup);
                            lock (DbTelegram)
                            {
                                DbTelegram.SaveChanges();
                                DbTelegram.GroupChats.Load();
                            }
                        });


                        client.SendAsync(new CreateGroupResultMessage(AuthResult.Success,
                                                                      newGroup.Id,
                                                                      createNewGroupMessage.LocalId));


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

                            lock (DbTelegram) {
                                DbTelegram.SaveChanges();
                            }


                            client.SendAsync(new GroupJoinResultMessage(AuthResult.Success, group.Id));


                            SendMessageToUsers(new GroupUpdateMessage(group.Id) { NewUserId = sender.Id },
                                               sender.Id,
                                               senderClient.Id,
                                               group.Members.ToList());

                        }
                        else
                            client.SendAsync(new GroupJoinResultMessage(AuthResult.Denied));

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
                                client.SendAsync(new DeleteChatMessageResultMessage(AuthResult.Denied));
                        }
                        else
                            client.SendAsync(new DeleteChatMessageResultMessage(AuthResult.Denied));

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
                                    break;
                                }
                        }


                        break;
                    }
                case "GroupLeaveMessage":
                    {
                        UserClient senderClient = ClientsOnline[client];
                        User sender = senderClient.User;

                        GroupLeaveMessage
                            leaveFromGroup = (GroupLeaveMessage)msg;

                        GroupChat group
                            = DbTelegram.GroupChats.FirstOrDefault(gc => gc.Id == leaveFromGroup.GroupId);


                        if (group != null)
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


                            if (group.Members.Count == 0) {
                                Dispatcher.Invoke(() =>
                                {
                                    DbTelegram.GroupChats.Remove(group);
                                    DbTelegram.GroupChats.Load();
                                });


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
                        GroupChat updatedGroup = DbTelegram.GroupChats.FirstOrDefault(g => g.Id == groupUpdateMessage.GroupId);

                        if (updatedGroup != null)
                        {
                            bool ChangesExists = false;
                            if (groupUpdateMessage.NewDescription != null)
                            {
                                updatedGroup.Description = groupUpdateMessage.NewDescription;
                                ChangesExists = true;
                            }
                         
                            if (groupUpdateMessage.NewName != null)
                            {
                                updatedGroup.Name = groupUpdateMessage.NewName;
                                ChangesExists = true;
                            }
                            
                            if (ChangesExists) {
                                lock (DbTelegram) {
                                    DbTelegram.SaveChanges();
                                    DbTelegram.GroupChats.Load();
                                }

                                client.SendAsync(new GroupUpdateResultMessage(AuthResult.Success,
                                                                              updatedGroup.Id));

                            }

                            groupUpdateMessage.SenderId = sender.Id;

                            SendMessageToUsers(groupUpdateMessage,
                                               sender.Id,
                                               senderClient.Id,
                                               updatedGroup.Members);

                        }

                        break;
                    }
                case "UserUpdateMessage":
                    {
                        UserUpdateMessage userUpdateMessage = (UserUpdateMessage)msg;
                        UserClient senderClient = ClientsOnline[client];
                        User sender = senderClient.User;

                        bool changesExists = false;

                        if (userUpdateMessage.NewLogin != null)
                        {
                            if(!DbTelegram.Users.Any(u => u.Login == userUpdateMessage.NewLogin))
                            {
                                sender.Login = userUpdateMessage.NewLogin;
                                changesExists = true;
                            }
                            else
                            {
                                client.SendAsync(new UserUpdateResultMessage(AuthResult.Denied));
                                return;
                            }
                        }

                        if (userUpdateMessage.NewName != null)
                        {
                            sender.Name = userUpdateMessage.NewName;
                            changesExists = true;
                        }
                     
                        if(userUpdateMessage.NewDescription != null)
                        {
                            sender.Description = userUpdateMessage.NewDescription;
                            changesExists = true;
                        }
                           


                        if (changesExists) {
                            lock (DbTelegram) {
                                DbTelegram.SaveChanges();
                            }

                            client.SendAsync(new UserUpdateResultMessage(AuthResult.Success));

                            var MembersCollections = sender.Chats.Select(c => c.Members);
                            List<User> members = new List<User>();

                            foreach (var memberCollection in MembersCollections)
                                foreach (var member in memberCollection)
                                    members.Add(member);

                            var usersToSend = members.Distinct().ToList();

                            userUpdateMessage.UserId = sender.Id;
                            SendMessageToUsers(userUpdateMessage, sender.Id, senderClient.Id, usersToSend);
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
                case "KickUserMessage":
                    {
                        KickUserMessage kickUserMessage = (KickUserMessage)msg;

                        UserClient senderClient = ClientsOnline[client];
                        User sender = senderClient.User;

                        GroupChat group = 
                            DbTelegram.GroupChats.FirstOrDefault(g => g.Id == kickUserMessage.GroupId);


                        if(group != null)
                        {
                            if(group.Administrators.Any(a => a.Id == sender.Id) &&
                              !group.Administrators.Any(a => a.Id == kickUserMessage.UserId))
                            {
                                User deletedUser
                                    = group.Members.FirstOrDefault(m => m.Id == kickUserMessage.UserId);

                                group.Members.Remove(deletedUser);
                                deletedUser.Chats.Remove(group);

                                lock (DbTelegram) {
                                    DbTelegram.SaveChanges();
                                }

                                client.SendAsync(new UserActionResultMessage(UserActionType.Kick,
                                                                             AuthResult.Success,
                                                                             deletedUser.Id, group.Id));

                                SendMessageToUsers(new GroupUpdateMessage(group.Id) { RemovedUserId = deletedUser.Id },
                                                   sender.Id,
                                                   senderClient.Id,
                                                   new List<User>(group.Members) { deletedUser });
                            }
                            else
                            {
                                client.SendAsync(new UserActionResultMessage(UserActionType.Kick,
                                                                             AuthResult.Denied,
                                                                             sender.Id, group.Id));
                            }
                        }
                        break;
                    }



            }
        }

        //private void FileServer_FileChunkReceived(TcpFileClientWrap client, FileChunk chunk)
        //{
        //    UserClient senderClient = FileClientsOnline.First(fc => fc.Value == client).Key;
        //    UserDownloads downloads = UsersDownloads[senderClient];

        //    KeyValuePair<int, FileDownload> ChunksInfo;

        //    if (downloads.FilesInProcess.Any(kv => kv.Key == chunk.FileId))
        //        ChunksInfo = downloads.FilesInProcess.First(kv => kv.Key == chunk.FileId);
        //    else
        //    {
        //        ChunksInfo = new KeyValuePair<int, FileDownload>(chunk.FileId, new FileDownload());
        //        downloads.FilesInProcess.Add(ChunksInfo);
        //    }

        //    FileDownload fileDownload = ChunksInfo.Value;


        //    fileDownload.AddChunk(chunk);

        //    if (chunk.IsLast)
        //    {
        //        KeyValuePair<int, FileMetadata> metadataInfo = downloads.RemainingFiles.FirstOrDefault(ri => ri.Key == chunk.FileId);
        //        FileMetadata metaData = metadataInfo.Value;

        //        MemoryStream stream = new MemoryStream();
        //        foreach (var fileChunk in fileDownload.Chunks)
        //            stream.Write(fileChunk.Data, 0, fileChunk.Data.Length);

        //        FileContainer newFile = new FileContainer(metaData.Name, stream.ToArray());

        //        lock (DbTelegram) {
        //            DbTelegram.Files.Add(newFile);
        //            DbTelegram.SaveChanges();
        //            DbTelegram.Files.Load();
        //        }


        //        downloads.FileFinished(chunk.FileId, newFile.Id);
        //    }

            //if (fileDownload.isCompleted)
            //{
            //    KeyValuePair<int, FileMetadata> metadataInfo = downloads.RemainingFiles.FirstOrDefault(ri => ri.Key == chunk.FileId);
            //    FileMetadata metaData = metadataInfo.Value;

            //    MemoryStream stream = new MemoryStream();
            //    foreach (var fileChunk in fileDownload.GetOrderedChanks())
            //        stream.Write(fileChunk.Data, 0, fileChunk.Data.Length);

            //    FileContainer newFile = new FileContainer(metaData.Name, stream.ToArray());

            //    lock (DbTelegram)
            //    {
            //        DbTelegram.Files.Add(newFile);
            //        DbTelegram.SaveChanges();
            //        DbTelegram.Files.Load();
            //    }


            //    downloads.FileFinished(chunk.FileId, newFile.Id);
            //}

        //    if (downloads.IsCompleted)
        //    {
        //        MetadataResultMessage resultMessage
        //            = new MetadataResultMessage(downloads.ForMessageId,
        //                                        downloads.FinishedImages,
        //                                        downloads.FinishedFiles);

        //        TcpClientWrap TcpClient = ClientsOnline.FirstOrDefault(co => co.Value == senderClient).Key;

        //        TcpClient.SendAsync(resultMessage);
        //    }
        //}

        //private void FileServer_ImageChunkReceived(TcpFileClientWrap client, FileChunk chunk)
        //{
        //    UserClient senderClient = FileClientsOnline.First(fc => fc.Value == client).Key;
        //    UserDownloads downloads = UsersDownloads[senderClient];


        //    KeyValuePair<int, FileDownload> ChunksInfo;

        //    if (downloads.ImagesInProcess.Any(kv => kv.Key == chunk.FileId))
        //        ChunksInfo = downloads.ImagesInProcess.First(kv => kv.Key == chunk.FileId);
        //    else
        //    {
        //        ChunksInfo = new KeyValuePair<int, FileDownload>(chunk.FileId, new FileDownload());
        //        downloads.ImagesInProcess.Add(ChunksInfo);
        //    }

        //    FileDownload fileDownload = ChunksInfo.Value;

        //    fileDownload.AddChunk(chunk);

        //    if (fileDownload.isCompleted)
        //    {
        //        KeyValuePair<int, ImageMetadata> metadataInfo = downloads.RemainingImages.FirstOrDefault(ri => ri.Key == chunk.FileId);
        //        ImageMetadata metaData = metadataInfo.Value;

        //        MemoryStream stream = new MemoryStream();
        //        foreach (var fileChunk in fileDownload.GetOrderedChanks())
        //            stream.Write(fileChunk.Data, 0, fileChunk.Data.Length);

        //        ImageContainer newImage = new ImageContainer(metaData.Name, stream.ToArray());

        //        DbTelegram.Images.Add(newImage);
        //        DbTelegram.SaveChanges();
        //        DbTelegram.Images.Load();

        //        downloads.ImageFinished(chunk.FileId, newImage.Id);
        //    }

        //    if (downloads.IsCompleted)
        //    {
        //        MetadataResultMessage resultMessage
        //            = new MetadataResultMessage(downloads.ForMessageId,
        //                                        downloads.FinishedImages,
        //                                        downloads.FinishedFiles);

        //        TcpClientWrap TcpClient = ClientsOnline.FirstOrDefault(co => co.Value == senderClient).Key;

        //        TcpClient.SendAsync(resultMessage);
        //    }
        //}

    }
}
