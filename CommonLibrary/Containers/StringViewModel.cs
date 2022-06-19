using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Containers
{
    public class StringViewModel
    {
        public string String { get; set; }

        public StringViewModel()
        {

        }
        public StringViewModel(string str) => String = str;
    }
}
