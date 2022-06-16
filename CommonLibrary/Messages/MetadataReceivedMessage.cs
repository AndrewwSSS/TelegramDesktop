using MessageLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Messages
{
    [Serializable]
    public class MetadataReceivedMessage : Message
    {
        public int LocalId { get; set; }

        public MetadataReceivedMessage(int localId)
        {
            LocalId = localId;
        }

        public MetadataReceivedMessage() { }
    }
}
