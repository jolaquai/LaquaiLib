namespace LaquaiLib.Generators.RuntimeTests.Fixtures.Proxies;

public static partial class Level2Outer
{
    public static partial class Level2Inner
    {
        [FullAccessProxy(typeof(SimpleFixture))]
        public partial class SimpleProxy;
    }
}
