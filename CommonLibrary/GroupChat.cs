using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace CommonLibrary
{
    public enum GroupType
    {
        Personal,
        Public
    }

    [Serializable]
    public class GroupChat
    {
        public int Id { get; private set; }
        public GroupType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public List<ImageSource> Images { get; set; }
        public List<ChatMessage> Messages { get; set; }
        public List<User> Members { get; set; }

        public ImageSource Avatar => Images == null && Images.Count == 0 ? null : Images[0];

    }
}
