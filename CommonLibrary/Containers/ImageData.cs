using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CommonLibrary.Containers
{
    [Serializable]
    public class ImageData
    {
        private byte[] bytes;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; }

        public byte[] Bytes
        {
            get => bytes;
            set
            {
                bytes = value;

                MemoryStream stream = new MemoryStream(bytes);
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = stream;
                image.EndInit();
            }
        }

        [NotMapped]
        public ImageSource ImageSource { get; set; }

        public ImageData(byte[] data) => Bytes = data;

        public ImageData() { }

    }

}
