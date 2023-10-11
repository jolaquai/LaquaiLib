namespace LaquaiLib.Extensions;

/// <summary>
/// Provides Extension Methods for the <see cref="Comparison{T}"/> <see langword="delegate"/> Type.
/// </summary>
public static class ComparisonExtensions
{
    /// <summary>
    /// Creates an <see cref="IComparer{T}"/> instance from the specified <see cref="Comparison{T}"/> <see langword="delegate"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the objects to compare.</typeparam>
    /// <param name="comparison">The <see cref="Comparison{T}"/> <see langword="delegate"/> to convert.</param>
    /// <returns>The <see cref="IComparer{T}"/> instance.</returns>
    public static IComparer<T> AsComparer<T>(this Comparison<T> comparison) => Comparer<T>.Create(comparison);
}
