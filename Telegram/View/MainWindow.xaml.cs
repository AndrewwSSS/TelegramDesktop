using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace Telegram
{

    public enum MenuState
    {
        Hidden,
        Open
    }

    public partial class MainWindow : Window
    {
        public const int LeftMenuWidth = 280;

        public MenuState RighMenuState { get; set; }
        public MenuState LeftMenuState { get; set; }

        public ThicknessAnimation OpenLeftMenuAnimation = new ThicknessAnimation();
        public ThicknessAnimation CloseLeftMenuAnimation = new ThicknessAnimation();


        public MainWindow()
        {
            InitializeComponent();
            HideRightManu();

            RighMenuState = MenuState.Hidden;
            LeftMenuState = MenuState.Hidden;

            OpenLeftMenuAnimation.From = new Thickness(-LeftMenuWidth, 0, 0, 0);
            OpenLeftMenuAnimation.To = new Thickness(0, 0, 0, 0);
            OpenLeftMenuAnimation.Duration = TimeSpan.FromMilliseconds(200);

            CloseLeftMenuAnimation.From = LeftMenu.BorderThickness;
            CloseLeftMenuAnimation.To = new Thickness(-LeftMenuWidth, 0, 0, 0);
            CloseLeftMenuAnimation.Duration = TimeSpan.FromMilliseconds(200);


        }



        private void BTNAllScreen_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;

        }


        private void ShowOrOpenRightMenu_Click(object sender, RoutedEventArgs e)
        {
            if (RighMenuState == MenuState.Open)
                HideRightManu();
            else
                OpenRightMenu();
        }

        public void HideRightManu()
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
                    HideRightManu();
        }

        private void BTNOpenLeftMenu_Click(object sender, RoutedEventArgs e)
        {
            LeftMenu.BeginAnimation(Border.MarginProperty, OpenLeftMenuAnimation);
            MainGrid.Opacity = 0.5;
            LeftMenuState = MenuState.Open;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LeftMenu.BeginAnimation(Border.MarginProperty, CloseLeftMenuAnimation);
            MainGrid.Opacity = 1;
        }

        private void Border_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (LeftMenuState == MenuState.Open)
            {
                LeftMenu.BeginAnimation(Border.MarginProperty, CloseLeftMenuAnimation);
                MainGrid.Opacity = 1;
                LeftMenuState = MenuState.Hidden;
            }
        }

    }

    
}
