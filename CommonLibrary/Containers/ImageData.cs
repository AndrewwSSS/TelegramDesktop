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
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; }

        public byte[] Data;

        [NotMapped]
        public ImageSource ImageSource
        {
            get
            {
                MemoryStream stream = new MemoryStream(Data);
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = stream;
                image.EndInit();
                return image;
            }
        }

        public ImageData(byte[] data) => Data = data;

        public ImageData() { }

    }

}
