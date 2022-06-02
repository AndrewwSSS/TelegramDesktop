using MessageLibrary;
using System;


namespace CommonLibrary.Messages.Groups
{
    [Serializable]
    public class MessageToGroupMessage : Message
    {
        public int LocalMessageId { get; set; }
        public ChatMessage Message { get; set; }


        public MessageToGroupMessage(ChatMessage message, int localId)
        {
            LocalMessageId = localId;
            Message = message;
        }
    }
}
