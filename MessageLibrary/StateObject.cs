using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MessageLibrary
{
    public class StateObject
    {
        public Socket Socket { get; set; } = null;
        public const int BufferSize = 4096;
        public byte[] Buffer { get; set; } = new byte[BufferSize];
    }
}
