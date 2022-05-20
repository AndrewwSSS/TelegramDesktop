using MessageLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace Telegram
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static TcpClientWrap Client { get;  set; }= new TcpClientWrap(IPAddress.Parse("26.87.230.148"), 5000);
    }
}
