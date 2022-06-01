using MessageLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Messages
{
    public enum DataRequestType
    {
        User,
        Image,
        File
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
