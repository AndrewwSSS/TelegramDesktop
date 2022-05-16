using System;
using System.Collections.Generic;
using MessageLibrary;

namespace CommonLibrary.Entitites
{
    [Serializable]
    public class CreateNewGroupMessage : Message
    {
        public string Name { get; set; }
        public ImageContainer Image { get; set; }
        public List<int> MembersId { get; set; }

        public CreateNewGroupMessage(string Name, List<int> MembersId,ImageContainer Image = null)
        {
            this.Name = Name;
            this.Image = Image;
            this.MembersId = MembersId;
        }

        public CreateNewGroupMessage() { }

    }
}
