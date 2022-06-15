using MessageLibrary;
using System;
using System.Collections.Generic;

namespace CommonLibrary.Messages.Groups
{
    [Serializable]
    public class ChatMessageSendResult : Message
    {
        public int LocalId { get; set; }
        public int MessageId { get; set; }

        public ChatMessageSendResult(int localId, int dbId) 
        {
            LocalId = localId;
            MessageId = dbId;
        }

        public ChatMessageSendResult()
        {

        }

    }
}
