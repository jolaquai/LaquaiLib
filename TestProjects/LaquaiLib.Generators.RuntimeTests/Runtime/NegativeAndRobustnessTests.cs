namespace LaquaiLib.Generators.RuntimeTests.Runtime;

public class NegativeAndRobustnessTests
{
    [Fact]
    public void ProxyConstructorThrowsArgumentNullExceptionForNullInstance() => Assert.Throws<ArgumentNullException>(() => new DerivedFixtureProxy(null));

    [Fact]
    public void ProxyConstructorThrowsArgumentNullExceptionForNullInstanceOnBclProxy() => Assert.Throws<ArgumentNullException>(() => new MemoryStreamProxy(null));

    [Fact]
    public void NoMissingMemberExceptionEscapesFromOverloadedMethodDispatch()
    {
        var proxy = new DerivedFixtureProxy(new DerivedFixture());
        var exception = Record.Exception(() =>
        {
            _ = proxy.Add(1, 2);
            _ = proxy.Add(1, 2, 3);
            _ = DerivedFixtureProxy.StaticAdd(1, 2);
        });
        Assert.Null(exception);
    }

    [Fact]
    public void NoMissingMemberExceptionEscapesFromFieldAndPropertyAccess()
    {
        var proxy = new DerivedFixtureProxy(new DerivedFixture());
        var exception = Record.Exception(() =>
        {
            proxy._field = 1;
            _ = proxy._field;
            proxy.PropGetSet = 2;
            _ = proxy.PropGetSet;
            _ = proxy.PropGetOnly;
            _ = proxy.PropInitOnly;
            _ = proxy._baseField;
            _ = proxy.BaseOnlyMethod();
        });
        Assert.Null(exception);
    }
}
