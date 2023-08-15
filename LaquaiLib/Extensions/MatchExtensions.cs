using System.Reflection;
using System.Text.RegularExpressions;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Match"/> Type.
/// </summary>
public static class MatchExtensions
{
    /// <summary>
    /// Retrieves the <see cref="Regex"/> instance that was used to create this <see cref="Match"/>.
    /// </summary>
    /// <param name="match">The <see cref="Match"/> instance to retrieve the <see cref="Regex"/> instance from.</param>
    /// <returns>The <see cref="Regex"/> instance that was used to create this <see cref="Match"/>.</returns>
    public static Regex GetRegex(this Match match)
    {
        ArgumentNullException.ThrowIfNull(match);
        return typeof(Match).GetField("_regex", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue<Regex>(match);
    }
}
