using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CommonLibrary
{
    public static class ImageConverter
    {
        public static ImageSource Resize(byte[] source, int width, int height)
        {
            MemoryStream ms1 = new MemoryStream(source);

            Bitmap bitmap = new Bitmap(Image.FromStream(ms1));

            Bitmap bitmapResult = new Bitmap(bitmap, width, height);

            MemoryStream ms2 = new MemoryStream();
            bitmapResult.Save(ms2, System.Drawing.Imaging.ImageFormat.Png);

            BitmapImage result = new BitmapImage();
            result.BeginInit();
            result.StreamSource = ms2;
            result.EndInit();

            return result;
        }
    }
}
