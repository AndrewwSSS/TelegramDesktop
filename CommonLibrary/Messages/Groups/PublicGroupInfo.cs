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
        public List<int> MembersId { get; set; }
        public List<ChatMessage> Messages { get; set; }
        public List<int> AdministratorsId { get; set; }

        public PublicGroupInfo(string name, string desc, int id) : this()
        {
            Name = name;
            Description = desc;
            Id = id;
        }

        public PublicGroupInfo()
        {
            MembersId = new List<int>();
            Messages = new List<ChatMessage>();
            AdministratorsId = new List<int>();
        }


    }
}
