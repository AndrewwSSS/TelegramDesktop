using CommonLibrary.Containers;
using System.Collections.Generic;
using System.IO;

namespace CommonLibrary.Messages.Users
{
    public class UserDownloads
    {
        // LocalId - MetaData
        public List<KeyValuePair<int, ImageMetadata>> RemainingImages { get; set; }
        public List<KeyValuePair<int, FileMetadata>> RemainingFiles { get; set; }

        // LocalId - finished data
        public List<KeyValuePair<int, MemoryStream>> ImagesInProcess { get; set; }
        public List<KeyValuePair<int, MemoryStream>> FilesInProcess { get; set; }

        public List<KeyValuePair<int, int>> FinishedImages { get; set; }
        public List<KeyValuePair<int, int>> FinishedFiles { get; set; }

        public int ForMessageId { get; set; }


        public bool IsCompleted
        {
            get
            {
                return RemainingFiles.Count == 0 && RemainingImages.Count == 0;
            }
        }

        public UserDownloads(int msgId, List<KeyValuePair<int, ImageMetadata>> localImages,
            List<KeyValuePair<int, FileMetadata>> localFiles) : this()
        {

            ForMessageId = msgId;

            if(localImages != null)
                RemainingImages = localImages;

            if(localFiles != null)
                RemainingFiles = localFiles;

        }

        public UserDownloads()
        {
            ImagesInProcess = new List<KeyValuePair<int, MemoryStream>>();
            FilesInProcess = new List<KeyValuePair< int, MemoryStream >> ();
            FinishedImages = new List<KeyValuePair<int, int>>();
            FinishedFiles = new List<KeyValuePair<int, int>>();
            RemainingImages = new List<KeyValuePair<int, ImageMetadata>> ();
            RemainingFiles = new List<KeyValuePair<int, FileMetadata>>();

        }



    }
}
