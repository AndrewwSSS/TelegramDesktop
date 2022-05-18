using CommonLibrary.Messages.Users;
using System;
using System.Collections.Generic;


namespace CommonLibrary.Messages.Groups
{
    [Serializable]
    public class PublicGroupInfo : UserEntity
    {
        public int Id { get; set; }
        public List<PublicUserInfo> Users { get; set; }
        public GroupType GroupType { get; set; }
        public List<ChatMessage> Messages { get; set; }

        public PublicGroupInfo(string Name, string Description, int Id)
        {
            this.Name = Name;
            this.Description = Description;
            this.Id = Id;
        }


    }
}
