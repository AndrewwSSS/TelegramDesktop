﻿using System.Collections.Generic;

namespace CommonLibrary.Messages.Files
{
    public class MetadataResultMessage
    {
        //LocalId - DbId
        public List<KeyValuePair<int, int>> Images { get; set; }
        public List<KeyValuePair<int, int>> Files { get; set; }


        public MetadataResultMessage()
        {
            Images = new List<KeyValuePair<int, int>>();
            Files = new List<KeyValuePair<int, int>>();
        }

    }
}