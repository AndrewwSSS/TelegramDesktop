using CommonLibrary.Containers;
using MessageLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommonLibrary.Messages.Files
{

    public delegate void FileClientMessageEventHandler(TcpFileClientWrap client, FileChunk chunk);
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
        public event FileClientMessageEventHandler FileChunkSent;
        public event FileClientMessageEventHandler ImageChunkSent;
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


        private static object SendLocker = new object();
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
        public void DisconnectAsync() => Task.Run(Disconnect);
        /// <summary>
        /// Для синхронизации
        /// </summary>
        /// <param name="id">ID пользователя</param>
        private bool SendUserInfoAsync()
        {
            if (client != null && client.Connected)
            {
                using (var ns = Tcp.GetStream())
                {
                    byte[] idBytes = BitConverter.GetBytes(UserId);
                    byte[] guidBytes = Encoding.UTF8.GetBytes(Guid);
                    byte[] guidLengthBytes = BitConverter.GetBytes(guidBytes.Length);
                    ns.Write(idBytes, 0, idBytes.Length);
                    ns.Write(guidLengthBytes, 0, guidLengthBytes.Length);
                    ns.Write(guidBytes, 0, guidBytes.Length);
                }
                return true;
            }
            return false;
        }

        private static List<string> FilesInProcessing { get; set; } = new List<string>();

        public bool SendFileAsync(string path, int localId, bool isImage = false)
        {
            if (client != null && client.Connected && !FilesInProcessing.Contains(path))
            {
                Task.Run(() =>
                {
                    long fileSize = new FileInfo(path).Length;
                    using (FileStream reader = new FileStream(path, FileMode.Open))
                    using (var ns = Tcp.GetStream())
                    {
                        FilesInProcessing.Add(path);
                        ns.Write(BitConverter.GetBytes(localId), 0, 4);
                        ns.Write(BitConverter.GetBytes(isImage), 0, 1);
                        var kek = (int)fileSize;
                        ns.Write(BitConverter.GetBytes((int)fileSize), 0, 4);
                        reader.CopyTo(ns);
                        FilesInProcessing.Remove(path);
                    }

                });
                return true;
            }
            return false;
        }

        /// <summary>
        /// Асинхронно отправляет файл(метод для клиента)
        /// </summary>
        /// <param name="path">Сообщение</param>
        /// <returns>Была ли начата операция отправки</returns>
        //private bool SendFileAsync(string path, int localId, bool isImage = false)
        //{
        //    if (client != null && client.Connected && !FilesInProcessing.Contains(path))
        //    {
        //        Task.Run(() =>
        //        {
        //            long remaining = new FileInfo(path).Length;
        //            using (FileStream reader = new FileStream(path, FileMode.Open))
        //            {
        //                FilesInProcessing.Add(path);
        //                int bufSize = remaining < DEFAULT_BUFFER_SIZE ? (int)remaining : DEFAULT_BUFFER_SIZE;
        //                byte[] data = new byte[bufSize];
        //                int bytesRead = 0;
        //                for (int chunkOrder = 0; remaining != 0; chunkOrder++)
        //                {
        //                    bytesRead = reader.Read(data, 0, bufSize);
        //                    remaining -= bytesRead;
        //                    if (bytesRead == 0)
        //                        continue;

        //                    Stream ns = Tcp.GetStream();
        //                    lock (SendLocker)
        //                    {
        //                        // Посылаемые байты оформлены следующим образом:
        //                        // Локальный ID, bool если изображение, bool если кусок последний,
        //                        // номер куска, размер содержимого, содержимое файла
        //                        ns.Write(BitConverter.GetBytes(localId), 0, 4);
        //                        ns.Write(BitConverter.GetBytes(isImage), 0, 1);
        //                        ns.Write(BitConverter.GetBytes(remaining == 0), 0, 1);
        //                        ns.Write(BitConverter.GetBytes(chunkOrder), 0, 4);
        //                        ns.Write(BitConverter.GetBytes(bufSize), 0, 4);
        //                        ns.Write(data, 0, bufSize);
        //                    };
        //                }
        //                FilesInProcessing.Remove(path);
        //            }
        //        });
        //        return true;
        //    }
        //    return false;
        //}
        /// <summary>
        /// Асинхронно отправляет файл(метод для сервера)
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public bool SendFileAsync(FileContainer file)
        {
            if (client != null && client.Connected)
            {
                Task.Run(() =>
                {
                    var data = file.FileData.Bytes;

                    using (var ns = Tcp.GetStream())
                    {
                        lock (SendLocker)
                        {
                            // Посылаемые байты оформлены следующим образом:
                            // Локальный ID, bool если изображение, bool если кусок последний, 
                            // номер куска, размер содержимого, содержимое файла
                            ns.Write(BitConverter.GetBytes(file.Id), 0, 4);
                            ns.Write(BitConverter.GetBytes(false), 0, 1);
                            ns.Write(BitConverter.GetBytes(data.Length), 0, 4);
                            ns.Write(data, 0, data.Length);
                        }
                    }
                });
                return true;
            }
            return false;

        }
        public bool SendImageAsync(ImageContainer img)
        {
            if (client != null && client.Connected)
            {
                Task.Run(() =>
                {
                    var data = img.ImageData.Bytes;

                    using (var ns = Tcp.GetStream())
                    {
                        lock (SendLocker)
                        {
                            // Посылаемые байты оформлены следующим образом:
                            // Локальный ID, bool если изображение, bool если кусок последний, 
                            // номер куска, размер содержимого, содержимое файла
                            ns.Write(BitConverter.GetBytes(img.Id), 0, 4);
                            ns.Write(BitConverter.GetBytes(true), 0, 1);
                            ns.Write(BitConverter.GetBytes(data.Length), 0, 4);
                            ns.Write(data, 0, data.Length);
                        }
                    }
                });
                return true;
            }
            return false;
        }
        public int UserId { get; private set; } = -1;
        public string Guid { get; private set; }
        public bool Synchronized { get; private set; } = false;
        private const int DEFAULT_BUFFER_SIZE = 4096;

        public void Receive()
        {
            try
            {
                NetworkStream ns = Tcp.GetStream();
                MemoryStream stream = new MemoryStream();

                FileChunk chunk = new FileChunk();
                int remaining = 0;
                {
                    byte[] intBytes = new byte[4];

                    int sizeReceived = ns.Read(intBytes, 0, 4);
                    if (sizeReceived == 0)
                    {
                        DisconnectAsync();
                        return;
                    }
                    if (RequiresSync && !Synchronized)
                    {
                        UserId = BitConverter.ToInt32(intBytes, 0);
                        Synchronized = true;
                        // Получаем длинну ГУИДа
                        int guidLength = 0;
                        {
                            byte[] guidLengthBytes = new byte[4];
                            ns.Read(guidLengthBytes, 0, 4);
                            guidLength = BitConverter.ToInt32(guidLengthBytes, 0);
                        }
                        byte[] guidBytes = new byte[guidLength];
                        ns.Read(guidBytes, 0, guidLength);
                        Guid = Encoding.UTF8.GetString(guidBytes);
                        UserSynchronized?.Invoke(this);
                        ReceiveAsync();
                        return;
                    }
                    else
                    {
                        chunk.FileId = BitConverter.ToInt32(intBytes, 0);
                        byte[] boolBytes = new byte[1];
                        ns.Read(boolBytes, 0, 1);
                        chunk.IsImage = BitConverter.ToBoolean(boolBytes, 0);

                        ns.Read(intBytes, 0, 4);
                        remaining = BitConverter.ToInt32(intBytes, 0);
                    }
                }

                for (int chunkOrder = 0; remaining > 0; chunkOrder++)
                {
                    chunk.Data = new byte[DEFAULT_BUFFER_SIZE > remaining ? remaining : DEFAULT_BUFFER_SIZE];

                    int received = ns.Read(chunk.Data, 0, chunk.Data.Length);
                    remaining -= received;
                    chunk.IsLast = remaining == 0;
                    Console.WriteLine($"{(chunk.IsImage ? "Image" : "File")} #{chunk.FileId} Chunk #{chunkOrder}{(chunk.IsLast ? " LAST" : "")}");
                    if (chunk.IsImage)
                        ImageChunkReceived?.Invoke(this, chunk);
                    else
                        FileChunkReceived?.Invoke(this, chunk);
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
                Console.WriteLine($"Outer TcpClientWrap.ReceiveAsync() exception: {ex.Message}");
                Disconnected?.Invoke(this);
            }
        }


    }
}
