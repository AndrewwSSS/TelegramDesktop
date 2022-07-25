using MessageLibrary;
using System;
using CommonLibrary.Messages.Auth;

namespace CommonLibrary.Messages.Users
{
    [Serializable]
    public class UserUpdateResultMessage : Message
    {
        public AuthResult Result { get; set; }

        public UserUpdateResultMessage(AuthResult res) => Result = res;

        public UserUpdateResultMessage() { }
    }
}
