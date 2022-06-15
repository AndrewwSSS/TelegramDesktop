using MessageLibrary;
using System;
using System.Collections.Generic;

namespace CommonLibrary.Messages.Groups
{
    [Serializable]
    public class MessageToGroupMessage : Message
    {
        private List<int> filesId = new List<int>();
        private List<int> imagesId = new List<int>();

        public int LocalMessageId { get; set; }
        public ChatMessage Message { get; set; }

        public List<int> FilesId { 
            get => filesId;
            set
            {
                Message.FilesId = value;
                filesId = value;
            }
        }
        public List<int> ImagesId { 
            get => imagesId;
            set
            {
                Message.ImagesId = value;
                imagesId = value;
            }
        }


        public MessageToGroupMessage(ChatMessage message, int localId)
        {
            LocalMessageId = localId;
            Message = message;
        }

        public MessageToGroupMessage()
        {
        }
    }
}
