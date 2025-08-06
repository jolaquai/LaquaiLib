namespace LaquaiLib.Extensions.Memory;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

/// <summary>
/// Provides static extension methods for the <see cref="Span{T}"/>, <see cref="ReadOnlySpan{T}"/>, <see cref="Memory{T}"/> and <see cref="ReadOnlyMemory{T}"/> types.
/// </summary>
public static partial class MemoryExtensions
{
    extension<T>(Span<T>)
    {
        /// <summary>
        /// Proxies the <see cref="Span{T}.Span(void*, int)"/> constructor to avoid callers having to use <see langword="unsafe"/> for the <see langword="nint"/> to <see langword="void"/>* cast.
        /// </summary>
        /// <param name="address">The address of the memory to wrap.</param>
        /// <param name="length">The length of the span.</param>
        /// <returns>The created span.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<T> AroundUnmanaged(nint address, int length) => new Span<T>((void*)address, length);
    }
}
