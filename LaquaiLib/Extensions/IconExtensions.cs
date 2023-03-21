﻿using System.Windows.Interop;
using System.Windows.Media;

namespace LaquaiLib.Extensions;

public static class IconExtensions
{
    /// <summary>
    /// Converts this <see cref="Icon"/> instance to an <see cref="ImageSource"/>.
    /// </summary>
    /// <param name="icon"></param>
    /// <returns></returns>
    public static ImageSource ToImageSource(this Icon icon) => Imaging.CreateBitmapSourceFromHIcon(icon.Handle, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
}