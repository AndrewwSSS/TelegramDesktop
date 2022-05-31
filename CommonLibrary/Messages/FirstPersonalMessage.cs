using MessageLibrary;
using System;

namespace CommonLibrary.Messages
{
    [Serializable]
    public class FirstPersonalMessage : Message
    {
        public string Guid { get; set; }
        public ChatMessage Message { get; set; }
        public int ToUserId { get; set; }

        public FirstPersonalMessage(ChatMessage message, string guid, int toUserId)
        {
            Message = message;
            Guid = guid;
            ToUserId = toUserId;
        }
    }
}
