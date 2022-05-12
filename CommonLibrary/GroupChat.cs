using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CommonLibrary
{
    public enum GroupType
    {
        Personal,
        Public
    }

    public class GroupChat
    {
        public string Guid { get; private set; } = System.Guid.NewGuid().ToString();
        public GroupType Type { get; set; }
        public List<User> Members { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<ImageSource> Images { get; set; }
        public ImageSource Avatar => Images == null && Images.Count == 0 ? null : Images[0];
        public List<ChatMessage> Messages { get; set; }
    }
}
