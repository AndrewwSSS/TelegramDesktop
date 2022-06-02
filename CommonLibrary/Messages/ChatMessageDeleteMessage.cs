using System;

namespace CommonLibrary.Messages
{
    [Serializable]
    public class ChatMessageDeleteMessage : BaseMessage
    {
        public int DeletedMessageId { get; set; }
        public int GroupId { get; set; }
        public int FromUserId { get; set; }

        public ChatMessageDeleteMessage(int messageId, int groupId, int fromUserId)
        {
            DeletedMessageId = messageId;
            GroupId = groupId;
            FromUserId = fromUserId;
        }

        public ChatMessageDeleteMessage() { }
    }
}
