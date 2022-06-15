using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace MessageLibrary
{
    [Serializable]
    public abstract class Message
    {
        public DateTime Time { get; protected set; } = DateTime.UtcNow;
        protected static BinaryFormatter bf = new BinaryFormatter();

    


        /// <summary>
        /// Возвращает байт-массив с сериализованным объектом. Первые четыре байте отведены под размер(для чтения на клиенте)
        /// </summary>
        /// <returns></returns>
        public virtual byte[] ToByteArray()
        {
            MemoryStream ms = new MemoryStream();

            bf.Serialize(ms, this);

            byte[] msBuf = ms.ToArray();

            // Message size in bytes
            int size = msBuf.Length;

            MemoryStream ms2 = new MemoryStream();

            byte[] sizeBytes = BitConverter.GetBytes(size);
            
            ms2.Write(sizeBytes, 0, sizeBytes.Length);
            ms2.Write(msBuf, 0, msBuf.Length);
            
            ms2.Position = 0;
            return ms2.ToArray();
        }

        public void SendTo(TcpClientWrap client)
        {
            byte[] buf = ToByteArray();
            client.Tcp.Client.Send(buf, 0, buf.Length, SocketFlags.None);
        }

        public void SendTo(Socket socket)
        {
            byte[] buf = ToByteArray();
            socket.Send(buf, 0, buf.Length, SocketFlags.None);
        }

        public void SendToAsync(TcpClientWrap client, AsyncCallback cb)
        {
            StateObject stateObject = new StateObject
            {
                Buffer = ToByteArray(),
                Socket = client.Tcp.Client
            };
            stateObject.Socket.BeginSend(stateObject.Buffer, 0, stateObject.Buffer.Length, SocketFlags.None, cb, stateObject);
        }

        public void SendToAsync(Socket socket, AsyncCallback cb)
        {
            StateObject stateObject = new StateObject
            {
                Buffer = ToByteArray(),
                Socket = socket 
            };
            socket.BeginSend(stateObject.Buffer, 0, stateObject.Buffer.Length, SocketFlags.None, cb, stateObject);
        }

        public void StreamTo(Stream stream) => bf.Serialize(stream, this);
 
        public static Message FromNetworkStream(NetworkStream stream) => bf.Deserialize(stream) as Message;

        public static T FromNetworkStream<T>(NetworkStream stream) where T : Message => bf.Deserialize(stream) as T;

        public static Message FromMemoryStream(MemoryStream ms) => bf.Deserialize(ms) as Message;

        public static Message FromByteArray(byte[] buffer) => bf.Deserialize(new MemoryStream(buffer)) as Message;
        

        public static T FromByteArray<T>(byte[] buffer) where T : Message => bf.Deserialize(new MemoryStream(buffer)) as T;
    }
}
