using MessageLibrary;
using MessageLibrary.Containers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfTest
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpFileClientWrap FileClient { get; set; } = new TcpFileClientWrap(IPAddress.Parse("192.168.0.107"), 5001, 228, "guid!");
        private TcpFileServerWrap FileServer { get; set; } = new TcpFileServerWrap();
        private FileStream writer = new FileStream("receivedimage.png", FileMode.OpenOrCreate);
        public MainWindow()
        {
            InitializeComponent();

            FileServer.Started += FileServer_Started;
            FileServer.UserSynchronized += UserSynchronized;
            FileServer.FileChunkReceived += FileChunkReceived;
            FileClient.FileChunkReceived += FileChunkReceived;

            FileServer.Start(5001,1);
        }

        private void FileChunkReceived(TcpFileClientWrap client, int fileId, byte[] chunk, bool isLast)
        {
            writer.Write(chunk, 0, chunk.Length);
            if (isLast)
                writer.Close();
        }

        private void FileServer_Started(TcpFileServerWrap server)
        {
            FileClient.Connected += FileClient_Connected;
            FileClient.ConnectAsync();
        }

        private void UserSynchronized(TcpFileClientWrap client)
        {
            Console.WriteLine(client.UserId);
            FileContainer container = FileContainer.FromFile("testimage.png");
            FileClient.SendAsync(new FileMessage(container.FileData, 451));
        }

        private void FileClient_Connected(TcpFileClientWrap client)
        {
            Console.WriteLine("connected");
        }
    }
}
