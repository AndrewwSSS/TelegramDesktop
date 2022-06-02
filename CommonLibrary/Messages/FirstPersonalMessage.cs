using MessageLibrary;
using System;

namespace CommonLibrary.Messages
{
    [Serializable]
    public class FirstPersonalMessage : Message
    {
        public ChatMessage Message { get; set; }
        public int ToUserId { get; set; }
        public int LocalId { get; set; }

        public FirstPersonalMessage(ChatMessage message, int toUserId, int localId)
        {
            Message = message;
            ToUserId = toUserId;
            LocalId = localId;
        }
    }
}
