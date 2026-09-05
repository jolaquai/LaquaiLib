namespace LaquaiLib.Extensions;

public static partial class MemoryExtensions
{
    extension<T>(in Span<T> span)
    {
        // This is the ONLY place in the codebase where Span.Clear() is acceptable, EVERYWHERE else should use ZeroMemory() unconditionally
        /// <summary>
        /// Generalizes <see cref="System.Security.Cryptography.CryptographicOperations.ZeroMemory(Span{byte})"/> to arbitrary <see cref="Span{T}"/>s of <typeparamref name="T"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public void ZeroMemory() => span.Clear();

        /// <summary>
        /// Fills the specified <see cref="Span{T}"/> with the <see langword="default"/> value for type <typeparamref name="T"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill() => span.ZeroMemory();
        /// <summary>
        /// Fills the specified <see cref="Span{T}"/> using the specified <paramref name="factory"/>.
        /// </summary>
        /// <param name="factory">The factory method that produces the values to fill the span with.</param>
        public void Fill(Func<T> factory)
        {
            for (var i = 0; i < span.Length; i++)
                span[i] = factory();
        }
        /// <summary>
        /// Fills the specified <see cref="Span{T}"/> using the specified <paramref name="factory"/>. It is passed the previous iteration's value, seeded with the <see langword="default"/> value for type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="factory">The factory method that produces the values to fill the span with.</param>
        public void Fill(Func<T, T> factory)
        {
            T last = default;
            for (var i = 0; i < span.Length; i++)
                last = span[i] = factory(last);
        }
        /// <summary>
        /// Fills the specified <see cref="Span{T}"/> using the specified <paramref name="factory"/>. It is passed the index in the span that is being assigned.
        /// </summary>
        /// <param name="factory">The factory method that produces the values to fill the span with.</param>
        public void FillIndexed(Func<int, T> factory)
        {
            for (var i = 0; i < span.Length; i++)
                span[i] = factory(i);
        }
        /// <summary>
        /// Fills the specified <see cref="Span{T}"/> using the specified <paramref name="factory"/>. It is passed the index in the span that is being assigned and the previous iteration's value, seeded with the <see langword="default"/> value for type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="factory">The factory method that produces the values to fill the span with.</param>
        public void FillIndexed(Func<int, T, T> factory)
        {
            T last = default;
            for (var i = 0; i < span.Length; i++)
                last = span[i] = factory(i, last);
        }
    }
}
