using System;

namespace CommonLibrary.Messages
{
    [Serializable]
    public class ChatMessageDeleteMessage : BaseMessage
    {
        public int MessageId { get; set; }
        public int GroupId { get; set; }
        public int FromUserId { get; set; }

        public ChatMessageDeleteMessage(int messageId, int groupId, int fromUserId)
        {
            MessageId = messageId;
            GroupId = groupId;
            FromUserId = fromUserId;
        }

        public ChatMessageDeleteMessage() { }
    }
}
