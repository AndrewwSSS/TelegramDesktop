using CommonLibrary.Containers;
using CommonLibrary.Messages;
using CommonLibrary.Messages.Files;
using CommonLibrary.Messages.Groups;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegram
{
    public partial class MainWindow
    {
        public List<MessageItemWrap> PendingImageMsg { get; set; } = new List<MessageItemWrap>();
        public List<MessageItemWrap> PendingFileMetadataMsg { get; set; } = new List<MessageItemWrap>();
        public Dictionary<int, MessageToGroupMessage> PendingMsgWithAttachments { get; set; } = new Dictionary<int, MessageToGroupMessage>();
        public List<ImageMetadata> CachedImagesMetadata { get; set; } = new List<ImageMetadata>();
        public List<FileMetadata> CachedFilesMetadata { get; set; } = new List<FileMetadata>();

        public Dictionary<int, FileStream> FileDownloadStreams = new Dictionary<int, FileStream>();
        public Dictionary<int, FileStream> ImageDownloadStreams = new Dictionary<int, FileStream>();
        public Dictionary<int, FileMetadata> PendingFiles { get; set; } = new Dictionary<int, FileMetadata>();
        public Dictionary<int, ImageMetadata> PendingImages { get; set; } = new Dictionary<int, ImageMetadata>();

        private void FileClient_ImageChunkReceived(TcpFileClientWrap client, FileChunk chunk)
        {
            string saveName = "";
            if (!ImageDownloadStreams.ContainsKey(chunk.FileId))
            {
                var metadata = CachedImagesMetadata.First(md => md.Id == chunk.FileId);
                var dirName = "Downloads\\Files";
                Directory.CreateDirectory(dirName);
                saveName = $"{chunk.FileId}_{metadata.Name}";
                if (File.Exists(Path.Combine(dirName, saveName)))
                {
                    int i = 0;
                    while (File.Exists($"{dirName}\\{i}_" + saveName))
                        i++;
                    saveName = $"{i}_" + saveName;
                }
                var stream = new FileStream(Path.Combine(dirName, saveName), FileMode.OpenOrCreate);
                ImageDownloadStreams.Add(chunk.FileId, stream);
                stream.Write(chunk.Data, 0, chunk.Data.Length);
            }
            else
                ImageDownloadStreams[chunk.FileId].Write(chunk.Data, 0, chunk.Data.Length);
            

            if (chunk.IsLast)
            {
                ImageDownloadStreams[chunk.FileId].Close();
                CachedImages.Add(chunk.FileId, saveName);
                List<MessageItemWrap> fullMessages = new List<MessageItemWrap>();
                foreach(var item in PendingImageMsg.Where(msg => msg.Message.ImagesId.Contains(chunk.FileId)))
                {
                    item.Images.Add(new StringViewModel(saveName));
                    if (item.FilesMetadata.Count == item.Message.FilesId.Count)
                        fullMessages.Add(item);
                }
                foreach (var item in fullMessages)
                    PendingImageMsg.Remove(item);
                
                ImageDownloadStreams.Remove(chunk.FileId);
            }
        }

        private void FileClient_FileChunkReceived(TcpFileClientWrap client, FileChunk chunk)
        {
            if (!FileDownloadStreams.ContainsKey(chunk.FileId))
            {
                var metadata = CachedFilesMetadata.First(md => md.Id == chunk.FileId);
                var dirName = "Downloads\\Files";
                Directory.CreateDirectory(dirName);
                string saveName = $"{chunk.FileId}_{metadata.Name}";
                if (File.Exists(Path.Combine(dirName, saveName)))
                {
                    int i = 0;
                    while (File.Exists($"{dirName}\\{i}_" + saveName))
                        i++;
                    saveName = $"{i}_" + saveName;
                }
                var stream = new FileStream(Path.Combine(dirName, saveName), FileMode.OpenOrCreate);
                FileDownloadStreams.Add(chunk.FileId, stream);
                stream.Write(chunk.Data, 0, chunk.Data.Length);
            }
            else
            {
                FileDownloadStreams[chunk.FileId].Write(chunk.Data, 0, chunk.Data.Length);
            }

            if (chunk.IsLast)
            {
                FileDownloadStreams[chunk.FileId].Close();
                FileDownloadStreams.Remove(chunk.FileId);
            }
        }
        public void AddMetadataToMessages(FileMetadata metadata)
        {
            List<MessageItemWrap> fullMessages = new List<MessageItemWrap>();
            foreach(var msg in PendingFileMetadataMsg)
            {
                if(msg.Message.FilesId.Contains(metadata.Id) &&
                    !msg.FilesMetadata.Contains(metadata))
                    msg.FilesMetadata.Add(metadata);

                if (msg.FilesMetadata.Count == msg.Message.FilesId.Count)
                    fullMessages.Add(msg);
            }
            foreach (var msg in fullMessages)
                PendingFileMetadataMsg.Remove(msg);
        }
    }
}
