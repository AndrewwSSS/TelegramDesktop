using System;
using MessageLibrary;

namespace CommonLibrary.Messages
{
    [Serializable]
    public class ClientDisconnectMessage : Message
    {
        public int UserId { get; set; }

        public ClientDisconnectMessage(int UserId) => this.UserId = UserId;

        public ClientDisconnectMessage() { }
    }
}
