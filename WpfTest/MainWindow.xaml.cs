using CommonLibrary.Containers;
using CommonLibrary.Messages.Files;
using MessageLibrary;
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
        private TcpFileClientWrap FileClient { get; set; } = new TcpFileClientWrap(IPAddress.Parse("26.87.230.148"), 5001, 228, "guid!");
        
        private FileStream writer = new FileStream("receivedimage.jpg", FileMode.OpenOrCreate);
        public MainWindow()
        {
            InitializeComponent();

            //FileServer.Started += FileServer_Started;
            //FileServer.UserSynchronized += UserSynchronized;
            //FileServer.FileChunkReceived += FileChunkReceived;
            FileClient.ImageChunkReceived += FileServer_ImageChunkReceived;
            FileClient.FileChunkReceived += FileChunkReceived;

            FileClient.Connected += FileClient_Connected;
            FileClient.ConnectFailed += FileClient_ConnectFailed;
            FileClient.ConnectAsync();
        }

        private void FileClient_ConnectFailed(TcpFileClientWrap client)
        {
            Console.WriteLine("connect failed!");
        }

        private void FileServer_ImageChunkReceived(TcpFileClientWrap client, int id, byte[] chunk, bool isLast)
        {
            Console.WriteLine("Image chunk: " + chunk.Length);
            writer.Write(chunk, 0, chunk.Length);
            if (isLast)
                writer.Close();
        }

        private void FileChunkReceived(TcpFileClientWrap client, int fileId, byte[] chunk, bool isLast)
        {
            Console.WriteLine("File chunk: " + chunk.Length);
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
            ImageContainer container = ImageContainer.FromFile("testimage.png");
            FileClient.SendAsync(new FileMessage(container.ImageData, 451));
        }

        private void FileClient_Connected(TcpFileClientWrap client)
        {
            Console.WriteLine("connected");
            FileClient.ReceiveAsync();
        }
    }
}
