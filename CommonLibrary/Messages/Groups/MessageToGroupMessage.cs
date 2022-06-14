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

        public List<int> LocalFilesId { get; set; }
        public List<int> LocalImagesId { get; set; }


        public MessageToGroupMessage(ChatMessage message, int localId)
        {
            LocalMessageId = localId;
            Message = message;
        }
    }
}
