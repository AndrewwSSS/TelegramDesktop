using MessageLibrary;
using System;

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
