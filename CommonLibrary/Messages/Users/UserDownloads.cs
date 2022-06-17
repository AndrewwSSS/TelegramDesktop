using CommonLibrary.Containers;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CommonLibrary.Messages.Users
{
    public class UserDownloads
    {
        // LocalId - MetaData
        public List<KeyValuePair<int, ImageMetadata>> RemainingImages { get; set; }
        public List<KeyValuePair<int, FileMetadata>> RemainingFiles { get; set; }

        // LocalId - Unordered chunks
        public List<KeyValuePair<int, FileDownload>> ImagesInProcess { get; set; }
        public List<KeyValuePair<int, FileDownload>> FilesInProcess { get; set; }

        // LocalId - Data base id
        public List<KeyValuePair<int, int>> FinishedImages { get; set; }
        public List<KeyValuePair<int, int>> FinishedFiles { get; set; }


        // Id for user client 
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
            ImagesInProcess = new List<KeyValuePair<int, FileDownload>>();
            FilesInProcess = new List<KeyValuePair< int, FileDownload>> ();
            FinishedImages = new List<KeyValuePair<int, int>>();
            FinishedFiles = new List<KeyValuePair<int, int>>();
            RemainingImages = new List<KeyValuePair<int, ImageMetadata>> ();
            RemainingFiles = new List<KeyValuePair<int, FileMetadata>>();
          

        }


        public void ImageFinished(int localId, int DbId)
        {
            KeyValuePair<int, FileDownload> downloadPair = ImagesInProcess.FirstOrDefault(ip => ip.Key == localId);
            KeyValuePair<int, ImageMetadata> metadataPair = RemainingImages.FirstOrDefault(mp => mp.Key == localId);

            RemainingImages.Remove(metadataPair);
            ImagesInProcess.Remove(downloadPair);

            FinishedImages.Add(new KeyValuePair<int, int>(localId, DbId));
        }

        public void FileFinished(int localId, int DbId)
        {
            KeyValuePair<int, FileDownload> downloadPair = FilesInProcess.FirstOrDefault(ip => ip.Key == localId);
            KeyValuePair<int, FileMetadata> metadataPair = RemainingFiles.FirstOrDefault(mp => mp.Key == localId);

            RemainingFiles.Remove(metadataPair);
            FilesInProcess.Remove(downloadPair);

            FinishedFiles.Add(new KeyValuePair<int, int>(localId, DbId));
        }


    }
}
