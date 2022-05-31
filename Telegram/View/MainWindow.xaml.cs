using CacheLibrary;
using CommonLibrary.Containers;
using CommonLibrary.Messages;
using CommonLibrary.Messages.Auth;
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
        public MainWindow() : this(new PublicUserInfo(999, "existeddim4", "Дмитрий Осипов", "Description")) { }

        public MainWindow(PublicUserInfo me)
        {

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


            DataContext = this;
            InitializeComponent();
            HideRightMenu();
            Me = me;
            Users.Add(new UserItemWrap(me));

            RighMenuState = MenuState.Hidden;
            LeftMenuState = MenuState.Hidden;

            OpenLeftMenuAnim.From = new Thickness(-LeftMenuWidth, 0, 0, 0);
            OpenLeftMenuAnim.To = new Thickness(0, 0, 0, 0);
            OpenLeftMenuAnim.Duration = TimeSpan.FromMilliseconds(200);

            CloseLeftMenuAnim.From = LeftMenu.BorderThickness;
            CloseLeftMenuAnim.To = new Thickness(-LeftMenuWidth, 0, 0, 0);
            CloseLeftMenuAnim.Duration = TimeSpan.FromMilliseconds(200);



            Messages = new ObservableCollection<MessageItemWrap>();


            Closing += OnClosed;
        }
        private List<GroupItemWrap> CachedGroups { get; set; } = new List<GroupItemWrap>();
        private void OnClosed(object sender, EventArgs e)
        {
            Client?.Disconnect();
            SaveCache();
        }

        private void Client_MessageReceived(TcpClientWrap client, Message msg)
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
                            MembersId = new List<int>() { Me.Id }
                        };
                        var newGroup = new GroupItemWrap(info)
                        {
                            Members = new ObservableCollection<UserItemWrap>(Users.FindAll(u => u.User == Me))
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
                        }
                    }
                }
                else if (msg is ArrayMessage<PublicGroupInfo>)
                {
                    var array = (msg as ArrayMessage<PublicGroupInfo>).Array;
                    if (array != null && array.Length != 0)
                    {
                        B_CloseFoundGroups.IsEnabled = true;
                        LB_FoundGroups.Visibility = Visibility.Visible;

                        FoundGroups.Clear();
                        List<int> requestedId = new List<int>();
                        foreach (var group in array)
                        {
                            var cachedGroup = CachedGroups.FirstOrDefault(g => g.GroupChat.Id == group.Id);
                            if (cachedGroup != null)
                            {
                                FoundGroups.Add(cachedGroup);
                                continue;
                            }
                            GroupItemWrap item = new GroupItemWrap(group);
                            foreach (var uId in group.MembersId.Where(
                                id => !requestedId.Contains(id)
                                ))
                            {
                                var user = Users.FirstOrDefault(u => u.User.Id == uId);
                                if (user == null)
                                    Client.SendAsync(new DataRequestMessage(uId, RequestType.User));
                                else
                                    item.Members.Add(user);
                                FoundGroups.Add(item);
                                CachedGroups.Add(item);
                            }
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
                        AddMessage(chatMsg);
                }
                else if (msg is GroupUpdateMessage)
                {
                    var info = msg as GroupUpdateMessage;
                    if (info.NewUser != null)
                    {
                        var user = Users.FirstOrDefault(u => u.User.Id == info.NewUser.Id);
                        if (user == null)
                            Client.SendAsync(new DataRequestMessage(info.NewUser.Id, RequestType.User));
                        else
                            Groups.First(g => g.GroupChat.Id == info.GroupId).Members.Add(user);
                    }
                }
            });
        }

        private void AddMessage(ChatMessage msg)
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

            var group = Groups.FirstOrDefault(g => g.GroupChat.Id == msg.GroupId);

            if (group == null)
            {
                group = FoundGroups.FirstOrDefault(g => g.GroupChat.Id == msg.GroupId);
                if (group == null)
                    return;
            }

            if (LB_Groups.SelectedItem == group ||
                LB_FoundGroups.SelectedItem == group ||
                item.Message.FromUserId == Me.Id)
            {
                if (Messages.Count != 0 && Messages.Last().Message.FromUserId == msg.FromUserId)
                    Messages.Last().ShowAvatar = false;
                item.ShowAvatar = true;
                Messages.Add(item);
            }
        }
        private void ShowGroupMessages(GroupItemWrap group)
        {
            Messages.Clear();
            RespondingTo = null;
            if (CurGroup.GroupChat.Messages != null)
                foreach (var msg in group.Messages)
                    AddMessage(msg);
        }
        private void BTNFullScreen_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;

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
                    if (Client.IsConnected)
                    {
                        Client.SendAsync(new GroupLookupMessage(textBox.Text, Me.Id, App.MyGuid));
                    }
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
            if (!Groups.Contains(CurGroup))
                B_JoinGroup.Visibility = Visibility.Visible;
            ShowGroupMessages(CurGroup);
        }

        private void B_CloseFoundGroups_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            button.IsEnabled = false;
            FoundGroups.Clear();
            LB_FoundGroups.Visibility = Visibility.Hidden;
            B_JoinGroup.Visibility = Visibility.Hidden;
        }

        private void B_JoinGroup_Click(object sender, RoutedEventArgs e)
        {
            if (LB_FoundGroups.SelectedIndex == -1)
                return;
            var group = LB_FoundGroups.SelectedItem as GroupItemWrap;
            Client.SendAsync(new GroupJoinMessage(group.GroupChat.Id, Me.Id, App.MyGuid));
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
                    {
                        msg.SetRespondingTo(RespondingTo.Message);
                        RespondingTo = null;
                    }
                    Client.SendAsync(msg);

                    CurGroup.GroupChat.Messages.Add(msg);
                    CurGroup.OnPropertyChanged("Messages");
                    CurGroup.OnPropertyChanged("LastMessage");
                    
                    AddMessage(msg);
                    textBox.Text = "";
                }
            });
        }

        private void CloseRespondingToPanel(object sender, RoutedEventArgs e)
        {
            RespondingTo = null;
        }
    }


}
