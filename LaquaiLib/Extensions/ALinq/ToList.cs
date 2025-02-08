namespace LaquaiLib.Extensions.ALinq;

// Provides parallel extensions for the System.Linq.Enumerable.ToList family of methods.
public static partial class IEnumerableExtensions
{
    public static Task<List<TSource>> ToListAsync<TSource>(this IEnumerable<TSource> source, CancellationToken cancellationToken = default)
        => Task.Run(() => source.ToList(), cancellationToken);

}