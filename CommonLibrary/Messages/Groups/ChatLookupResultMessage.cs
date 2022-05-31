using CommonLibrary.Messages.Users;
using MessageLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Messages.Groups
{
    [Serializable]
    public class ChatLookupResultMessage : Message
    {
        public List<PublicGroupInfo> Groups { get; set; }
        public List<int> Users { get; set; }

        public ChatLookupResultMessage(IEnumerable<int> userId, IEnumerable<PublicGroupInfo> groups) 
        {
            Users = userId.ToList();
            Groups = groups.ToList();
        }
    }
}
