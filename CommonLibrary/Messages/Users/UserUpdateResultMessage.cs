using MessageLibrary;
using System;
using CommonLibrary.Messages.Auth;

namespace CommonLibrary.Messages.Users
{
    [Serializable]
    public class UserUpdateResultMessage : Message
    {
        public AuthResult result { get; set; }

        public UserUpdateResultMessage(AuthResult res) => result = res;

        public UserUpdateResultMessage() { }
    }
}
