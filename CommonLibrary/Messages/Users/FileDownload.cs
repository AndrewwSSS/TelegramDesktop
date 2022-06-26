using CommonLibrary.Containers;
using System.Collections.Generic;
using System.Linq;

namespace CommonLibrary.Messages.Users
{

    public class FileDownload
    {

        public List<FileChunk> Chunks { get; set; }
        //public int RightCount { get; set; } = -1;
        //public bool isCompleted => Chunks.Count == RightCount;

        public FileDownload() {
            Chunks = new List<FileChunk>();
        }




        //public List<FileChunk> GetOrderedChanks() {
        //    return Chunks.OrderBy(c => c.Order).ToList();
        //}

        public void AddChunk(FileChunk chunk)
        {
            Chunks.Add(chunk);

            //if (chunk.IsLast)
            //    RightCount = chunk.Order + 1;

        }   
    
    }
}
