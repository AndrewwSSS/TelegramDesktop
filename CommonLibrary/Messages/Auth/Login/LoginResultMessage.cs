using System;
using CommonLibrary.Messages.Users;
using MessageLibrary;

namespace CommonLibrary.Messages.Auth.Login
{
    [Serializable]
    public class LoginResultMessage : Message
    {
        public AuthenticationResult Result { get; set; }
        public string Message { get; set; }
        public PublicUserInfo UserInfo { get; set; }
        public string Guid { get; set; }

        public LoginResultMessage(AuthenticationResult Result, PublicUserInfo info, string guid)
        {
            this.Result = Result;
            UserInfo = info;
        }

        public LoginResultMessage(AuthenticationResult Result, string Message)
        {
            this.Message = Message;
            this.Result = Result;
        }

    }
}
