namespace LaquaiLib.Analyzers;

internal static class Util
{
    /// <summary>
    /// Generates an analyzer ID from the specified type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>A string representing the analyzer ID.</returns>
    internal static string GetAnalyzerId(Type type) => $"{type.Namespace}.{type.Name}";

    /// <summary>
    /// Generates an analyzer category from the specified type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>A string representing the analyzer category.</returns>
    internal static string GetAnalyzerCategory(Type type) => type.Namespace;
}
