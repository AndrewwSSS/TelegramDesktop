using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
        public string ProfileDescription { get; set; }
        public DateTime DateRegistration { get; set; }
        public DateTime DateLastVisition { get; set; }
        public bool isBanned { get; set; } = false;

        public byte[] ImagesByte { get; set; }

     
        public List<BitmapImage> Images
        {
            get
            {
                BinaryFormatter bf = new BinaryFormatter();
                using(MemoryStream ms = new MemoryStream(ImagesByte))
                {
                    return (List<BitmapImage>)bf.Deserialize(ms);

                }
            }
            
            set
            {
                BinaryFormatter bf = new BinaryFormatter();
                using (MemoryStream ms = new MemoryStream())
                {
                    bf.Serialize(ms, value);
                    ImagesByte = ms.ToArray();
                }
            }

        }

        public List<User> BlockedUsers { get; set; }
        public List<GroupChat> Chats { get; set; }
  

        public ImageSource Avatar => Images == null && Images.Count == 0 ? null : Images[0];
        public List<ChatMessage> Messages 
            => Chats.Select(chat => chat.Messages)
            .Aggregate((x, y) => 
            {
            x.AddRange(y);
            return x;
        });


        public User(string Name)
        {
            this.Name = Name;
        }

        public User()
        {
            Images = new List<BitmapImage>();
            BlockedUsers = new List<User>();
            Chats = new List<GroupChat>();
        }
       
    }
}
