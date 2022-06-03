using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Messages.Groups
{
    [Serializable]
    public class LeaveFromGroupMessage : BaseMessage
    {
        public int GroupId { get; set; }

        public LeaveFromGroupMessage(int groupId) => GroupId = groupId;
    }
}
