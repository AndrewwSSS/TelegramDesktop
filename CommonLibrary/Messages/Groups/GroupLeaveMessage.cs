using MessageLibrary;
using System;

namespace CommonLibrary.Messages.Groups
{
    [Serializable]
    public class GroupLeaveMessage : Message
    {
        public int GroupId { get; set; }

        public GroupLeaveMessage(int groupId) => GroupId = groupId;

    }
}
