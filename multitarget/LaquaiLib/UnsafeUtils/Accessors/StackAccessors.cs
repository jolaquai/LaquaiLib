namespace LaquaiLib.UnsafeUtils.Accessors;

public static class StackAccessors<T>
{
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref T[] _items(Stack<T> _);
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref int _size(Stack<T> _);
}
