namespace LaquaiLib.Extensions;

public static partial class IEnumerableExtensions
{
    /// <summary>
    /// Determines whether all elements of a sequence of <see cref="bool"/> values are true.
    /// </summary>
    /// <param name="source">The sequence of <see cref="bool"/> values to check.</param>
    /// <returns>A value that indicates whether all elements of the sequence are true.</returns>
    public static bool All(this IEnumerable<bool> source) => source.All(static x => x);
}
