using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Windows.Media;
using CommonLibrary.Messages.Groups;
using MessageLibrary;

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
        public bool Banned { get; set; } = false;
        public DateTime RegistrationDate { get; set; }
        public DateTime LastVisitDate { get; set; }
        public virtual List<User> BlockedUsers { get; set; }
        public virtual List<GroupChat> Chats { get; set; }
        public virtual List<BaseMessage> MessagesToSend { get; set; }
       


        public User()
        {
            //BlockedUsers = new List<User>();
            //Chats = new List<GroupChat>();
        }

        [NotMapped]
        public TcpClientWrap client { get; set; }

        [NotMapped]
        public bool isOnline { get; set; } = false;

        [NotMapped]
        public List<ChatMessage> Messages
             => Chats.Select(chat => chat.Messages)
             .Aggregate((x, y) =>
             {
                 x.AddRange(y);
                 return x;
             });


        //public User(string name) => Name = name;

        //public User(int id) => Id = id;

    }
}
