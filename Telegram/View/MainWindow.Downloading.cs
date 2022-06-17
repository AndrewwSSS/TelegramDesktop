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
        public List<MessageItemWrap> PendingMetadataMsg { get; set; } = new List<MessageItemWrap>();

        public Dictionary<int, MessageToGroupMessage> PendingMsgWithFiles { get; set; } = new Dictionary<int, MessageToGroupMessage>();
        public List<ImageMetadata> CachedImagesMetadata { get; set; } = new List<ImageMetadata>();
        public List<FileMetadata> CachedFilesMetadata { get; set; } = new List<FileMetadata>();

        public Dictionary<int, FileStream> FileDownloadStreams = new Dictionary<int, FileStream>();
        public Dictionary<int, FileStream> ImageDownloadStreams = new Dictionary<int, FileStream>();
        public Dictionary<int, FileMetadata> PendingFiles { get; set; } = new Dictionary<int, FileMetadata>();
        public Dictionary<int, ImageMetadata> PendingImages { get; set; } = new Dictionary<int, ImageMetadata>();

        private void FileClient_ImageChunkReceived(TcpFileClientWrap client, FileChunk chunk)
        {
            throw new NotImplementedException();
        }

        private void FileClient_FileChunkReceived(TcpFileClientWrap client, FileChunk chunk)
        {
            if (!FileDownloadStreams.ContainsKey(chunk.FileId))
            {
                FileMetadata metadata = CachedFilesMetadata.First(md => md.Id == chunk.FileId);
                Directory.CreateDirectory("Downloads");
                string saveName = $"{chunk.FileId}_{metadata.Name}";
                int i = 0;
                if (File.Exists("Downloads\\" + saveName))
                    while (File.Exists($"Downloads\\{i}_" + saveName))
                        i++;
                saveName = $"{i}_" + saveName; 
                var stream = new FileStream($"Downloads\\{saveName}", FileMode.OpenOrCreate);
                FileDownloadStreams.Add(chunk.FileId, stream);
                stream.Write(chunk.Data, 0, chunk.Data.Length);
            }
            else
            {
                FileDownloadStreams[chunk.FileId].Write(chunk.Data, 0, chunk.Data.Length);
                if (chunk.IsLast)
                    FileDownloadStreams[chunk.FileId].Close();
                FileDownloadStreams.Remove(chunk.FileId);
            }
        }
        public void AddMetadataToMessages(FileMetadata metadata)
        {
            List<MessageItemWrap> fullMessages = new List<MessageItemWrap>();
            foreach(var msg in PendingMetadataMsg)
            {
                if(msg.Message.FilesId.Contains(metadata.Id) &&
                    !msg.FilesMetadata.Contains(metadata))
                {
                    msg.FilesMetadata.Add(metadata);
                }
                if (msg.FilesMetadata.Count == msg.Message.FilesId.Count
                    && msg.ImagesMetadata.Count == msg.Message.ImagesId.Count)
                    fullMessages.Add(msg);
            }
            foreach (var msg in fullMessages)
                PendingMetadataMsg.Remove(msg);
        }
        public void AddMetadataToMessages(ImageMetadata metadata)
        {
            List<MessageItemWrap> fullMessages = new List<MessageItemWrap>();
            foreach (var msg in PendingMetadataMsg)
            {
                if (msg.Message.ImagesId.Contains(metadata.Id) &&
                    !msg.ImagesMetadata.Contains(metadata))
                {
                    msg.ImagesMetadata.Add(metadata);
                }
                if (msg.FilesMetadata.Count == msg.Message.FilesId.Count
                    && msg.ImagesMetadata.Count == msg.Message.ImagesId.Count)
                    fullMessages.Add(msg);
            }
            foreach (var msg in fullMessages)
                PendingMetadataMsg.Remove(msg);
        }
    }
}
