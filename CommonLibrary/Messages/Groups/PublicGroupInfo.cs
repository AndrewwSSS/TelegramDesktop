using CommonLibrary.Containers;
using CommonLibrary.Messages.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonLibrary.Messages.Groups
{
    /// <summary>
    /// Представляет собой класс, содержащий доступную для клиентов информацию о группе
    /// </summary>
    [Serializable]
    public class PublicGroupInfo : UserEntity
    {
        public int Id { get; set; }
        public GroupType GroupType { get; set; }

        public List<PublicUserInfo> Members { get; set; }
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
         

        public PublicGroupInfo(string name, string desc, int id) : this()
        {
            Name = name;
            Description = desc;
            Id = id;
        }

        public PublicGroupInfo()
        {
            Members = new List<PublicUserInfo>();
            Messages = new List<ChatMessage>();
        }


    }
}
