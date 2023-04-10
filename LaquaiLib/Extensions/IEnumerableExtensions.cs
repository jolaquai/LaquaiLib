namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="IEnumerable{T}"/> Type.
/// </summary>
public static class IEnumerableExtensions
{
    public static IEnumerable<T> Select<T>(this IEnumerable<T> source) => source.Select(item => item);
}
