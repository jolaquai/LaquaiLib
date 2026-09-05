namespace LaquaiLib.Analyzers.Shared;

/// <summary>
/// Shared logic for locating the separator argument of a <c>string.Join</c> call.
/// Used by both <c>JoinWithEmptySeparatorAnalyzer</c> and <c>JoinWithEmptySeparatorFixer</c> so the two stay in lockstep.
/// </summary>
public static class JoinWithEmptySeparatorHelper
{
    /// <summary>
    /// The key under which the analyzer carries the index of the separator argument to the fixer.
    /// </summary>
    public const string SeparatorIndexKey = "SeparatorIndex";

    /// <summary>
    /// Gets the index of the argument bound to the separator parameter, or <c>-1</c> if no argument is.
    /// </summary>
    /// <param name="arguments">The arguments of the <c>string.Join</c> call.</param>
    /// <param name="separatorParameterName">The name of the invoked overload's first parameter.</param>
    public static int GetSeparatorIndex(SeparatedSyntaxList<ArgumentSyntax> arguments, string separatorParameterName)
    {
        if (arguments.Count == 0)
            return -1;
        if (arguments[0].NameColon is null)
            return 0;

        for (var i = 0; i < arguments.Count; i++)
            if (arguments[i].NameColon?.Name.Identifier.ValueText == separatorParameterName)
                return i;
        return -1;
    }
}
