using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Match"/> Type.
/// </summary>
public static class MatchExtensions
{
    [UnsafeAccessor(UnsafeAccessorKind.Field)]
    private static extern ref Regex _regex(Match match);

    /// <summary>
    /// Retrieves the <see cref="Regex"/> instance that was used to create this <see cref="Match"/>.
    /// </summary>
    /// <param name="match">The <see cref="Match"/> instance to retrieve the <see cref="Regex"/> instance from.</param>
    /// <returns>The <see cref="Regex"/> instance that was used to create this <see cref="Match"/>.</returns>
    public static Regex GetRegex(this Match match)
    {
        ArgumentNullException.ThrowIfNull(match);
        // Throw away the ref
        return _regex(match);
    }
}
