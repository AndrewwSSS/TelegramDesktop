using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommonLibrary.Containers
{
    [Serializable]
    public class FileMetadata
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; } 
        public int Size { get; set; }

        public FileMetadata(string name, int size)
        {
            FileId = fileId;
            Name = name;
            Size = size;
        }

        public FileMetadata() { }

    }
}
