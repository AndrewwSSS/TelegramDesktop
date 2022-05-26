using MessageLibrary;
using System;

namespace CommonLibrary.Messages.Auth
{
    [Serializable]
    public class SignUpStage2Message : Message
    {
        public string CodeFromEmail { get; set; }

        public SignUpStage2Message(string code) => CodeFromEmail = code;
    }
}
