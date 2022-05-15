using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary
{
    [Serializable]
    public class DictionaryWrap<T1, T2>
    {
        public T1 Key;
        public T2 Value;
    }
}
