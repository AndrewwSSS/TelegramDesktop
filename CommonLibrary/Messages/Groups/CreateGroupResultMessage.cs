using System;
using CommonLibrary.Messages.Auth;
using MessageLibrary;

namespace CommonLibrary.Messages.Groups
{
    [Serializable]
    public class CreateGroupResultMessage : Message
    {
        public AuthResult Result { get; set; }
        public string Message { get; set; }
        public int GroupId { get; set; }

        public string GroupName { get; set; }
        public string GroupDescription { get; set; }

        public CreateGroupResultMessage(AuthResult Result, int GroupId, string gName, string gDesc)
        {
            this.Result = Result;
            this.GroupId = GroupId;
            GroupName = gName;
            GroupDescription = gDesc;
        }

        public CreateGroupResultMessage(AuthResult Result, string Message)
        {
            this.Result = Result;
            this.Message = Message;
        }

        public CreateGroupResultMessage() { }


    }
}
