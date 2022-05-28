using System;
using MessageLibrary;

namespace CommonLibrary.Messages.Auth.Login
{
    [Serializable]
    public class LoginMessage : Message
    {
        // Email or Login
        public string Login { get; set; }

        public string MachineName { get; set; }

        public string Password { get; set; }

        public LoginMessage(string Login, string Password, string MachineName)
        {
            this.Login = Login;
            this.Password = Password;
            this.MachineName = MachineName; 
        }

        public LoginMessage() { }

    }
}
