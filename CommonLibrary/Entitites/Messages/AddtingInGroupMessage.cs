using System;
using MessageLibrary;

namespace CommonLibrary.Entitites.Messages
{
    [Serializable]
    public class AddtingInGroupMessage : Message
    {
        public PublicGroupInfo Group { get; set; }

        public AddtingInGroupMessage(PublicGroupInfo Group) => this.Group = Group;
    }
}
