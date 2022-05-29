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

        public virtual List<UserClient> Clients { get; set; }
        public virtual List<int> BlockedUsersId { get; set; } 
        public virtual List<GroupChat> Chats { get; set; }
        public virtual List<ChatMessage> Messages { get; set; }

        public DateTime LocalRegistrationDate => RegistrationDate.ToLocalTime();
        public DateTime LocalVisitDate => VisitDate.ToLocalTime();

        [NotMapped]
        public DateTime LocalLastVistDate => VisitDate.ToLocalTime();


        public User()
        {
            BlockedUsersId = new List<int>();
            Chats = new List<GroupChat>();
            Clients = new List<UserClient>();
            Banned = false;
            Messages = new List<ChatMessage>();
            
        }

        public User(int id) : this()
        {
            Id = id;
        }


    

    }
}
