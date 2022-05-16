using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace CommonLibrary
{
    [Serializable]
    public class FileContainer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; }

        public string Name { get; set; }
        public byte[] Data { get; set; }
        
        public static FileContainer FromFile(string path)
        {
            if (!File.Exists(path))
                throw new ArgumentException($"Не удалось создать FileContainer: файл {path} не существует");
            FileContainer result = new FileContainer
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