namespace LaquaiLib.UnsafeUtils.Accessors;

public static class ListAccessors<T>
{
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref T[] _items(List<T> list);
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref int _size(List<T> list);
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref int _version(List<T> list);
}
