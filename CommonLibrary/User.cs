using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Windows.Media;

namespace CommonLibrary
{
    [Serializable]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime LastVisitDate { get; set; }
        public List<ImageContainer> Images { get; set; }
        public string ProfileDescription { get; set; }
        public List<User> BlockedUsers { get; set; }
        public List<GroupChat> Chats { get; set; }

        [NotMapped]
        public List<ChatMessage> Messages 
            => Chats.Select(chat => chat.Messages)
            .Aggregate((x, y) => 
            {
            x.AddRange(y);
            return x;
        });

        [NotMapped]
        public ImageSource Avatar => Images == null && Images.Count == 0 ? null : Images[0].ImageSource;


        public bool Banned { get; set; } = false;

        public User(string name) => Name = name;
        
        public User() { }

        public User AddImage(string path)
        {
            if (Images == null)
                Images = new List<ImageContainer>();
            Images.Add(ImageContainer.FromFile(path));
            return this;
        }

       



    }
}
