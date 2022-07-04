using MessageLibrary;
using System;

namespace CommonLibrary.Messages.Auth.Login
{
    [Serializable]
    public class FastLoginResultMessage : Message
    {
        public AuthResult Result { get; set; }

        public FastLoginResultMessage(AuthResult result) => Result = result;

    }
}
