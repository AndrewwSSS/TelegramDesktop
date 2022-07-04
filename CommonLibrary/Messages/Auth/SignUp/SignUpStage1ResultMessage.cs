using System;

namespace CommonLibrary.Messages.Auth.SignUp
{
    [Serializable]
    public class SignUpStage1ResultMessage : BaseMessage
    {
        public AuthResult Result { get; set; }
        public string Message { get; set; }


        public SignUpStage1ResultMessage(AuthResult Result, string Message = null)
        {
            this.Result = Result;
            this.Message = Message;
        }

        public SignUpStage1ResultMessage(AuthResult Result) => this.Result = Result;


        public SignUpStage1ResultMessage() { }
    }
}
