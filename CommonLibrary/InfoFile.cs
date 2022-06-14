using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary
{
    public class InfoFile
    {
        public int FileId { get; set; }
        public bool IsFile { get; set; }

        public InfoFile(int fileId, bool isFile)
        {
            FileId = fileId;
            IsFile = isFile;
        }
    }
}
