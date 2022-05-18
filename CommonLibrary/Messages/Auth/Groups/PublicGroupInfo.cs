using CommonLibrary.Messages.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonLibrary.Messages.Groups
{
    [Serializable]
    public class PublicGroupInfo : UserEntity
    {
        public int Id { get; set; }
        public List<PublicUserInfo> Users { get; set; }
        public GroupType GroupType { get; set; }
        public List<ChatMessage> Messages { get; set; }

        public ChatMessage LastMessage => Messages == null ? null : Messages.LastOrDefault();
        public PublicGroupInfo(string name, string desc, int id)
        {
            Name = name;
            Description = desc;
            Id = id;
        }


    }
}
