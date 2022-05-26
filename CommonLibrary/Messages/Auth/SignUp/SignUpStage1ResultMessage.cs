using System;

namespace CommonLibrary.Messages.Auth.SignUp
{
    [Serializable]
    public class SignUpStage1ResultMessage : BaseMessage
    {
        public AuthenticationResult Result { get; set; }
        public string Message { get; set; }


        public SignUpStage1ResultMessage(AuthenticationResult Result, string Message = null)
        {
            this.Result = Result;
            this.Message = Message;
        }

        public SignUpStage1ResultMessage(AuthenticationResult Result) => this.Result = Result;


        public SignUpStage1ResultMessage() { }
    }
}
