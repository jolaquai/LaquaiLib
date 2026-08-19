namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.SkipWhile{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TSource> SkipWhile(Func<TSource, bool> predicate)
        {
            var newStart = source.Length;
            for (var i = 0; i < source.Length; i++)
                if (!predicate(source[i]))
                {
                    newStart = i;
                    break;
                }
            return source[newStart..];
        }

        /// <inheritdoc cref="Enumerable.SkipWhile{TSource}(IEnumerable{TSource}, Func{TSource, int, bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TSource> SkipWhile(Func<TSource, int, bool> predicate)
        {
            var newStart = source.Length;
            for (var i = 0; i < source.Length; i++)
                if (!predicate(source[i], i))
                {
                    newStart = i;
                    break;
                }
            return source[newStart..];
        }
    }

    extension<TSource>(Span<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.SkipWhile{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<TSource> SkipWhile(Func<TSource, bool> predicate)
        {
            var newStart = source.Length;
            for (var i = 0; i < source.Length; i++)
                if (!predicate(source[i]))
                {
                    newStart = i;
                    break;
                }
            return source[newStart..];
        }

        /// <inheritdoc cref="Enumerable.SkipWhile{TSource}(IEnumerable{TSource}, Func{TSource, int, bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<TSource> SkipWhile(Func<TSource, int, bool> predicate)
        {
            var newStart = source.Length;
            for (var i = 0; i < source.Length; i++)
                if (!predicate(source[i], i))
                {
                    newStart = i;
                    break;
                }
            return source[newStart..];
        }
    }
}