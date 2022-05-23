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

namespace Telegram
{

    public enum MenuState
    {
        Hidden,
        Open
    }

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public TcpClientWrap Client
        {
            get => client;
            set
            {
                client = value;

                client.MessageReceived += Client_MessageReceived;
            }
        }
        public const int LeftMenuWidth = 280;
        public MenuState RighMenuState { get; set; }
        public MenuState LeftMenuState { get; set; }
        public MenuState AddGroupMenuState { get; set; }

        public static DoubleAnimation MakeDoubleAnim(double value, double seconds) => new DoubleAnimation(value, new Duration(TimeSpan.FromSeconds(seconds)));


        public DoubleAnimation MainGridDark = new DoubleAnimation(0.5, new Duration(TimeSpan.FromSeconds(0.2)));
        public DoubleAnimation MainGridDarkReverse = new DoubleAnimation(1, new Duration(TimeSpan.FromSeconds(0.2)));
        public ThicknessAnimation OpenLeftMenuAnim = new ThicknessAnimation();
        public ThicknessAnimation CloseLeftMenuAnim = new ThicknessAnimation();

        private ObservableCollection<PublicGroupInfo> groups = new ObservableCollection<PublicGroupInfo>();
        private ObservableCollection<MessageItemWrap> messages;
        private TcpClientWrap client;
        private ObservableCollection<PublicGroupInfo> foundGroups;
        private PublicGroupInfo curGroup;

        public PublicUserInfo Me { get; set; }
        public static PublicUserInfo ivan { get; set; } = new PublicUserInfo(-2, "ivandovg", "Ivan Dovgolutsky", "", DateTime.Now);

        public ObservableCollection<MessageItemWrap> Messages
        {
            get => messages;
            set
            {
                messages = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<PublicGroupInfo> Groups
        {
            get => groups;
            set
            {

                groups = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<PublicGroupInfo> FoundGroups
        {
            get => foundGroups;
            set
            {
                foundGroups = value;
                OnPropertyChanged();
            }
        }
        public PublicGroupInfo CurGroup { 
            get => curGroup;
            set
            {
                curGroup = value;
                OnPropertyChanged();
                ShowGroupMessages(CurGroup);
            }
        }
        public List<PublicUserInfo> Users { get; set; } = new List<PublicUserInfo>();
        public MainWindow() : this(new PublicUserInfo(999, "existeddim4", "Дмитрий Осипов", "Description", DateTime.Now)) { }
        public MainWindow(PublicUserInfo me)
        {

            DataContext = this;
            InitializeComponent();
            HideRightMenu();

            Groups.Add(new PublicGroupInfo("TEST", "", -1)
            {
                Id = 0,
                Messages = new List<ChatMessage>() { new ChatMessage("Прувет!") }
            });
            Me = me;
            Users.Add(me);
            Users.Add(ivan);


            RighMenuState = MenuState.Hidden;
            LeftMenuState = MenuState.Hidden;

            OpenLeftMenuAnim.From = new Thickness(-LeftMenuWidth, 0, 0, 0);
            OpenLeftMenuAnim.To = new Thickness(0, 0, 0, 0);
            OpenLeftMenuAnim.Duration = TimeSpan.FromMilliseconds(200);

            CloseLeftMenuAnim.From = LeftMenu.BorderThickness;
            CloseLeftMenuAnim.To = new Thickness(-LeftMenuWidth, 0, 0, 0);
            CloseLeftMenuAnim.Duration = TimeSpan.FromMilliseconds(200);



            Messages = new ObservableCollection<MessageItemWrap>();
            var respTo = new ChatMessage("Всем привет, это телеграм").SetFrom(Me);
            Groups[0].Messages = new List<ChatMessage>()
            {
                respTo,
                new ChatMessage("Скоро командный проект").SetFrom(Me).SetResendUser(ivan),
                new ChatMessage("Да, готовьтесь").SetFrom(ivan).SetRespondingTo(respTo),
                new ChatMessage("тест").SetFrom(Me),
                new ChatMessage("тест").SetFrom(Me),
                new ChatMessage("тест").SetFrom(Me),
                new ChatMessage("тест").SetFrom(Me)
            };


            Closing += OnClosed;
        }

        private void OnClosed(object sender, EventArgs e)
        {
            if (Me != null)
                Client.Send(new ClientDisconnectMessage(Me.Id));
            Client.Disconnect();
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

                        Groups.Add(new PublicGroupInfo(Buffers.GroupName, "", result.GroupId){
                            Users = new List<PublicUserInfo>() { Me}
                        });
                    }
                }
                else if (msg is ArrayMessage<PublicGroupInfo>)
                {
                    var array = (msg as ArrayMessage<PublicGroupInfo>).Array;
                    if (array != null && array.Length != 0)
                    {
                        B_CloseFoundGroups.IsEnabled = true;
                        LB_FoundGroups.Visibility = Visibility.Visible;
                        FoundGroups = new ObservableCollection<PublicGroupInfo>(array.ToList());
                    }
                }
                else if (msg is GroupJoinResultMessage)
                {
                    var result = msg as GroupJoinResultMessage;
                    if (result.Result == AuthenticationResult.Success)
                    {
                        Groups.Add(Buffers.GroupJoinInfo);
                        Users.AddRange(Buffers.GroupJoinInfo.Users);
                        B_JoinGroup.Visibility = Visibility.Hidden;
                    }
                }
                else if (msg is ChatMessage)
                {
                    var chatMsg = msg as ChatMessage;
                    var group = Groups.First(g => g.Id == chatMsg.GroupId);
                    if (group.Messages == null)
                        group.Messages = new List<ChatMessage>();
                    group.Messages.Add(chatMsg);
                    AddMessage(chatMsg);
                }
            });
        }

        private void AddMessage(ChatMessage msg)
        {
            MessageItemWrap item = new MessageItemWrap(msg);
            item.FromUser = Users.First(u => u.Id == msg.FromUserId);
            if (msg.RespondingTo != -1)
                item.RespondingTo = Messages.First(m => m.Message.Id == msg.RespondingTo);
            if (msg.RepostUserId != -1)
                item.RepostUser = Users.First(u => u.Id == msg.RepostUserId);

            var group = Groups.First(g => g.Id == msg.GroupId);

            if (Messages.Count != 0 && Messages.Last().Message.FromUserId == msg.FromUserId)
            {
                Messages.Last().ShowAvatar = false;
                item.ShowAvatar = true;
                if (LB_Groups.SelectedItem == group)
                    Messages.Add(item);
            }
            else
            {
                item.ShowAvatar = true;
                if (LB_Groups.SelectedItem == group)
                    Messages.Add(item);
            }
        }
        private void ShowGroupMessages(PublicGroupInfo group)
        {

            Messages.Clear();
            if (CurGroup.Messages != null)
                foreach (var msg in CurGroup.Messages)
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

        private void BTNClose_Click(object sender, RoutedEventArgs e) => this.Close();


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
                    MainGrid.BeginAnimation(OpacityProperty, MainGridDarkReverse));
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
                        Client.SendAsync(new GroupLookupMessage(textBox.Text));
                        Client.ReceiveAsync();
                    }
        }

        private void B_AddGroupMenu_OnClick(object sender, RoutedEventArgs e)
        {
            var fadeIn = MakeDoubleAnim(1, 0.3);

            HideMenus(null, null);
            AddGroupMenuState = MenuState.Open;
            AddGroupMenu.Opacity = 0;
            AddGroupMenu.BeginAnimation(Border.OpacityProperty, fadeIn);

            MainGrid.BeginAnimation(Border.OpacityProperty, MainGridDark);

        }

        private void B_AddGroup_OnClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() => Buffers.GroupName = TB_NewGroupName.Text);
            Client.MessageSent -= Client_MessageSent;
            Client.MessageSent += Client_MessageSent;
            var msg = new CreateGroupMessage(Buffers.GroupName, Me.Id);
            Client.SendAsync(msg);
            AddGroupButton.IsEnabled = false;
            Client.ReceiveAsync();
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

            CurGroup = lb.SelectedItem as PublicGroupInfo;
        }

        private void FoundGroupSelected(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = sender as ListBox;
            if (lb.SelectedIndex == -1)
                return;
            CurGroup = lb.SelectedItem as PublicGroupInfo;
            if (!Groups.Contains(CurGroup))
                B_JoinGroup.Visibility = Visibility.Visible;
        }

        private void B_CloseFoundGroups_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            button.IsEnabled = false;
            FoundGroups = null;
            LB_FoundGroups.Visibility = Visibility.Hidden;
            B_JoinGroup.Visibility = Visibility.Hidden;
        }

        private void B_JoinGroup_Click(object sender, RoutedEventArgs e)
        {
            if (LB_FoundGroups.SelectedIndex == -1)
                return;
            var groupInfo = LB_FoundGroups.SelectedItem as PublicGroupInfo;
            Buffers.GroupJoinInfo = groupInfo;
            Client.SendAsync(new GroupJoinMessage(groupInfo.Id, Me.Id));
            Client.ReceiveAsync();
        }

        private void TB_SendMsg_OnEnter(object sender, KeyEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                var textBox = sender as TextBox;
                if (e.Key == Key.Enter && !String.IsNullOrEmpty(textBox.Text))
                {
                    ChatMessage msg = new ChatMessage(textBox.Text).SetFrom(Me).SetGroupId(CurGroup.Id);
                    Client.SendAsync(msg);
                    AddMessage(msg);
                    textBox.Text = "";
                }
            });
        }
    }


}
