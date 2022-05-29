using MessageLibrary;
using System;

namespace CommonLibrary.Messages.Auth.Logout
{
    [Serializable]
    public class LogoutMessage : Message
    {
        public string Guid { get; set; }

        public LogoutMessage(string guid) => Guid = guid;

    }
}
