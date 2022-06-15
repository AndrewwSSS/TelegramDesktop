using MessageLibrary;
using System;
using System.Collections.Generic;

namespace CommonLibrary.Messages.Files
{
    [Serializable]
    public class MetadataResultMessage : Message
    {
        //LocalId - DbId
        public List<KeyValuePair<int, int>> Images { get; set; }
        public List<KeyValuePair<int, int>> Files { get; set; }


        public MetadataResultMessage(List<KeyValuePair<int, int>> images, List<KeyValuePair<int, int>> files)
        {
            Images = images;
            Files = files;
        }



    }
}
