using System.Net.Sockets;

namespace MessageLibrary
{
    public class StateObject
    {
        public Socket Socket { get; set; }
        public const int DefaultBufferSize = 4096;
        public int CurrentBufferSize => Buffer.Length;
        public byte[] Buffer { get; set; }

        public StateObject() {
            Buffer = new byte[DefaultBufferSize];
        }


        public void SetBuffer(byte[] newBuffer) => Buffer = newBuffer;

    }
}
