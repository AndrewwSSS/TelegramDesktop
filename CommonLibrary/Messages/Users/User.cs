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
        public virtual List<User> BlockedUsers { get; set; } 
        public virtual List<GroupChat> Chats { get; set; }
        public virtual List<BaseMessage> MessagesToSend { get; set; }
        public virtual List<ChatMessage> Messages { get; set; }

        [NotMapped]
        public DateTime LocalLastVistDate => VisitDate.ToLocalTime();


        public User()
        {
            BlockedUsers = new List<User>();
            Chats = new List<GroupChat>();
            MessagesToSend = new List<BaseMessage>();
            Banned = false;
        }

        public User(int id) : this()
        {
            Id = id;
        }


    

    }
}
