using System;
using CommonLibrary.Messages.Auth;
using MessageLibrary;

namespace CommonLibrary.Messages.Groups
{
    [Serializable]
    public class GroupJoinResultMessage : BaseMessage
    {
        public AuthResult Result { get; set; }
        public int GroupId { get; set; }

        public GroupJoinResultMessage(AuthResult Result, int groupId) {
            this.Result = Result;
            GroupId = groupId;
        }

        public GroupJoinResultMessage(AuthResult Result) => this.Result = Result;


        public GroupJoinResultMessage() { }
    }
}
