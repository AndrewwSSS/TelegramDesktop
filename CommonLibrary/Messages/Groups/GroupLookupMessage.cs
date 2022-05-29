using System;
using MessageLibrary;

namespace CommonLibrary.Messages.Groups
{
    [Serializable]
    public class GroupLookupMessage : Message
    {
        public int UserId { get; set; }
        public string GroupName { get; set; }
        public string Guid { get; set; }

        public GroupLookupMessage(string GroupName, int userId){
            this.GroupName = GroupName;
            this.UserId = userId; 
        }


    }
}
