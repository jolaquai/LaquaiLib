using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Capture"/> Type.
/// </summary>
public static class CaptureExtensions
{
    [UnsafeAccessor(UnsafeAccessorKind.Field)] private static extern ref Regex _regex(this Capture capture);
    [UnsafeAccessor(UnsafeAccessorKind.Method)] private static extern ref string get_Text(this Capture capture);

    /// <summary>
    /// Retrieves the <see cref="Regex"/> instance that was used to create this <see cref="Capture"/>.
    /// </summary>
    /// <param name="capture">The <see cref="Capture"/> instance to retrieve the <see cref="Regex"/> instance from.</param>
    /// <returns>The <see cref="Regex"/> instance that was used to create this <see cref="Capture"/>.</returns>
    public static Regex GetRegex(this Capture capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        // Throw away the ref
        return capture._regex();
    }

    /// <summary>
    /// Retrieves the original <see cref="string"/> that was matched by a <see cref="Regex"/> instance to produce this <paramref name="capture"/>.
    /// </summary>
    /// <param name="capture">The <see cref="Capture"/> instance to retrieve the original <see cref="string"/> from.</param>
    /// <returns>The original <see cref="string"/> that was matched by a <see cref="Regex"/> instance to produce this <paramref name="capture"/>.</returns>
    public static string GetMatchedText(this Capture capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        // Throw away the ref
        return capture.get_Text();
    }
}
