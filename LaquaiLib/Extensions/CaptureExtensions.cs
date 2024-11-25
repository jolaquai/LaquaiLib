using System.Reflection;
using System.Text.RegularExpressions;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Capture"/> Type.
/// </summary>
public static class CaptureExtensions
{
    /// <summary>
    /// Retrieves the original <see cref="string"/> that was matched by a <see cref="Regex"/> instance to produce this <paramref name="capture"/>.
    /// </summary>
    /// <param name="capture">The <see cref="Capture"/> instance to retrieve the original <see cref="string"/> from.</param>
    /// <returns>The original <see cref="string"/> that was matched by a <see cref="Regex"/> instance to produce this <paramref name="capture"/>.</returns>
    public static string GetSource(this Capture capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        return typeof(Capture).GetProperty("Text", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue<string>(capture);
    }
}
