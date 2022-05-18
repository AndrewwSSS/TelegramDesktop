using MessageLibrary;
using System;


namespace CommonLibrary.Messages.Groups
{
    [Serializable]
    public class AddingInGroupMessage : BaseMessage
    {
        public PublicGroupInfo GroupInfo { get; set; }

        public AddingInGroupMessage(PublicGroupInfo GroupInfo)
        {
            this.GroupInfo = GroupInfo;
        }
    }
}
