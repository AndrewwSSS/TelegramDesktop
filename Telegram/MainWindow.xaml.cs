using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace Telegram
{

    public partial class MainWindow : Window
    {
        public MenuState RighMenuState { get; set; }
        public MenuState LeftMenuState { get; set; }
        public ThicknessAnimation OpenLeftmenuAnim = new ThicknessAnimation();
        public ThicknessAnimation CloseLeftmenuAnim = new ThicknessAnimation();


        public MainWindow()
        {
            InitializeComponent();
            HideRightManu();


            RighMenuState = MenuState.Hidden;
            LeftMenuState = MenuState.Hidden;

            OpenLeftmenuAnim.From = new Thickness(-280, 0, 0, 0);
            OpenLeftmenuAnim.To = new Thickness(0, 0, 0, 0);
            OpenLeftmenuAnim.Duration = TimeSpan.FromMilliseconds(200);

            CloseLeftmenuAnim.From = LeftMenu.BorderThickness;
            CloseLeftmenuAnim.To = new Thickness(-280, 0, 0, 0);
            CloseLeftmenuAnim.Duration = TimeSpan.FromMilliseconds(200);
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
            showOrOpenRightMenu();
        }

        public void showOrOpenRightMenu()
        {
            if (RighMenuState == MenuState.Open)
                HideRightManu();
            else
                OpenRightMenu();

        }

        public void HideRightManu()
        {
            this.Width -= ColumnRightMenu.ActualWidth;
            Spliter.Visibility = Visibility.Collapsed;
            RightMenu.Visibility = Visibility.Collapsed;


            RighMenuState = MenuState.Hidden;
        }

        public void OpenRightMenu()
        {
            this.Width += 280;
            Spliter.Visibility = Visibility.Visible;
            RightMenu.Visibility = Visibility.Visible;


            RighMenuState = MenuState.Open;
        }



        private void TopPanel_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            this.DragMove();
        }

        private void BTNClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void BTNHiDEWindow_Click(object sender, RoutedEventArgs e)
        {
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
            LeftMenu.BeginAnimation(Border.MarginProperty, OpenLeftmenuAnim);
            MainGrid.Opacity = 0.5;
            LeftMenuState = MenuState.Open;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LeftMenu.BeginAnimation(Border.MarginProperty, CloseLeftmenuAnim);
            MainGrid.Opacity = 1;
        }

        private void Border_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (LeftMenuState == MenuState.Open)
            {
                LeftMenu.BeginAnimation(Border.MarginProperty, CloseLeftmenuAnim);
                MainGrid.Opacity = 1;
                LeftMenuState = MenuState.Hidden;
            }
        }

    }

    public enum MenuState { Hidden, Open }
}
