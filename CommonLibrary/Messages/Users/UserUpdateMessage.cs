using MessageLibrary;
using System;

namespace CommonLibrary.Messages.Users
{
    [Serializable]
    public class UserUpdateMessage : BaseMessage
    {
        public int UserId { get; set; }
        public bool? OnlineStatus { get; set; }
        public string NewDescription { get; set; }
        public string NewName { get; set; }
        public string NewLogin { get; set; }

        public UserUpdateMessage() { }
        
    }
}
