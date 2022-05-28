using System;
using CommonLibrary.Messages.Auth;
using MessageLibrary;

namespace CommonLibrary.Messages.Groups
{
    [Serializable]
    public class GroupJoinResultMessage : BaseMessage
    {
        public AuthenticationResult Result { get; set; }
        public int GroupId { get; set; }
        public GroupJoinResultMessage(AuthenticationResult Result, int groupId) {
            this.Result = Result;
            GroupId = groupId;
        }
        public GroupJoinResultMessage() { }
    }
}
