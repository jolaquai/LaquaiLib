using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Extensions;

public class DelegateExtensionsTests
{
    [Fact]
    public void GetInvocationListReturnsNullForNullDelegate()
    {
        Action testDelegate = null;

        var result = testDelegate.GetInvocationList<Action>();

        Assert.Null(result);
    }

    [Fact]
    public void GetInvocationListReturnsSingleElementForSingleDelegate()
    {
        Action testDelegate = () => { };

        var result = testDelegate.GetInvocationList<Action>();

        Assert.Single(result);
        Assert.IsType<Action>(result[0]);
    }

    [Fact]
    public void GetInvocationListReturnsMultipleElementsForMulticastDelegate()
    {
        Action action1 = () => { };
        Action action2 = () => { };
        Action action3 = () => { };

        var multicastDelegate = action1 + action2 + action3;

        var result = multicastDelegate.GetInvocationList<Action>();

        Assert.Equal(3, result.Length);
        Assert.All(result, d => Assert.IsType<Action>(d));
    }

    [Fact]
    public void GetInvocationListPreservesOriginalDelegateOrder()
    {
        var counter = 0;
        var callOrder = new int[3];

        Action action1 = () => callOrder[0] = ++counter;
        Action action2 = () => callOrder[1] = ++counter;
        Action action3 = () => callOrder[2] = ++counter;

        var multicastDelegate = action1 + action2 + action3;

        var result = multicastDelegate.GetInvocationList<Action>();

        foreach (var action in result)
        {
            action();
        }

        Assert.Equal(1, callOrder[0]);
        Assert.Equal(2, callOrder[1]);
        Assert.Equal(3, callOrder[2]);
    }

    [Fact]
    public void GetInvocationListWorksWithDifferentDelegateTypes()
    {
        Func<int> func1 = () => 1;
        Func<int> func2 = () => 2;

        var multicastFunc = func1 + func2;

        var result = multicastFunc.GetInvocationList<Func<int>>();

        Assert.Equal(2, result.Length);
        Assert.Equal(1, result[0]());
        Assert.Equal(2, result[1]());
    }

    [Fact]
    public void GetInvocationListWorksWithCustomDelegateTypes()
    {
        CustomDelegate del1 = (x) => x + 1;
        CustomDelegate del2 = (x) => x + 2;

        var multicast = del1 + del2;

        var result = multicast.GetInvocationList<CustomDelegate>();

        Assert.Equal(2, result.Length);
        Assert.Equal(11, result[0](10));
        Assert.Equal(12, result[1](10));
    }

    [Fact]
    public void GetInvocationListCanBeUsedWithCombinedDelegates()
    {
        Action action1 = () => { };
        Action action2 = () => { };

        var combined = Delegate.Combine(action1, action2) as Action;

        var result = combined.GetInvocationList<Action>();

        Assert.Equal(2, result.Length);
    }

    [Fact]
    public void GetInvocationListCanBeCalledMultipleTimes()
    {
        Action action1 = () => { };
        Action action2 = () => { };

        var multicast = action1 + action2;

        var result1 = multicast.GetInvocationList<Action>();
        var result2 = multicast.GetInvocationList<Action>();

        Assert.Equal(2, result1.Length);
        Assert.Equal(2, result2.Length);
        Assert.NotSame(result1, result2);
    }

    public delegate int CustomDelegate(int x);
}