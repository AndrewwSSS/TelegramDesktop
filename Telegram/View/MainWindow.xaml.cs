﻿using CacheLibrary;
using CommonLibrary.Containers;
using CommonLibrary.Messages;
using CommonLibrary.Messages.Auth;
using CommonLibrary.Messages.Auth.Logout;
using CommonLibrary.Messages.Files;
using CommonLibrary.Messages.Groups;
using CommonLibrary.Messages.Users;
using MessageLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Telegram.Utility;
using Telegram.WPF_Entities;
using ImageMetadata = CommonLibrary.Containers.ImageMetadata;

namespace Telegram
{

    public enum MenuState
    {
        Hidden,
        Open
    }

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MessageItemWrap RespondingTo
        {
            get => respondingTo;
            set
            {
                respondingTo = value;
                OnPropertyChanged();
            }
        }
        public bool EditingGroup
        {
            get => editingGroup;
            set
            {
                editingGroup = value;
                if (editingGroup)
                    B_EditGroup.Content = "SAVE";
                else
                    B_EditGroup.Content = "EDIT";

                OnPropertyChanged();
            }
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public UICommand MessageDoubleClick { get; set; }

        public TcpClientWrap Client
        {
            get => client;
            set
            {
                client = value;
                if (client != null)
                    client.MessageReceived += Client_MessageReceived;
            }
        }
        public const int LeftMenuWidth = 290;
        public MenuState RightMenuState { get; set; }
        public MenuState LeftMenuState { get; set; }
        public MenuState EditUserMenuState { get; set; }
        public MenuState AddGroupMenuState { get; set; }

        public static DoubleAnimation MakeDoubleAnim(double value, double seconds) => new DoubleAnimation(value, new Duration(TimeSpan.FromSeconds(seconds)));


        public DoubleAnimation MainGridDark = new DoubleAnimation(0.5, new Duration(TimeSpan.FromSeconds(0.2)));
        public DoubleAnimation MainGridDarkReverse = new DoubleAnimation(1, new Duration(TimeSpan.FromSeconds(0.2)));
        public ThicknessAnimation OpenLeftMenuAnim = new ThicknessAnimation();
        public ThicknessAnimation CloseLeftMenuAnim = new ThicknessAnimation();

        private ObservableCollection<MessageItemWrap> messages;
        private TcpClientWrap client;
        private GroupItemWrap curGroup;
        private MessageItemWrap respondingTo;
        private bool editingGroup = false;

        public UserItemWrap MeWrap { get; set; }
        public ObservableCollection<MessageItemWrap> Messages
        {
            get => messages;
            set
            {
                messages = value;
                OnPropertyChanged();
            }
        }

        public List<UserItemWrap> CachedUsers { get; set; } = new List<UserItemWrap>();
        public MainWindow() :
            this(
                new PublicUserInfo(999, "existeddim4", "Дмитрий Осипов", "Description"),
                null)
        { }

        TcpFileClientWrap FileClient { get; set; }

        public UICommand MsgFileClick { get; set; }

        public MainWindow(PublicUserInfo me, TcpClientWrap client)
        {
            Client = client;
            Client.Disconnected += Client_Disconnected;
            CacheManager.Instance.CachePath = "Cache\\";
            MsgFileClick = new UICommand((o) => true, (obj) =>
            {
                FileMetadata file = (FileMetadata)obj;
                Client.SendAsync(new DataRequestMessage(file.Id, DataRequestType.FileData));
            });
            MessageDoubleClick = new UICommand((o) => true, (obj) =>
              {
                  Dispatcher.Invoke(() =>
                  {
                      MessageItemWrap wrap = (MessageItemWrap)obj;
                      RespondingTo = wrap;
                  });
              });
            LoadCache();

            OpenLeftMenuAnim.EasingFunction = new CubicEase();
            CloseLeftMenuAnim.EasingFunction = new CubicEase();

            DataContext = this;
            MeWrap = CachedUsers.FirstOrDefault(wrap => wrap.User.Id == me.Id);
            if (MeWrap == null)
            {
                MeWrap = new UserItemWrap(me);
                CachedUsers.Add(MeWrap);
            }

            InitializeComponent();
            HideRightMenu();

            RightMenuState = MenuState.Hidden;
            LeftMenuState = MenuState.Hidden;
            AddGroupMenuState = MenuState.Hidden;
            EditUserMenuState = MenuState.Hidden;

            OpenLeftMenuAnim.From = new Thickness(-LeftMenuWidth, 0, 0, 0);
            OpenLeftMenuAnim.To = new Thickness(0, 0, 0, 0);
            OpenLeftMenuAnim.Duration = TimeSpan.FromMilliseconds(300);

            CloseLeftMenuAnim.From = LeftMenu.BorderThickness;
            CloseLeftMenuAnim.To = new Thickness(-LeftMenuWidth, 0, 0, 0);
            CloseLeftMenuAnim.Duration = TimeSpan.FromMilliseconds(300);



            Messages = new ObservableCollection<MessageItemWrap>();
            //FileClient = new TcpFileClientWrap(IPAddress.Parse("26.87.230.148"), 5001, MeWrap.User.Id, App.MyGuid);
            //FileClient.Connected += FileClient_Connected;
            //FileClient.FileChunkReceived += FileClient_FileChunkReceived; ;
            //FileClient.ImageChunkReceived += FileClient_ImageChunkReceived; ;
            //FileClient.Disconnected += FileClient_Disconnected;
            //FileClient.ConnectAsync();


            Closing += OnClosed;
            //CachedImages.Add(-1, @"G:\VS Repo\Project2\Project2\Textures\rat.png");
            //Groups.Add(new GroupItemWrap(new PublicGroupInfo()
            //{
            //    Id=-1,
            //    Name = "TESTGROUP",
            //    Messages = new List<ChatMessage>()
            //    {
            //       new ChatMessage("hello")
            //       {
            //           Id=-1,
            //           ImagesId = new List<int>()
            //           {
            //               -1
            //           }
            //       }.SetFrom(me).SetGroupId(-1)
            //    }
            //}));

            Client.SendAsync(new SystemMessage(SystemMessageType.GetOfflineMessages));
            var userStatusQuery = from g in CachedGroups select g.GroupChat.MembersId;
            var userStatusQueryArray = userStatusQuery.Any() ? userStatusQuery.Aggregate((a, b) =>
            {
                a.AddRange(b);
                return a;
            }) : null;
            if (userStatusQueryArray != null)
                Client.SendAsync(new DataRequestMessage(userStatusQueryArray, DataRequestType.UsersOnlineStatus));
            SaveCache();
        }

        private void Client_Disconnected(TcpClientWrap client)
        {
            MessageBox.Show("Подключение к серверу потеряно. Попробуйте перезапустить приложение", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            Dispatcher.Invoke(Close);
        }

        private void FileClient_Disconnected(TcpFileClientWrap client)
        {
            Console.WriteLine("DISCONNECT");
        }

        private void FileClient_Connected(TcpFileClientWrap client)
        {
            Console.WriteLine("FileClient Connected");
            client.ReceiveAsync();
        }

        private List<GroupItemWrap> CachedGroups { get; set; } = new List<GroupItemWrap>();
        private void OnClosed(object sender, EventArgs e)
        {
            Client.Disconnected -= Client_Disconnected;
            Client?.Disconnect();
            SaveCache();
        }

        public Dictionary<int, GroupItemWrap> TemporaryUserGroups { get; set; } = new Dictionary<int, GroupItemWrap>();
        public Dictionary<int, ChatMessage> PendingMessages { get; set; } = new Dictionary<int, ChatMessage>();

        public void Client_MessageReceived(TcpClientWrap client, Message msg)
        {
            Dispatcher.Invoke(() =>
            {
                if (msg is CreateGroupResultMessage)
                {
                    var result = msg as CreateGroupResultMessage;
                    if (result.Result == AuthResult.Success)
                    {
                        MessageBox.Show("Создана группа с ID " + result.GroupId.ToString());
                        if (!Buffers.NewGroupSettings.ContainsKey(result.GroupLocalId))
                            return;
                        var pair = Buffers.NewGroupSettings[result.GroupLocalId];
                        var info = new PublicGroupInfo(pair.Key, pair.Value, result.GroupId)
                        {
                            GroupType = GroupType.Public,
                            MembersId = new List<int>() { MeWrap.User.Id },
                            AdministratorsId = new List<int> { }
                        };
                        var newGroup = new GroupItemWrap(info)
                        {
                            Members = new ObservableCollection<UserItemWrap> { CachedUsers.First(u => u.User.Id == MeWrap.User.Id) }
                        };

                        Groups.Add(newGroup);
                        CachedGroups.Add(newGroup);
                        CacheManager.Instance.SaveGroup(newGroup);
                    }
                }
                else if (msg is ArrayMessage<BaseMessage>)
                {
                    var arrMsg = msg as ArrayMessage<BaseMessage>;
                    foreach (var obj in arrMsg.Array)
                        Client_MessageReceived(client, obj);
                }
                else if (msg is DataRequestResultMessage<KeyValuePair<int, bool>>)
                {
                    var map = msg as DataRequestResultMessage<KeyValuePair<int, bool>>;
                    foreach (var pair in map.Result)
                    {
                        var user = CachedUsers.FirstOrDefault(g => g.User.Id == pair.Key);
                        if (user != null)
                        {
                            user.IsUserOnline = pair.Value;
                            var userGroup = CachedGroups.FirstOrDefault(g => g.AssociatedUserId == pair.Key);
                            if (userGroup != null)
                                userGroup.IsUserOnline = pair.Value;
                        }
                    }
                }
                else if (msg is DataRequestResultMessage<UserContainer>)
                {
                    var drr = msg as DataRequestResultMessage<UserContainer>;
                    Client.SendAsync(new DataRequestMessage(drr.Result.Select(u => u.User.Id), DataRequestType.UsersOnlineStatus));
                    foreach (var container in drr.Result)
                    {
                        UserItemWrap user = new UserItemWrap(container.User);
                        //TO DO: caching
                        //if (container.Images != null)
                        //    user.Images = new ObservableCollection<ImageContainer>(container.Images);
                        CachedUsers.Add(user);
                        CacheManager.Instance.SaveUser(user);
                        foreach (var group in CachedGroups)
                        {
                            if (group.GroupChat.MembersId.Contains(user.User.Id))
                            {
                                group.Members.Add(user);
                                if (group.AssociatedUserId == user.User.Id)
                                {
                                    group.GroupChat.Name = user.User.Name;
                                    group.GroupChat.Description = user.User.Description;
                                    group.OnPropertyChanged("Name");
                                    group.OnPropertyChanged("Description");
                                    //group.Images = user.Images;
                                }
                            }

                            if (group.GroupChat.AdministratorsId.Contains(user.User.Id))
                                group.Admins.Add(user);
                        }
                        foreach (var pair in TemporaryUserGroups)
                        {
                            var group = pair.Value;
                            if (group.AssociatedUserId == user.User.Id)
                            {
                                group.Members.Add(user);
                                group.GroupChat.Name = user.User.Name;
                                group.GroupChat.Description = user.User.Description;
                                group.OnPropertyChanged("Name");
                                group.OnPropertyChanged("Description");
                                //group.Images = user.Images;
                                if (LB_FoundGroups.Visibility == Visibility.Hidden)
                                    Groups.Add(group);
                                else
                                    FoundGroups.Add(group);
                            }
                        }
                    }
                }
                //else if (msg is DataRequestResultMessage<FileMetadata>)
                //{
                //    var result = msg as DataRequestResultMessage<FileMetadata>;
                //    foreach (var md in result.Result)
                //    {
                //        AddMetadataToMessages(md);
                //        CachedFilesMetadata.Add(md);
                //    }
                //}
                //else if (msg is DataRequestResultMessage<ImageMetadata>)
                //{
                //    var result = msg as DataRequestResultMessage<ImageMetadata>;
                //    foreach (var md in result.Result)
                //    {
                //        CachedImagesMetadata.Add(md);
                //        Client.SendAsync(new DataRequestMessage(md.Id, DataRequestType.ImageData));
                //    }
                //}
                else if (msg is UserUpdateMessage)
                {
                    var upd = msg as UserUpdateMessage;
                    var user = CachedUsers.FirstOrDefault(u => u.User.Id == upd.UserId);
                    if (user != null)
                    {
                        user.IsUserOnline = upd.OnlineStatus ?? user.IsUserOnline;
                        user.User.Name = upd.NewName ?? user.User.Name;
                        user.User.Login = upd.NewLogin ?? user.User.Login;
                        user.User.Description = upd.NewDescription ?? user.User.Description;
                        user.OnPropertyChanged("User");

                        var userGroup = CachedGroups.FirstOrDefault(g => g.AssociatedUserId == user.User.Id);
                        if (userGroup != null)
                        {
                            userGroup.IsUserOnline = upd.OnlineStatus ?? userGroup.IsUserOnline;
                            userGroup.GroupChat.Name = user.User.Name;
                            userGroup.GroupChat.Description = user.User.Description;
                            userGroup.OnPropertyChanged("Name");
                            userGroup.OnPropertyChanged("Description");
                        }
                        CacheManager.Instance.SaveUser(user);
                    }
                }
                else if (msg is UserUpdateResultMessage)
                {
                    var result = msg as UserUpdateResultMessage;
                    if (result.Result == AuthResult.Success)
                    {
                        MeWrap.User.Login = result.NewLogin ?? MeWrap.User.Login;
                        MeWrap.User.Description = result.NewDescription ?? MeWrap.User.Description;
                        MeWrap.User.Name = result.NewName ?? MeWrap.User.Name;
                        OnPropertyChanged("MeWrap.User");
                        MeWrap.OnPropertyChanged("User");
                        CacheManager.Instance.SaveUser(MeWrap);
                    }

                }
                else if (msg is ChatLookupResultMessage)
                {
                    var result = msg as ChatLookupResultMessage;
                    var groups = result.Groups;
                    var users = result.UsersId;
                    FoundGroups.Clear();

                    foreach (var userId in users)
                    {
                        UserItemWrap user = CachedUsers.FirstOrDefault(u => u.User.Id == userId);
                        if (user != null)
                        {
                            GroupItemWrap tempGroup = new GroupItemWrap(
                                    new PublicGroupInfo(user.User.Name, user.User.Description, -1)
                                    {
                                        GroupType = GroupType.Personal,
                                        MembersId = new List<int> { MeWrap.User.Id, user.User.Id }
                                    }
                                    )
                            {
                                AssociatedUserId = user.User.Id,
                                Joined = false
                            };
                            Client.SendAsync(new DataRequestMessage(user.User.Id, DataRequestType.UsersOnlineStatus));
                            FoundGroups.Add(tempGroup);
                            TemporaryUserGroups.Add(App.UserGroupLocalIdCounter++, tempGroup);
                        }
                        else
                        {
                            TemporaryUserGroups.Add(App.UserGroupLocalIdCounter++, new GroupItemWrap(new PublicGroupInfo()
                            {
                                Id = -1,
                                MembersId = new List<int> { MeWrap.User.Id, userId }
                            })
                            { 
                                AssociatedUserId = userId,
                                Joined = false
                            });
                            Client.SendAsync(new DataRequestMessage(userId, DataRequestType.User));
                        }
                    }

                    if (groups.Count != 0)
                    {
                        List<int> requestedId = new List<int>();
                        foreach (var group in groups)
                        {
                            var cachedGroup = CachedGroups.FirstOrDefault(g => g.GroupChat.Id == group.Id);
                            if (cachedGroup != null)
                            {
                                FoundGroups.Add(cachedGroup);
                                continue;
                            }
                            GroupItemWrap item = new GroupItemWrap(group);
                            CachedGroups.Add(item);
                            CacheManager.Instance.SaveGroup(item);
                            foreach (var uId in group.MembersId.Where(
                                id => !requestedId.Contains(id)
                                ))
                            {
                                var user = CachedUsers.FirstOrDefault(u => u.User.Id == uId);
                                if (user == null)
                                    Client.SendAsync(new DataRequestMessage(uId, DataRequestType.User));
                                else
                                {
                                    if (item.GroupChat.AdministratorsId.Contains(user.User.Id))
                                        item.Admins.Add(user);
                                    else
                                        item.Members.Add(user);
                                }
                            }
                            item.Joined = false;
                            FoundGroups.Add(item);
                        }
                    }
                }
                //else if (msg is MetadataSyncMessage)
                //{
                //    var syncMsg = msg as MetadataSyncMessage;
                //    var md = PendingMetadata[syncMsg.LocalReturnId];
                //    PendingMetadata.Remove(syncMsg.LocalReturnId);
                //    if (md.FilesLocalId != null)
                //        for (int i = 0; i < md.FilesLocalId.Count; i++)
                //            FileClient.SendFileAsync(md.FilesName[i], md.FilesLocalId[i]);
                //    if (md.ImagesLocalId != null)
                //        for (int i = 0; i < md.ImagesLocalId.Count; i++)
                //            FileClient.SendFileAsync(md.ImagesName[i], md.FilesLocalId[i], true);
                //}
                else if (msg is GroupJoinResultMessage)
                {
                    var result = msg as GroupJoinResultMessage;
                    if (result.Result == AuthResult.Success)
                    {
                        var group = CachedGroups.Find(g => g.GroupChat.Id == result.GroupId);
                        Groups.Add(group);
                        group.Members.Add(MeWrap);
                        group.Joined = true;
                        CacheManager.Instance.SaveGroup(group);
                    }
                }
                else if (msg is ChatMessage)
                {
                    var chatMsg = msg as ChatMessage;
                    var group = CachedGroups.FirstOrDefault(g => g.GroupChat.Id == chatMsg.GroupId);
                    if (group == null)
                        return;
                    if (group.GroupChat.Messages == null)
                        group.GroupChat.Messages = new List<ChatMessage>();
                    group.GroupChat.Messages.Add(chatMsg);
                    group.OnPropertyChanged("Messages");
                    group.OnPropertyChanged("LastMessage");

                    if (group == CurGroup)
                        AddMessageToUI(chatMsg);
                }
                else if (msg is GroupUpdateMessage)
                {
                    var info = msg as GroupUpdateMessage;
                    var group = CachedGroups.FirstOrDefault(g => g.GroupChat.Id == info.GroupId);
                    if (group == null)
                        return;
                    if (!string.IsNullOrEmpty(info.NewName))
                    {
                        group.GroupChat.Name = info.NewName;
                        group.OnPropertyChanged("Name");
                    }
                    if (!string.IsNullOrEmpty(info.NewDescription))
                    {
                        group.GroupChat.Description = info.NewDescription;
                        group.OnPropertyChanged("Description");
                    }

                    if (info.NewUserId != -1)
                    {
                        group.GroupChat.MembersId.Add(info.NewUserId);
                        var user = CachedUsers.FirstOrDefault(u => u.User.Id == info.NewUserId);
                        if (user == null)
                            Client.SendAsync(new DataRequestMessage(info.NewUserId, DataRequestType.User));
                        else
                            group.Members.Add(user);
                    }
                    if (info.RemovedUserId != -1)
                    {
                        var user = CachedUsers.FirstOrDefault(u => u.User.Id == info.RemovedUserId);
                        if (user != null)
                        {
                            group.Members.Remove(user);
                            if (user.User.Id == MeWrap.User.Id)
                            {
                                group.Joined = false;
                                Groups.Remove(group);
                                FoundGroups.Remove(group);
                            }
                            CurGroup = FoundGroups.Count != 0 ? FoundGroups.LastOrDefault() : Groups.LastOrDefault();
                        }
                    }
                    CacheManager.Instance.SaveGroup(group);
                }
                else if (msg is GroupUpdateResultMessage)
                {
                    var result = msg as GroupUpdateResultMessage;
                    var group = CachedGroups.FirstOrDefault(g => g.GroupChat.Id == result.GroupId);
                    if (group != null)
                    {
                        if (result.Result == AuthResult.Success)
                        {
                            group.GroupChat.Name = Buffers.EditGroupSettings[result.GroupId].Key ?? group.GroupChat.Name;
                            group.GroupChat.Description = Buffers.EditGroupSettings[result.GroupId].Value ?? group.GroupChat.Description;
                        }
                        group.OnPropertyChanged("Name");
                        group.OnPropertyChanged("Description");
                    }

                    Buffers.EditGroupSettings.Remove(result.GroupId);
                    group.IsEditable = true;
                    CacheManager.Instance.SaveGroup(group);
                }
                else if (msg is FirstPersonalResultMessage)
                {
                    var result = msg as FirstPersonalResultMessage;
                    var group = TemporaryUserGroups[result.LocalId];
                    group.GroupChat.Id = result.GroupId;
                    group.Members.Add(MeWrap);
                    group.AssociatedUserId = group.GroupChat.MembersId.First(id => id != MeWrap.User.Id);
                    TemporaryUserGroups.Remove(result.LocalId);
                    CachedGroups.Add(group);
                    Groups.Add(group);
                    CacheManager.Instance.SaveGroup(group);
                    ShowGroupMessages(CurGroup);
                }
                else if (msg is PersonalChatCreatedMessage)
                {
                    var personalChatCreated = msg as PersonalChatCreatedMessage;
                    GroupItemWrap newGroup = new GroupItemWrap(personalChatCreated.Group);
                    newGroup.AssociatedUserId = personalChatCreated.Group.MembersId.First(id => id != MeWrap.User.Id);
                    foreach (var userId in personalChatCreated.Group.MembersId)
                    {
                        UserItemWrap user = CachedUsers.FirstOrDefault(u => u.User.Id == userId);

                        if (user == null)
                            client.SendAsync(new DataRequestMessage(userId, DataRequestType.User));
                        else
                            newGroup.Members.Add(user);
                    }
                    CachedGroups.Add(newGroup);
                    Groups.Add(newGroup);
                    CacheManager.Instance.SaveGroup(newGroup);
                }
                else if (msg is ChatMessageSendResult)
                {
                    var result = msg as ChatMessageSendResult;
                    PendingMessages[result.LocalId].Id = result.MessageId;
                    PendingMessages.Remove(result.LocalId);
                }
                else if (msg is DeleteChatMessageResultMessage)
                {
                    var result = msg as DeleteChatMessageResultMessage;
                    if (result.Result == AuthResult.Success)
                    {
                        var group = Groups.First(g => g.GroupChat.Id == result.GroupId);
                        group.Messages.RemoveAll(m => m.Id == result.DeletedMessageId);
                        RemoveMessageFromUI(result.DeletedMessageId);
                        group.OnPropertyChanged("LastMessage");
                        CacheManager.Instance.SaveGroup(group);
                    }
                }
                else if (msg is ChatMessageDeleteMessage)
                {
                    var msgDel = msg as ChatMessageDeleteMessage;
                    var group = Groups.First(g => g.GroupChat.Id == msgDel.GroupId);
                    if (group != null)
                    {
                        group.Messages.RemoveAll(m => m.Id == msgDel.DeletedMessageId);
                        RemoveMessageFromUI(msgDel.DeletedMessageId);
                        CacheManager.Instance.SaveGroup(group);
                    }

                }
                //else if (msg is MetadataResultMessage)
                //{
                //    var result = msg as MetadataResultMessage;
                //    foreach (var pair in result.Files)
                //    {
                //        var file = PendingFiles[pair.Key];
                //        PendingFiles.Remove(pair.Key);
                //        file.Id = pair.Value;
                //        CachedFilesMetadata.Add(file);
                //    }
                //    foreach (var pair in result.Images)
                //    {
                //        var img = PendingImages[pair.Key];
                //        PendingImages.Remove(pair.Key);
                //        CachedImages[pair.Value] = img;
                //    }
                //    MessageToGroupMessage msgToGroup = PendingMsgWithAttachments[result.LocalMessageId];
                //    msgToGroup.FilesId = result.Files.Select(p => p.Value).ToList();
                //    msgToGroup.ImagesId = result.Images.Select(p => p.Value).ToList();
                //    PendingMsgWithAttachments.Remove(result.LocalMessageId);


                //    Client.SendAsync(msgToGroup);

                //    var group = Groups.FirstOrDefault(g => g.GroupChat.Id == msgToGroup.Message.GroupId);
                //    if (group != null)
                //    {
                //        group.GroupChat.Messages.Add(msgToGroup.Message);
                //        group.OnPropertyChanged("Messages");
                //        group.OnPropertyChanged("LastMessage");

                //        if (CurGroup == group)
                //            AddMessageToUI(msgToGroup.Message);
                //    }
                //}
                else if (msg is UserActionResultMessage)
                {
                    var result = msg as UserActionResultMessage;
                    if (result.Result != AuthResult.Denied)
                    {
                        switch (result.Type)
                        {
                            case UserActionType.Kick:
                                {
                                    var group = CachedGroups.FirstOrDefault(g => g.GroupChat.Id == result.GroupId);
                                    if (group != null)
                                    {
                                        var user = group.Members.FirstOrDefault(m => m.User.Id == result.UserId);
                                        if (user != null)
                                            group.Members.Remove(user);
                                        CacheManager.Instance.SaveGroup(group);
                                    }
                                }
                                break;
                        }
                    }
                }
            });
        }

        private MessageItemWrap MakeMsgWrap(ChatMessage msg)
        {
            MessageItemWrap item = new MessageItemWrap(msg);
            item.FromUser = CachedUsers.First(u => u.User.Id == msg.FromUserId);

            if (msg.RespondingTo != null)
            {
                item.RespondingTo = new MessageItemWrap(msg.RespondingTo);
                item.RespondingTo.FromUser = CachedUsers.Find(u => u.User.Id == msg.RespondingTo.FromUserId);
            }
            if (msg.RepostUserId != -1)
                item.RepostUser = CachedUsers.FirstOrDefault(u => u.User.Id == msg.RepostUserId);

            if (Messages.Count != 0 && Messages.Last().Message.FromUserId == msg.FromUserId)
                Messages.Last().ShowAvatar = false;
            else
                item.ShowUsername = true;

            if (item.Message.FromUserId == MeWrap.User.Id)
                item.ShowUsername = false;
            item.ShowAvatar = true;
            {
                bool pending = false;
                foreach (var id in msg.FilesId)
                {
                    var metadata = CachedFilesMetadata.FirstOrDefault(md => md.Id == id);
                    if (metadata == null)
                    {
                        if (!pending)
                        {
                            pending = true;
                            PendingFileMetadataMsg.Add(item);
                        }
                        Client.SendAsync(new DataRequestMessage(id, DataRequestType.FileMetadata));
                    }
                    else
                        item.FilesMetadata.Add(new FileMetadataViewModel(metadata));
                }
                pending = false;
                foreach (var id in msg.ImagesId)
                {
                    string path = null;
                    CachedImages.TryGetValue(id, out path);
                    if (path == null)
                    {
                        if (!pending)
                        {
                            pending = true;
                            PendingImageMsg.Add(item);
                        }
                        Client.SendAsync(new DataRequestMessage(id, DataRequestType.ImageMetaData));
                    }
                    else
                        item.Images.Add(new StringViewModel(path));
                }
            }

            //foreach(var fileId in msg.FilesId)
            //{
            //    var file = CachedFiles.FirstOrDefault(f => f.Id == fileId);
            //    if (file != null)
            //        item.FilesMetadata.Add(file.Metadata);
            //    else
            //        Client.SendAsync(new DataRequestMessage(fileId, DataRequestType.FileData));
            //}
            //foreach(var imgId in msg.ImagesId)
            //{
            //    var file = cache.FirstOrDefault(f => f.Id == fileId);
            //    if (file != null)
            //        item.FilesMetadata.Add(file.Metadata);
            //    else
            //        Client.SendAsync(new DataRequestMessage(fileId, DataRequestType.FileData));

            //}

            return item;
        }
        private void RemoveMessageFromUI(int msgId)
        {
            var toDel = Messages.FirstOrDefault(m => m.Message.Id == msgId);
            if (toDel != null)
            {
                if (Messages.Count > 1)
                {
                    int index = Messages.IndexOf(toDel);
                    // Следующее сообщение должно показывать имя пользователя(если оно им отправлено)
                    // Если удалено своё сообщение, то имя не будет показываться
                    if (index != Messages.Count - 1 && Messages[index + 1].FromUser.User.Id == toDel.FromUser.User.Id)
                        if (Messages[index + 1].FromUser.User.Id != MeWrap.User.Id)
                            Messages[index + 1].ShowUsername = true;
                    if (index != 0 && Messages[index - 1].FromUser.User.Id == toDel.FromUser.User.Id)
                        Messages[index - 1].ShowAvatar = true;

                }
                Messages.Remove(toDel);
            }
        }
        private void AddMessageToUI(ChatMessage msg)
        {
            var item = MakeMsgWrap(msg);

            Messages.Add(item);
            LB_Messages.SelectedIndex = LB_Messages.Items.Count - 1;
            LB_Messages.ScrollIntoView(LB_Messages.SelectedItem);
        }
        private void ShowGroupMessages(GroupItemWrap group)
        {
            Action action = () =>
            {
                Messages.Clear();
                RespondingTo = null;
                if (CurGroup.GroupChat.Messages != null)
                    foreach (var msg in group.Messages)
                        AddMessageToUI(msg);
            };
            Dispatcher.Invoke(action);


        }
        private void BTNFullScreen_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ?
                WindowState.Normal :
                WindowState.Maximized;

        }



        private void ShowOrOpenRightMenu_Click(object sender, RoutedEventArgs e)
        {
            if (RightMenuState == MenuState.Open)
                HideRightMenu();
            else
                OpenRightMenu();
        }

        public void HideRightMenu()
        {
            this.Width -= ColumnRightMenu.ActualWidth;

            //Spliter.Visibility = Visibility.Collapsed;
            RightMenu.Visibility = Visibility.Collapsed;


            RightMenuState = MenuState.Hidden;
        }

        public void OpenRightMenu()
        {
            this.Width += 280;
            //Spliter.Visibility = Visibility.Visible;
            RightMenu.Visibility = Visibility.Visible;
            RightMenuState = MenuState.Open;
        }

        private void TopPanel_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;

            this.DragMove();
        }

        private void BTNClose_Click(object sender, RoutedEventArgs e)
        {
            Client.Disconnected -= Client_Disconnected;
            Close();
        }


        private void BTNHiDEWindow_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MainContent_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (MainContent.MinWidth == e.NewSize.Width)
                if (RightMenuState != MenuState.Hidden)
                    HideRightMenu();
        }

        private void BTNOpenLeftMenu_Click(object sender, RoutedEventArgs e)
        {
            LeftMenu.BeginAnimation(Border.MarginProperty, OpenLeftMenuAnim);
            MainGrid.BeginAnimation(Border.OpacityProperty, MainGridDark);
            LeftMenuState = MenuState.Open;
        }

        private void ConsequentFadeAway(UIElement elem, UIElement bg)
        {
            var fadeAway = MakeDoubleAnim(0, 0.1);
            fadeAway.Completed += (v1, v2) =>
            {
                Dispatcher.Invoke(() =>
                {
                    bg.BeginAnimation(OpacityProperty, MainGridDarkReverse);
                    elem.Visibility = Visibility.Hidden;
                });
            };
            elem.BeginAnimation(OpacityProperty, fadeAway);
        }

        private void HideMenus(object sender, MouseButtonEventArgs e)
        {
            if (LeftMenuState == MenuState.Open)
            {
                LeftMenu.BeginAnimation(MarginProperty, CloseLeftMenuAnim);
                MainGrid.BeginAnimation(OpacityProperty, MainGridDarkReverse);
                LeftMenuState = MenuState.Hidden;
            }
            if (EditUserMenuState == MenuState.Open)
            {
                EditUserMenuState = MenuState.Hidden;
                ConsequentFadeAway(EditUserMenu, MainGrid);
            }
            if (AddGroupMenuState == MenuState.Open)
            {
                AddGroupMenuState = MenuState.Hidden;
                ConsequentFadeAway(AddGroupMenu, MainGrid);
            }
        }

        private void Responce_OnClick(object sender, MouseButtonEventArgs e)
        {
        }

        private void TB_GroupLookUp_OnEnter(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Text.Length != 0)
                if (e.Key == Key.Enter)
                    Client.SendAsync(new ChatLookupMessage(textBox.Text));
        }

        private void B_AddGroupMenu_OnClick(object sender, RoutedEventArgs e)
        {
            var fadeIn = MakeDoubleAnim(1, 0.3);

            HideMenus(null, null);
            AddGroupMenuState = MenuState.Open;
            AddGroupMenu.Visibility = Visibility.Visible;
            AddGroupMenu.Opacity = 0;
            AddGroupMenu.BeginAnimation(OpacityProperty, fadeIn);

            MainGrid.BeginAnimation(OpacityProperty, MainGridDark);
        }

        private void B_AddGroup_OnClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                AddGroupButton.IsEnabled = false;
                HideMenus(null, null);
                Buffers.NewGroupSettings.Add(
                    App.GroupLocalIdCounter,
                    new KeyValuePair<string, string>(TB_NewGroupName.Text, string.IsNullOrEmpty(TB_NewGroupDesc.Text) ? null : TB_NewGroupDesc.Text)
                    );
                var msg = new CreateGroupMessage(App.GroupLocalIdCounter++, TB_NewGroupName.Text, string.IsNullOrEmpty(TB_NewGroupDesc.Text) ? null : TB_NewGroupDesc.Text, MeWrap.User.Id);
                TB_NewGroupName.Text = "";
                Client.SendAsync(msg);
            });
        }


        private void TB_NewGroupName_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            AddGroupButton.IsEnabled = !string.IsNullOrEmpty(textBox.Text);
        }

        private void GroupSelected(object sender, RoutedEventArgs e)
        {
            ListBox lb = sender as ListBox;
            if (lb.SelectedIndex == -1)
                return;

            CurGroup = lb.SelectedItem as GroupItemWrap;
            MsgFilesUI.Clear();
            MsgImagesUI.Clear();
            if (MsgFiles.ContainsKey(CurGroup))
            {
                var fileList = MsgFiles[CurGroup];
                foreach (var md in fileList)
                    MsgFilesUI.Add(new FileMetadata(md.Name, md.Size));
            }
            if (MsgImages.ContainsKey(CurGroup))
            {
                var imgList = MsgImages[CurGroup];
                foreach (var md in imgList)
                    MsgImagesUI.Add(new ImageMetadata(md.Name, md.Size));
            }

            ShowGroupMessages(CurGroup);
        }

        private void FoundGroupSelected(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = sender as ListBox;
            if (lb.SelectedIndex == -1)
                return;
            CurGroup = lb.SelectedItem as GroupItemWrap;
            ShowGroupMessages(CurGroup);
        }

        private void B_CloseFoundLB_Click(object sender, RoutedEventArgs e)
        {
            FoundGroups.Clear();
            TemporaryUserGroups.Clear();
            if (LB_Groups.SelectedItem != null)
            {
                CurGroup = LB_Groups.SelectedItem as GroupItemWrap;
                ShowGroupMessages(CurGroup);
            }
            else if (Groups.Contains(CurGroup))
            {
                LB_Groups.SelectedItem = CurGroup;
                ShowGroupMessages(CurGroup);
            }

        }

        private void B_JoinGroup_Click(object sender, RoutedEventArgs e)
        {
            if (LB_FoundGroups.SelectedIndex == -1)
                return;
            var group = LB_FoundGroups.SelectedItem as GroupItemWrap;
            Client.SendAsync(new GroupJoinMessage(group.GroupChat.Id));
        }

        public GroupItemWrap CurGroup
        {

            get => curGroup;
            set
            {
                curGroup = value;
                EditingGroup = false;
                OnPropertyChanged();
            }
        }

        private void TB_SendMsg_OnEnter(object sender, KeyEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                var textBox = sender as TextBox;
                if (CurGroup != null && e.Key == Key.Enter && !String.IsNullOrEmpty(textBox.Text))
                {
                    ChatMessage msg = new ChatMessage(textBox.Text)
                    .SetFrom(MeWrap.User)
                    .SetGroupId(CurGroup.GroupChat.Id);

                    if (RespondingTo != null)
                        msg.SetRespondingTo(RespondingTo.Message);
                    if (msg.FromUserId == MeWrap.User.Id)
                        if (!PendingMessages.ContainsKey(App.MessageLocalIdCounter))
                            PendingMessages.Add(App.MessageLocalIdCounter, msg);

                    if (CurGroup.GroupChat.Id == -1 && CurGroup.GroupChat.GroupType == GroupType.Personal)
                    {
                        FirstPersonalMessage fpMsg
                        = new FirstPersonalMessage(msg,
                        CurGroup.GroupChat.MembersId.First(m => m != MeWrap.User.Id),
                        TemporaryUserGroups.First(p => p.Value == CurGroup).Key);
                        Client.SendAsync(fpMsg);
                    }
                    else
                    {
                        MessageToGroupMessage msgToGroup = new MessageToGroupMessage(msg, App.MessageLocalIdCounter++);
                        // отправляем файлы
                        //if (
                        //    (MsgFiles.ContainsKey(CurGroup) &&
                        //    MsgFiles[CurGroup].Count != 0)
                        //    ||
                        //    (MsgImages.ContainsKey(CurGroup) &&
                        //    MsgImages[CurGroup].Count != 0))
                        //{
                        //    List<FileMetadata> fileMdList = null;
                        //    MsgFiles.TryGetValue(CurGroup, out fileMdList);
                        //    if (fileMdList == null)
                        //        fileMdList = new List<FileMetadata>();

                        //    List<ImageMetadata> imgMdList = null;
                        //    MsgImages.TryGetValue(CurGroup, out imgMdList);
                        //    if (imgMdList == null)
                        //        imgMdList = new List<ImageMetadata>();

                        //    PendingMsgWithAttachments.Add(msgToGroup.LocalMessageId, msgToGroup);
                        //    List<int> filesLocalId = new List<int>();
                        //    List<int> imagesLocalId = new List<int>();
                        //    var mdMsg = new MetadataMessage(
                        //            msgToGroup.LocalMessageId,
                        //            App.MessageLocalIdCounter++,
                        //            files:
                        //            fileMdList?.Select(
                        //                file =>
                        //                {
                        //                    PendingFiles.Add(App.MetadataLocalIdCounter, file);
                        //                    filesLocalId.Add(App.MetadataLocalIdCounter);
                        //                    return new KeyValuePair<int, FileMetadata>(App.MetadataLocalIdCounter++, file);
                        //                }
                        //                ),
                        //            images:
                        //            imgMdList?.Select(
                        //                img =>
                        //                {
                        //                    PendingImages.Add(App.MetadataLocalIdCounter, img.Name);
                        //                    imagesLocalId.Add(App.MetadataLocalIdCounter);
                        //                    return new KeyValuePair<int, ImageMetadata>(App.MetadataLocalIdCounter++, img);
                        //                }
                        //                )
                        //            );

                        //    var state = new MetadataState();
                        //    if (imgMdList != null)
                        //    {
                        //        state.ImagesName = new List<string>(imgMdList.Select(img => img.Name));
                        //        state.ImagesLocalId = imagesLocalId;
                        //    }
                        //    if (fileMdList != null)
                        //    {
                        //        state.FilesName = new List<string>(fileMdList.Select(file =>
                        //        {
                        //            string result = file.Name;
                        //            file.Name = Path.GetFileName(file.Name);
                        //            return result;
                        //        }));
                        //        state.FilesLocalId = filesLocalId;
                        //    }
                        //    PendingMetadata[mdMsg.LocalReturnId] = state;

                        //    Client.SendAsync(mdMsg);
                        //    Dispatcher.Invoke(ResetMessageForm);
                        //    return;
                        //}
                        //else
                        Client.SendAsync(msgToGroup);
                    }

                    CurGroup.GroupChat.Messages.Add(msg);
                    CurGroup.OnPropertyChanged("Messages");
                    CurGroup.OnPropertyChanged("LastMessage");

                    AddMessageToUI(msg);
                    Dispatcher.Invoke(ResetMessageForm);
                }
            });
        }

        private Dictionary<int, MetadataState> PendingMetadata { get; set; } = new Dictionary<int, MetadataState>();

        private void ResetMessageForm()
        {
            TB_Message.Text = "";
            RespondingTo = null;
            MsgFilesUI.Clear();
            MsgImagesUI.Clear();
            MsgImages.Clear();
            MsgFiles.Clear();
        }
        private void CloseRespondingToPanel(object sender, RoutedEventArgs e)
        {
            RespondingTo = null;
        }

        private void CtxMenu_Message_Respond_OnClick(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var msg = (MessageItemWrap)menuItem.DataContext;
            RespondingTo = msg;
        }

        private void CtxMenu_Message_Delete_OnClick(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var msg = (MessageItemWrap)menuItem.DataContext;
            Client.SendAsync(new ChatMessageDeleteMessage(msg.Message.Id, CurGroup.GroupChat.Id, MeWrap.User.Id));
        }


        public Dictionary<GroupItemWrap, List<FileMetadata>> MsgFiles { get; set; } = new Dictionary<GroupItemWrap, List<FileMetadata>>();
        public Dictionary<GroupItemWrap, List<ImageMetadata>> MsgImages { get; set; } = new Dictionary<GroupItemWrap, List<ImageMetadata>>();
        public ObservableCollection<FileMetadata> MsgFilesUI { get; set; } = new ObservableCollection<FileMetadata>();
        public ObservableCollection<ImageMetadata> MsgImagesUI { get; set; } = new ObservableCollection<ImageMetadata>();
        private void B_AddFilesToMsg_OnClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (var fileName in dialog.FileNames)
                {
                    var info = new FileInfo(fileName);
                    if (ImageMetadata.AllowedExtensions.Contains(Path.GetExtension(fileName)))
                    {
                        if (!MsgImages.ContainsKey(CurGroup))
                            MsgImages.Add(CurGroup, new List<ImageMetadata>());
                        var md = new ImageMetadata(info.FullName, (int)info.Length);
                        MsgImages[CurGroup].Add(md);
                        MsgImagesUI.Add(md);
                    }
                    else
                    {
                        if (!MsgFiles.ContainsKey(CurGroup))
                            MsgFiles.Add(CurGroup, new List<FileMetadata>());
                        var md = new FileMetadata(info.FullName, (int)info.Length);
                        MsgFiles[CurGroup].Add(md);
                        MsgFilesUI.Add(md);
                    }
                }
            }
        }

        private void B_Quit_OnClick(object sender, RoutedEventArgs e)
        {
            Client?.Send(new SystemMessage(SystemMessageType.Logout));
            Close();
        }

        private void B_QuitGroup_OnClick(object sender, RoutedEventArgs e)
        {
            Client?.SendAsync(new GroupLeaveMessage(CurGroup.GroupChat.Id));
            Groups.Remove(CurGroup);
            CurGroup = null;
        }
        private void CtxMenu_User_Kick_OnClick(object sender, RoutedEventArgs e)
        {
            var user = (sender as FrameworkElement).DataContext as UserItemWrap;
            Client?.SendAsync(new KickUserMessage(user.User.Id, CurGroup.GroupChat.Id));
        }

        private void B_EditGroup_OnClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (EditingGroup)
                {
                    if (string.IsNullOrEmpty(TB_CurGroupName.Text))
                    {
                        MessageBox.Show("Группу нельзя переименовать в пустую.");
                        return;
                    }

                    Buffers.EditGroupSettings.Add(
                        CurGroup.GroupChat.Id,
                        new KeyValuePair<string, string>(
                            string.IsNullOrEmpty(TB_CurGroupName.Text) ||
                            TB_CurGroupName.Text == CurGroup.GroupChat.Name ? null : TB_CurGroupName.Text,
                            TB_CurGroupDesc.Text == CurGroup.GroupChat.Description ? null : TB_CurGroupDesc.Text)
                        );
                    CurGroup.IsEditable = false;

                    Client.SendAsync(new GroupUpdateMessage()
                    {
                        NewDescription =
                        TB_CurGroupDesc.Text == CurGroup.GroupChat.Description ? null : TB_CurGroupDesc.Text,
                        NewName =
                         string.IsNullOrEmpty(TB_CurGroupName.Text) ||
                         TB_CurGroupName.Text == CurGroup.GroupChat.Name ? null : TB_CurGroupName.Text,
                        GroupId = CurGroup.GroupChat.Id,
                    });
                }
                EditingGroup = !EditingGroup;
            });
        }

        private void TB_NewUserInfo_TextChanged(object sender, TextChangedEventArgs e)
        {
            B_FinishUserEdit.IsEnabled = !string.IsNullOrEmpty(TB_NewUserLogin.Text) && !string.IsNullOrEmpty(TB_NewUserName.Text);
        }

        private void B_FinishUserEdit_Click(object sender, RoutedEventArgs e)
        {
            var msg = new UserUpdateMessage()
            {
                NewName = TB_NewUserName.Text == MeWrap.User.Name ? null : TB_NewUserName.Text,
                NewLogin = TB_NewUserLogin.Text == MeWrap.User.Login ? null : TB_NewUserLogin.Text,
                NewDescription = string.IsNullOrEmpty(TB_NewUserDesc.Text) || TB_NewUserDesc.Text == MeWrap.User.Description ? null : TB_NewUserDesc.Text
            };
            Client.SendAsync(msg);
            HideMenus(null, null);
        }

        private void B_EditUserInfo_Click(object sender, RoutedEventArgs e)
        {
            HideMenus(null, null);
            EditUserMenuState = MenuState.Open;
            EditUserMenu.Visibility = Visibility.Visible;
            MainGrid.BeginAnimation(OpacityProperty, MainGridDark);
            TB_NewUserLogin.Text = MeWrap.User.Login;
            TB_NewUserName.Text = MeWrap.User.Name;
            TB_NewUserDesc.Text = MeWrap.User.Description;
            EditUserMenu.BeginAnimation(OpacityProperty, MakeDoubleAnim(1, 0.1));
        }
    }


}
