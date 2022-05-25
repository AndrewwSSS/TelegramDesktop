using System;
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
        public bool ReceiveAsync()
        {
            if (client != null && client.Connected)
            {
                Message.ReceiveFromSocket(Tcp.Client, ReceiveCB);
                return true;
            }
            return false;
        }

        private void ReceiveCB(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket socket = state.Socket;
          

            try
            {
                if (Tcp == null|| Tcp.Client==null)
                    return;
                
                int bytesRead = Tcp.Client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    Task.Run(() =>
                    {

                        MemoryStream ms = new MemoryStream();

                        byte[] tmp = new byte[StateObject.DefaultBufferSize];
                        ms.Write(state.Buffer, 0, StateObject.DefaultBufferSize);

                        while (socket.Available > 0)
                        {
                            int br = socket.Receive(tmp, StateObject.DefaultBufferSize, SocketFlags.None);

                            if (br == 0)
                                continue;

                            ms.Write(tmp, 0, br);

                        };

                        byte[] newBuffer = ms.ToArray();

                        Message msg = Message.FromByteArray(state.Buffer);

                        //For DEBUG
                        Console.WriteLine("Message received. Type of message: " + msg.GetType().Name);

                        MessageReceived?.Invoke(this, msg);
                    });
                }
                socket.BeginReceive(state.Buffer, 0, state.CurrentBufferSize, SocketFlags.None, ReceiveCB, state);
            }
            catch (Exception e)
            {
                Disconnected?.Invoke(this);
                Console.WriteLine(e.Message);
                Console.WriteLine("TcpClientWrap.Receive: SOCKET EXCEPTION");
                return;
            }
        }
    }
}
