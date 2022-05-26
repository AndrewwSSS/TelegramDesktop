using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CommonLibrary.Containers
{
    [Serializable]
    public class ImageContainer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; }

        private byte[] data;

        public string Name { get; set; }

        public byte[] Data
        {
            get => data;
            set
            {
                data = value;
            }
        }

        public static readonly string[] AllowedExtensions
            = new string[] {
                ".png",
                ".jpg",
                ".jpeg"
            };

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


        public static ImageContainer FromImage(Image img)
        {
            ImageContainer result = new ImageContainer();
            result.Name = "unnamed" + GetImageFormat(img);

            using (var stream = new MemoryStream())
            {
                img.Save(stream, img.RawFormat);
                result.Data = stream.ToArray();
            }
            return result;
        }

        public static ImageContainer FromFile(string path)
        {
            if (!File.Exists(path))
                throw new ArgumentException($"Не удалось создать ImageContainer: изображение {path} не существует");
            if (!AllowedExtensions.Contains(new FileInfo(path).Extension))
                throw new ArgumentException($"Не удалось создать ImageContainer: формат изображения {path} не поддерживается");
            ImageContainer result = new ImageContainer
            {
                Name = Path.GetFileName(path),
                Data = File.ReadAllBytes(path)
            };
            return result;
        }
        //public static ImageContainer FromFile(Uri uri)
        //{
        //    var stream = System.Windows.Application.GetResourceStream(uri);
        //    ImageContainer result = new ImageContainer
        //    {
        //        Name = Path.GetFileName(path),
        //        Data = File.ReadAllBytes(path)
        //    };
        //    return result;
        //}

        public static string GetImageFormat(Image img)
        {
            if (img.RawFormat.Equals(ImageFormat.Jpeg))
                return ".jpeg";
            if (img.RawFormat.Equals(ImageFormat.Bmp))
                return ".bmp";
            if (img.RawFormat.Equals(ImageFormat.Png))
                return ".png";
            if (img.RawFormat.Equals(ImageFormat.Emf))
                return ".emf";
            if (img.RawFormat.Equals(ImageFormat.Exif))
                return ".exif";
            if (img.RawFormat.Equals(ImageFormat.Gif))
                return ".gif";
            if (img.RawFormat.Equals(ImageFormat.Icon))
                return ".ico";
            if (img.RawFormat.Equals(ImageFormat.MemoryBmp))
                return ".bmp";
            if (img.RawFormat.Equals(ImageFormat.Tiff))
                return ".tiff";
            else
                return ".wmf";
        }

        /// <summary>
        /// Сохраняет этот файл на устройстве
        /// </summary>
        /// <param name="path">Путь к папке для сохранения файла</param>
        public void ToFile(string path) => File.WriteAllBytes(path, Data);

 
    }
}
