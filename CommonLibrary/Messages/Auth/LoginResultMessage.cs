using System;
using MessageLibrary;

namespace CommonLibrary.Messages.Auth
{
    [Serializable]
    public class LoginResultMessage : Message
    {
        public AuthenticationResult Result { get; set; }
        public string Message { get; set; }

        public int UserId { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public DateTime RegistrationDate { get; set; }

        public LoginResultMessage(AuthenticationResult Result, int UserId)
        {
            this.Result = Result;
            this.Message = Message;
            this.UserId = UserId;
        }

        public LoginResultMessage(AuthenticationResult Result, string Message)
        {
            this.Message = Message;
            this.Result = Result;
        }

    }
}
