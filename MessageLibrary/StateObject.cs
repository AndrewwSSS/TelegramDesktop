using System.Net.Sockets;

namespace MessageLibrary
{
    public class StateObject
    {
        public Socket Socket { get; set; } = null;
        public const int BufferSize = 4096;
        public byte[] Buffer { get; set; } = new byte[BufferSize];
    }
}
