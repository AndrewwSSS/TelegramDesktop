using System;

namespace CommonLibrary.Messages.Auth
{
    [Serializable]
    public class SingUpStage1ResultMessage : BaseMessage
    {
        public AuthenticationResult Result { get; set; }
        public string Message { get; set; }


        public SingUpStage1ResultMessage(AuthenticationResult Result, string Message = null)
        {
            this.Result = Result;
            this.Message = Message;
        }

        public SingUpStage1ResultMessage(AuthenticationResult Result) => this.Result = Result;


        public SingUpStage1ResultMessage() { }
    }
}
