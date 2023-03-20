namespace LaquaiLib.Extensions;

public static class IEnumerableExtensions
{
    public static IEnumerable<T> Select<T>(this IEnumerable<T> source) => source.Select(item => item);
}
