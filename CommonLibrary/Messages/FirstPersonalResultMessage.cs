using MessageLibrary;
using System;

namespace CommonLibrary.Messages
{
    [Serializable]
    public class FirstPersonalResultMessage : Message
    {
        public int GroupId { get; set; }
        public int LocalId { get; set; }
        public FirstPersonalResultMessage(int groupId, int localId) {
            this.GroupId = groupId;
            LocalId = localId;
        }

    }
}
