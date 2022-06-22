using CommonLibrary.Messages.Groups;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommonLibrary.Messages.Users
{
    [Serializable]
    public class User : UserEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; }

        public string Login { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool Banned { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime VisitDate { get; set; }

        public virtual ICollection<UserClient> Clients { get; set; }
        public virtual ICollection<int> BlockedUsersId { get; set; } 
        public virtual ICollection<GroupChat> Chats { get; set; }
        public virtual ICollection<ChatMessage> Messages { get; set; }

        [NotMapped]
        public DateTime LocalRegistrationDate => RegistrationDate.ToLocalTime();
        [NotMapped]
        public DateTime LocalVistDate => VisitDate.ToLocalTime();


        public User()
        {
            BlockedUsersId = new HashSet<int>();
            Chats = new HashSet<GroupChat>();
            Clients = new HashSet<UserClient>();
            Banned = false;
            Messages = new HashSet<ChatMessage>();
        }
    }
}
