using System.IO;
using System.Windows.Media.Imaging;

namespace LaquaiLib;

public static class Convert
{
    public static class ImageSource
    {
        public static System.Windows.Media.ImageSource FromByteArray(byte[] imageData)
        {
            BitmapImage biImg = new();
            MemoryStream ms = new(imageData);
            biImg.BeginInit();
            biImg.StreamSource = ms;
            biImg.EndInit();

            System.Windows.Media.ImageSource imgSrc = biImg;

            return imgSrc;
        }
    }
}