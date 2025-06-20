namespace LaquaiLib.Extensions.MemoryExtensions.Linq;

public static partial class MemoryExtensions
{
    extension<TSource>(ReadOnlySpan<TSource> source)
    {
        /// <summary>
        /// Combines <see cref="Select{TSource, TResult}(ReadOnlySpan{TSource}, Func{TSource, TResult}, Span{TResult})"/> and <see cref="ReadOnlySpan{T}.ToArray"/>
        /// </summary>
        /// <param name="selector">A <see cref="Func{T, TResult}"/> that is passed each element of the source <see cref="ReadOnlySpan{T}"/> and returns a transformed element.</param>
        /// <returns>An array of <typeparamref name="TResult"/> containing the elements produced by the <paramref name="selector"/>.</returns>
        /// <remarks>
        /// This is provided as a replacement for something like <c>AsEnumerable</c>, since yielding is not possible with <see cref="ReadOnlySpan{T}"/>.
        /// </remarks>
        public TResult[] ToArray<TResult>(Func<TSource, TResult> selector)
        {
            var arr = GC.AllocateUninitializedArray<TResult>(source.Length);
            for (var i = 0; i < source.Length; i++)
            {
                arr[i] = selector(source[i]);
            }
            return arr;
        }
        /// <summary>
        /// Combines <see cref="Select{TSource, TResult}(ReadOnlySpan{TSource}, Func{TSource, int, TResult}, Span{TResult})"/> and <see cref="ReadOnlySpan{T}.ToArray"/>
        /// </summary>
        /// <param name="selector">A <see cref="Func{T, TResult}"/> that is passed each element of the source <see cref="ReadOnlySpan{T}"/> and returns a transformed element.</param>
        /// <returns>An array of <typeparamref name="TResult"/> containing the elements produced by the <paramref name="selector"/>.</returns>
        /// <remarks>
        /// This is provided as a replacement for something like <c>AsEnumerable</c>, since yielding is not possible with <see cref="ReadOnlySpan{T}"/>.
        /// </remarks>
        public TResult[] ToArray<TResult>(Func<TSource, int, TResult> selector)
        {
            var arr = GC.AllocateUninitializedArray<TResult>(source.Length);
            for (var i = 0; i < source.Length; i++)
            {
                arr[i] = selector(source[i], i);
            }
            return arr;
        }

        /// <summary>
        /// Combines <see cref="Where{TSource}(ReadOnlySpan{TSource}, Func{TSource, bool}, Span{TSource})"/> and <see cref="Select{TSource, TResult}(ReadOnlySpan{TSource}, Func{TSource, TResult}, Span{TResult})"/> and returns an array of <typeparamref name="TResult"/> containing the results.
        /// Neither parameter may be <see langword="null"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements in the resulting array.</typeparam>
        /// <param name="where">A <see cref="Func{T, TResult}"/> that is passed each element of the source <see cref="ReadOnlySpan{T}"/> and returns a <see langword="bool"/> indicating whether the element should be included in the result.</param>
        /// <param name="select">A <see cref="Func{T, TResult}"/> that is passed each element of the source <see cref="ReadOnlySpan{T}"/> and returns a transformed element.</param>
        /// <returns>The created array of <typeparamref name="TResult"/>.</returns>
        public TResult[] WhereSelectToArray<TResult>(Func<TSource, bool> where, Func<TSource, TResult> select)
        {
            ArgumentNullException.ThrowIfNull(where);
            ArgumentNullException.ThrowIfNull(select);

            if (where.IsStatic && select.IsStatic)
            {
                return UWSToArray(source, where, select);
            }
            else
            {
                var ret = GC.AllocateUninitializedArray<TResult>(source.Length);
                var k = 0;
                for (var i = 0; i < source.Length; i++)
                {
                    if (where(source[i]))
                    {
                        ret[k++] = select(source[i]);
                    }
                }
                Array.Resize(ref ret, k);
                return ret;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe TResult[] UWSToArray<TResult>(Func<TSource, bool> where, Func<TSource, TResult> select)
        {
            // Optimize for static methods by using function pointers
            var wherePtr = (delegate*<TSource, bool>)where.Method.MethodHandle.GetFunctionPointer();
            var selectPtr = (delegate*<TSource, TResult>)select.Method.MethodHandle.GetFunctionPointer();

            var ret = GC.AllocateUninitializedArray<TResult>(source.Length);
            var k = 0;
            for (var i = 0; i < source.Length; i++)
            {
                if (wherePtr(source[i]))
                {
                    ret[k++] = selectPtr(source[i]);
                }
            }
            Array.Resize(ref ret, k);
            return ret;
        }
    }
}