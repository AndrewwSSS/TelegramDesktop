using CommonLibrary;
using MessageLibrary;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace Telegram
{

    public enum MenuState
    {
        Hidden,
        Open
    }

    public partial class MainWindow : Window
    {
        TcpClientWrap client;

        public const int LeftMenuWidth = 280;
        public MenuState RighMenuState { get; set; }
        public MenuState LeftMenuState { get; set; }

        public DoubleAnimation OpenLeftMenuDark = new DoubleAnimation(0.5, new Duration(TimeSpan.FromSeconds(0.3)));
        public DoubleAnimation OpenLeftMenuReverse = new DoubleAnimation(1, new Duration(TimeSpan.FromSeconds(0.3)));
        public ThicknessAnimation OpenLeftMenuAnimation = new ThicknessAnimation();
        public ThicknessAnimation CloseLeftMenuAnimation = new ThicknessAnimation();
        public User Me { get; set; }
        public static User ivan { get; set; } = new User("Иван Довголуцкий");
            
        
        public ObservableCollection<MessageItemWrap> Messages { get; set; }
        public MainWindow() : this(null, new User("Дмитрий Осипов")
        {
            RegistrationDate = DateTime.Now
        })
        { }
        public MainWindow(TcpClientWrap client, User me) 
        {
            Me = me;
            ivan.AddImage("Resources/ivan.jpg");
            Me.AddImage("Resources/darkl1ght.png");
            this.client = client;
            InitializeComponent();
            HideRightMenu();

            RighMenuState = MenuState.Hidden;
            LeftMenuState = MenuState.Hidden;

            OpenLeftMenuAnimation.From = new Thickness(-LeftMenuWidth, 0, 0, 0);
            OpenLeftMenuAnimation.To = new Thickness(0, 0, 0, 0);
            OpenLeftMenuAnimation.Duration = TimeSpan.FromMilliseconds(200);

            CloseLeftMenuAnimation.From = LeftMenu.BorderThickness;
            CloseLeftMenuAnimation.To = new Thickness(-LeftMenuWidth, 0, 0, 0);
            CloseLeftMenuAnimation.Duration = TimeSpan.FromMilliseconds(200);

            DataContext = this;

            Messages = new ObservableCollection<MessageItemWrap>();
            AddMessage(new ChatMessage("Всем привет, это телеграм").SetFrom(Me));
            AddMessage(new ChatMessage("Скоро командный проект").SetFrom(Me).SetResendUser(ivan));
            AddMessage(new ChatMessage("Да, готовьтесь").SetFrom(ivan).SetRespondingTo(Messages[1].Message));
            AddMessage(new ChatMessage("тест").SetFrom(Me));
            AddMessage(new ChatMessage("тест").SetFrom(Me));
            AddMessage(new ChatMessage("тест").SetFrom(Me));
            AddMessage(new ChatMessage("тест").SetFrom(Me));
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


        private void BTNHiDEWindow_Click(object sender, RoutedEventArgs e) {
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
            LeftMenu.BeginAnimation(Border.MarginProperty, OpenLeftMenuAnimation);
            MainGrid.BeginAnimation(Border.OpacityProperty, OpenLeftMenuDark);
            LeftMenuState = MenuState.Open;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LeftMenu.BeginAnimation(Border.MarginProperty, CloseLeftMenuAnimation);
            MainGrid.BeginAnimation(Border.OpacityProperty, OpenLeftMenuReverse);
        }

        private void HideLeftMenu(object sender, MouseButtonEventArgs e)
        {
            if (LeftMenuState == MenuState.Open)
            {
                LeftMenu.BeginAnimation(Border.MarginProperty, CloseLeftMenuAnimation);
                MainGrid.BeginAnimation(Border.OpacityProperty, OpenLeftMenuReverse);
                LeftMenuState = MenuState.Hidden;
            }
        }

        private void Responce_OnClick(object sender, MouseButtonEventArgs e)
        {
        }
    }

    
}
