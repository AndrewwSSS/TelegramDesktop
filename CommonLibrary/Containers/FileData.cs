using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

    
        public byte[] Data { get; set; }


        public FileData(byte[] data) => Data = data;

        public FileData() { }

    }
}
