namespace LaquaiLib.Windows.Extensions;

/// <summary>
/// Provides extensions for the <see cref="Color"/> type.
/// </summary>
public static class ColorExtensions
{
    extension(Color color)
    {
        /// <summary>
        /// Formats the <see cref="Color"/> as a HTML color string.
        /// </summary>
        /// <returns>The HTML color string.</returns>
        public string Html => $"#{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}";
    }
}
