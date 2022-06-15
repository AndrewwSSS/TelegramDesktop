using CommonLibrary.Containers;
using System.Collections.Generic;
using System.IO;

namespace CommonLibrary.Messages.Users
{
    public class UserDownload
    {
        // LocalId - MetaData
        private List<KeyValuePair<int, ImageMetadata>> RemainingImages { get; set; }
        private List<KeyValuePair<int, FileMetadata>> RemainingFiles { get; set; }

        // LocalId - finished data
        private List<KeyValuePair<int, MemoryStream>> ImagesInProcess { get; set; }
        private List<KeyValuePair<int, MemoryStream>> FilesInProcess { get; set; }

        private List<KeyValuePair<int, int>> FinishedImages { get; set; }
        private List<KeyValuePair<int, int>> FinishedFiles { get; set; }


        public bool IsCompleted
        {
            get
            {
                return RemainingFiles.Count == 0 && RemainingImages.Count == 0;
            }
        }

        public UserDownload(List<KeyValuePair<int, ImageMetadata>> localImages,
            List<KeyValuePair<int, FileMetadata>> localFiles)
        {
            RemainingImages = localImages;
            RemainingFiles = localFiles;
        }


     
    }
}
