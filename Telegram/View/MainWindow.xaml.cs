﻿using CommonLibrary.Containers;
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

        TcpClientWrap client;

        public const int LeftMenuWidth = 280;
        public MenuState RighMenuState { get; set; }
        public MenuState LeftMenuState { get; set; }
        public MenuState AddGroupMenuState { get; set; }

        public static DoubleAnimation MakeDoubleAnim(double value, double seconds) => new DoubleAnimation(value, new Duration(TimeSpan.FromSeconds(seconds)));


        public DoubleAnimation MainGridDark = new DoubleAnimation(0.5, new Duration(TimeSpan.FromSeconds(0.2)));
        public DoubleAnimation MainGridDarkReverse = new DoubleAnimation(1, new Duration(TimeSpan.FromSeconds(0.2)));
        public ThicknessAnimation OpenLeftMenuAnim = new ThicknessAnimation();
        public ThicknessAnimation CloseLeftMenuAnim = new ThicknessAnimation();
        private ObservableCollection<GroupChat> groups = new ObservableCollection<GroupChat>();
        private ObservableCollection<MessageItemWrap> messages;

        public User Me { get; set; }
        public static User ivan { get; set; } = new User("Иван Довголуцкий");


        public ObservableCollection<MessageItemWrap> Messages
        {
            get => messages;
            set
            {
                messages = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<GroupChat> Groups
        {
            get => groups;
            set
            {

                groups = value;
                OnPropertyChanged();
            }
        }
        public MainWindow() : this(null, new User("Дмитрий Осипов")
        {
            RegistrationDate = DateTime.Now
        })
        { }
        public MainWindow(TcpClientWrap client, User me)
        {

            DataContext = this;
            InitializeComponent();
            HideRightMenu();

            Groups.Add(new GroupChat()
            {
                Name = "ПВ011",
                Images = new List<ImageContainer>() { ImageContainer.FromFile("Resources/darkl1ght.png") },
                Messages = new List<ChatMessage>() { new ChatMessage("Прувет!") }
            });

            Me = me;
            ivan.AddImage("Resources/ivan.jpg");
            Me.AddImage("Resources/darkl1ght.png");
            this.client = client;
            if (client != null)
                this.client.MessageReceived += Client_MessageReceived;

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

                        Groups.Add(new GroupChat() { DateCreated = DateTime.Now, Name = Buffers.GroupName });
                    }
                }
            });
        }

        private void AddMessage(ChatMessage msg)
        {
            MessageItemWrap item = new MessageItemWrap(msg);
            if (Messages.Count != 0 && Messages.Last().Message.FromUser == msg.FromUser)
            {
                Messages.Last().ShowAvatar = false;
                item.ShowAvatar = true;
                Messages.Add(item);
            }
            else
            {
                item.ShowAvatar = true;
                Messages.Add(item);
            }
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
                    MainGrid.BeginAnimation(Border.OpacityProperty, MainGridDarkReverse));
                };
                AddGroupMenuState = MenuState.Hidden;
                AddGroupMenu.BeginAnimation(Border.OpacityProperty, fadeAway);
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
                    if (client.IsConnected)
                    {
                        client.SendAsync(new GroupLookupMessage(textBox.Text));
                        client.ReceiveAsync();
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
            Buffers.GroupName = TB_NewGroupName.Text;
            client.SendAsync(new CreateGroupMessage(TB_NewGroupName.Text, Me.Id));
            client.ReceiveAsync();
        }

        private void TB_GroupName_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            AddGroupButton.IsEnabled = !string.IsNullOrEmpty(textBox.Text);
        }

        private void GroupSelected(object sender, RoutedEventArgs e)
        {
            ListBox lb = sender as ListBox;
            Messages.Clear();
            if((lb.SelectedItem as GroupChat).Messages != null)
                foreach(var msg in (lb.SelectedItem as GroupChat).Messages)
                    AddMessage(msg);
        }
    }


}
