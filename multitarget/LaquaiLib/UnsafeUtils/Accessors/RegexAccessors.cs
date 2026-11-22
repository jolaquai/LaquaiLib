using System.Text.RegularExpressions;

namespace LaquaiLib.UnsafeUtils.Accessors;

/// <summary>
/// Contains accessors for the <see cref="System.Text.RegularExpressions"/> family of types.
/// </summary>
public static class RegexAccessors
{
    /// <summary>
    /// Accesses the private field <c>_regex</c> of a <see cref="Match"/> instance.
    /// </summary>
    /// <param name="_">The <see cref="Match"/> instance to access.</param>
    /// <returns>A <see langword="ref"/> into the field.</returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref Regex _regex(Match _);

    /// <summary>
    /// Accesses the private method <c>get_Text</c> of a <see cref="Capture"/> instance.
    /// </summary>
    /// <param name="_">The <see cref="Capture"/> instance to access.</param>
    /// <returns>The value of the <c>Text</c> property.</returns>
    [UnsafeAccessor(UnsafeAccessorKind.Method)] public static extern string get_Text(Capture _);
}
