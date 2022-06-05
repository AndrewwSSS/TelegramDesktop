using CommonLibrary.Messages;
using CommonLibrary.Messages.Users;
using MessageLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace TelegramServer
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<User> UsersOnline;
        private ObservableCollection<User> UsersOffline;
        private Dictionary<TcpClientWrap, UserClient> ClientsOnline;
       // private Dictionary<>
        private TelegramDb DbTelegram;
        private TcpServerWrap Server;
        private static Mutex mutex;

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public MainWindow()
        {
            InitializeComponent();

            bool isUniqueApp;
            mutex = new Mutex(true, "TelegramServer", out isUniqueApp);

            if (!isUniqueApp)
            {
                SetForegroundWindow(Process.GetProcessesByName("TelegramServer").First(x => x.Id != Process.GetCurrentProcess().Id).MainWindowHandle);
                Environment.Exit(0);
                Close();
            }
               

            DbTelegram = new TelegramDb();
            ClientsOnline = new Dictionary<TcpClientWrap, UserClient>();
            UsersOnline = new ObservableCollection<User>();
            UsersOffline = new ObservableCollection<User>();

   

            DbTelegram.GroupChats.Load();
            

            Server = new TcpServerWrap();
            Server.Started += OnServerStarted;
            Server.Stopped += OnServerStopped;
            Server.MessageReceived += ClientMessageRecived;


            LB_UsersOffline.ItemsSource = UsersOffline;
            LB_UsersOnline.ItemsSource = UsersOnline;
            LB_Groups.ItemsSource = DbTelegram.GroupChats.Local;

            foreach (var user in DbTelegram.Users)
                UsersOffline.Add(user);

            //MailAddress from = new MailAddress("telegramdesktopbyadat@gmail.com");
            //MailAddress to = new MailAddress("");


            //MailMessage message = new MailMessage(from, to);
            //message.IsBodyHtml = false;
            //message.Subject = "test";
            //message.Body = "some code...";


            //SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587)
            //{
            //    Credentials = new NetworkCredential("telegramdesktopbyadat@gmail.com", ""),
            //    EnableSsl = true
            //};

            //smtp.Send(message);

        }

        #region ServerEvents

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
                TB_ListenerPort.IsEnabled = true;
            });
        }

        private void OnClientDisconnected(TcpClientWrap client)
        {
            UserClient DisconnectedClient
                = ClientsOnline[client];

            DisconnectedClient.User.VisitDate = DateTime.UtcNow;
            DbTelegram.SaveChanges();

            Dispatcher.Invoke(() =>
            {
                UsersOnline.Remove(DisconnectedClient.User);
                UsersOffline.Add(DisconnectedClient.User);
                ClientsOnline.Remove(client);
            });
        }

        #endregion ServerEvents


        #region WpfEvents

        private void BtnStartServer_Click(object sender, RoutedEventArgs e)
        {
            int port;
            if (int.TryParse(TB_ListenerPort.Text, out port) && port > 999 && port < 10000)
            {
                BtnStartServer.IsEnabled = false;
                TB_ListenerPort.IsEnabled = false;
                Server.Start(port, 1000);
            }
            else
                MessageBox.Show("Invalid port", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

        }

        private void BtnStopServer_Click(object sender, RoutedEventArgs e)
        {
            Server.Shutdown();

            UsersOnline.Clear();
            UsersOffline.Clear();

            foreach (var user in DbTelegram.Users)
                UsersOffline.Add(user);

        }

        //TODO
        private void BlockOfflineUser_Click(object sender, RoutedEventArgs e) {
            MessageBox.Show(LB_UsersOffline.SelectedItem.GetType().Name);
        }

        //TODO
        private void BlockOnlineUser_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion WpfEvents

        private void SendMessageToUsers(BaseMessage msg, int senderId, int senderClientId, List<User> usersToSend)
        {
            bool changesExist = false;

            foreach (var user in usersToSend)
            {
                if(user.Id == senderId)
                {
                    foreach(var userClient in user.Clients)
                        if(userClient.Id != senderClientId)
                        {
                            userClient.MessagesToSend.Add(msg);
                            changesExist = true;
                        }
                }
                else
                {
                    foreach (var userClient in user.Clients)
                    {
                        if (isUserOnline(userClient))
                            TcpClientByUserClient(userClient).SendAsync(msg);
                        else
                        {
                            userClient.MessagesToSend.Add(msg);
                            changesExist = true;
                        }
                            
                    }
                }
            }

            if(changesExist)
                DbTelegram.SaveChanges();
        }

        private bool isUserOnline(UserClient userClient) => ClientsOnline.ContainsValue(userClient);

        private TcpClientWrap TcpClientByUserClient(UserClient client){
            return ClientsOnline.FirstOrDefault(c => c.Value == client).Key;
        }
    }
}
