using System;
using System.IO;

namespace CommonLibrary
{
    public class ChatFile
    {
        public string Name { get; set; }
        public byte[] Data { get; set; }
        
        public static ChatFile FromFile(string path)
        {
            if (!File.Exists(path))
                throw new ArgumentException($"Не удалось создать ChatFile: файл {path} не существует");
            ChatFile result = new ChatFile
            {
                Name = Path.GetFileName(path),
                Data = File.ReadAllBytes(path)
            };
            return result;
        }
        /// <summary>
        /// Сохраняет этот файл на устройстве
        /// </summary>
        /// <param name="path">Путь к папке для сохранения файла</param>
        public void ToFile(string path) => File.WriteAllBytes(path, Data);
    }
}