using CommonLibrary.Messages.Groups;
using System;

namespace CommonLibrary.Messages.Groups
{
    [Serializable]
    public class GroupInviteMessage : BaseMessage
    {
        public PublicGroupInfo GroupInfo { get; set; }
        public int FromUserId { get; set; }

        public GroupInviteMessage(PublicGroupInfo GroupInfo, int FromUserId)
        {
            this.GroupInfo = GroupInfo;
            this.FromUserId = FromUserId;
        }

    }
}
