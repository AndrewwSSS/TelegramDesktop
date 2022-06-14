using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary
{
    public class PreparingFileData
    {
        public int FileId { get; set; }
        public bool IsFile { get; set; }

        public string FileName { get; set; }


        public PreparingFileData(int fileId, bool isFile)
        {
            FileId = fileId;
            IsFile = isFile;
        }
    }
}
