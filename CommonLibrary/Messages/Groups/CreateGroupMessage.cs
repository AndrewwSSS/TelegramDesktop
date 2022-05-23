using System;
using System.Collections.Generic;
using CommonLibrary.Containers;
using MessageLibrary;

namespace CommonLibrary.Messages.Groups
{
    [Serializable]
    public class CreateGroupMessage : Message
    {
        public string Name { get; set; }
        public ImageContainer Image { get; set; }
        public List<int> MembersId { get; set; } 
        public int FromUserId { get; set; }


        public CreateGroupMessage(string Name, int FromUserId, List<int> MembersId = null, ImageContainer Image = null) : this()
        {
            this.Name = Name;
            this.Image = Image;
            this.MembersId = MembersId;
            this.FromUserId = FromUserId;

        }

        public CreateGroupMessage() {
            MembersId = new List<int>();
        }

    }
}
