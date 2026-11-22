namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(in ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.Aggregate{TSource}(IEnumerable{TSource}, Func{TSource, TSource, TSource})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource Aggregate(Func<TSource, TSource, TSource> func)
        {
            var result = source[0];
            for (var i = 1; i < source.Length; i++)
                result = func(result, source[i]);
            return result;
        }

        /// <inheritdoc cref="Enumerable.Aggregate{TSource, TAccumulate}(IEnumerable{TSource}, TAccumulate, Func{TAccumulate, TSource, TAccumulate})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TAccumulate Aggregate<TAccumulate>(TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
        {
            for (var i = 0; i < source.Length; i++)
                seed = func(seed, source[i]);
            return seed;
        }

        /// <inheritdoc cref="Enumerable.Aggregate{TSource, TAccumulate, TResult}(IEnumerable{TSource}, TAccumulate, Func{TAccumulate, TSource, TAccumulate}, Func{TAccumulate, TResult})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TResult Aggregate<TAccumulate, TResult>(TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
        {
            for (var i = 0; i < source.Length; i++)
                seed = func(seed, source[i]);
            return resultSelector(seed);
        }
    }
}