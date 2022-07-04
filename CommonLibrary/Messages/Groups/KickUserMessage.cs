using MessageLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Messages.Groups
{
    [Serializable]
    public class KickUserMessage : Message
    {
        public int UserId { get; set; }
        public int GroupId { get; set; }
        public KickUserMessage(int uId, int gId)
        {
            UserId = uId;
            GroupId = gId;
        }
    }
}
