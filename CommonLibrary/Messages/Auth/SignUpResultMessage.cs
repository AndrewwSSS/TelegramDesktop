using System;
using MessageLibrary;

namespace CommonLibrary.Messages.Auth
{
    [Serializable]
    public class SignUpResultMessage : BaseMessage
    {
        public AuthenticationResult Result { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; }
        public DateTime RegistrationDate { get; set; }

        public SignUpResultMessage(AuthenticationResult Result, string Message = null)
        {
            this.Result = Result;
            this.Message = Message;
        }

        public SignUpResultMessage(AuthenticationResult Result, int UserId)
        {
            this.Result = Result;
            this.UserId = UserId;
        }



        public SignUpResultMessage SetRegistrationDate(DateTime RegistrationDate)
        {
            this.RegistrationDate = RegistrationDate;
            return this;
        }
    }
}
