namespace LaquaiLib.UnsafeUtils.Accessors;

/// <summary>
/// Contains accessors for the <see cref="MemoryStream"/> type.
/// </summary>
public static class MemoryStreamAccessors
{
    /// <summary>
    /// Accesses the private field <c>_buffer</c> of a <see cref="MemoryStream"/> instance.
    /// </summary>
    /// <param name="_">The <see cref="MemoryStream"/> instance to access.</param>
    /// <returns>A <see langword="ref"/> into the field.</returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref byte[] _buffer(MemoryStream _);
    /// <summary>
    /// Accesses the private field <c>_origin</c> of a <see cref="MemoryStream"/> instance.
    /// </summary>
    /// <param name="_">The <see cref="MemoryStream"/> instance to access.</param>
    /// <returns>A <see langword="ref"/> into the field.</returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref int _origin(MemoryStream _);
    /// <summary>
    /// Accesses the private field <c>_capacity</c> of a <see cref="MemoryStream"/> instance.
    /// </summary>
    /// <param name="_">The <see cref="MemoryStream"/> instance to access.</param>
    /// <returns>A <see langword="ref"/> into the field.</returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref int _capacity(MemoryStream _);
    /// <summary>
    /// Accesses the private field <c>_length</c> of a <see cref="MemoryStream"/> instance.
    /// </summary>
    /// <param name="_">The <see cref="MemoryStream"/> instance to access.</param>
    /// <returns>A <see langword="ref"/> into the field.</returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref int _length(MemoryStream _);
    /// <summary>
    /// Accesses the private field <c>_position</c> of a <see cref="MemoryStream"/> instance.
    /// </summary>
    /// <param name="_">The <see cref="MemoryStream"/> instance to access.</param>
    /// <returns>A <see langword="ref"/> into the field.</returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref int _position(MemoryStream _);
    /// <summary>
    /// Accesses the private field <c>_expandable</c> of a <see cref="MemoryStream"/> instance.
    /// </summary>
    /// <param name="_">The <see cref="MemoryStream"/> instance to access.</param>
    /// <returns>A <see langword="ref"/> into the field.</returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref bool _expandable(MemoryStream _);
    /// <summary>
    /// Accesses the private field <c>_writable</c> of a <see cref="MemoryStream"/> instance.
    /// </summary>
    /// <param name="_">The <see cref="MemoryStream"/> instance to access.</param>
    /// <returns>A <see langword="ref"/> into the field.</returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref bool _writable(MemoryStream _);
    /// <summary>
    /// Accesses the private field <c>_exposable</c> of a <see cref="MemoryStream"/> instance.
    /// </summary>
    /// <param name="_">The <see cref="MemoryStream"/> instance to access.</param>
    /// <returns>A <see langword="ref"/> into the field.</returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref bool _exposable(MemoryStream _);
    /// <summary>
    /// Accesses the private field <c>_isOpen</c> of a <see cref="MemoryStream"/> instance.
    /// </summary>
    /// <param name="_">The <see cref="MemoryStream"/> instance to access.</param>
    /// <returns>A <see langword="ref"/> into the field.</returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref bool _isOpen(MemoryStream _);
}
