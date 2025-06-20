namespace LaquaiLib.Extensions.MemoryExtensions.Linq;

public static partial class MemoryExtensions
{
    extension<TSource>(ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.ElementAtOrDefault{TSource}(IEnumerable{TSource}, int)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource ElementAtOrDefault(int index) => index >= 0 && index < source.Length ? source[index] : default;

        /// <inheritdoc cref="Enumerable.ElementAtOrDefault{TSource}(IEnumerable{TSource}, Index)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource ElementAtOrDefault(Index index)
        {
            var offset = index.GetOffset(source.Length);
            return offset < 0 || offset >= source.Length ? default : source[offset];
        }
    }
}