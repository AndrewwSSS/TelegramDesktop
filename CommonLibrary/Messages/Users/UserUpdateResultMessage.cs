using MessageLibrary;
using System;
using CommonLibrary.Messages.Auth;

namespace CommonLibrary.Messages.Users
{
    [Serializable]
    public class UserUpdateResultMessage : Message
    {
        public AuthResult Result { get; set; }
        public string NewDescription { get; set; }
        public string NewName { get; set; }
        public string NewLogin { get; set; }

        public UserUpdateResultMessage(AuthResult res) => Result = res;

        public UserUpdateResultMessage() { }
    }
}
