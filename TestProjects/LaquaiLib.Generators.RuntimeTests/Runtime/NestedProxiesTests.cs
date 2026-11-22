namespace LaquaiLib.Generators.RuntimeTests.Runtime;

public class NestedProxiesTests
{
    [Fact]
    public void NestedOneLevelProxyReachesPrivateMember()
    {
        var proxy = new Level1Container.SimpleProxy(new SimpleFixture());
        Assert.Equal(42, proxy.GetValue());
    }

    [Fact]
    public void NestedTwoLevelsProxyReachesPrivateMember()
    {
        var proxy = new Level2Outer.Level2Inner.SimpleProxy(new SimpleFixture());
        Assert.Equal(42, proxy.GetValue());
    }

    [Fact]
    public void InternalContainerProxyReachesPrivateMember()
    {
        var proxy = new InternalContainer.SimpleProxy(new SimpleFixture());
        Assert.Equal(42, proxy.GetValue());
    }

    [Fact]
    public void GenericContainerProxyReproducesTypeParameterAndReachesPrivateMember()
    {
        var proxy = new GenericContainer<int>.SimpleProxy(new SimpleFixture());
        Assert.Equal(42, proxy.GetValue());
    }

    [Fact]
    public void StringFormAttributeResolvesTypeAndReachesPrivateMember()
    {
        var proxy = new StringFormProxy(new SimpleFixture());
        Assert.Equal(42, proxy.GetValue());
    }
}
