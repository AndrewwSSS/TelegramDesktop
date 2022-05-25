using System.Net.Sockets;

namespace MessageLibrary
{
    public class StateObject
    {
        public Socket Socket { get; set; }
        public const int DefaultBufferSize = 4096;
        public int CurrentBufferSize { get; set; } = 4096;
        public byte[] Buffer { get; set; }

        public StateObject() {
            Buffer = new byte[DefaultBufferSize];
        }


        public bool SetBuffer(byte[] newBuffer, int size)
        {
            if (size > 0 && newBuffer != null)
                return false;

            Buffer = newBuffer;
            CurrentBufferSize = size;

            return true;
        }

    }
}
