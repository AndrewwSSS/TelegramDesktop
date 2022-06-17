using CommonLibrary.Containers;
using System.Collections.Generic;
using System.Linq;

namespace CommonLibrary.Messages.Users
{

    public class FileDownload
    {

        public List<FileChunk> Chunks { get; set; }
        public int RightCount { get; set; } = -1;
        public bool isComplete => Chunks.Count == RightCount;

        public FileDownload() {
            Chunks = new List<FileChunk>();
        }

        public List<FileChunk> GetOrderedChanks() {
            return Chunks.OrderBy(c => c.Order).ToList();
        }

    }
}
