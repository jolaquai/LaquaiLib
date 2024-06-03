namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Range"/> Type.
/// </summary>
public static class RangeExtensions
{
    /// <summary>
    /// Returns an <see cref="IEnumerable{T}"/> of <see cref="int"/>s that are within the given <paramref name="range"/>.
    /// </summary>
    /// <param name="range">The <see cref="Range"/> to get the range from.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="int"/>s that are within the given <paramref name="range"/>.</returns>
    public static IEnumerable<int> GetRange(this Range range)
    {
        return range.Start.IsFromEnd || range.End.IsFromEnd
            ? throw new ArgumentException("Range indices cannot be from the end since there is no end to reference from.", nameof(range))
            : Enumerable.Range(range.Start.Value, range.End.Value - range.Start.Value);
    }
    /// <summary>
    /// Returns an <see cref="IEnumerator{T}"/> of <see cref="int"/>s that may be used to iterate through the numbers within the given <paramref name="range"/>.
    /// </summary>
    /// <param name="range">The <see cref="Range"/> to get the range from.</param>
    /// <returns>The <see cref="IEnumerator{T}"/> as described.</returns>
    /// <remarks>
    /// This wouldn't typically be called directly, but rather through a <see langword="foreach"/> loop.
    /// </remarks>
    public static IEnumerator<int> GetEnumerator(this Range range) => range.GetRange().GetEnumerator();
}
