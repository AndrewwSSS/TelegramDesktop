using MessageLibrary;
using System;

namespace CommonLibrary.Messages
{
    [Serializable]
    public class FirstPersonalResultMessage : Message
    {
        public int GroupId { get; set; }

        public FirstPersonalResultMessage(int groupId) {
            this.GroupId = groupId;
        }

    }
}
