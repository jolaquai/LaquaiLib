namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.All{TSource}(IEnumerable{TSource}, Func{TSource, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool All(Func<TSource, bool> predicate)
        {
            for (var i = 0; i < source.Length; i++)
            {
                if (!predicate(source[i]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}