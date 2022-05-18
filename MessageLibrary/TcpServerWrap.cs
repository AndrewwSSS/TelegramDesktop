using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

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
        public event ServerHandler Stopped;
        private TcpListener listener;
        private Thread ListenerThread;
        public event ClientMessageHandler MessageReceived;
        public event ClientMessageHandler MessageSent;

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
                    TcpClientWrap client;
                    try { client = new TcpClientWrap(listener.AcceptTcpClient()); }
                    catch(Exception) { return; }
                    
                    ClientConnected?.Invoke(client);
                    ReceiveAsync(client);
                } while (true);
            });

            ListenerThread.IsBackground = true;
            ListenerThread.Start();
        }

        private void Receive(TcpClientWrap client)
        {
         
            ClientConnected?.Invoke(client);
            client.Disconnected += ClientDisconnected;
            client.MessageReceived += OnMessageReceived;
            client.MessageSent += MessageSent;
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

        private void OnMessageReceived(TcpClientWrap client, Message msg){
            MessageReceived?.Invoke(client, msg);
        }

        public void Shutdown()
        {
            if(listener != null)
            {
                listener?.Stop();
                ListenerThread.Abort();
              
                listener = null;
                Stopped?.Invoke(this);
            }            
        }
    }
}
