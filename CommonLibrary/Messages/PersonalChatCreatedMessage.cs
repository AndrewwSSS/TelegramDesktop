using CommonLibrary.Messages.Groups;
using MessageLibrary;
using System;


namespace CommonLibrary.Messages
{
    [Serializable]
    public class PersonalChatCreatedMessage : BaseMessage
    {
        public PublicGroupInfo Group { get; set; }
        
        public PersonalChatCreatedMessage(PublicGroupInfo publicGroupInfo)
            => Group = publicGroupInfo;

        public PersonalChatCreatedMessage() { }

    }
}
