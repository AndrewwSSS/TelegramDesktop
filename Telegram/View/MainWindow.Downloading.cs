using CommonLibrary.Containers;
using CommonLibrary.Messages;
using CommonLibrary.Messages.Files;
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
        public List<MessageItemWrap> PendingMetadataMsg = new List<MessageItemWrap>();
        
        public List<ImageMetadata> CachedImagesMetadata = new List<ImageMetadata>();
        public List<FileMetadata> CachedFilesMetadata = new List<FileMetadata>();

        public Dictionary<int, FileStream> FileDownloadStreams = new Dictionary<int, FileStream>();
        public Dictionary<int, FileStream> ImageDownloadStreams = new Dictionary<int, FileStream>();
        private void FileClient_ImageChunkReceived(TcpFileClientWrap client, int id, byte[] chunk, bool isLast)
        {
            
        }

        private void FileClient_FileChunkReceived(TcpFileClientWrap client, int id, byte[] chunk, bool isLast)
        {
            if (!FileDownloadStreams.ContainsKey(id))
            {
                FileMetadata metadata = CachedFilesMetadata.First(md => md.Id == id);
                var stream = new FileStream($"Downloads/{id}_{metadata.Name}", FileMode.OpenOrCreate);
                FileDownloadStreams.Add(id, stream);
                stream.Write(chunk, 0, chunk.Length);
            }
            else
                FileDownloadStreams[id].Write(chunk, 0, chunk.Length);
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
