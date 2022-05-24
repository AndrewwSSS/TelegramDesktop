using System;

namespace CommonLibrary.Messages.Auth
{
    [Serializable]
    public class SignUpResultMessage : BaseMessage
    {
        public AuthenticationResult Result { get; set; }
        public string Message { get; set; }


        public SignUpResultMessage(AuthenticationResult Result, string Message = null)
        {
            this.Result = Result;
            this.Message = Message;
        }

        public SignUpResultMessage(AuthenticationResult Result) => this.Result = Result;


        public SignUpResultMessage() { }
    }
}
