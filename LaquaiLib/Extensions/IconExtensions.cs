using System.Windows.Interop;
using System.Windows.Media;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Icon"/> Type.
/// </summary>
public static class IconExtensions
{
    /// <summary>
    /// Converts this <see cref="Icon"/> instance to an <see cref="ImageSource"/>.
    /// </summary>
    /// <param name="icon">The <see cref="Icon"/> instance to convert.</param>
    /// <returns>The created <see cref="ImageSource"/> instance.</returns>
    public static ImageSource ToImageSource(this Icon icon) => Imaging.CreateBitmapSourceFromHIcon(icon.Handle, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
}
