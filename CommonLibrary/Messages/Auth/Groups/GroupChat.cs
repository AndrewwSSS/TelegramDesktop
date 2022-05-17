using CommonLibrary.Messages.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Media;

namespace CommonLibrary.Messages.Groups
{
    public enum GroupType
    {
        Personal,
        Public
    }

    [Serializable]
    public class GroupChat : UserEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; }

        public GroupType Type { get; set; }
        public DateTime DateCreated { get; set; }
        public List<ChatMessage> Messages { get; set; }
        public List<User> Members { get; set; }



    }
}
