using MessageLibrary;
using System;

namespace CommonLibrary.Messages.Groups
{
    [Serializable]
    public class DisconnectFromGroupMessage : Message
    {
        public int GroupId { get; set; }

        public DisconnectFromGroupMessage(int groupId) => GroupId = groupId;

    }
}
