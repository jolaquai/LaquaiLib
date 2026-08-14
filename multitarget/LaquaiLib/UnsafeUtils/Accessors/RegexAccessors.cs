using System.Text.RegularExpressions;

namespace LaquaiLib.UnsafeUtils.Accessors;

public static class RegexAccessors
{
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref Regex _regex(Match _);
    [UnsafeAccessor(UnsafeAccessorKind.Method)] public static extern string get_Text(Capture _);
}
