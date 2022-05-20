using System;
using CommonLibrary.Messages.Groups;
using MessageLibrary;

namespace CommonLibrary.Messages.Auth.Groups
{
    [Serializable]
    public class GroupJoinResultMessage : BaseMessage
    {
        public AuthenticationResult Result { get; set; }
    
        public GroupJoinResultMessage(AuthenticationResult Result) => this.Result = Result;

        public GroupJoinResultMessage() { }
    }
}
