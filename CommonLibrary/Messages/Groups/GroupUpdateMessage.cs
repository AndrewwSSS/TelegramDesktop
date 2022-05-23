using CommonLibrary.Messages.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Messages.Groups
{
    [Serializable]
    public class GroupUpdateMessage : BaseMessage
    {
        public List<PublicUserInfo> NewUsers { get; set; }
        public List<PublicUserInfo> RemovedUsers { get; set; }
        public string NewDescription { get; set; }
        public string NewName { get; set; }
        public GroupUpdateMessage()
        {

        }
    }
}
