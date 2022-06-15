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
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
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

                client.MessageReceived += Client_MessageReceived;
            }
        }
        public const int LeftMenuWidth = 290;
        public MenuState RighMenuState { get; set; }
        public MenuState LeftMenuState { get; set; }
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

        public PublicUserInfo Me { get; set; }
        public static PublicUserInfo ivan { get; set; } = new PublicUserInfo(-2, "ivandovg", "Ivan Dovgolutsky", "");

        public ObservableCollection<MessageItemWrap> Messages
        {
            get => messages;
            set
            {
                messages = value;
                OnPropertyChanged();
            }
        }

        public List<UserItemWrap> Users { get; set; } = new List<UserItemWrap>();
        public MainWindow() :
            this(
                new PublicUserInfo(999, "existeddim4", "Дмитрий Осипов", "Description"),
                null,
                null)
        { }

        TcpFileClientWrap FileClient { get; set; }

        public MainWindow(PublicUserInfo me, TcpClientWrap client, ArrayMessage<BaseMessage> offlineMessages)
        {
            Client = client;
            CacheManager.Instance.CachePath = "Cache\\";
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
            InitializeComponent();
            HideRightMenu();
            Me = me;
            Users.Add(new UserItemWrap(me));

            RighMenuState = MenuState.Hidden;
            LeftMenuState = MenuState.Hidden;

            OpenLeftMenuAnim.From = new Thickness(-LeftMenuWidth, 0, 0, 0);
            OpenLeftMenuAnim.To = new Thickness(0, 0, 0, 0);
            OpenLeftMenuAnim.Duration = TimeSpan.FromMilliseconds(300);

            CloseLeftMenuAnim.From = LeftMenu.BorderThickness;
            CloseLeftMenuAnim.To = new Thickness(-LeftMenuWidth, 0, 0, 0);
            CloseLeftMenuAnim.Duration = TimeSpan.FromMilliseconds(300);



            Messages = new ObservableCollection<MessageItemWrap>();
            FileClient = new TcpFileClientWrap(IPAddress.Parse("26.87.230.148"), 5001, Me.Id, App.MyGuid);
            FileClient.Connected += FileClient_Connected;
            FileClient.FileChunkReceived += FileClient_FileChunkReceived; ;
            FileClient.ImageChunkReceived += FileClient_ImageChunkReceived; ;
            FileClient.ConnectAsync();

            if (offlineMessages != null)
                Client_MessageReceived(Client, offlineMessages);

            Closing += OnClosed;
        }



        private void FileClient_Connected(TcpFileClientWrap client)
        {
            Console.WriteLine("FileClient Connected");
            client.ReceiveAsync();
        }

        private List<GroupItemWrap> CachedGroups { get; set; } = new List<GroupItemWrap>();
        private void OnClosed(object sender, EventArgs e)
        {
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
                    if (result.Result == AuthenticationResult.Success)
                    {
                        MessageBox.Show("Создана группа с ID " + result.GroupId.ToString());
                        var info = new PublicGroupInfo(Buffers.GroupName, "", result.GroupId)
                        {
                            MembersId = new List<int>() { Me.Id },
                            AdministratorsId = new List<int> { Me.Id }
                        };
                        var newGroup = new GroupItemWrap(info)
                        {
                            Admins = new ObservableCollection<UserItemWrap> { Users.First(u => u.User.Id == Me.Id) }
                        };
                        Groups.Add(newGroup);
                        CachedGroups.Add(newGroup);
                    }
                }
                else if (msg is ArrayMessage<BaseMessage>)
                {
                    var arrMsg = msg as ArrayMessage<BaseMessage>;
                    foreach (var obj in arrMsg.Array)
                        Client_MessageReceived(client, obj);
                }
                else if (msg is DataRequestResultMessage<UserContainer>)
                {
                    var drr = msg as DataRequestResultMessage<UserContainer>;
                    foreach (var container in drr.Result)
                    {
                        UserItemWrap user = new UserItemWrap(container.User);
                        if (container.Images != null)
                            user.Images = new ObservableCollection<ImageContainer>(container.Images);
                        Users.Add(user);
                        foreach (var group in CachedGroups.Where(g => g.GroupChat.MembersId.Contains(container.User.Id)))
                        {
                            group.Members.Add(user);
                            if (group.GroupChat.GroupType == GroupType.Personal)
                            {
                                group.GroupChat.Name = user.User.Name;
                                group.GroupChat.Description = user.User.Description;
                                group.Images = user.Images;
                            }
                        }
                        foreach (var pair in TemporaryUserGroups)
                        {
                            var group = pair.Value;
                            if (group.GroupChat.MembersId.Contains(user.User.Id))
                            {
                                group.Members.Add(user);
                                group.GroupChat.Name = user.User.Name;
                                group.GroupChat.Description = user.User.Description;
                                group.Images = user.Images;

                                Dispatcher.Invoke(() =>
                                FoundGroups.Add(group));
                            }
                        }
                    }
                }
                else if (msg is DataRequestResultMessage<FileMetadata>)
                {
                    var result = msg as DataRequestResultMessage<FileMetadata>;
                    foreach (var md in result.Result)
                    {
                        AddMetadataToMessages(md);
                        CachedFilesMetadata.Add(md);
                    }
                }
                else if (msg is DataRequestResultMessage<ImageMetadata>)
                {
                    var result = msg as DataRequestResultMessage<ImageMetadata>;
                    foreach (var md in result.Result)
                    {
                        AddMetadataToMessages(md);
                        CachedImagesMetadata.Add(md);
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
                        UserItemWrap user = Users.FirstOrDefault(u => u.User.Id == userId);
                        if (user != null)
                        {
                            GroupItemWrap tempGroup = new GroupItemWrap(
                                    new PublicGroupInfo(user.User.Name, user.User.Description, -1)
                                    {
                                        GroupType = GroupType.Personal,
                                        MembersId = new List<int> { Me.Id, user.User.Id }
                                    }
                                    );
                            FoundGroups.Add(tempGroup);
                            TemporaryUserGroups.Add(App.UserGroupLocalIdCounter++, tempGroup);
                        }
                        else
                        {
                            TemporaryUserGroups.Add(App.UserGroupLocalIdCounter++, new GroupItemWrap(new PublicGroupInfo()
                            {
                                Id = -1,
                                MembersId = new List<int> { Me.Id, userId }
                            }));
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
                            foreach (var uId in group.MembersId.Where(
                                id => !requestedId.Contains(id)
                                ))
                            {
                                var user = Users.FirstOrDefault(u => u.User.Id == uId);
                                if (user == null)
                                    Client.SendAsync(new DataRequestMessage(uId, DataRequestType.User));
                                else
                                {
                                    item.Members.Add(user);
                                    if (item.GroupChat.AdministratorsId.Contains(user.User.Id))
                                        item.Admins.Add(user);
                                }
                            }
                            FoundGroups.Add(item);
                        }
                    }
                }
                else if (msg is GroupJoinResultMessage)
                {
                    var result = msg as GroupJoinResultMessage;
                    if (result.Result == AuthenticationResult.Success)
                    {
                        Groups.Add(CachedGroups.Find(g => g.GroupChat.Id == result.GroupId));
                        B_JoinGroup.Visibility = Visibility.Hidden;
                    }
                }
                else if (msg is ChatMessage)
                {
                    var chatMsg = msg as ChatMessage;
                    var group = Groups.First(g => g.GroupChat.Id == chatMsg.GroupId);

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
                    if (info.NewUserId != -1)
                    {
                        var user = Users.FirstOrDefault(u => u.User.Id == info.NewUserId);
                        if (user == null)
                            Client.SendAsync(new DataRequestMessage(info.NewUserId, DataRequestType.User));
                        else
                            Groups.First(g => g.GroupChat.Id == info.GroupId).Members.Add(user);
                    }
                }
                else if (msg is FirstPersonalResultMessage)
                {
                    var result = msg as FirstPersonalResultMessage;
                    var group = TemporaryUserGroups[result.LocalId];
                    group.GroupChat.Id = result.GroupId;
                    TemporaryUserGroups.Remove(result.LocalId);
                    Groups.Add(group);
                    ShowGroupMessages(CurGroup);
                }
                else if (msg is PersonalChatCreatedMessage)
                {
                    var personalChatCreated = msg as PersonalChatCreatedMessage;
                    GroupItemWrap newGroup = new GroupItemWrap(personalChatCreated.Group);

                    CachedGroups.Add(newGroup);

                    foreach (var userId in personalChatCreated.Group.MembersId)
                    {
                        UserItemWrap user = Users.FirstOrDefault(u => u.User.Id == userId);

                        if (user == null)
                            client.SendAsync(new DataRequestMessage(userId, DataRequestType.User));
                        else
                            newGroup.Members.Add(user);
                    }
                    Groups.Add(newGroup);
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
                    if (result.Result == AuthenticationResult.Success)
                    {
                        var group = Groups.First(g => g.GroupChat.Id == result.GroupId);
                        group.Messages.RemoveAll(m => m.Id == result.DeletedMessageId);
                        group.OnPropertyChanged("LastMessage");
                    }
                    Messages.Remove(Messages.First(m => m.Message.Id == result.DeletedMessageId));

                }
                else if (msg is ChatMessageDeleteMessage)
                {
                    var msgDel = msg as ChatMessageDeleteMessage;
                    Groups.First(g => g.GroupChat.Id == msgDel.GroupId).Messages.RemoveAll(m => m.Id == msgDel.DeletedMessageId);
                    Messages.Remove(Messages.First(m => m.Message.Id == msgDel.DeletedMessageId));
                }
                else if (msg is MetadataResultMessage)
                {
                    var result = msg as MetadataResultMessage;
                    foreach (var pair in result.Files)
                    {
                        var file = PendingFiles[pair.Key];
                        PendingFiles.Remove(pair.Key);
                        file.Id = pair.Value;
                        CachedFilesMetadata.Add(file);
                    }
                    var msgToGroup = PendingMsgWithFiles[result.LocalMessageId];
                    msgToGroup.FilesId = result.Files.Select(p=>p.Value).ToList();
                    msgToGroup.ImagesId = result.Images.Select(p => p.Value).ToList();
                    Client.SendAsync(msgToGroup);
                    
                    var group = Groups.FirstOrDefault(g => g.GroupChat.Id == msgToGroup.Message.GroupId);
                    if (group != null)
                    {
                        group.GroupChat.Messages.Add(msgToGroup.Message);
                        group.OnPropertyChanged("Messages");
                        group.OnPropertyChanged("LastMessage");

                        if (CurGroup == group)
                            AddMessageToUI(msgToGroup.Message);
                    }
                }
            });
        }

        private MessageItemWrap MakeMsgWrap(ChatMessage msg)
        {
            MessageItemWrap item = new MessageItemWrap(msg);
            item.FromUser = Users.First(u => u.User.Id == msg.FromUserId);

            if (msg.RespondingTo != null)
            {
                item.RespondingTo = new MessageItemWrap(msg.RespondingTo);
                item.RespondingTo.FromUser = Users.Find(u => u.User.Id == msg.RespondingTo.FromUserId);
            }
            if (msg.RepostUserId != -1)
                item.RepostUser = Users.FirstOrDefault(u => u.User.Id == msg.RepostUserId);

            if (Messages.Count != 0 && Messages.Last().Message.FromUserId == msg.FromUserId)
                Messages.Last().ShowAvatar = false;
            else
                item.ShowUsername = true;

            if (item.Message.FromUserId == Me.Id)
                item.ShowUsername = false;
            item.ShowAvatar = true;

            {
                bool pending = false;
                foreach (var id in msg.FilesId)
                {
                    var metadata = CachedFilesMetadata.FirstOrDefault(md => md.Id == id);
                    if (metadata == null)
                    {
                        Client.SendAsync(new DataRequestMessage(id, DataRequestType.FileMetadata));
                        if (!pending)
                        {
                            pending = true;
                            PendingMetadataMsg.Add(item);
                        }
                    }
                    else
                        item.FilesMetadata.Add(metadata);
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
            if (RighMenuState == MenuState.Open)
                HideRightMenu();
            else
                OpenRightMenu();
        }

        public void HideRightMenu()
        {
            this.Width -= ColumnRightMenu.ActualWidth;

            //Spliter.Visibility = Visibility.Collapsed;
            RightMenu.Visibility = Visibility.Collapsed;


            RighMenuState = MenuState.Hidden;
        }

        public void OpenRightMenu()
        {
            this.Width += 280;
            //Spliter.Visibility = Visibility.Visible;
            RightMenu.Visibility = Visibility.Visible;
            RighMenuState = MenuState.Open;
        }

        private void TopPanel_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;

            this.DragMove();
        }

        private void BTNClose_Click(object sender, RoutedEventArgs e) => Close();


        private void BTNHiDEWindow_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MainContent_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (MainContent.MinWidth == e.NewSize.Width)
                if (RighMenuState != MenuState.Hidden)
                    HideRightMenu();
        }

        private void BTNOpenLeftMenu_Click(object sender, RoutedEventArgs e)
        {
            LeftMenu.BeginAnimation(Border.MarginProperty, OpenLeftMenuAnim);
            MainGrid.BeginAnimation(Border.OpacityProperty, MainGridDark);
            LeftMenuState = MenuState.Open;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LeftMenu.BeginAnimation(Border.MarginProperty, CloseLeftMenuAnim);
            MainGrid.BeginAnimation(Border.OpacityProperty, MainGridDarkReverse);
        }

        private void HideMenus(object sender, MouseButtonEventArgs e)
        {
            if (LeftMenuState == MenuState.Open)
            {
                LeftMenu.BeginAnimation(Border.MarginProperty, CloseLeftMenuAnim);
                MainGrid.BeginAnimation(Border.OpacityProperty, MainGridDarkReverse);
                LeftMenuState = MenuState.Hidden;
            }
            if (AddGroupMenuState == MenuState.Open)
            {

                var fadeAway = MakeDoubleAnim(0, 0.1);
                fadeAway.Completed += (v1, v2) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        MainGrid.BeginAnimation(OpacityProperty, MainGridDarkReverse);
                        AddGroupMenu.Visibility = Visibility.Hidden;
                    });
                };
                AddGroupMenuState = MenuState.Hidden;
                AddGroupMenu.BeginAnimation(OpacityProperty, fadeAway);

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
                Buffers.GroupName = TB_NewGroupName.Text;
                AddGroupButton.IsEnabled = false;
                HideMenus(null, null);
            });
            Client.MessageSent -= Client_MessageSent;
            Client.MessageSent += Client_MessageSent;
            var msg = new CreateGroupMessage(Buffers.GroupName, Me.Id);
            Client.SendAsync(msg);

        }

        private void Client_MessageSent(TcpClientWrap client, Message msg)
        {

        }

        private void TB_GroupName_TextChanged(object sender, TextChangedEventArgs e)
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
            ShowGroupMessages(CurGroup);
        }

        private void FoundGroupSelected(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = sender as ListBox;
            if (lb.SelectedIndex == -1)
                return;
            CurGroup = lb.SelectedItem as GroupItemWrap;
            if (CurGroup.GroupChat.GroupType != GroupType.Personal && !Groups.Contains(CurGroup))
                B_JoinGroup.Visibility = Visibility.Visible;
            else
                B_JoinGroup.Visibility = Visibility.Hidden;
            ShowGroupMessages(CurGroup);
        }

        private void B_CloseFoundLB_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            button.IsEnabled = false;
            FoundGroups.Clear();
            TemporaryUserGroups.Clear();
            B_JoinGroup.Visibility = Visibility.Hidden;
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
                    .SetFrom(Me)
                    .SetGroupId(CurGroup.GroupChat.Id);

                    if (RespondingTo != null)
                        msg.SetRespondingTo(RespondingTo.Message);
                    if (msg.FromUserId == Me.Id)
                        if (!PendingMessages.ContainsKey(App.MessageLocalIdCounter))
                            PendingMessages.Add(App.MessageLocalIdCounter, msg);

                    if (CurGroup.GroupChat.Id == -1 && CurGroup.GroupChat.GroupType == GroupType.Personal)
                    {
                        FirstPersonalMessage fpMsg
                        = new FirstPersonalMessage(msg,
                        CurGroup.GroupChat.MembersId.First(m => m != Me.Id),
                        TemporaryUserGroups.First(p => p.Value == CurGroup).Key);
                        Client.SendAsync(fpMsg);
                    }
                    else
                    {
                        MessageToGroupMessage msgToGroup = new MessageToGroupMessage(msg, App.MessageLocalIdCounter++);
                        // отправляем файлы
                        if (MsgFilesUI.Count != 0 ||
                            MsgImagesUI.Count != 0)
                        {
                            PendingMsgWithFiles.Add(msgToGroup.LocalMessageId, msgToGroup);
                            List<int> filesLocalId = new List<int>();
                            Client.SendAsync(
                                new MetadataMessage(
                                    msgToGroup.LocalMessageId,
                                    //images:
                                    //MsgImages.Select(
                                    //    img => new KeyValuePair<int, ImageMetadata>(App.MetadataLocalIdCounter++, img)
                                    //    ) 
                                    files:
                                    MsgFilesUI.Select(
                                        file =>
                                        {
                                            PendingFiles.Add(App.MetadataLocalIdCounter, file);
                                            filesLocalId.Add(App.MetadataLocalIdCounter);
                                            return new KeyValuePair<int, FileMetadata>(App.MetadataLocalIdCounter++, file);
                                        }
                                        )
                                    )
                                );
                            for (int i = 0; i < filesLocalId.Count; i++)
                                FileClient.SendFileAsync(MsgFiles[i], filesLocalId[i], false);
                            Dispatcher.Invoke(ResetMessageForm);
                            return;
                        }
                        else
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
            Client.SendAsync(new ChatMessageDeleteMessage(msg.Message.Id, CurGroup.GroupChat.Id, Me.Id));
        }


        public List<string> MsgFiles { get; set; } = new List<string>();
        public List<string> MsgImages { get; set; } = new List<string>();
        public ObservableCollection<FileMetadata> MsgFilesUI { get; set; } = new ObservableCollection<FileMetadata>();
        public ObservableCollection<ImageMetadata> MsgImagesUI { get; set; } = new ObservableCollection<ImageMetadata>();
        private void B_AddFilesToMsg_OnClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (var fileName in dialog.FileNames)
                {
                    FileInfo info = new FileInfo(fileName);
                    if (ImageMetadata.AllowedExtensions.Contains(Path.GetExtension(fileName)))
                    {
                        MsgImages.Add(fileName);
                        MsgImagesUI.Add(new ImageMetadata(info.Name, (int)info.Length));
                    }
                    else
                    {
                        MsgFiles.Add(fileName);
                        MsgFilesUI.Add(new FileMetadata(info.Name, (int)info.Length));
                    }
                }
            }
        }

        private void B_Quit_OnClick(object sender, RoutedEventArgs e)
        {
            Client?.SendAsync(new LogoutMessage(App.MyGuid));
        }
    }


}
