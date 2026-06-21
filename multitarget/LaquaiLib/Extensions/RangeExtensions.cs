using System.Linq.Expressions;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Range"/> Type.
/// </summary>
public static class RangeExtensions
{
    extension(Range range)
    {
        /// <summary>
        /// Returns an <see cref="IEnumerable{T}"/> of <see cref="int"/>s that are within the given <paramref name="range"/>.
        /// </summary>
        /// <param name="range">The <see cref="Range"/> to get the range from.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="int"/>s that are within the given <paramref name="range"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<int> GetRange() => range.Start.IsFromEnd || range.End.IsFromEnd
                ? throw new ArgumentException("Range indices cannot be from the end since there is no end to reference from.", nameof(range))
                : Enumerable.Range(range.Start.Value, range.End.Value - range.Start.Value);
        /// <summary>
        /// Returns an <see cref="IEnumerable{T}"/> of <see cref="int"/>s that are within the given <paramref name="range"/>, calculating the required indices from the given <paramref name="length"/>.
        /// </summary>
        /// <param name="range">The <see cref="Range"/> to get the range from.</param>
        /// <param name="length">The length of the range to reference.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="int"/>s that are within the given <paramref name="range"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<int> GetRange(int length)
        {
            var (offs, len) = range.GetOffsetAndLength(length);
            return Enumerable.Range(offs, len);
        }
        /// <summary>
        /// Returns an <see cref="IEnumerator{T}"/> of <see cref="int"/>s that may be used to iterate through the numbers within the given <paramref name="range"/>.
        /// </summary>
        /// <param name="range">The <see cref="Range"/> to get the range from.</param>
        /// <returns>The <see cref="IEnumerator{T}"/> as described.</returns>
        /// <remarks>
        /// This wouldn't typically be called directly, but rather through a <see langword="foreach"/> loop.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<int> GetEnumerator() => new RangeEnumerator(range);

        public IEnumerable<TResult> Select<TResult>(Func<int, TResult> selector) =>
        range.GetRange().Select(selector);

        public IEnumerable<int> Where(Func<int, bool> predicate) =>
            range.GetRange().Where(predicate);

        public IEnumerable<TResult> SelectMany<TResult>(Func<int, IEnumerable<TResult>> selector) =>
            range.GetRange().SelectMany(selector);

        public IEnumerable<TResult> SelectMany<TCollection, TResult>(
            Func<int, IEnumerable<TCollection>> collectionSelector,
            Func<int, TCollection, TResult> resultSelector) =>
            range.GetRange().SelectMany(collectionSelector, resultSelector);

        public IEnumerable<TResult> Join<TInner, TKey, TResult>(
            IEnumerable<TInner> inner,
            Func<int, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<int, TInner, TResult> resultSelector) =>
            range.GetRange().Join(inner, outerKeySelector, innerKeySelector, resultSelector);

        public IEnumerable<TResult> GroupJoin<TInner, TKey, TResult>(
            IEnumerable<TInner> inner,
            Func<int, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<int, IEnumerable<TInner>, TResult> resultSelector) =>
            range.GetRange().GroupJoin(inner, outerKeySelector, innerKeySelector, resultSelector);

        public IOrderedEnumerable<int> OrderBy<TKey>(Func<int, TKey> keySelector) =>
            range.GetRange().OrderBy(keySelector);

        public IOrderedEnumerable<int> OrderByDescending<TKey>(Func<int, TKey> keySelector) =>
            range.GetRange().OrderByDescending(keySelector);

        public IEnumerable<IGrouping<TKey, int>> GroupBy<TKey>(Func<int, TKey> keySelector) =>
            range.GetRange().GroupBy(keySelector);

        public IEnumerable<IGrouping<TKey, TElement>> GroupBy<TKey, TElement>(
            Func<int, TKey> keySelector,
            Func<int, TElement> elementSelector) =>
            range.GetRange().GroupBy(keySelector, elementSelector);
    }
}

file struct RangeEnumerator(Range range) : IEnumerator<int>
{
    private readonly int _start = range.Start.IsFromEnd ? throw new ArgumentException("Range indices cannot be from the end since there is no end to reference from.", nameof(range)) : range.Start.Value;
    private readonly int _end = range.End.IsFromEnd ? throw new ArgumentException("Range indices cannot be from the end since there is no end to reference from.", nameof(range)) : range.End.Value;
    private bool _started;
    public int Current { get; private set; }
    readonly object IEnumerator.Current => Current;
    public readonly void Dispose() { }
    public bool MoveNext()
    {
        if (!_started)
        {
            Current = _start - 1; // Start at one less than the start value so that the first MoveNext() call sets it to the start value.
            _started = true;
        }
        else
            Current++;
        return Current < _end;
    }
    public void Reset()
    {
        _started = false;
        Current = default;
    }
}