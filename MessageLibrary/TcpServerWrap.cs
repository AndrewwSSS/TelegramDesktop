using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
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

        public void Start(int port)
        {
            if (listener != null)
                return;

            listener = new TcpListener(new IPEndPoint(IPAddress.Any, port));
            listener.Start(10);
            Started?.Invoke(this);

            Task.Run(() => {
                do
                {
                    TcpClient client = listener.AcceptTcpClient();
                    ReceiveAsync(client);
                } while (true);
            });
        }
        public event ClientMessageHandler MessageReceived;
        private void Receive(TcpClient client)
        {
            TcpClientWrap user = new TcpClientWrap(client);
            ClientConnected?.Invoke(user);
            user.Disconnected += ClientDisconnected;
            user.MessageReceived += OnMessageReceived;
            do
            {
                user.Receive();
            } while (user.Tcp.Client.Available > 0);
        }
        private void ReceiveAsync(TcpClient client)
        {
            TcpClientWrap user = new TcpClientWrap(client);
            ClientConnected?.Invoke(user);
            user.Disconnected += ClientDisconnected;
            user.MessageReceived += OnMessageReceived;

            user.ReceiveAsync();
        }

        private void OnMessageReceived(TcpClientWrap client, Message msg)
        {
            MessageReceived?.Invoke(client, msg);
        }

        public void Shutdown()
        {
            Disconnected?.Invoke(this);
            listener?.Stop();
        }
    }
}
