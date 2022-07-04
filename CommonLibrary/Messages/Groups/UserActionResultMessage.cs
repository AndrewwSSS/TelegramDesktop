using CommonLibrary.Messages.Auth;
using MessageLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Messages.Groups
{
    public enum UserActionType
    {
        Kick
    }
    [Serializable]
    public class UserActionResultMessage : Message
    {
        public UserActionType Type { get; set; }
        public AuthResult Result { get; set; }
        public int UserId { get; set; }
        public int GroupId { get; set; }
        public UserActionResultMessage(UserActionType type, AuthResult result,
            int uId, int gId)
        {
            Type = type;
            Result = result;
            UserId = uId;
            GroupId = gId;
        }
    }
}
