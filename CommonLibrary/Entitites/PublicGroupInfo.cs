using System;
using System.Collections.Generic;

namespace CommonLibrary
{
    [Serializable]
    public class PublicGroupInfo : UserEntity
    {
        public int Id { get; set; }
        public List<PublicUserInfo> Users { get; set; }

        public PublicGroupInfo(string Name, string Description, int Id)
        {
            this.Name = Name;
            this.Description = Description;
            this.Id = Id;

            Users = new List<PublicUserInfo>();
        }


    }
}
