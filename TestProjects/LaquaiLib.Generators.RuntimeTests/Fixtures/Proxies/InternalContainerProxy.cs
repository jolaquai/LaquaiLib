namespace LaquaiLib.Generators.RuntimeTests.Fixtures.Proxies;

internal static partial class InternalContainer
{
    [FullAccessProxy(typeof(SimpleFixture))]
    public partial class SimpleProxy;
}
