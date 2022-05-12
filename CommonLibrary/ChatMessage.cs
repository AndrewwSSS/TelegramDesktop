using MessageLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CommonLibrary
{
    public class ChatMessage : TextMessage
    {
        public ChatMessage(string text) : base(text) => Type = MessageType.Custom;
        public User FromUser { get; set; }
        public User ToUser { get; set; }
        public User ResendUser { get; set; }
        public List<ImageSource> Images { get; set; }


        public ChatMessage SetFrom(User user)
        {
            FromUser = user;
            return this;
        }

        public ChatMessage SetTo(User user)
        {
            ToUser = user;
            return this;
        }

        public ChatMessage SetResendUser(User user)
        {
            ResendUser = user;
            return this;
        }

    }
}
