namespace LaquaiLib.Generators.RuntimeTests.Fixtures.Proxies;

public partial class GenericContainer<T>
{
    [FullAccessProxy(typeof(SimpleFixture))]
    public partial class SimpleProxy;
}
