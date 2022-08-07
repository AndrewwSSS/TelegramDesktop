using CommonLibrary;
using CommonLibrary.Messages;
using CommonLibrary.Messages.Files;
using CommonLibrary.Messages.Groups;
using CommonLibrary.Messages.Users;
using MessageLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using TelegramServer.View;

namespace TelegramServer
{

    public partial class MainWindow : Window
    {
        private ObservableCollection<User> UsersOnline;
        private ObservableCollection<User> UsersOffline;
        private Dictionary<TcpClientWrap, UserClient> ClientsOnline;
        private Dictionary<UserClient , TcpFileClientWrap> FileClientsOnline;
        private TelegramDb DbTelegram;
        private TcpServerWrap Server;
        private TcpFileServerWrap FileServer;
        private Dictionary<UserClient, UserDownloads> UsersDownloads;
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
            FileClientsOnline = new Dictionary<UserClient, TcpFileClientWrap>();
            Server = new TcpServerWrap();
            FileServer = new TcpFileServerWrap();
            UsersDownloads = new Dictionary<UserClient, UserDownloads>();

            DbTelegram.GroupChats.Load();
            
            Server.Started += OnServerStarted;
            Server.Stopped += OnServerStopped;
            Server.MessageReceived += ClientMessageRecived;

            //FileServer.UserSynchronized += UserSynchronized;
            //FileServer.FileChunkReceived += FileServer_FileChunkReceived;
            //FileServer.ImageChunkReceived += FileServer_ImageChunkReceived;

            LB_UsersOffline.ItemsSource = UsersOffline;
            LB_UsersOnline.ItemsSource = UsersOnline;
            LB_Groups.ItemsSource = DbTelegram.GroupChats.Local;

            foreach (var user in DbTelegram.Users)
                UsersOffline.Add(user);

            var random = new Random();

            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(PasswordGenerator.GeneratePassword(10));
            }
           
       


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
            UserClient disconnectedClient
                = ClientsOnline[client];

            User disconnectedUser = disconnectedClient.User;

            disconnectedClient.User.VisitDate = DateTime.UtcNow;

            Dispatcher.Invoke(() =>
            {
                UsersOnline.Remove(disconnectedClient.User);
                UsersOffline.Add(disconnectedClient.User);
                ClientsOnline.Remove(client);
                FileClientsOnline.Remove(disconnectedClient);
            });


            lock (DbTelegram)    {
                DbTelegram.SaveChanges();
            }

       
            if (!isUserOnline(disconnectedUser)) {
                UserUpdateMessage msg = new UserUpdateMessage()
                {
                    OnlineStatus = false,
                    UserId = disconnectedUser.Id
                };

                SendMessageToOnlineUsers(msg, disconnectedUser.Id, disconnectedClient.Id, disconnectedUser.UniqueRelations);
            }
          



        }

        //private void UserSynchronized(TcpFileClientWrap client)
        //{
        //    User sender = DbTelegram.Users.FirstOrDefault(u => client.UserId == u.Id);
        //    UserClient senderClient = sender.Clients.FirstOrDefault(c => c.Guid == client.Guid);

        //    FileClientsOnline[senderClient] = client;
        //}

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

                FileServer.Start(port+1, 1000);
            }
            else
                MessageBox.Show("Invalid port", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

        }

        private void BtnStopServer_Click(object sender, RoutedEventArgs e)
        {
            Server.Shutdown();
            FileServer.Shutdown();

            UsersOnline.Clear();
            UsersOffline.Clear();

            foreach (var user in DbTelegram.Users)
                UsersOffline.Add(user);

        }


        private void BTN_GroupInfo_Click(object sender, RoutedEventArgs e)
        {
            if(LB_Groups.SelectedItem != null)  {

                GroupChat selectedGroup = (GroupChat)LB_Groups.SelectedItem;
                GroupViewer groupViewer
                    = new GroupViewer(DbTelegram, selectedGroup.Id);

                groupViewer.Show();
            }
        }

        #endregion WpfEvents

        private void SendMessageToOnlineUsers(Message msg, int senderId, int senderClientId, ICollection<User> usersToSend)
        {
          
            foreach (var user in usersToSend.Where(u => u.Id != senderId)) {
                foreach (var userClient in user.Clients) {
                    if (isClientOnline(userClient))
                        TcpClientByUserClient(userClient).SendAsync(msg);
                }
            }
        }

        private void SendMessageToUsers(BaseMessage msg, int senderId, int senderClientId, ICollection<User> usersToSend)
        {  
            foreach (var user in usersToSend)
            {
                if(user.Id == senderId)
                {
                    foreach(var userClient in user.Clients)
                        if(userClient.Id != senderClientId)
                        {
                            userClient.MessagesToSend.Add(msg);
                            lock (DbTelegram)
                            {
                                DbTelegram.SaveChanges();
                            }
                        }
                }
                else {
                    if (user.Clients.Count == 0)
                    {
                        user.MessagesToSend.Add(msg);
                        lock (DbTelegram) {
                            DbTelegram.SaveChanges();
                        }
                    }
                    else
                    {
                        foreach (var userClient in user.Clients) {
                            if (isClientOnline(userClient))
                                TcpClientByUserClient(userClient).SendAsync(msg);
                            else {

                            

                                userClient.MessagesToSend.Add(msg);

                                lock (DbTelegram)       {
                                    DbTelegram.SaveChanges();
                                }

                                //changesExist = true;
                      
                            }
                            
                        }
                    }

                    
                }
            }


               
        }

        private bool isUserOnline(User user)
        {
            if(user == null)
                throw new ArgumentNullException("user");

            foreach(UserClient client in user.Clients)
            {
                if (isClientOnline(client))
                    return true;
            }
            return false;
        }

        private bool isClientOnline(UserClient userClient) => ClientsOnline.ContainsValue(userClient);

        private TcpClientWrap TcpClientByUserClient(UserClient client){
            return ClientsOnline.FirstOrDefault(c => c.Value == client).Key;
        }

        private void InfoAboutUserOffline_Click(object sender, RoutedEventArgs e)
        {
            if (LB_UsersOffline.SelectedItem != null)
            {
                User user = (User)LB_UsersOffline.SelectedItem;
                UserViewer userViewer = new UserViewer(DbTelegram, user.Id);
                userViewer.Show();
            }
        }

        private void InfoAboutUserOnline_Click(object sender, RoutedEventArgs e)
        {
            if (LB_UsersOnline.SelectedItem != null)
            {
                User user = (User)LB_UsersOnline.SelectedItem;
                UserViewer userViewer = new UserViewer(DbTelegram, user.Id);
                userViewer.Show();
            }
        }
    }
}
