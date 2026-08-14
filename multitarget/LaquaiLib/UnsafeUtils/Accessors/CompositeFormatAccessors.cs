namespace LaquaiLib.UnsafeUtils.Accessors;

public static class CompositeFormatAccessors
{
    [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref (string Literal, int ArgIndex, int Alignment, string Format)[] _segments(CompositeFormat comp);

}
