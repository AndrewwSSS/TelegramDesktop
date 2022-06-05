using MessageLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Messages.Files
{

    public delegate void FileClientMessageEventHandler(TcpFileClientWrap client, int id, byte[] chunk, bool isLast);
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
        public event FileClientMessageEventHandler ImageChunkReceived;
        public event Action<TcpFileClientWrap, FileMessage> MessageSent;
        public event FileClientEventHandler UserSynchronized;
        public bool RequiresSync { get; set; } = false;
        public TcpFileClientWrap(IPAddress ip, int port, int userId = -1, string guid = null)
        {
            UserId = userId;
            Guid = guid;
            if (ip == null)
                throw new ArgumentException("IP не может быть пустым");

            endPoint = new IPEndPoint(ip, port);
            client = null;
        }


        public TcpFileClientWrap(TcpClient tcpClient, int userId = -1, string guid = null)
        {
            UserId = userId;
            Guid = guid;
            if (tcpClient == null)
                throw new ArgumentException("Подключение не может быть пустым");

            endPoint = tcpClient.Client.RemoteEndPoint as IPEndPoint;
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
                SendUserInfoAsync();
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
        /// <summary>
        /// Для синхронизации
        /// </summary>
        /// <param name="id">ID пользователя</param>
        private bool SendUserInfoAsync()
        {
            if (client != null && client.Connected)
            {
                StateObject state = new StateObject()
                {
                    Socket = Tcp.Client
                };
                MemoryStream ms = new MemoryStream();
                byte[] idBytes = BitConverter.GetBytes(UserId);
                byte[] guidBytes = Encoding.UTF8.GetBytes(Guid);
                byte[] guidLengthBytes = BitConverter.GetBytes(guidBytes.Length);
                ms.Write(idBytes, 0, idBytes.Length);
                ms.Write(guidLengthBytes, 0, guidLengthBytes.Length);
                ms.Write(guidBytes, 0, guidBytes.Length);

                byte[] buffer = ms.ToArray();
                state.SetBuffer(buffer, buffer.Length);
                Tcp.Client.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, SendCB, state);
                return true;
            }
            return false;
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
        public bool SendAsync(FileMessage message)
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

        }
        public int UserId { get; private set; } = -1;
        public string Guid { get; private set; }
        public bool Synchronized { get; private set; } = false;
        private const int DEFAULT_BUFFER_SIZE = 4096;
        public void Receive()
        {
            try
            {
                MemoryStream stream = new MemoryStream();

                // ID файла в системе Telegram
                int fileId = -1;
                // Является ли файл изображением
                bool isImage = false;
                {
                    byte[] idBytes = new byte[4];

                    int sizeReceived = Tcp.Client.Receive(idBytes, 4, SocketFlags.None);
                    if (sizeReceived == 0)
                    {
                        DisconnectAsync();
                        return;
                    }
                    if (RequiresSync && !Synchronized)
                    {
                        UserId = BitConverter.ToInt32(idBytes, 0);
                        Synchronized = true;
                        // Получаем длинну ГУИДа
                        int guidLength = 0;
                        {
                            byte[] guidLengthBytes = new byte[4];
                            Tcp.Client.Receive(guidLengthBytes, 4, SocketFlags.None);
                            guidLength = BitConverter.ToInt32(guidLengthBytes, 0);
                        }
                        byte[] guidBytes = new byte[guidLength];
                        Tcp.Client.Receive(guidBytes, guidLength, SocketFlags.None);
                        Guid = Encoding.UTF8.GetString(guidBytes);
                        UserSynchronized?.Invoke(this);
                        ReceiveAsync();
                        return;
                    }
                    else
                    {
                        fileId = BitConverter.ToInt32(idBytes, 0);
                        byte[] isImageBytes = new byte[1];
                        Tcp.Client.Receive(isImageBytes, 1, SocketFlags.None);
                        isImage = BitConverter.ToBoolean(isImageBytes, 0);
                        Console.WriteLine(fileId + ": " + isImage);
                    }
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

                if (isImage)
                    ImageChunkReceived?.Invoke(this, fileId, buffer, client.Available == 0);
                else
                    FileChunkReceived?.Invoke(this, fileId, buffer, client.Available == 0);

                int remaining = fileSize - bufSize;
                while (client.Available > 0 && remaining != 0)
                {
                    int received = Tcp.Client.Receive(buffer, remaining < DEFAULT_BUFFER_SIZE ? remaining : DEFAULT_BUFFER_SIZE, SocketFlags.None);
                    remaining -= received;

                    if (isImage)
                        ImageChunkReceived?.Invoke(this, fileId, buffer, client.Available == 0);
                    else
                        FileChunkReceived?.Invoke(this, fileId, buffer, client.Available == 0);
                }

                ReceiveAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Inner TcpClientWrap.ReceiveAsync() exception: {ex.Message}");
                DisconnectAsync();
                return;
            }
        }
        public void ReceiveAsync()
        {
            try
            {
                if (client != null && client.Client != null && client.Connected)
                    Task.Run(Receive);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Out TcpClientWrap.ReceiveAsync() exception: {ex.Message}");
                Disconnected?.Invoke(this);
            }
        }


    }
}
