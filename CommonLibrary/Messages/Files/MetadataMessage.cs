using CommonLibrary.Containers;
using MessageLibrary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonLibrary.Messages.Files
{
    [Serializable]
    public class MetadataMessage : Message
    {
        // Списки пар LocalID - Image/File metadata
        public List<KeyValuePair<int, ImageMetadata>> Images { get; set; }
        public List<KeyValuePair<int, FileMetadata>> Files { get; set; }

        public int LocalMessageId { get; set; }
        public MetadataMessage(int msgId, IEnumerable<KeyValuePair<int, ImageMetadata>> images = null, IEnumerable<KeyValuePair<int, FileMetadata>> files = null)
        {
            LocalMessageId = msgId;
            Images = images?.ToList();
            Files = files?.ToList();
        }

    }
}
