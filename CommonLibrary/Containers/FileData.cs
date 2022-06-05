using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Containers
{
    [Serializable]
    public class FileData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

    
        public byte[] Bytes { get; set; }


        public FileData(byte[] data) => Bytes = data;

        public FileData() { }

        public static FileData FromFile(string path)
        {
            if (!File.Exists(path))
                return null;
            return new FileData(File.ReadAllBytes(path));
        }
    }
}
