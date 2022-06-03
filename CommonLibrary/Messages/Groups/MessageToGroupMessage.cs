using CommonLibrary.Containers;
using MessageLibrary;
using System;
using System.Collections.Generic;

namespace CommonLibrary.Messages.Groups
{
    [Serializable]
    public class MessageToGroupMessage : Message
    {
        public int LocalMessageId { get; set; }
        public ChatMessage Message { get; set; }

        public List<KeyValuePair<FileContainer, int>> Files { get; set; }
        public List<KeyValuePair<ImageContainer, int>> Images { get; set; }


        public MessageToGroupMessage(ChatMessage message, int localId) : this()
        {
            LocalMessageId = localId;
            Message = message;
        }

        public MessageToGroupMessage()
        {
            Files = new List<KeyValuePair<int, FileContainer>>();
            Images = new List<KeyValuePair<int, ImageContainer>>();
        }
    }
}
