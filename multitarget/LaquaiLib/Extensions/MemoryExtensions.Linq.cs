// THIS FILE IS CURRENTLY NOT INCLUDED FOR COMPILATION.

namespace LaquaiLib.Extensions;

public partial class MemoryExtensions
{
    extension<TSource>(ReadOnlySpan<TSource> source)
    {
        #region Aggregate
        /// <inheritdoc cref="Enumerable.Aggregate{TSource}(IEnumerable{TSource}, Func{TSource, TSource, TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource Aggregate(Func<TSource, TSource, TSource> func)
        {
            var result = source[0];
            for (var i = 1; i < source.Length; i++)
            {
                result = func(result, source[i]);
            }
            return result;
        }

        /// <inheritdoc cref="Enumerable.Aggregate{TSource, TAccumulate}(IEnumerable{TSource}, TAccumulate, Func{TAccumulate, TSource, TAccumulate})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TAccumulate Aggregate<TAccumulate>(TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
        {
            for (var i = 0; i < source.Length; i++)
            {
                seed = func(seed, source[i]);
            }
            return seed;
        }

        /// <inheritdoc cref="Enumerable.Aggregate{TSource, TAccumulate, TResult}(IEnumerable{TSource}, TAccumulate, Func{TAccumulate, TSource, TAccumulate}, Func{TAccumulate, TResult})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TResult Aggregate<TAccumulate, TResult>(TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
        {
            for (var i = 0; i < source.Length; i++)
            {
                seed = func(seed, source[i]);
            }
            return resultSelector(seed);
        }
        #endregion

        #region Any
        /// <inheritdoc cref="Enumerable.Any{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Any() => source.Length > 0;

        /// <inheritdoc cref="Enumerable.Any{TSource}(IEnumerable{TSource}, Func{TSource, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Any(Func<TSource, bool> predicate)
        {
            for (var i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region All
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
        #endregion

        #region Append
        /// <inheritdoc cref="Enumerable.Append{TSource}(IEnumerable{TSource}, TSource)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> Append(TSource element)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region Prepend
        /// <inheritdoc cref="Enumerable.Prepend{TSource}(IEnumerable{TSource}, TSource)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> Prepend(TSource element)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region Average
        /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, int})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Average(Func<TSource, int> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, long})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Average(Func<TSource, long> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, float})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Average(Func<TSource, float> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, double})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Average(Func<TSource, double> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, decimal})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal Average(Func<TSource, decimal> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, int?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double? Average(Func<TSource, int?> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, long?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double? Average(Func<TSource, long?> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, float?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float? Average(Func<TSource, float?> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, double?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double? Average(Func<TSource, double?> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, decimal?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal? Average(Func<TSource, decimal?> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region Cast
        /// <inheritdoc cref="Enumerable.Cast{TResult}(Collections.IEnumerable)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TResult> Cast()
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region Chunk
        /// <inheritdoc cref="Enumerable.Chunk{TSource}(IEnumerable{TSource}, int)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource[]> Chunk(int size)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region Concat
        /// <inheritdoc cref="Enumerable.Concat{TSource}(IEnumerable{TSource}, IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> Concat(IEnumerable<TSource> second)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region Contains
        /// <inheritdoc cref="Enumerable.Contains{TSource}(IEnumerable{TSource}, TSource)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(TSource value)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Contains{TSource}(IEnumerable{TSource}, TSource, IEqualityComparer{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(TSource value, IEqualityComparer<TSource> comparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region AggregateBy
        /// <inheritdoc cref="Enumerable.AggregateBy{TSource, TKey, TAccumulate}(IEnumerable{TSource}, Func{TSource, TKey}, TAccumulate, Func{TAccumulate, TSource, TAccumulate}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<KeyValuePair<TKey, TAccumulate>> AggregateBy<TKey, TAccumulate>(Func<TSource, TKey> keySelector, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, IEqualityComparer<TKey> keyComparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.AggregateBy{TSource, TKey, TAccumulate}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TKey, TAccumulate}, Func{TAccumulate, TSource, TAccumulate}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<KeyValuePair<TKey, TAccumulate>> AggregateBy<TKey, TAccumulate>(Func<TSource, TKey> keySelector, Func<TKey, TAccumulate> seedSelector, Func<TAccumulate, TSource, TAccumulate> func, IEqualityComparer<TKey> keyComparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region CountBy
        /// <inheritdoc cref="Enumerable.CountBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<KeyValuePair<TKey, int>> CountBy<TKey>(Func<TSource, TKey> keySelector, IEqualityComparer<TKey> keyComparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region Count
        /// <inheritdoc cref="Enumerable.Count{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count() => source.Length;

        /// <inheritdoc cref="Enumerable.Count{TSource}(IEnumerable{TSource}, Func{TSource, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count(Func<TSource, bool> predicate)
        {
            var count = 0;
            for (var i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    count++;
                }
            }
            return count;
        }
        #endregion

        #region TryGetNonEnumeratedCount
        /// <inheritdoc cref="Enumerable.TryGetNonEnumeratedCount{TSource}(IEnumerable{TSource}, out int)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNonEnumeratedCount(out int count) => (count = source.Length) > -1;
        #endregion

        #region LongCount
        /// <inheritdoc cref="Enumerable.LongCount{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long LongCount()
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.LongCount{TSource}(IEnumerable{TSource}, Func{TSource, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long LongCount(Func<TSource, bool> predicate)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region DefaultIfEmpty
        /// <inheritdoc cref="Enumerable.DefaultIfEmpty{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> DefaultIfEmpty()
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.DefaultIfEmpty{TSource}(IEnumerable{TSource}, TSource)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> DefaultIfEmpty(TSource defaultValue)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region Distinct
        /// <inheritdoc cref="Enumerable.Distinct{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> Distinct()
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Distinct{TSource}(IEnumerable{TSource}, IEqualityComparer{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> Distinct(IEqualityComparer<TSource> comparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region DistinctBy
        /// <inheritdoc cref="Enumerable.DistinctBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> DistinctBy<TKey>(Func<TSource, TKey> keySelector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.DistinctBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> DistinctBy<TKey>(Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region ElementAt
        /// <inheritdoc cref="Enumerable.ElementAt{TSource}(IEnumerable{TSource}, int)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource ElementAt(int index)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.ElementAt{TSource}(IEnumerable{TSource}, Index)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource ElementAt(Index index)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region ElementAtOrDefault
        /// <inheritdoc cref="Enumerable.ElementAtOrDefault{TSource}(IEnumerable{TSource}, int)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource ElementAtOrDefault(int index)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.ElementAtOrDefault{TSource}(IEnumerable{TSource}, Index)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource ElementAtOrDefault(Index index)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region AsEnumerable
        /// <inheritdoc cref="Enumerable.AsEnumerable{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> AsEnumerable()
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region Empty
        /// <inheritdoc cref="Enumerable.Empty{TResult}()" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TResult> Empty()
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region Except
        /// <inheritdoc cref="Enumerable.Except{TSource}(IEnumerable{TSource}, IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> Except(IEnumerable<TSource> second)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Except{TSource}(IEnumerable{TSource}, IEnumerable{TSource}, IEqualityComparer{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> Except(IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region ExceptBy
        /// <inheritdoc cref="Enumerable.ExceptBy{TSource, TKey}(IEnumerable{TSource}, IEnumerable{TKey}, Func{TSource, TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> ExceptBy<TKey>(IEnumerable<TKey> second, Func<TSource, TKey> keySelector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.ExceptBy{TSource, TKey}(IEnumerable{TSource}, IEnumerable{TKey}, Func{TSource, TKey}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> ExceptBy<TKey>(IEnumerable<TKey> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region First
        /// <inheritdoc cref="Enumerable.First{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource First()
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.First{TSource}(IEnumerable{TSource}, Func{TSource, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource First(Func<TSource, bool> predicate)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region FirstOrDefault
        /// <inheritdoc cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource FirstOrDefault()
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource}, TSource)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource FirstOrDefault(TSource defaultValue)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource FirstOrDefault(Func<TSource, bool> predicate)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool}, TSource)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource FirstOrDefault(Func<TSource, bool> predicate, TSource defaultValue)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region GroupBy
        /// <inheritdoc cref="Enumerable.GroupBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<IGrouping<TKey, TSource>> GroupBy<TKey>(Func<TSource, TKey> keySelector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.GroupBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<IGrouping<TKey, TSource>> GroupBy<TKey>(Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.GroupBy{TSource, TKey, TElement}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TSource, TElement})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<IGrouping<TKey, TElement>> GroupBy<TKey, TElement>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.GroupBy{TSource, TKey, TElement}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TSource, TElement}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<IGrouping<TKey, TElement>> GroupBy<TKey, TElement>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.GroupBy{TSource, TKey, TResult}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TKey, IEnumerable{TSource}, TResult})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TResult> GroupBy<TKey, TResult>(Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.GroupBy{TSource, TKey, TResult}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TKey, IEnumerable{TSource}, TResult}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TResult> GroupBy<TKey, TResult>(Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.GroupBy{TSource, TKey, TElement, TResult}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TSource, TElement}, Func{TKey, IEnumerable{TElement}, TResult})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TResult> GroupBy<TKey, TElement, TResult>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.GroupBy{TSource, TKey, TElement, TResult}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TSource, TElement}, Func{TKey, IEnumerable{TElement}, TResult}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TResult> GroupBy<TKey, TElement, TResult>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region GroupJoin
        /// <inheritdoc cref="Enumerable.GroupJoin{TOuter, TInner, TKey, TResult}(IEnumerable{TOuter}, IEnumerable{TInner}, Func{TOuter, TKey}, Func{TInner, TKey}, Func{TOuter, IEnumerable{TInner}, TResult})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TResult> GroupJoin<TInner, TKey, TResult>(IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.GroupJoin{TOuter, TInner, TKey, TResult}(IEnumerable{TOuter}, IEnumerable{TInner}, Func{TOuter, TKey}, Func{TInner, TKey}, Func{TOuter, IEnumerable{TInner}, TResult}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TResult> GroupJoin<TInner, TKey, TResult>(IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region Index
        /// <inheritdoc cref="Enumerable.Index{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<ValueTuple<int, TSource>> Index()
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region Intersect
        /// <inheritdoc cref="Enumerable.Intersect{TSource}(IEnumerable{TSource}, IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> Intersect(IEnumerable<TSource> second)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Intersect{TSource}(IEnumerable{TSource}, IEnumerable{TSource}, IEqualityComparer{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> Intersect(IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region IntersectBy
        /// <inheritdoc cref="Enumerable.IntersectBy{TSource, TKey}(IEnumerable{TSource}, IEnumerable{TKey}, Func{TSource, TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> IntersectBy<TKey>(IEnumerable<TKey> second, Func<TSource, TKey> keySelector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.IntersectBy{TSource, TKey}(IEnumerable{TSource}, IEnumerable{TKey}, Func{TSource, TKey}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> IntersectBy<TKey>(IEnumerable<TKey> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region Join
        /// <inheritdoc cref="Enumerable.Join{TOuter, TInner, TKey, TResult}(IEnumerable{TOuter}, IEnumerable{TInner}, Func{TOuter, TKey}, Func{TInner, TKey}, Func{TOuter, TInner, TResult})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TResult> Join<TInner, TKey, TResult>(IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Join{TOuter, TInner, TKey, TResult}(IEnumerable{TOuter}, IEnumerable{TInner}, Func{TOuter, TKey}, Func{TInner, TKey}, Func{TOuter, TInner, TResult}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TResult> Join<TInner, TKey, TResult>(IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region Last
        /// <inheritdoc cref="Enumerable.Last{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource Last()
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Last{TSource}(IEnumerable{TSource}, Func{TSource, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource Last(Func<TSource, bool> predicate)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region LastOrDefault
        /// <inheritdoc cref="Enumerable.LastOrDefault{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource LastOrDefault()
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.LastOrDefault{TSource}(IEnumerable{TSource}, TSource)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource LastOrDefault(TSource defaultValue)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.LastOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource LastOrDefault(Func<TSource, bool> predicate)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.LastOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool}, TSource)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource LastOrDefault(Func<TSource, bool> predicate, TSource defaultValue)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region ToLookup
        /// <inheritdoc cref="Enumerable.ToLookup{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILookup<TKey, TSource> ToLookup<TKey>(Func<TSource, TKey> keySelector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.ToLookup{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILookup<TKey, TSource> ToLookup<TKey>(Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.ToLookup{TSource, TKey, TElement}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TSource, TElement})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILookup<TKey, TElement> ToLookup<TKey, TElement>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.ToLookup{TSource, TKey, TElement}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TSource, TElement}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILookup<TKey, TElement> ToLookup<TKey, TElement>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region Max
        /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource Max()
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, IComparer{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource Max(IComparer<TSource> comparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, int})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Max(Func<TSource, int> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, int?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int? Max(Func<TSource, int?> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, long})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long Max(Func<TSource, long> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, long?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long? Max(Func<TSource, long?> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, float})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Max(Func<TSource, float> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, float?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float? Max(Func<TSource, float?> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, double})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Max(Func<TSource, double> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, double?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double? Max(Func<TSource, double?> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, decimal})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal Max(Func<TSource, decimal> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, decimal?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal? Max(Func<TSource, decimal?> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Max{TSource, TResult}(IEnumerable{TSource}, Func{TSource, TResult})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TResult Max<TResult>(Func<TSource, TResult> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region MaxBy
        /// <inheritdoc cref="Enumerable.MaxBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource MaxBy<TKey>(Func<TSource, TKey> keySelector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.MaxBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey}, IComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource MaxBy<TKey>(Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region Min
        /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource Min()
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource}, IComparer{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource Min(IComparer<TSource> comparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource}, Func{TSource, int})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Min(Func<TSource, int> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource}, Func{TSource, int?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int? Min(Func<TSource, int?> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource}, Func{TSource, long})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long Min(Func<TSource, long> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource}, Func{TSource, long?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long? Min(Func<TSource, long?> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource}, Func{TSource, float})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Min(Func<TSource, float> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource}, Func{TSource, float?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float? Min(Func<TSource, float?> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource}, Func{TSource, double})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Min(Func<TSource, double> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource}, Func{TSource, double?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double? Min(Func<TSource, double?> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource}, Func{TSource, decimal})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal Min(Func<TSource, decimal> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource}, Func{TSource, decimal?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal? Min(Func<TSource, decimal?> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Min{TSource, TResult}(IEnumerable{TSource}, Func{TSource, TResult})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TResult Min<TResult>(Func<TSource, TResult> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region MinBy
        /// <inheritdoc cref="Enumerable.MinBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource MinBy<TKey>(Func<TSource, TKey> keySelector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.MinBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey}, IComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource MinBy<TKey>(Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region OfType
        /// <inheritdoc cref="Enumerable.OfType{TResult}(Collections.IEnumerable)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TResult> OfType()
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region Order
        /// <inheritdoc cref="Enumerable.Order{T}(IEnumerable{T})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IOrderedEnumerable<T> Order()
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Order{T}(IEnumerable{T}, IComparer{T})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IOrderedEnumerable<T> Order(IComparer<T> comparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region OrderBy
        /// <inheritdoc cref="Enumerable.OrderBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IOrderedEnumerable<TSource> OrderBy<TKey>(Func<TSource, TKey> keySelector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.OrderBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey}, IComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IOrderedEnumerable<TSource> OrderBy<TKey>(Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region OrderDescending
        /// <inheritdoc cref="Enumerable.OrderDescending{T}(IEnumerable{T})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IOrderedEnumerable<T> OrderDescending()
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.OrderDescending{T}(IEnumerable{T}, IComparer{T})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IOrderedEnumerable<T> OrderDescending(IComparer<T> comparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region OrderByDescending
        /// <inheritdoc cref="Enumerable.OrderByDescending{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IOrderedEnumerable<TSource> OrderByDescending<TKey>(Func<TSource, TKey> keySelector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.OrderByDescending{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey}, IComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IOrderedEnumerable<TSource> OrderByDescending<TKey>(Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region ThenBy
        /// <inheritdoc cref="Enumerable.ThenBy{TSource, TKey}(IOrderedEnumerable{TSource}, Func{TSource, TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IOrderedEnumerable<TSource> ThenBy<TKey>(Func<TSource, TKey> keySelector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.ThenBy{TSource, TKey}(IOrderedEnumerable{TSource}, Func{TSource, TKey}, IComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IOrderedEnumerable<TSource> ThenBy<TKey>(Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region ThenByDescending
        /// <inheritdoc cref="Enumerable.ThenByDescending{TSource, TKey}(IOrderedEnumerable{TSource}, Func{TSource, TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IOrderedEnumerable<TSource> ThenByDescending<TKey>(Func<TSource, TKey> keySelector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.ThenByDescending{TSource, TKey}(IOrderedEnumerable{TSource}, Func{TSource, TKey}, IComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IOrderedEnumerable<TSource> ThenByDescending<TKey>(Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region Select
        /// <inheritdoc cref="Enumerable.Select{TSource, TResult}(IEnumerable{TSource}, Func{TSource, TResult})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TResult> Select<TResult>(Func<TSource, TResult> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Select{TSource, TResult}(IEnumerable{TSource}, Func{TSource, int, TResult})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TResult> Select<TResult>(Func<TSource, int, TResult> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region SelectMany
        /// <inheritdoc cref="Enumerable.SelectMany{TSource, TResult}(IEnumerable{TSource}, Func{TSource, IEnumerable{TResult}})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TResult> SelectMany<TResult>(Func<TSource, IEnumerable<TResult>> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.SelectMany{TSource, TResult}(IEnumerable{TSource}, Func{TSource, int, IEnumerable{TResult}})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TResult> SelectMany<TResult>(Func<TSource, int, IEnumerable<TResult>> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.SelectMany{TSource, TCollection, TResult}(IEnumerable{TSource}, Func{TSource, int, IEnumerable{TCollection}}, Func{TSource, TCollection, TResult})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TResult> SelectMany<TCollection, TResult>(Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.SelectMany{TSource, TCollection, TResult}(IEnumerable{TSource}, Func{TSource, IEnumerable{TCollection}}, Func{TSource, TCollection, TResult})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TResult> SelectMany<TCollection, TResult>(Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region SequenceEqual
        /// <inheritdoc cref="Enumerable.SequenceEqual{TSource}(IEnumerable{TSource}, IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SequenceEqual(IEnumerable<TSource> second)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.SequenceEqual{TSource}(IEnumerable{TSource}, IEnumerable{TSource}, IEqualityComparer{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SequenceEqual(IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region Single
        /// <inheritdoc cref="Enumerable.Single{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource Single()
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Single{TSource}(IEnumerable{TSource}, Func{TSource, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource Single(Func<TSource, bool> predicate)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region SingleOrDefault
        /// <inheritdoc cref="Enumerable.SingleOrDefault{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource SingleOrDefault()
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.SingleOrDefault{TSource}(IEnumerable{TSource}, TSource)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource SingleOrDefault(TSource defaultValue)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.SingleOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource SingleOrDefault(Func<TSource, bool> predicate)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.SingleOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool}, TSource)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource SingleOrDefault(Func<TSource, bool> predicate, TSource defaultValue)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region Skip
        /// <inheritdoc cref="Enumerable.Skip{TSource}(IEnumerable{TSource}, int)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> Skip(int count)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region SkipWhile
        /// <inheritdoc cref="Enumerable.SkipWhile{TSource}(IEnumerable{TSource}, Func{TSource, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> SkipWhile(Func<TSource, bool> predicate)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.SkipWhile{TSource}(IEnumerable{TSource}, Func{TSource, int, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> SkipWhile(Func<TSource, int, bool> predicate)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region SkipLast
        /// <inheritdoc cref="Enumerable.SkipLast{TSource}(IEnumerable{TSource}, int)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> SkipLast(int count)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region Sum
        /// <inheritdoc cref="Enumerable.Sum{TSource}(IEnumerable{TSource}, Func{TSource, int})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Sum(Func<TSource, int> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Sum{TSource}(IEnumerable{TSource}, Func{TSource, long})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long Sum(Func<TSource, long> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Sum{TSource}(IEnumerable{TSource}, Func{TSource, float})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Sum(Func<TSource, float> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Sum{TSource}(IEnumerable{TSource}, Func{TSource, double})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Sum(Func<TSource, double> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Sum{TSource}(IEnumerable{TSource}, Func{TSource, decimal})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal Sum(Func<TSource, decimal> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Sum{TSource}(IEnumerable{TSource}, Func{TSource, int?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int? Sum(Func<TSource, int?> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Sum{TSource}(IEnumerable{TSource}, Func{TSource, long?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long? Sum(Func<TSource, long?> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Sum{TSource}(IEnumerable{TSource}, Func{TSource, float?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float? Sum(Func<TSource, float?> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Sum{TSource}(IEnumerable{TSource}, Func{TSource, double?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double? Sum(Func<TSource, double?> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Sum{TSource}(IEnumerable{TSource}, Func{TSource, decimal?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal? Sum(Func<TSource, decimal?> selector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region Take
        /// <inheritdoc cref="Enumerable.Take{TSource}(IEnumerable{TSource}, int)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> Take(int count)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Take{TSource}(IEnumerable{TSource}, Range)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> Take(Range range)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region TakeWhile
        /// <inheritdoc cref="Enumerable.TakeWhile{TSource}(IEnumerable{TSource}, Func{TSource, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> TakeWhile(Func<TSource, bool> predicate)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.TakeWhile{TSource}(IEnumerable{TSource}, Func{TSource, int, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> TakeWhile(Func<TSource, int, bool> predicate)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region TakeLast
        /// <inheritdoc cref="Enumerable.TakeLast{TSource}(IEnumerable{TSource}, int)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> TakeLast(int count)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region ToArray
        /// <inheritdoc cref="Enumerable.ToArray{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource[] ToArray()
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region ToList
        /// <inheritdoc cref="Enumerable.ToList{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<TSource> ToList()
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region ToDictionary
        /// <inheritdoc cref="Enumerable.ToDictionary{TKey, TValue}(IEnumerable{KeyValuePair{TKey, TValue}})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<TKey, TValue> ToDictionary<TValue>()
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.ToDictionary{TKey, TValue}(IEnumerable{KeyValuePair{TKey, TValue}}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<TKey, TValue> ToDictionary<TValue>(IEqualityComparer<TKey> comparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.ToDictionary{TKey, TValue}(IEnumerable{ValueTuple{TKey, TValue}})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<TKey, TValue> ToDictionary<TValue>()
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.ToDictionary{TKey, TValue}(IEnumerable{ValueTuple{TKey, TValue}}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<TKey, TValue> ToDictionary<TValue>(IEqualityComparer<TKey> comparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.ToDictionary{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<TKey, TSource> ToDictionary<TKey>(Func<TSource, TKey> keySelector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.ToDictionary{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<TKey, TSource> ToDictionary<TKey>(Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.ToDictionary{TSource, TKey, TElement}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TSource, TElement})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<TKey, TElement> ToDictionary<TKey, TElement>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.ToDictionary{TSource, TKey, TElement}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TSource, TElement}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<TKey, TElement> ToDictionary<TKey, TElement>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region ToHashSet
        /// <inheritdoc cref="Enumerable.ToHashSet{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashSet<TSource> ToHashSet()
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.ToHashSet{TSource}(IEnumerable{TSource}, IEqualityComparer{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashSet<TSource> ToHashSet(IEqualityComparer<TSource> comparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region Union
        /// <inheritdoc cref="Enumerable.Union{TSource}(IEnumerable{TSource}, IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> Union(IEnumerable<TSource> second)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Union{TSource}(IEnumerable{TSource}, IEnumerable{TSource}, IEqualityComparer{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> Union(IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region UnionBy
        /// <inheritdoc cref="Enumerable.UnionBy{TSource, TKey}(IEnumerable{TSource}, IEnumerable{TSource}, Func{TSource, TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> UnionBy<TKey>(IEnumerable<TSource> second, Func<TSource, TKey> keySelector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.UnionBy{TSource, TKey}(IEnumerable{TSource}, IEnumerable{TSource}, Func{TSource, TKey}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> UnionBy<TKey>(IEnumerable<TSource> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region Where
        /// <inheritdoc cref="Enumerable.Where{TSource}(IEnumerable{TSource}, Func{TSource, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> Where(Func<TSource, bool> predicate)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Where{TSource}(IEnumerable{TSource}, Func{TSource, int, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> Where(Func<TSource, int, bool> predicate)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

        #region Zip
        /// <inheritdoc cref="Enumerable.Zip{TFirst, TSecond, TResult}(IEnumerable{TFirst}, IEnumerable{TSecond}, Func{TFirst, TSecond, TResult})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TResult> Zip<TSecond, TResult>(IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Zip{TFirst, TSecond}(IEnumerable{TFirst}, IEnumerable{TSecond})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<ValueTuple<TFirst, TSecond>> Zip<TSecond>(IEnumerable<TSecond> second)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }

        /// <inheritdoc cref="Enumerable.Zip{TFirst, TSecond, TThird}(IEnumerable{TFirst}, IEnumerable{TSecond}, IEnumerable{TThird})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<ValueTuple<TFirst, TSecond, TThird>> Zip<TSecond, TThird>(IEnumerable<TSecond> second, IEnumerable<TThird> third)
        {
            for (var i = 0; i < source.Length; i++)
            {
            }
        }
        #endregion

    }
}
