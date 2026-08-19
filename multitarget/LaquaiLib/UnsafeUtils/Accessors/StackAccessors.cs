namespace LaquaiLib.UnsafeUtils.Accessors;

/// <summary>
/// Contains accessors for the <see cref="Stack{T}"/> type.
/// </summary>
/// <typeparam name="T">The type of the elements in the <see cref="Stack{T}"/>.</typeparam>
public static class StackAccessors<T>
{
    /// <summary>
    /// Accesses the private field <c>_items</c> of a <see cref="Stack{T}"/> instance.
    /// </summary>
    /// <param name="_">The <see cref="Stack{T}"/> instance to access.</param>
    /// <returns>A <see langword="ref"/> into the field.</returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref T[] _items(Stack<T> _);
    /// <summary>
    /// Accesses the private field <c>_size</c> of a <see cref="Stack{T}"/> instance.
    /// </summary>
    /// <param name="_">The <see cref="Stack{T}"/> instance to access.</param>
    /// <returns>A <see langword="ref"/> into the field.</returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref int _size(Stack<T> _);
}
