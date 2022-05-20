using System;
using MessageLibrary;

namespace CommonLibrary.Messages.Auth.Groups
{
    [Serializable]
    public class GroupJoinResultMessage : Message
    {
        public AuthenticationResult Result { get; set; }

        public GroupJoinResultMessage(AuthenticationResult Result) => this.Result = Result;

        public GroupJoinResultMessage() { }
    }
}
