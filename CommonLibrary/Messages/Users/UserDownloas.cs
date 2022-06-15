using CommonLibrary.Containers;
using System.Collections.Generic;
using System.IO;

namespace CommonLibrary.Messages.Users
{
    public class UserDownloas
    {
        // LocalId - MetaData
        public List<KeyValuePair<int, ImageMetadata>> RemainingImages { get; set; }
        public List<KeyValuePair<int, FileMetadata>> RemainingFiles { get; set; }

        // LocalId - finished data
        public List<KeyValuePair<int, MemoryStream>> ImagesInProcess { get; set; }
        public List<KeyValuePair<int, MemoryStream>> FilesInProcess { get; set; }

        public List<KeyValuePair<int, int>> FinishedImages { get; set; }
        public List<KeyValuePair<int, int>> FinishedFiles { get; set; }


        public bool IsCompleted
        {
            get
            {
                return RemainingFiles.Count == 0 && RemainingImages.Count == 0;
            }
        }

        public UserDownloas(List<KeyValuePair<int, ImageMetadata>> localImages,
            List<KeyValuePair<int, FileMetadata>> localFiles)
        {
            RemainingImages = localImages;
            RemainingFiles = localFiles;
        }


     
    }
}
