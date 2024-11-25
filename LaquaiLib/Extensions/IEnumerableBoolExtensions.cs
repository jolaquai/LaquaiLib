namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="IEnumerable{T}"/> of <see cref="bool"/> Type.
/// </summary>
public static class IEnumerableBoolExtensions
{
    /// <summary>
    /// Determines whether all elements of a sequence of <see cref="bool"/> values are true.
    /// </summary>
    /// <param name="source">The sequence of <see cref="bool"/> values to check.</param>
    /// <returns>A value that indicates whether all elements of the sequence are true.</returns>
    public static bool All(this IEnumerable<bool> source) => source.All(static x => x);
}
