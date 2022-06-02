using System;

namespace CommonLibrary.Messages
{
    [Serializable]
    public class DeleteMessage : BaseMessage
    {
        public int MessageId { get; set; }
        public int GroupId { get; set; }
        public int FromUserId { get; set; }

        public DeleteMessage(int messageId, int groupId, int fromUserId)
        {
            MessageId = messageId;
            GroupId = groupId;
            FromUserId = fromUserId;
        }

        public DeleteMessage() { }
    }
}
