using CommonLibrary.Containers;
using MessageLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Messages.Files
{
    [Serializable]
    public class MetadataMessage : Message
    {
        public List<ImageMetadata> Images { get; set; }
        public List<FileMetadata> Files { get; set; }
        public MetadataMessage(IEnumerable<ImageMetadata> images = null, IEnumerable<FileMetadata> files = null)
        {
            Images = images?.ToList();
            Files = files?.ToList();
        }

    }
}
