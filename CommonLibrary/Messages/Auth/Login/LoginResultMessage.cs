using System;
using CommonLibrary.Messages.Users;
using MessageLibrary;

namespace CommonLibrary.Messages.Auth.Login
{
    [Serializable]
    public class LoginResultMessage : Message
    {
        public AuthResult Result { get; set; }
        public string Message { get; set; }
        public PublicUserInfo UserInfo { get; set; }
        public string Guid { get; set; }

        public LoginResultMessage(AuthResult Result, PublicUserInfo info, string guid)
        {
            this.Result = Result;
            UserInfo = info;
            Guid = guid;
        }

        public LoginResultMessage(AuthResult Result, string Message)
        {
            this.Message = Message;
            this.Result = Result;
        }

    }
}
