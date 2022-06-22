using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Containers
{
    public class FileMetadataViewModel
    {
        public FileMetadata Value { get; set; }
        public FileMetadataViewModel()
        {

        }
        public FileMetadataViewModel(FileMetadata value)
        {
            Value = value;
        }
    }
}
