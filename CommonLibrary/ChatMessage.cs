using MessageLibrary;
using System.Collections.Generic;
using System.Windows.Media;

namespace CommonLibrary
{
    public class ChatMessage : TextMessage
    {

        public User FromUser { get; set; }
        public User ToUser { get; set; }
        public User ResendUser { get; set; }
        public List<ImageSource> Images { get; set; }
        public List<ChatFile> Files { get; set; }

        public ChatMessage(string text) : base(text) => Type = MessageType.Custom;


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
