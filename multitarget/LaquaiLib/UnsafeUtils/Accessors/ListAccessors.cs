namespace LaquaiLib.UnsafeUtils.Accessors;

/// <summary>
/// Contains accessors for the <see cref="List{T}"/> type.
/// </summary>
/// <typeparam name="T">The type of the elements in the <see cref="List{T}"/>.</typeparam>
public static class ListAccessors<T>
{
    /// <summary>
    /// Accesses the private field <c>_items</c> of a <see cref="List{T}"/> instance.
    /// </summary>
    /// <param name="_">The <see cref="List{T}"/> instance to access.</param>
    /// <returns>A <see langword="ref"/> into the field.</returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref T[] _items(List<T> _);
    /// <summary>
    /// Accesses the private field <c>_size</c> of a <see cref="List{T}"/> instance.
    /// </summary>
    /// <param name="_">The <see cref="List{T}"/> instance to access.</param>
    /// <returns>A <see langword="ref"/> into the field.</returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref int _size(List<T> _);
    /// <summary>
    /// Accesses the private field <c>_version</c> of a <see cref="List{T}"/> instance.
    /// </summary>
    /// <param name="_">The <see cref="List{T}"/> instance to access.</param>
    /// <returns>A <see langword="ref"/> into the field.</returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref int _version(List<T> _);
}
