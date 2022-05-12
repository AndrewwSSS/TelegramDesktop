using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace MessageLibrary
{
    [Serializable]
    public abstract class Message
    {
        public MessageType Type { get; protected set; } = MessageType.Undefined;
        public DateTime Time { get; protected set; }

        private static BinaryFormatter bf = new BinaryFormatter();
        public byte[] ToByteArray()
        {
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, this);
            ms.Position = 0;
            return ms.ToArray();
        }
        public void SendToAsync(Socket socket, AsyncCallback cb)
        {
            StateObject stateObject = new StateObject();
            stateObject.Buffer = ToByteArray();
            stateObject.Socket = socket;
            socket.BeginSend(stateObject.Buffer, 0, stateObject.Buffer.Length, SocketFlags.None, cb, stateObject);
        }
        public void StreamTo(Stream stream)
        {
            bf.Serialize(stream, this);
        }

        public static void ReceiveFromSocket(Socket socket, AsyncCallback cb)
        {
            StateObject stateObject = new StateObject();
            stateObject.Buffer = new byte[8192];
            stateObject.Socket = socket;
            socket.BeginReceive(stateObject.Buffer, 0, StateObject.BufferSize, SocketFlags.None, cb, stateObject);
        }

        public static Message FromNetworkStream(NetworkStream stream) => bf.Deserialize(stream) as Message;

        public static T FromNetworkStream<T>(NetworkStream stream) where T : Message => bf.Deserialize(stream) as T;

        public static Message FromMemoryStream(MemoryStream ms) => bf.Deserialize(ms) as Message;

        public static Message FromByteArray(byte[] buffer)
        {
            MemoryStream ms = new MemoryStream(buffer);
            return bf.Deserialize(ms) as Message;
        }
        public static T FromByteArray<T>(byte[] buffer) where T : Message
        {
            MemoryStream ms = new MemoryStream(buffer);
            return bf.Deserialize(ms) as T;
        }
    }
}
