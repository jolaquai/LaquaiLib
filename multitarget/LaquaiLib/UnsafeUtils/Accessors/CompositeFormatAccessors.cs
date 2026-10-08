namespace LaquaiLib.UnsafeUtils.Accessors;

/// <summary>
/// Contains accessors for the <see cref="CompositeFormat"/> type.
/// </summary>
public static class CompositeFormatAccessors
{
    /// <summary>
    /// Accesses the private field <c>_segments</c> of a <see cref="CompositeFormat"/> instance.
    /// </summary>
    /// <param name="_">The <see cref="CompositeFormat"/> instance to access.</param>
    /// <returns>A <see langword="ref"/> into the field.</returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref (string Literal, int ArgIndex, int Alignment, string Format)[] _segments(CompositeFormat _);
}
