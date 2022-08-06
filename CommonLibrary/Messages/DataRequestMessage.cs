using MessageLibrary;
using System;
using System.Collections.Generic;
using System.Linq;


namespace CommonLibrary.Messages
{
    public enum DataRequestType
    {
        User,
        ImageMetaData,
        FileMetadata,
        FileData, 
        ImageData,
        UsersOnlineStatus
    }

    [Serializable]
    public class DataRequestMessage : Message
    {
        public DataRequestType Type { get; set; }
        public int[] ItemsId { get; set; }

        public DataRequestMessage() { }

        public DataRequestMessage(int id, DataRequestType type)
        {
            ItemsId = new int[] { id };
            Type = type;
        }

        public DataRequestMessage(IEnumerable<int> array, DataRequestType type)
        {
            if (array != null)
                ItemsId = array.ToArray();
            Type = type;
        }

    }
}
