using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
namespace MessageLibrary
{
    public sealed class TcpClientWrap
    {


        private IPEndPoint endPoint;
        private TcpClient client;

        private TcpClient Client {
            get => client;
            set => client = value;
        }
        public TcpClient Tcp => Client;
        public bool IsConnected => Client == null ? false : Client.Connected;

        public event ClientEventHandler Connected;
        public event ClientEventHandler Disconnected;

        public event ClientMessageEventHandler MessageReceived;
        public event ClientMessageEventHandler MessageSent;

        public TcpClientWrap(IPAddress ip, int port)
        {
            if (ip == null)
                throw new ArgumentException("IP не может быть пустым");

            endPoint = new IPEndPoint(ip, port);
            Client = null;
        }


        public TcpClientWrap(TcpClient tcpClient)
        {
            if (tcpClient == null)
                throw new ArgumentException("Подключение не может быть пустым");

            Client = tcpClient;
        }

        public bool Connect()
        {
            if (Client != null)
                return Client.Connected;
            try
            {
                Client = new TcpClient();
                Client.Connect(endPoint);
            }
            catch (Exception)
            {
                ConnectFailed?.Invoke(this);
                return false;
            }
            if (Client.Connected)
                Connected?.Invoke(this);

            return Client.Connected;
        }
        /// <summary>
        /// Асинхронно подключает клиент к адресу
        /// </summary>
        /// <returns>Была ли начата операция подключения</returns>
        public bool ConnectAsync()
        {
            if (Client != null)
                return false;
            try
            {
                Client = new TcpClient();
                Client.BeginConnect(endPoint.Address, endPoint.Port, ConnectCB, Client);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        public event Action<TcpClientWrap> ConnectFailed;
        private void ConnectCB(IAsyncResult ar)
        {
            TcpClient client = ar.AsyncState as TcpClient;
            try
            {
                client.EndConnect(ar);
            }
            catch (Exception)
            {
                ConnectFailed?.Invoke(this);
            }
            if (client.Connected)
            {
                Connected?.Invoke(this);
                ReceiveAsync();
            }
        }

        public void Disconnect()
        {
            Disconnected?.Invoke(this);
            Client?.Client?.Close();
            Client?.Close();
        }
        public void DisconnectAsync()
        {
            Task.Run(Disconnect);
        }
        public bool Send(Message message)
        {
            if (Client != null && Client.Connected)
            {
                Tcp.Client.Send(message.ToByteArray());
                MessageSent?.Invoke(this, message);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Асинхронно отправляет сообщение клиента
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <returns>Была ли начата операция отправки</returns>
        public bool SendAsync(Message message)
        {
            if (Client != null && Client.Connected)
            {
                message.SendToAsync(Tcp.Client, SendCB);
                return true;
            }
            return false;
        }
        private void SendCB(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            state.Socket.EndSend(ar);

            byte[] obj = state.Buffer.ToList().GetRange(4, state.Buffer.Length - 4).ToArray();
            MessageSent?.Invoke(this, Message.FromByteArray(obj));
        }

        public Message Receive()
        {
            if (Client != null && Client.Connected)
            {
                var message = Message.FromNetworkStream(Client.GetStream());
                MessageReceived?.Invoke(this, message);
                return message;
            }
            return null;
        }
        public const int DEFAULT_BUFFER_SIZE = 4096;
        public void ReceiveAsync()
        {
            try
            {
                if (Client != null && Client.Client != null && Client.Connected)
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            MemoryStream stream = new MemoryStream();

                            int objectSize;
                            {
                                byte[] lenBytes = new byte[4];

                                int sizeReceived = Tcp.Client.Receive(lenBytes, 4, SocketFlags.None);
                                if (sizeReceived == 0)
                                {
                                    Disconnect();
                                    return;
                                }
                                objectSize = BitConverter.ToInt32(lenBytes, 0);
                            }

                            int bufSize = objectSize < DEFAULT_BUFFER_SIZE ? objectSize : DEFAULT_BUFFER_SIZE;
                            byte[] buffer = new byte[bufSize];
                            int remaining = objectSize;
                            while (Client.Available > 0 && remaining != 0)
                            {
                                int received = Tcp.Client.Receive(buffer, remaining < DEFAULT_BUFFER_SIZE ? remaining : DEFAULT_BUFFER_SIZE, SocketFlags.None);
                                remaining -= received;
                                stream.Write(buffer, 0, received);
                            }
                            stream.Position = 0;
                            Message msg = Message.FromMemoryStream(stream);

                            MessageReceived?.Invoke(this, msg);


                            ReceiveAsync();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Inner TcpClientWrap.ReceiveAsync() exception: {ex.Message}");
                            DisconnectAsync();
                            return;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Out TcpClientWrap.ReceiveAsync() exception: {ex.Message}");
                Disconnected?.Invoke(this);
            }
        }


    }
}
