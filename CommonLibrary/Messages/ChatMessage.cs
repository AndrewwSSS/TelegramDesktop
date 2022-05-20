using CommonLibrary.Containers;
using CommonLibrary.Messages.Users;
using MessageLibrary;
using System;
using System.Collections.Generic;

namespace CommonLibrary.Messages
{
    [Serializable]
    public class ChatMessage : BaseMessage
    {
        private string text;

        public PublicUserInfo FromUser { get; set; }
        public PublicUserInfo ToUser { get; set; }
        public PublicUserInfo ResendUser { get; set; }
        public ChatMessage RespondingTo { get; set; }
        public List<ImageContainer> Images { get; set; }
        public List<FileContainer> Files { get; set; }

        public string Text
        {
            get => text;
            set
            {
                text = value;
            }
        }
        public int GroupId { get; set; }


        public ChatMessage(string text)
        {

            Text = text;
            Type = MessageType.Custom;
        }

        public ChatMessage() { }


        public ChatMessage SetFrom(PublicUserInfo user)
        {
            FromUser = user;
            return this;
        }

        public ChatMessage SetTo(PublicUserInfo user)
        {
            ToUser = user;
            return this;
        }

        public ChatMessage SetResendUser(PublicUserInfo user)
        {
            ResendUser = user;
            return this;
        }

        public ChatMessage SetRespondingTo(ChatMessage msg)
        {
            RespondingTo = msg;
            return this;
        }

        public ChatMessage SetGroupId(int GroupId)
        {
            this.GroupId = GroupId;
            return this;
        }
    }
}
