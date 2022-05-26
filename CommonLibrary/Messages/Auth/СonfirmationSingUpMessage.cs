using MessageLibrary;
using System;

namespace CommonLibrary.Messages.Auth
{
    [Serializable]
    public class SingUpStage2Message : Message
    {
        public string CodeFromEmail { get; set; }

        public SingUpStage2Message(string code) => CodeFromEmail = code;
    }
}
