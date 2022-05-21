using MessageLibrary;
using System;


namespace CommonLibrary.Messages.Groups
{
    [Serializable]
    public class GroupJoinMessage : BaseMessage
    {
        public int GroupId { get; set; }
        public int UserId { get; set; }

        public GroupJoinMessage(int GroupId, int UserId)
        {
            this.GroupId = GroupId;
            this.UserId = UserId;
        }

        public GroupJoinMessage() { }
    }
}
