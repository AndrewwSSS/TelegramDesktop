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
        public int GroupId { get; set; }
        public int NewUserId { get; set; } = -1;
        public int RemovedUserId { get; set; } = -1;
        public string NewDescription { get; set; }
        public string NewName { get; set; }


        public GroupUpdateMessage() { }
        public GroupUpdateMessage(int id) => GroupId = id;

    }
}
