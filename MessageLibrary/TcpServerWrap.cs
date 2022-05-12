using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MessageLibrary
{
    public delegate void ClientMessageHandler(TcpClientWrap client, Message msg);
    public delegate void ServerHandler(TcpServerWrap server);
    public delegate void ClientHandler(TcpClientWrap client);

    public class TcpServerWrap
    {
        public event ServerHandler Started;
        public event ClientHandler ClientConnected;
        public event ClientHandler ClientDisconnected;
        public event ServerHandler Disconnected;
        private TcpListener listener;
        private Thread ListenerThread;
        public event ClientMessageHandler MessageReceived;


        public void Start(int port, int backlog)
        {
            if (listener != null)
                return;

            if (port < 0 || port > ushort.MaxValue || backlog <= 0)
                throw new ArgumentOutOfRangeException("Invalid parameter");

    


            listener = new TcpListener(new IPEndPoint(IPAddress.Any, port));
            listener.Start(backlog);
            Started?.Invoke(this);


            ListenerThread = new Thread(() => {
                do
                {
                    TcpClientWrap client = new TcpClientWrap(listener.AcceptTcpClient());
                    ClientConnected?.Invoke(client);
                    ReceiveAsync(client);
                } while (true);
            });

            ListenerThread.IsBackground = true;
            ListenerThread.Start();
        }

        private void Receive(TcpClientWrap client)
        {

            client.Disconnected += ClientDisconnected;
            client.MessageReceived += OnMessageReceived;
            do
            {
                client.Receive();
            } while (client.Tcp.Client.Available > 0);
        }

        private void ReceiveAsync(TcpClientWrap client)
        {
            
            client.Disconnected += ClientDisconnected;
            client.MessageReceived += OnMessageReceived;

            client.ReceiveAsync();
        }

        private void OnMessageReceived(TcpClientWrap client, Message msg)
        {
            MessageReceived?.Invoke(client, msg);
        }

        public void Shutdown()
        {

            if(listener != null)
            {
                listener?.Stop();
                ListenerThread.Abort();
                Disconnected?.Invoke(this);
            }            
        }
    }
}
