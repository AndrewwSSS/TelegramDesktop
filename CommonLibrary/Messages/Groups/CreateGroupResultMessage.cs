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
        public int GroupLocalId { get; set; }
        
        public CreateGroupResultMessage(AuthResult Result, int gId, int gLocalId)
        {
            this.Result = Result;
            this.GroupId = gId;
            GroupLocalId = gLocalId;
        }

        public CreateGroupResultMessage(AuthResult Result, string Message)
        {
            this.Result = Result;
            this.Message = Message;
        }

        public CreateGroupResultMessage() { }


    }
}
