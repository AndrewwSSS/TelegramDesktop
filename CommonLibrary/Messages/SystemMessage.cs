using MessageLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Messages
{
    public enum SystemMessageType
    {
        Logout,
        GetOfflineMessages
    }

    [Serializable]
    public class SystemMessage : Message
    {
        public SystemMessageType Type { get; set; }

        public SystemMessage(SystemMessageType messageType) => Type = messageType;
    }
}
