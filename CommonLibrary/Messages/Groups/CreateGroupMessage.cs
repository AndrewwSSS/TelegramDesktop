using MessageLibrary;
using System;
using System.Collections.Generic;

namespace CommonLibrary.Messages.Groups
{
    [Serializable]
    public class CreateGroupMessage : Message
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public byte[] Image { get; set; }
        public List<int> MembersId { get; set; } 
        public int FromUserId { get; set; }


        public CreateGroupMessage(string Name, string desc, int FromUserId, List<int> MembersId = null, byte[] Image = null)
        {
            this.Name = Name;
            Description = desc;
            this.Image = Image;
            this.MembersId = MembersId;
            this.FromUserId = FromUserId;
        }

        public CreateGroupMessage() { } 

    }
}
