using MessageLibrary;
using System;


namespace CommonLibrary.Messages.Groups
{
    [Serializable]
    public class AddtingInGroupMessage : BaseMessage
    {
        public PublicGroupInfo GroupInfo { get; set; }

        public AddtingInGroupMessage(PublicGroupInfo GroupInfo)
        {
            this.GroupInfo = GroupInfo;
        }
    }
}
