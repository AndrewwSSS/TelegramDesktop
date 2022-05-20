using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageLibrary
{
    /// <summary>
    /// Message containing an array of arbitrary type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class ArrayMessage<T> : Message
    {
        public T[] Array { get; set; }
        public ArrayMessage(IEnumerable<T> arr)
        {
            if(arr != null)
                Array = arr.ToArray();
        }
    }
}
