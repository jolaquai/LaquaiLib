namespace LaquaiLib.Generators.RuntimeTests.Fixtures.Proxies;

public static partial class Level1Container
{
    [FullAccessProxy(typeof(SimpleFixture))]
    public partial class SimpleProxy;
}
