using System.Windows.Interop;
using System.Windows.Media;

namespace LaquaiLib.Windows.Extensions;

/// <summary>
/// Provides extensions for the <see cref="Icon"/> type.
/// </summary>
public static class IconExtensions
{
    extension(Icon icon)
    {
        /// <summary>
        /// Converts this <see cref="Icon"/> instance to an <see cref="ImageSource"/>.
        /// </summary>
        /// <returns>The created <see cref="ImageSource"/> instance.</returns>
        public ImageSource ImageSource => Imaging.CreateBitmapSourceFromHIcon(icon.Handle, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
    }
}
