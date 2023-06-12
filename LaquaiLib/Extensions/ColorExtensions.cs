namespace LaquaiLib.Extensions;

/// <summary>
/// Provides Extension Methods for the <see cref="Color"/> Type.
/// </summary>
public static class ColorExtensions
{
    /// <summary>
    /// Formats the <see cref="Color"/> as a HTML color string.
    /// </summary>
    /// <param name="color">The <see cref="Color"/> to format.</param>
    /// <returns>The HTML color string.</returns>
    public static string AsHtml(this Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}";
}
