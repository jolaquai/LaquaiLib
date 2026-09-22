namespace LaquaiLib.Generators.RuntimeTests.Runtime;

public class CtorFixtureProxyTests
{
    [Fact]
    public void StaticConstructorProxyUsesPrivateConstructorOverload()
    {
        var proxy = CtorFixtureProxy.Create(42);
        Assert.Equal(42, proxy.Instance.Value);
        Assert.Equal("private", proxy.Instance.Tag);
    }

    [Fact]
    public void StaticConstructorProxyUsesPublicConstructorOverload()
    {
        var proxy = CtorFixtureProxy.Create("hello");
        Assert.Equal(-1, proxy.Instance.Value);
        Assert.Equal("hello", proxy.Instance.Tag);
    }
}
