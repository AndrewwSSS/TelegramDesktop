using MessageLibrary;
using System.Collections.ObjectModel;
using System.Windows;
using CommonLibrary;
using System.Data.Entity;
using System.Linq;
using System.Collections.Generic;

namespace TelegramServer
{


    public partial class MainWindow : Window
    {
        private ObservableCollection<User> UsersOnline;
        private ObservableCollection<User> UsersOffline;
        private TelegramDb DbContext;

        private TcpServerWrap Server;

        public MainWindow()
        {
            

            InitializeComponent();

            UsersOnline = new ObservableCollection<User>();
            UsersOffline = new ObservableCollection<User>();

            LB_UsersOffline.ItemsSource = UsersOffline;
            LB_UsersOnline.ItemsSource = UsersOnline;


            DbContext = new TelegramDb();
            DbContext.SaveChanges();



     

            //foreach (var item in DbContext.Users)
            //{
            //    UsersOffline.Add(item);
            //}

            //LB_UsersOnline.ItemsSource = DbContext.Users.First().MessagesToSend;


            //LB_UsersOffline.ItemsSource = UsersOffline;
            ////LB_UsersOnline.ItemsSource = UsersOnline;

            //Server = new TcpServerWrap();

            //Server.Started += OnServerStarted;
            //Server.Stopped += OnServerStopped;
            //Server.MessageReceived += ClientMessageRecived;

        }


        private void BtnStartServer_Click(object sender, RoutedEventArgs e)
        {
            int port;
            if(int.TryParse(TB_ListenerPort.Text, out port) && port > 9999 && port < 100000)
            {
                BtnStartServer.IsEnabled = false;
                Server.Start(port, 1000);
            }
        }

        private void OnServerStarted(TcpServerWrap client)
        {
            Dispatcher.Invoke(() =>
            {
                BtnStopServer.IsEnabled = true;
            });
        }

        private void OnServerStopped(TcpServerWrap client)
        {
            Dispatcher.Invoke(() =>
            {
                BtnStopServer.IsEnabled = false;
                BtnStartServer.IsEnabled = true;
            });
        }

        private void ClientMessageRecived(TcpClientWrap client, Message msg)
        {
            

        }

    }
}
