using System;
using MessageLibrary;

namespace CommonLibrary
{
    [Serializable]
    public class GroupLookupMessage : Message
    {
        public string GroupName { get; set; }

        public GroupLookupMessage(string GroupName)
        {
            this.GroupName = GroupName;
        }


    }
}
