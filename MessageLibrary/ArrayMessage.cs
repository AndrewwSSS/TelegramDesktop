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
    public class ArrayMessage<T> : Message, ITypedMessage
    {
        public T[] Array { get; set; }
        public Type ContainedType { get; private set; }
        public ArrayMessage(IEnumerable<T> arr)
        {
            Array = arr.ToArray();
            Type = MessageType.Array;
            ContainedType = typeof(T);
        }
    }
}
