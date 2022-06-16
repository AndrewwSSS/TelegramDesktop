using MessageLibrary;
using System;

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
