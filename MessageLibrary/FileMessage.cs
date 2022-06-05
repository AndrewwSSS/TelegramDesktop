using System;
using System.IO;
using System.Text;

namespace MessageLibrary
{
    [Serializable]
    public class FileMessage : Message
    {
        public string FileName { get; set; }
        public byte[] Data { get; set; }

        public FileMessage(string fileName, byte[] data)
        {
            FileName = fileName;
            Data = data;
        }

        public override byte[] ToByteArray()
        {
            MemoryStream ms = new MemoryStream();

            byte[] nameLenBytes = BitConverter.GetBytes(FileName.Length);
            byte[] nameBytes = Encoding.UTF8.GetBytes(FileName);
            byte[] dataSizeBytes = BitConverter.GetBytes(Data.Length);
            ms.Write(nameLenBytes, 0, nameLenBytes.Length);
            ms.Write(nameBytes, 0, nameBytes.Length);
            ms.Write(dataSizeBytes, 0, dataSizeBytes.Length);
            ms.Write(Data, 0, Data.Length);
            return ms.ToArray();
        }
    }
}
