using MessageLibrary;
using System;
using System.Collections.Generic;

namespace CommonLibrary.Messages.Groups
{
    [Serializable]
    public class ChatMessageSendResult : Message
    {
        public int LocalId { get; set; }
        public int MessageId { get; set; }
        public List<KeyValuePair<int, int>> FilesId { get; set; }
        public List<KeyValuePair<int, int>> ImagesId { get; set; }

        public ChatMessageSendResult(int localId, int dbId) : this()
        {
            LocalId = localId;
            MessageId = dbId;
        }

        public ChatMessageSendResult()
        {
            FilesId = new List<KeyValuePair<int, int>>();
            ImagesId = new List<KeyValuePair<int, int>>();
        }

    }
}
