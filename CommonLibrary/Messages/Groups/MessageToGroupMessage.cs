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

        // Local 
        public List<KeyValuePair<int, int>> Files { get; set; }
        public List<KeyValuePair<int, int>> Images { get; set; }


        public MessageToGroupMessage(ChatMessage message, int localId)
        {
            LocalMessageId = localId;
            Message = message;
        }
    }
}
