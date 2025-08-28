namespace LaquaiLib;

internal class Internal
{
    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    private static extern MyStruct<int> InternalConstructor(int t);
}
public struct MyStruct<T>
{
    private T value;

    private MyStruct(T t)
    {
        value = t;
    }
}
