namespace LaquaiLib.UnsafeUtils.Accessors;

/// <summary>
/// Contains accessors for the <see cref="Queue{T}"/> type.
/// </summary>
/// <typeparam name="T">The type of the elements in the <see cref="Queue{T}"/>.</typeparam>
public static class QueueAccessors<T>
{
    /// <summary>
    /// Accesses the private field <c>_array</c> of a <see cref="Queue{T}"/> instance.
    /// </summary>
    /// <param name="_">The <see cref="Queue{T}"/> instance to access.</param>
    /// <returns>A <see langword="ref"/> into the field.</returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref T[] _array(Queue<T> _);
    /// <summary>
    /// Accesses the private field <c>_head</c> of a <see cref="Queue{T}"/> instance.
    /// </summary>
    /// <param name="_">The <see cref="Queue{T}"/> instance to access.</param>
    /// <returns>A <see langword="ref"/> into the field.</returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref int _head(Queue<T> _);
    /// <summary>
    /// Accesses the private field <c>_tail</c> of a <see cref="Queue{T}"/> instance.
    /// </summary>
    /// <param name="_">The <see cref="Queue{T}"/> instance to access.</param>
    /// <returns>A <see langword="ref"/> into the field.</returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref int _tail(Queue<T> _);
    /// <summary>
    /// Accesses the private field <c>_size</c> of a <see cref="Queue{T}"/> instance.
    /// </summary>
    /// <param name="_">The <see cref="Queue{T}"/> instance to access.</param>
    /// <returns>A <see langword="ref"/> into the field.</returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref int _size(Queue<T> _);
}
