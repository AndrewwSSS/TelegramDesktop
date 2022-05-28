using MessageLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Messages
{
    public enum RequestType
    {
        User,
        Image,
        File
    }

    [Serializable]
    public class DataRequestMessage : Message
    {
        public RequestType Type { get; set; }
        public int[] GroupId { get; set; }

        public DataRequestMessage() { }

        public DataRequestMessage(int id, RequestType type)
        {
            GroupId = new int[] { id };
            Type = type;
        }

        public DataRequestMessage(IEnumerable<int> array, RequestType type)
        {
            if (array != null)
                GroupId = array.ToArray();
            Type = type;
        }

    }
}
