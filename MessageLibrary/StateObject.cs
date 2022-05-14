using System.Net.Sockets;

namespace MessageLibrary
{
    public class StateObject
    {
        public Socket Socket { get; set; }
        public const int ConstBufferSize = 4096;
        public int CurrentBufferSize => Buffer.Length;
        public byte[] Buffer { get; set; }

        public StateObject() {
            Buffer = new byte[ConstBufferSize];
        }


        public void SetBuffer(byte[] newBuffer)
        {
             Buffer = newBuffer;
        }

    }
}
