using MessageLibrary.Containers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageLibrary
{
    /// <summary>
    /// Специальный вид сообщения для TcpFileClientWrap.
    /// Не используйте с TcpClientWrap, попытка десериализации объекта вызовет ошибку
    /// </summary>
    [Serializable]
    public class FileMessage : Message
    {
        public byte[] Data { get; set; }
        public int Id { get; set; }

        public bool IsImage { get; set; }

        public FileMessage(FileData data, int fileId)
        {
            Data = data.Bytes;
            Id = fileId;
            IsImage = false;
        }
        public FileMessage(ImageData data, int imageId)
        {
            Data = data.Bytes;
            Id = imageId;
            IsImage = true;
        }

        public override byte[] ToByteArray()
        {
            MemoryStream ms = new MemoryStream();
            byte[] idBytes = BitConverter.GetBytes(Id);
            byte[] boolBytes = BitConverter.GetBytes(IsImage);
            byte[] dataSizeBytes = BitConverter.GetBytes(Data.Length);
            ms.Write(idBytes, 0, 4);
            ms.Write(boolBytes, 0, boolBytes.Length);
            ms.Write(dataSizeBytes, 0, 4);
            ms.Write(Data, 0, Data.Length);
            return ms.ToArray();
        }
    }
}
