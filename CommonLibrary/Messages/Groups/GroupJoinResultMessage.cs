using System;
using CommonLibrary.Messages.Auth;
using MessageLibrary;

namespace CommonLibrary.Messages.Groups
{
    [Serializable]
    public class GroupJoinResultMessage : BaseMessage
    {
        public AuthenticationResult Result { get; set; }
    
        public GroupJoinResultMessage(AuthenticationResult Result) => this.Result = Result;

        public GroupJoinResultMessage() { }
    }
}
