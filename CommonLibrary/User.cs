using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CommonLibrary
{
    public class User
    {
        public User(string name) => Name = name;

        public string Guid { get; private set; } = System.Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public List<ImageSource> Images { get; set; }
        public ImageSource Avatar => Images == null && Images.Count == 0 ? null : Images[0];
        public string ProfileDescription { get; set; }
        public List<User> BlockedUsers { get; set; }
        public List<GroupChat> Chats { get; set; }
        public List<ChatMessage> Messages 
            => Chats.Select(chat => chat.Messages)
            .Aggregate((x, y) => 
            {
            x.AddRange(y);
            return x;
        });
        public bool Banned { get; set; } = false;
    }
}
