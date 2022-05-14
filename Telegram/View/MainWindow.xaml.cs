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
        private TcpClientWrap client ;

        public const int LeftMenuWidth = 280;
        public MenuState RighMenuState { get; set; }
        public MenuState LeftMenuState { get; set; }

        public DoubleAnimation OpenLeftMenuDark = new DoubleAnimation(0.5, new Duration(TimeSpan.FromSeconds(0.3)));
        public DoubleAnimation OpenLeftMenuReverse = new DoubleAnimation(1, new Duration(TimeSpan.FromSeconds(0.3)));
        public ThicknessAnimation OpenLeftMenuAnimation = new ThicknessAnimation();
        public ThicknessAnimation CloseLeftMenuAnimation = new ThicknessAnimation();
        private static readonly User me = new User("Дмитрий Осипов")
            .AddImage("Resources/darkl1ght.png");
        private static readonly User ivan = new User("Иван Довголуцкий")
            .AddImage("Resources/ivan.jpg");
        public ObservableCollection<ChatMessage> Messages { get; set; } 
            

        public MainWindow()
        {
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

            client = new TcpClientWrap(IPAddress.Parse("26.246.72.11"), 5000);
            DataContext = this;

            Messages = new ObservableCollection<ChatMessage>();
            AddMessage(new ChatMessage("Скоро командный проект").SetFrom(me).SetResendUser(ivan));
            AddMessage(new ChatMessage("Всем привет, это телеграм").SetFrom(me));
            AddMessage(new ChatMessage("Да, готовьтесь").SetFrom(ivan).SetRespondingTo(Messages[1]));
            AddMessage(new ChatMessage("тест").SetFrom(me));
            AddMessage(new ChatMessage("тест").SetFrom(me));
            AddMessage(new ChatMessage("тест").SetFrom(me));
            AddMessage(new ChatMessage("тест").SetFrom(me));
        }

        private void AddMessage(ChatMessage msg)
        {
            if(Messages.Count != 0 && Messages.Last().FromUser == msg.FromUser)
            {
                Messages.Last().ShowAvatar = false;
                msg.ShowAvatar = true;
                Messages.Add(msg);
            }
            else
            {
                msg.ShowAvatar = true;
                Messages.Add(msg);
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
