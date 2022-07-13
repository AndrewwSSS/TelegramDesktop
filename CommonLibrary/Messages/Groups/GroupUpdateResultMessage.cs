using CommonLibrary.Messages.Auth;
using MessageLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Messages.Groups
{
    [Serializable]
    public class GroupUpdateResultMessage : Message
    {
        public int GroupId { get; set; }
        public AuthResult Result { get; set; }
        public GroupUpdateResultMessage()
        {

        }
        public GroupUpdateResultMessage(AuthResult result, int gId)
        {
            Result = result;
            GroupId = gId;
        }
    }
}
