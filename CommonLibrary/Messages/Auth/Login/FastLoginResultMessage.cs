using MessageLibrary;
using System;

namespace CommonLibrary.Messages.Auth.Login
{
    [Serializable]
    public class FastLoginResultMessage : Message
    {
        public AuthenticationResult Result { get; set; }

        public FastLoginResultMessage(AuthenticationResult result) => Result = result;

    }
}
