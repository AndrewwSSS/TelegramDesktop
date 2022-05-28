using MessageLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Messages
{
    [Serializable]
    public class DataRequestResultMessage<T> : Message
    {
        public T[] Result { get; set; }

        public DataRequestResultMessage() { }

        public DataRequestResultMessage(IEnumerable<T> array)
        {
            if (array != null)
                Result = array.ToArray();
        }
    }
}
