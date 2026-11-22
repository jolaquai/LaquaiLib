using System.Collections.Frozen;
using System.Collections.Immutable;

namespace LaquaiLib.Extensions;

#pragma warning disable IDE0305 // Simplify collection initialization

public static partial class IEnumerableExtensions
{
    extension<T>(IEnumerable<T> source)
    {
        #region ToStack
        /// <summary>
        /// Creates a new <see cref="Stack{T}"/> from the specified <see cref="IEnumerable{T}"/>, optionally preserving the order of elements (that is, the default behavior of <see cref="Stack{T}"/> would cause the elements to be popped in reverse order).
        /// </summary>
        /// <param name="preserveOrder">Whether to preserve the order of elements in the resulting <see cref="Stack{T}"/>. If set to <see langword="true"/>, the elements will be popped in the same order as they appear in the source collection.</param>
        /// <returns>The created <see cref="Stack{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Stack<T> ToStack(bool preserveOrder = false) => preserveOrder ? new Stack<T>(source.Reverse()) : new Stack<T>(source);
        /// <summary>
        /// Creates a new <see cref="Stack{T}"/> from the specified <see cref="IEnumerable{T}"/> using the specified <paramref name="selector"/> function, optionally preserving the order of elements (that is, the default behavior of <see cref="Stack{T}"/> would cause the elements to be popped in reverse order).
        /// </summary>
        /// <typeparam name="TResult">The type of the elements in the resulting <see cref="Stack{T}"/>.</typeparam>
        /// <param name="selector">The function to transform each element of the source collection.</param>
        /// <param name="preserveOrder">Whether to preserve the order of elements in the resulting <see cref="Stack{T}"/>. If set to <see langword="true"/>, the elements will be popped in the same order as they appear in the source collection.</param>
        /// <returns>The created <see cref="Stack{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Stack<TResult> ToStack<TResult>(Func<T, TResult> selector, bool preserveOrder = false) => ToStack(source.Select(selector), preserveOrder);
        /// <summary>
        /// Creates a new <see cref="Stack{T}"/> from the specified <see cref="IEnumerable{T}"/> using the specified <paramref name="selector"/> function, optionally preserving the order of elements (that is, the default behavior of <see cref="Stack{T}"/> would cause the elements to be popped in reverse order).
        /// </summary>
        /// <typeparam name="TResult">The type of the elements in the resulting <see cref="Stack{T}"/>.</typeparam>
        /// <param name="selector">The function to transform each element of the source collection.</param>
        /// <param name="preserveOrder">Whether to preserve the order of elements in the resulting <see cref="Stack{T}"/>. If set to <see langword="true"/>, the elements will be popped in the same order as they appear in the source collection.</param>
        /// <returns>The created <see cref="Stack{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Stack<TResult> ToStack<TResult>(Func<T, int, TResult> selector, bool preserveOrder = false) => ToStack(source.Select(selector), preserveOrder);
        #endregion

        #region ToQueue
        /// <summary>
        /// Creates a new <see cref="Queue{T}"/> from the specified <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <returns>The created <see cref="Queue{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Queue<T> ToQueue() => new Queue<T>(source);
        /// <summary>
        /// Creates a new <see cref="Queue{T}"/> from the specified <see cref="IEnumerable{T}"/> using the specified <paramref name="selector"/> function.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements in the resulting <see cref="Queue{T}"/>.</typeparam>
        /// <param name="selector">The function to transform each element of the source collection.</param>
        /// <returns>The created <see cref="Queue{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Queue<TResult> ToQueue<TResult>(Func<T, TResult> selector) => source.Select(selector).ToQueue();
        /// <summary>
        /// Creates a new <see cref="Queue{T}"/> from the specified <see cref="IEnumerable{T}"/> using the specified <paramref name="selector"/> function.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements in the resulting <see cref="Queue{T}"/>.</typeparam>
        /// <param name="selector">The function to transform each element of the source collection.</param>
        /// <returns>The created <see cref="Queue{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Queue<TResult> ToQueue<TResult>(Func<T, int, TResult> selector) => source.Select(selector).ToQueue();
        #endregion

        #region ToArray
        /// <summary>
        /// Creates a new <see cref="Array"/> of <typeparamref name="TResult"/> from the specified <see cref="IEnumerable{T}"/> using the specified <paramref name="selector"/> function.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements in the resulting array.</typeparam>
        /// <param name="selector">The function to transform each element of the source collection.</param>
        /// <returns>The created array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TResult[] ToArray<TResult>(Func<T, TResult> selector) => source.Select(selector).ToArray();
        /// <summary>
        /// Creates a new <see cref="Array"/> of <typeparamref name="TResult"/> from the specified <see cref="IEnumerable{T}"/> using the specified <paramref name="selector"/> function.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements in the resulting array.</typeparam>
        /// <param name="selector">The function to transform each element of the source collection.</param>
        /// <returns>The created array.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TResult[] ToArray<TResult>(Func<T, int, TResult> selector) => source.Select(selector).ToArray();
        #endregion
        #region ToImmutableArray
        /// <summary>
        /// Creates a new <see cref="ImmutableArray{T}"/> of <typeparamref name="TResult"/> from the specified <see cref="IEnumerable{T}"/> using the specified <paramref name="selector"/> function.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements in the resulting <see cref="ImmutableArray{T}"/>.</typeparam>
        /// <param name="selector">The function to transform each element of the source collection.</param>
        /// <returns>The created <see cref="ImmutableArray{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ImmutableArray<TResult> ToImmutableArray<TResult>(Func<T, TResult> selector) => source.Select(selector).ToImmutableArray();
        /// <summary>
        /// Creates a new <see cref="ImmutableArray{T}"/> of <typeparamref name="TResult"/> from the specified <see cref="IEnumerable{T}"/> using the specified <paramref name="selector"/> function.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements in the resulting <see cref="ImmutableArray{T}"/>.</typeparam>
        /// <param name="selector">The function to transform each element of the source collection.</param>
        /// <returns>The created <see cref="ImmutableArray{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ImmutableArray<TResult> ToImmutableArray<TResult>(Func<T, int, TResult> selector) => source.Select(selector).ToImmutableArray();
        #endregion

        #region ToList
        /// <summary>
        /// Creates a new <see cref="List{T}"/> of <typeparamref name="TResult"/> from the specified <see cref="IEnumerable{T}"/> using the specified <paramref name="selector"/> function.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements in the resulting <see cref="List{T}"/>.</typeparam>
        /// <param name="selector">The function to transform each element of the source collection.</param>
        /// <returns>The created <see cref="List{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<TResult> ToList<TResult>(Func<T, TResult> selector) => source.Select(selector).ToList();
        /// <summary>
        /// Creates a new <see cref="List{T}"/> of <typeparamref name="TResult"/> from the specified <see cref="IEnumerable{T}"/> using the specified <paramref name="selector"/> function.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements in the resulting <see cref="List{T}"/>.</typeparam>
        /// <param name="selector">The function to transform each element of the source collection.</param>
        /// <returns>The created <see cref="List{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<TResult> ToList<TResult>(Func<T, int, TResult> selector) => source.Select(selector).ToList();
        #endregion

        #region ToHashSet
        /// <summary>
        /// Creates a new <see cref="HashSet{T}"/> of <typeparamref name="TResult"/> from the specified <see cref="IEnumerable{T}"/> using the specified <paramref name="selector"/> function.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements in the resulting <see cref="HashSet{T}"/>.</typeparam>
        /// <param name="selector">The function to transform each element of the source collection.</param>
        /// <returns>The created <see cref="HashSet{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashSet<TResult> ToHashSet<TResult>(Func<T, TResult> selector) => source.Select(selector).ToHashSet();
        /// <summary>
        /// Creates a new <see cref="HashSet{T}"/> of <typeparamref name="TResult"/> from the specified <see cref="IEnumerable{T}"/> using the specified <paramref name="selector"/> function.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements in the resulting <see cref="HashSet{T}"/>.</typeparam>
        /// <param name="selector">The function to transform each element of the source collection.</param>
        /// <returns>The created <see cref="HashSet{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashSet<TResult> ToHashSet<TResult>(Func<T, int, TResult> selector) => source.Select(selector).ToHashSet();
        #endregion
        #region ToFrozenSet
        /// <summary>
        /// Creates a new <see cref="FrozenSet{T}"/> of <typeparamref name="TResult"/> from the specified <see cref="IEnumerable{T}"/> using the specified <paramref name="selector"/> function.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements in the resulting <see cref="FrozenSet{T}"/>.</typeparam>
        /// <param name="selector">The function to transform each element of the source collection.</param>
        /// <returns>The created <see cref="FrozenSet{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FrozenSet<TResult> ToFrozenSet<TResult>(Func<T, TResult> selector) => source.Select(selector).ToFrozenSet();
        /// <summary>
        /// Creates a new <see cref="FrozenSet{T}"/> of <typeparamref name="TResult"/> from the specified <see cref="IEnumerable{T}"/> using the specified <paramref name="selector"/> function.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements in the resulting <see cref="FrozenSet{T}"/>.</typeparam>
        /// <param name="selector">The function to transform each element of the source collection.</param>
        /// <returns>The created <see cref="FrozenSet{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FrozenSet<TResult> ToFrozenSet<TResult>(Func<T, int, TResult> selector) => source.Select(selector).ToFrozenSet();
        #endregion
        #region ToImmutableHashSet
        /// <summary>
        /// Creates a new <see cref="ImmutableHashSet{T}"/> of <typeparamref name="TResult"/> from the specified <see cref="IEnumerable{T}"/> using the specified <paramref name="selector"/> function.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements in the resulting <see cref="ImmutableHashSet{T}"/>.</typeparam>
        /// <param name="selector">The function to transform each element of the source collection.</param>
        /// <returns>The created <see cref="ImmutableHashSet{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ImmutableHashSet<TResult> ToImmutableHashSet<TResult>(Func<T, TResult> selector) => source.Select(selector).ToImmutableHashSet();
        /// <summary>
        /// Creates a new <see cref="ImmutableHashSet{T}"/> of <typeparamref name="TResult"/> from the specified <see cref="IEnumerable{T}"/> using the specified <paramref name="selector"/> function.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements in the resulting <see cref="ImmutableHashSet{T}"/>.</typeparam>
        /// <param name="selector">The function to transform each element of the source collection.</param>
        /// <returns>The created <see cref="ImmutableHashSet{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ImmutableHashSet<TResult> ToImmutableHashSet<TResult>(Func<T, int, TResult> selector) => source.Select(selector).ToImmutableHashSet();
        #endregion
        #region ToImmutableSortedSet
        /// <summary>
        /// Creates a new <see cref="ImmutableSortedSet{T}"/> of <typeparamref name="TResult"/> from the specified <see cref="IEnumerable{T}"/> using the specified <paramref name="selector"/> function.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements in the resulting <see cref="ImmutableSortedSet{T}"/>.</typeparam>
        /// <param name="selector">The function to transform each element of the source collection.</param>
        /// <returns>The created <see cref="ImmutableSortedSet{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ImmutableSortedSet<TResult> ToImmutableSortedSet<TResult>(Func<T, TResult> selector) => source.Select(selector).ToImmutableSortedSet();
        /// <summary>
        /// Creates a new <see cref="ImmutableSortedSet{T}"/> of <typeparamref name="TResult"/> from the specified <see cref="IEnumerable{T}"/> using the specified <paramref name="selector"/> function.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements in the resulting <see cref="ImmutableSortedSet{T}"/>.</typeparam>
        /// <param name="selector">The function to transform each element of the source collection.</param>
        /// <returns>The created <see cref="ImmutableSortedSet{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ImmutableSortedSet<TResult> ToImmutableSortedSet<TResult>(Func<T, int, TResult> selector) => source.Select(selector).ToImmutableSortedSet();
        #endregion
    }
}
