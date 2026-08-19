namespace LaquaiLib.UnsafeUtils.Accessors;

public static class MemoryStreamAccessors
{
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref byte[] _buffer(MemoryStream _);
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref int _origin(MemoryStream _);
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref int _capacity(MemoryStream _);
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref int _length(MemoryStream _);
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref int _position(MemoryStream _);
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref bool _expandable(MemoryStream _);
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref bool _writable(MemoryStream _);
}
