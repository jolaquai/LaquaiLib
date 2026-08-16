namespace LaquaiLib.Generators.RuntimeTests.Fixtures.Proxies;

// Task itself turned out not to expose any member whose signature actually requires erasure (no [UnsafeAccessorType]
// was emitted), so StringBuilder is used as the fallback per spec: only ordinary public members are asserted here,
// since BCL internals are version-fragile.
[FullAccessProxy(typeof(System.Text.StringBuilder), IncludeInaccessible = true)]
public partial class StringBuilderProxy;
