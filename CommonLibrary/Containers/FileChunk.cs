using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Containers
{
    public class FileChunk
    {
        public int FileId { get; set; }
        /// <summary>
        /// Порядковый номер куска(на какой итерации чтения его послали)
        /// </summary>
        public int Order { get; set; }
        public byte[] Data { get; set; }
        public bool IsLast { get; set; }
        public bool IsImage { get; set; }
    }
}
