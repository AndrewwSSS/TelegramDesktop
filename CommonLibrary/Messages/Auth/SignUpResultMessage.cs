using System;

namespace CommonLibrary.Messages.Auth
{
    [Serializable]
    public class SignUpResultMessage : BaseMessage
    {
        public AuthenticationResult Result { get; set; }
        public string Message { get; set; }

        //To remove
        public DateTime RegistrationDate { get; set; }

        public SignUpResultMessage(AuthenticationResult Result, string Message = null)
        {
            this.Result = Result;
            this.Message = Message;
        }

        public SignUpResultMessage(AuthenticationResult Result) => this.Result = Result;


        public SignUpResultMessage() { }

        // To remove
        public SignUpResultMessage SetRegistrationDate(DateTime RegistrationDate)
        {
            this.RegistrationDate = RegistrationDate;
            return this;
        }
    }
}
