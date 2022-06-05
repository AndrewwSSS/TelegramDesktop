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
    public delegate void FileServerEventHandler(TcpFileServerWrap server);
    public class TcpFileServerWrap
    {
        public event FileServerEventHandler Started;
        public event FileClientEventHandler ClientConnected;
        public event FileClientEventHandler ClientDisconnected;
        public event FileServerEventHandler Stopped;
        public event FileClientEventHandler UserSynchronized;
        private TcpListener listener;
        private Thread ListenerThread;
        public event FileClientMessageEventHandler FileChunkReceived;
        

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
                    TcpFileClientWrap client;
                    try { client = new TcpFileClientWrap(listener.AcceptTcpClient()); }
                    catch (Exception) { return; }
                    ClientConnected?.Invoke(client);
                    ReceiveAsync(client);
                } while (true);
            });

            ListenerThread.IsBackground = true;
            ListenerThread.Start();
        }
        private void ReceiveAsync(TcpFileClientWrap client)
        {
            client.Disconnected += ClientDisconnected;
            client.FileChunkReceived += FileChunkReceived;
            client.UserSynchronized += UserSynchronized;

            client.ReceiveAsync();
        }
        public void Shutdown()
        {
            if (listener != null)
            {
                listener?.Stop();
                ListenerThread.Abort();

                listener = null;
                Stopped?.Invoke(this);
            }
        }
    }
}

