using MessageLibrary;
using System;

namespace CommonLibrary.Messages.Groups
{
    [Serializable]
    public class ChatMessageSendResult : Message
    {
        public int LocalMessageId { get; set; }
        public int DbMessageId { get; set; }

        public ChatMessageSendResult(int localId, int dbId)
        {
            LocalMessageId = localId;
            DbMessageId = dbId;
        }
    }
}
