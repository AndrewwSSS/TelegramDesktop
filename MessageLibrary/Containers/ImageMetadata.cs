using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace MessageLibrary.Containers
{
    [Serializable]
    public class ImageMetadata
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; }

        public string Name { get; set; }
        public int Size { get; set; }

        public static readonly string[] AllowedExtensions
            = new string[] {
                ".png",
                ".jpg",
                ".jpeg"
            };

        public ImageMetadata(string name, int size)
        {
            Name = name;
            Size = size;
        }

        public ImageMetadata() { }



    }
}
