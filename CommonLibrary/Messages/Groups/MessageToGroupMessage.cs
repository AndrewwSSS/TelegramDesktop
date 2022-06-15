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

        public List<int> FilesId { get; set; }
        public List<int> ImagesId { get; set; }


        public MessageToGroupMessage(ChatMessage message, int localId) : this()
        {
            LocalMessageId = localId;
            Message = message;
        }

        public MessageToGroupMessage()
        {
            FilesId = new List<int>();
            ImagesId = new List<int>();
        }
    }
}
