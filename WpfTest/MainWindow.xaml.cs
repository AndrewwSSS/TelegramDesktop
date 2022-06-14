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
        private TcpFileClientWrap FileClient { get; set; } = new TcpFileClientWrap(IPAddress.Parse("127.0.0.1"), 5001, 228, "guid!");
        private TcpFileServerWrap FileServer { get; set; } = new TcpFileServerWrap();
        private FileStream writer = new FileStream("receivedimage.png", FileMode.OpenOrCreate);
        public MainWindow()
        {
            InitializeComponent();

            FileServer.Started += FileServer_Started;
            FileServer.ImageChunkReceived += FileServer_ImageChunkReceived;
            FileClient.ImageChunkReceived += FileServer_ImageChunkReceived;
            FileServer.UserSynchronized += UserSync;
            FileClient.Connected += FileClient_Connected;
            
            FileServer.Start(5001, 1);
            
        }

        private void UserSync(TcpFileClientWrap client)
        {
            //ImageContainer file = ImageContainer.FromFile("testimage.png");
            //file.Id = 451;
            FileClient.SendFileAsync("testimage.png", -1, true);
            //FileClient.SendImageAsync(file);
        }

        private void FileServer_ImageChunkReceived(TcpFileClientWrap client, FileChunk chunk)
        {
            writer.Write(chunk.Data, 0, chunk.Data.Length);
            if (chunk.IsLast)
                writer.Close();
        }

        private void FileServer_Started(TcpFileServerWrap server)
        {
            FileClient.ConnectAsync();
        }

        private void FileClient_Connected(TcpFileClientWrap client)
        {
        }
    }
}
