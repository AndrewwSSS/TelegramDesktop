using System;
using MessageLibrary;

namespace CommonLibrary.Entitites.Messages
{
    [Serializable]
    public class CreateNewGroupResultMessage : Message
    {
        public AuthenticationResult Result { get; set; }
        public string Message { get; set; }
        public int GroupId { get; set; }


        public CreateNewGroupResultMessage(AuthenticationResult Result) => this.Result = Result;

        public CreateNewGroupResultMessage(AuthenticationResult Result, string Message) {
            this.Result = Result;
            this.Message = Message;
        }


    }
}
