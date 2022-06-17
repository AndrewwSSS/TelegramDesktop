using CommonLibrary.Containers;
using System.Collections.Generic;

namespace CommonLibrary.Messages.Users
{

    public class FileDownload
    {

        public List<FileChunk> Chunks { get; set; }
        public int RightCount { get; set; } = -1;
        public bool isComplete => Chunks.Count == RightCount;

        public FileDownload()
        {
            Chunks = new List<FileChunk>();
        }

        public List<FileChunk> GetOrderedChanks()
        {
            //tmp
            return Chunks;
        }

    }
}
