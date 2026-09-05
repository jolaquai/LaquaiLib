namespace LaquaiLib.Generators.RuntimeTests.Fixtures.Proxies;

// ExternalErasureFixture is genuinely inaccessible from this assembly (internal, no InternalsVisibleTo), so
// typeof(...) would fail to compile here (CS0122); the string overload defers resolution to the generator instead.
[FullAccessProxy("LaquaiLib.Generators.RuntimeTests.External.ExternalErasureFixture", IncludeInaccessible = true)]
public partial class ExternalErasureFixtureProxy;
