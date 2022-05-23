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
        public List<PublicUserInfo> Users { get; set; }
        public GroupType GroupType { get; set; }
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

        public ChatMessage LastMessage => Messages == null ? null : Messages.LastOrDefault();

        public PublicGroupInfo(string name, string desc, int id)
        {
            Name = name;
            Description = desc;
            Id = id;
        }


    }
}
