using System;
using CommonLibrary.Messages.Auth;
using MessageLibrary;

namespace CommonLibrary.Messages.Groups
{
    [Serializable]
    public class CreateGroupResultMessage : Message
    {
        public AuthenticationResult Result { get; set; }
        public string Message { get; set; }
        public int GroupId { get; set; }


        public CreateGroupResultMessage(AuthenticationResult Result) => this.Result = Result;

        public CreateGroupResultMessage(AuthenticationResult Result, string Message)
        {
            this.Result = Result;
            this.Message = Message;
        }


    }
}
