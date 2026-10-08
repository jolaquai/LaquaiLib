namespace LaquaiLib.Windows.Extensions;

/// <summary>
/// Provides extensions for the <see cref="Color"/> type.
/// </summary>
public static class ColorExtensions
{
    extension(in Color color)
    {
        /// <summary>
        /// Formats the <see cref="Color"/> as a HTML color string.
        /// </summary>
        /// <returns>The HTML color string.</returns>
        public string Html => string.Create(9, (color.R, color.G, color.B, color.A), static (span, state) =>
        {
            span[0] = '#';
            state.Item1.TryFormat(span.Slice(1, 2), out _, "X2");
            state.Item2.TryFormat(span.Slice(3, 2), out _, "X2");
            state.Item3.TryFormat(span.Slice(5, 2), out _, "X2");
            state.Item4.TryFormat(span.Slice(7, 2), out _, "X2");
        });
    }
}
