using CommonLibrary.Containers;
using CommonLibrary.Messages.Files;
using MessageLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
        TcpClient client;
        TcpListener other = new TcpListener(5000);
        public MainWindow()
        {
            other.Start();
            client = new TcpClient();
            var task = client.ConnectAsync("127.0.0.1", 5000);
            var client2 = other.AcceptTcpClient();
            task.Wait();
            using (var ns = client.GetStream())
                ns.Write(BitConverter.GetBytes(451), 0, 4);
            byte[] bytes = new byte[4];
            using (var ns = client2.GetStream())
                ns.Read(bytes, 0, 4);


        }
    }
}
