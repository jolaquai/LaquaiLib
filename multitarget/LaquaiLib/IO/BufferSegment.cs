namespace LaquaiLib.IO;

/// <summary>
/// Represents one array within a segmented buffer, paired with the portion of it the buffer actually addresses.
/// </summary>
/// <remarks>
/// <see cref="Length"/> is usually the whole of <see cref="Array"/>, but a segment may deliberately address less than it holds; the remainder is then owned by the segment and unreachable through the buffer until it is released. Keeping the addressed extent separate from the array's own length is what lets a buffer end a segment early instead of copying its contents somewhere larger.
/// </remarks>
/// <typeparam name="T">The element type of the underlying array.</typeparam>
/// <param name="array">The array backing the segment.</param>
/// <param name="length">The number of elements at the start of <paramref name="array"/> the buffer addresses.</param>
internal readonly struct BufferSegment<T>(T[] array, int length)
{
    /// <summary>
    /// Gets the array backing this segment. It may be longer than <see cref="Length"/>.
    /// </summary>
    public T[] Array { get; } = array;
    /// <summary>
    /// Gets the number of elements at the start of <see cref="Array"/> that the buffer addresses.
    /// </summary>
    public int Length { get; } = length;

    /// <summary>
    /// Gets a <see cref="Span{T}"/> over the addressed portion of this segment.
    /// </summary>
    public Span<T> Span
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Array.AsSpan(0, Length);
    }
    /// <summary>
    /// Gets a <see cref="Memory{T}"/> over the addressed portion of this segment.
    /// </summary>
    public Memory<T> Memory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Array.AsMemory(0, Length);
    }

    /// <summary>
    /// Creates a segment addressing the entirety of <paramref name="array"/>.
    /// </summary>
    /// <param name="array">The array backing the segment.</param>
    /// <returns>The created <see cref="BufferSegment{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BufferSegment<T> Full(T[] array) => new BufferSegment<T>(array, array.Length);
    /// <summary>
    /// Creates a segment over the same array as this one, addressing only the first <paramref name="length"/> elements.
    /// </summary>
    /// <param name="length">The number of elements to address. Must not exceed the current <see cref="Length"/>.</param>
    /// <returns>The created <see cref="BufferSegment{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BufferSegment<T> Truncate(int length)
    {
        Debug.Assert(length >= 0 && length <= Length, "A segment can only ever address less of its array, never more.");
        return new BufferSegment<T>(Array, length);
    }
}
