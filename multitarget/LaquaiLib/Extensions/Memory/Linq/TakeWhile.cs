namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.TakeWhile{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TSource> TakeWhile(Func<TSource, bool> predicate)
        {
            var count = 0;
            for (var i = 0; i < source.Length; i++)
            {
                if (!predicate(source[i]))
                    break;
                count++;
            }
            return source[..count];
        }

        /// <inheritdoc cref="Enumerable.TakeWhile{TSource}(IEnumerable{TSource}, Func{TSource, int, bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TSource> TakeWhile(Func<TSource, int, bool> predicate)
        {
            var count = 0;
            for (var i = 0; i < source.Length; i++)
            {
                if (!predicate(source[i], i))
                    break;
                count++;
            }
            return source[..count];
        }
    }

    extension<TSource>(Span<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.TakeWhile{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<TSource> TakeWhile(Func<TSource, bool> predicate)
        {
            var count = 0;
            for (var i = 0; i < source.Length; i++)
            {
                if (!predicate(source[i]))
                    break;
                count++;
            }
            return source[..count];
        }

        /// <inheritdoc cref="Enumerable.TakeWhile{TSource}(IEnumerable{TSource}, Func{TSource, int, bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<TSource> TakeWhile(Func<TSource, int, bool> predicate)
        {
            var count = 0;
            for (var i = 0; i < source.Length; i++)
            {
                if (!predicate(source[i], i))
                    break;
                count++;
            }
            return source[..count];
        }
    }
}