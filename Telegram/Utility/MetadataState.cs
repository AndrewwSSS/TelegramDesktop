using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegram.Utility
{
    public class MetadataState
    {
        public List<int> FilesLocalId { get; set; }
        public List<string> FilesName { get; set; }
        public List<int> ImagesLocalId { get; set; }
        public List<string> ImagesName { get; set; }
    }
}
