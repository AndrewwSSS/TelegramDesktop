using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MessageLibrary
{

    public delegate void FileClientMessageEventHandler(TcpFileClientWrap client, string fileName, byte[] chunk, bool isLast);
    public delegate void FileClientEventHandler(TcpFileClientWrap client);

    public sealed class TcpFileClientWrap
    {
        private IPEndPoint endPoint;
        private TcpClient client;
        public TcpClient Tcp => client;
        public bool IsConnected => client == null ? false : client.Connected;

        public event FileClientEventHandler Connected;
        public event FileClientEventHandler Disconnected;
        public event FileClientEventHandler ConnectFailed;
        public event FileClientMessageEventHandler FileChunkReceived;
        public event Action<TcpFileClientWrap, FileMessage> MessageSent;



        public TcpFileClientWrap(IPAddress ip, int port)
        {
            if (ip == null)
                throw new ArgumentException("IP не может быть пустым");

            endPoint = new IPEndPoint(ip, port);
            client = null;
        }


        public TcpFileClientWrap(TcpClient tcpClient)
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
            client?.Client?.Close();
            client?.Close();
        }
        public void DisconnectAsync()
        {
            Task.Run(Disconnect);
        }
        public bool Send(FileMessage message)
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
        public bool SendAsync(Message message)
        {
            if (client != null && client.Connected)
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
            //MessageSent?.Invoke(this, Message.FromByteArray(obj));
        }

        private const int DEFAULT_BUFFER_SIZE = 4096;
        public void ReceiveAsync()
        {
            try
            {
                if (client != null && client.Client != null && client.Connected)
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            MemoryStream stream = new MemoryStream();

                            string fileName;
                            {
                                int fileNameLength;
                                byte[] lenBytes = new byte[4];

                                int sizeReceived = Tcp.Client.Receive(lenBytes, 4, SocketFlags.None);
                                if (sizeReceived == 0)
                                {
                                    Disconnect();
                                    return;
                                }
                                fileNameLength = BitConverter.ToInt32(lenBytes, 0);
                                byte[] stringBytes = new byte[fileNameLength];
                                Tcp.Client.Receive(stringBytes, fileNameLength, SocketFlags.None);
                                fileName = BitConverter.ToString(stringBytes);
                            }
                            

                            int bufSize = DEFAULT_BUFFER_SIZE;
                            int fileSize = 0;
                            {
                                byte[] bufSizeBytes = new byte[4];
                                Tcp.Client.Receive(bufSizeBytes, 4, SocketFlags.None);
                                fileSize = BitConverter.ToInt32(bufSizeBytes, 0);
                                if (fileSize < DEFAULT_BUFFER_SIZE)
                                    bufSize = fileSize;
                            }

                            byte[] buffer = new byte[bufSize];
                            Tcp.Client.Receive(buffer, bufSize, SocketFlags.None);
                            FileChunkReceived?.Invoke(this, fileName, buffer, client.Available == 0);
                            int remaining = fileSize - bufSize;
                            while (client.Available > 0 && remaining != 0)
                            {
                                int received = Tcp.Client.Receive(buffer, remaining < DEFAULT_BUFFER_SIZE ? remaining : DEFAULT_BUFFER_SIZE, SocketFlags.None);
                                remaining -= received;
                                FileChunkReceived?.Invoke(this, fileName, buffer, client.Available == 0);
                            }

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
