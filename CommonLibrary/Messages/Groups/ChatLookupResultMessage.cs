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
        public List<int> UsersId { get; set; }

        public ChatLookupResultMessage(IEnumerable<int> usersId, IEnumerable<PublicGroupInfo> groups) : this() 
        {
            UsersId = usersId.ToList();
            Groups = groups.ToList();
        }

        public ChatLookupResultMessage()
        {
            UsersId = new List<int>();
            Groups = new List<PublicGroupInfo>();
        }

    }
}
