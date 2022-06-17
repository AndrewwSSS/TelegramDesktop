using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Containers
{
    public class FileChunk
    {
        /// <summary>
        /// Индекс куска, показывающий, каким по счёту он был послан. Начинается с 0.
        /// </summary>
        public int Order { get; set; }

        public int FileId { get; set; }
        public byte[] Data { get; set; }
        public bool IsLast { get; set; }
        public bool IsImage { get; set; }
    }
}
