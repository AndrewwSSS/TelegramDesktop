using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;

namespace CommonLibrary.Containers
{
    [Serializable]
    public class ImageContainer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public ImageMetadata Metadata { get; set; }
        public ImageData ImageData { get; set; }

       

        public ImageContainer(string name, byte[] data)
        {
            Metadata = new ImageMetadata(name, data.Length);
            ImageData = new ImageData(data);
        }

        public ImageContainer() { }





        //public static ImageContainer FromImage(Image img)
        //{
        //    ImageContainer result = new ImageContainer();
        //    result.Name = "unnamed" + GetImageFormat(img);

        //    using (var stream = new MemoryStream())
        //    {
        //        img.Save(stream, img.RawFormat);
        //        result.Data = stream.ToArray();
        //    }
        //    return result;
        //}

        public static ImageContainer FromFile(string path)
        {
            if (!File.Exists(path))
                throw new ArgumentException($"Не удалось создать ImageContainer: изображение {path} не существует");
            if (!ImageMetadata.AllowedExtensions.Contains(new FileInfo(path).Extension))
                throw new ArgumentException($"Не удалось создать ImageContainer: формат изображения {path} не поддерживается");

            byte[] bytes = File.ReadAllBytes(path);
            ImageContainer result = new ImageContainer()
            {
                ImageData = new ImageData(bytes),
                Metadata = new ImageMetadata(Path.GetFileName(path), bytes.Length)
            };
            return result;
        }

        //public static string GetImageFormat(Image img)
        //{
        //    if (img.RawFormat.Equals(ImageFormat.Jpeg))
        //        return ".jpeg";
        //    if (img.RawFormat.Equals(ImageFormat.Bmp))
        //        return ".bmp";
        //    if (img.RawFormat.Equals(ImageFormat.Png))
        //        return ".png";
        //    if (img.RawFormat.Equals(ImageFormat.Emf))
        //        return ".emf";
        //    if (img.RawFormat.Equals(ImageFormat.Exif))
        //        return ".exif";
        //    if (img.RawFormat.Equals(ImageFormat.Gif))
        //        return ".gif";
        //    if (img.RawFormat.Equals(ImageFormat.Icon))
        //        return ".ico";
        //    if (img.RawFormat.Equals(ImageFormat.MemoryBmp))
        //        return ".bmp";
        //    if (img.RawFormat.Equals(ImageFormat.Tiff))
        //        return ".tiff";
        //    else
        //        return ".wmf";
        //}

        /// <summary>
        /// Сохраняет этот файл на устройстве
        /// </summary>
        /// <param name="path">Путь к папке для сохранения файла</param>
        public void ToFile(string path) => File.WriteAllBytes(path, ImageData.Bytes);


    }
}
