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
    public class TcpClientWrap
    {


        private IPEndPoint endPoint;
        private TcpClient client;
        public TcpClient Tcp => client;
        public bool IsConnected => client.Connected;

        public event ClientHandler Connected;
        public event ClientHandler Disconnected;

        public event ClientMessageHandler MessageReceived;
        public event ClientMessageHandler MessageSent;

        public TcpClientWrap(IPAddress ip, int port)
        {
            if (ip == null)
                throw new ArgumentException("IP не может быть пустым");

            endPoint = new IPEndPoint(ip, port);
            client = null;
        }

        public void SendAsync(object gameInfoMessage)
        {
            throw new NotImplementedException();
        }

        public TcpClientWrap(TcpClient tcpClient)
        {
            if (tcpClient == null)
                throw new ArgumentException("Подключение не может быть пустым");

            client = tcpClient;

        }

        public bool Connect()
        {
            if (client != null)
                return client.Connected;
            try
            {
                client = new TcpClient();
                client.Connect(endPoint);
            }
            catch (Exception)
            {
                ConnectFailed?.Invoke(this);
                return false;
            }
            if (client.Connected)
                Connected?.Invoke(this);
            
            return client.Connected;
        }
        /// <summary>
        /// Асинхронно подключает клиент к адресу
        /// </summary>
        /// <returns>Была ли начата операция подключения</returns>
        public bool ConnectAsync()
        {
            if (client != null)
                return false;
            try
            {
                client = new TcpClient();
                client.BeginConnect(endPoint.Address, endPoint.Port, ConnectCB, client);
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
            if(client.Connected)
                Connected?.Invoke(this);
        }

        public void Disconnect()
        {
            Disconnected?.Invoke(this);
            client?.Close();
        }
        public void DisconnectAsync()
        {
            Disconnected?.Invoke(this);
            Task.Run(() => client?.Close());
        }
        public bool Send(Message message)
        {
            if (client != null && client.Connected)
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
        public bool SendAsync(Message message) {
            if (client != null && client.Connected)
            {
                message.SendToAsync(Tcp.Client, SendCB);
                return true;
            }
            return false;
        }
        private void SendCB(IAsyncResult ar) {
            StateObject state = (StateObject)ar.AsyncState;
            state.Socket.EndSend(ar);

            MessageSent?.Invoke(this, Message.FromByteArray(state.Buffer));
        }

        public Message Receive()
        {
            if (client != null && client.Connected)
            {
                var message = Message.FromNetworkStream(client.GetStream());
                MessageReceived?.Invoke(this, message);
                return message ;
            }
            return null;
        }
        public void ReceiveAsync()
        {
            if (client != null && client.Connected)
            {
                Task.Run(() =>
                {
                    MemoryStream stream = new MemoryStream();
                    byte[] buffer = new byte[4096];
                    {
                        int firstReceive = Tcp.Client.Receive(buffer);
                        stream.Write(buffer, 4, firstReceive);
                    }
                    int objLen = 0;
                    int remaining;
                    
                    byte[] lenBytes = new List<byte>(buffer).GetRange(0, 4).ToArray();
                    remaining = objLen = BitConverter.ToInt32(lenBytes, 0);
                    while (client.Available > 0 && remaining != 0)
                    {
                        int received = Tcp.Client.Receive(buffer, remaining < 4096 ? remaining : 4096, SocketFlags.None);
                        remaining -= received;
                        stream.Write(buffer, 0, received);
                    }
                    Message msg = Message.FromMemoryStream(stream);
                    MessageReceived?.Invoke(this, msg);

                    if (client.Available > 0)
                        ReceiveAsync();
                });
            }
        }


    }
}
