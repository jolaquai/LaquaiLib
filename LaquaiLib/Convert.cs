using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LaquaiLib;

public static class Convert
{
    public static ImageSource ToImageSource(byte[] imageData) => ToImageSource(new MemoryStream(imageData));

    public static ImageSource ToImageSource(Stream stream) => (ImageSource)new ImageSourceConverter().ConvertFrom(stream);
}