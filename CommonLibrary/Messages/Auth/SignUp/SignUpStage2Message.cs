using MessageLibrary;
using System;

namespace CommonLibrary.Messages.Auth.SignUp
{
    [Serializable]
    public class SignUpStage2Message : Message
    {
        public string CodeFromEmail { get; set; }
        public string Login { get; set; }

        public SignUpStage2Message(string code, string login)
        {
            CodeFromEmail = code;
            Login = login;
        }
    }
}
