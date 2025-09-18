namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.ElementAt{TSource}(IEnumerable{TSource}, int)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource ElementAt(int index) => source[index];

        /// <inheritdoc cref="Enumerable.ElementAt{TSource}(IEnumerable{TSource}, Index)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource ElementAt(Index index) => source[index];
    }
}