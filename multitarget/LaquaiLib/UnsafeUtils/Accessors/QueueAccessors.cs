namespace LaquaiLib.UnsafeUtils.Accessors;

public static class QueueAccessors<T>
{
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref T[] _array(Queue<T> _);
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref int _head(Queue<T> _);
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref int _tail(Queue<T> _);
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref int _size(Queue<T> _);
}
