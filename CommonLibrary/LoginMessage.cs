using System;
using MessageLibrary;

namespace CommonLibrary
{
    [Serializable]
    public class LoginMessage : Message
    {
        // Email or Login
        public string Login { get; set; }

        public string Password { get; set; }

        public LoginMessage(string Login, string Password)
        {
            this.Login = Login;
            this.Password = Password;
        }
    }
}
