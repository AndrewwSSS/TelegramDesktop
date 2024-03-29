﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.IO;

namespace CommonLibrary.Containers
{
    [Serializable]
    public class ImageMetadata
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

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
