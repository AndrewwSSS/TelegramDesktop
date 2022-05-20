using MessageLibrary;
using System;


namespace CommonLibrary.Messages.Groups
{
    [Serializable]
    public class GroupJoinMessage : BaseMessage
    {
        public PublicGroupInfo GroupInfo { get; set; }

        public GroupJoinMessage(PublicGroupInfo GroupInfo) => this.GroupInfo = GroupInfo;

        public GroupJoinMessage() { }
    }
}
