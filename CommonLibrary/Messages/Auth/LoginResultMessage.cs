using System;
using MessageLibrary;

namespace CommonLibrary.Messages.Auth
{
    [Serializable]
    public class LoginResultMessage : BaseMessage
    {
        public AuthenticationResult Result { get; set; }
        public string Message { get; set; }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public DateTime RegistrationDate { get; set; }

        public LoginResultMessage(AuthenticationResult Result, int Id)
        {
            this.Result = Result;
            this.Message = Message;
            this.Id = Id;
        }

        public LoginResultMessage(AuthenticationResult Result, string Message)
        {
            this.Message = Message;
            this.Result = Result;
        }

    }
}
