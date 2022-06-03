using CommonLibrary.Containers;
using CommonLibrary.Messages.Users;
using System;
using System.Collections.Generic;

namespace CommonLibrary.Messages
{
    [Serializable]
    public class ChatMessage : BaseMessage
    {
  
        public int FromUserId { get; set; }
        public int RepostUserId { get; set; } = -1;
        public ChatMessage RespondingTo { get; set; }

        public List<int> ImagesId { get; set; }
        public List<int> FilesId { get; set; }

        public List<FileContainer> Files { get; set; }
        public List<ImageContainer> Images { get; set; }

        public string Text { get; set; }
        public int GroupId { get; set; }

        public DateTime LocalTime => Time.ToLocalTime();

        public ChatMessage(string text) : this()
        {
            Text = text;
        }

        public ChatMessage() 
        {
            ImagesId = new List<int>();
            FilesId = new List<int>();
            Images = new List<ImageContainer>();
            Files = new List<FileContainer>();
        }


        public ChatMessage SetFrom(PublicUserInfo info)
        {
            FromUserId = info.Id;
            return this;
        }

        public ChatMessage SetFrom(int id)
        {
            FromUserId = id;
            return this;
        }

        public ChatMessage SetResendUser(PublicUserInfo info)
        {
            RepostUserId = info.Id;
            return this;
        }

        public ChatMessage SetResendUser(int id)
        {
            RepostUserId = id;
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
