using MessageLibrary;
using System;


namespace CommonLibrary.Messages.Groups
{
    [Serializable]
    public class GroupJoinMessage : BaseMessage
    {
        public int GroupId { get; set; }

        public GroupJoinMessage(int GroupId) => this.GroupId = GroupId;

        public GroupJoinMessage() { }
    }
}
