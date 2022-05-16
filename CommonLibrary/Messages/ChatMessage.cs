using CommonLibrary.Containers;
using CommonLibrary.Messages.Groups;
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

        public User FromUser { get; set; }
        public User ToUser { get; set; }
        public User ResendUser { get; set; }
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
        public GroupChat Chat { get; set; }

        public ChatMessage(string text)
        {

            Text = text;
            Type = MessageType.Custom;
        }

        public ChatMessage SetFrom(User user)
        {
            FromUser = user;
            return this;
        }

        public ChatMessage SetTo(User user)
        {
            ToUser = user;
            return this;
        }

        public ChatMessage SetResendUser(User user)
        {
            ResendUser = user;
            return this;
        }

        public ChatMessage SetRespondingTo(ChatMessage msg)
        {
            RespondingTo = msg;
            return this;
        }

        public ChatMessage SetGroupChat(GroupChat gchat)
        {
            Chat = gchat;
            return this;
        }




    }
}
