namespace LaquaiLib.Generators.RuntimeTests.Fixtures.Proxies;

// Declared internal (not public) so no FAP002 is emitted for the internal proxied type.
[FullAccessProxy(typeof(InternalOnlyFixture))]
internal partial class InternalOnlyFixtureProxy;
