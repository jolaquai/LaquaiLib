using LaquaiLib.UnsafeUtils.Accessors;

namespace LaquaiLib.UnitTests.UnsafeUtils.Accessors;

public class StackAccessorsTests
{
    [Fact]
    public void ArrayLengthAccommodatesCount()
    {
        var stack = new Stack<int>([1, 2, 3]);

        ref var array = ref StackAccessors<int>._array(stack);

        Assert.True(array.Length >= stack.Count);
    }

    [Fact]
    public void SizeMatchesCount()
    {
        var stack = new Stack<int>([1, 2, 3]);
        stack.Push(4);
        stack.Pop();
        stack.Pop();

        Assert.Equal(stack.Count, StackAccessors<int>._size(stack));
    }
}
