using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MessageLibrary.Containers
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
            Name = name;
            Size = size;
        }

        public FileMetadata() { }

    }
}
