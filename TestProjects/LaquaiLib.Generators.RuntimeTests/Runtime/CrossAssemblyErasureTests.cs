namespace LaquaiLib.Generators.RuntimeTests.Runtime;

public class CrossAssemblyErasureTests
{
    [Fact]
    public void ProxiedStringBuilderRoundTripsOrdinaryPublicMembers()
    {
        var proxy = new StringBuilderProxy(new System.Text.StringBuilder());
        proxy.Append("hello");
        proxy.Append(' ');
        proxy.Append("world");

        Assert.Equal("hello world", proxy.ToString());
        Assert.Equal(11, proxy.Length);
    }

    [Fact]
    public void ExternalErasedMethodResultRoundTripsThroughErasedParameter()
    {
        var proxy = ExternalErasureFixtureProxy.Create();
        var hidden = proxy.MakeHidden(42);
        Assert.NotNull(hidden);
        Assert.Equal(42, proxy.ReadHidden(hidden));
    }

    [Fact]
    public void ExternalErasedPropertyGetAndSetRoundTrip()
    {
        var proxy = ExternalErasureFixtureProxy.Create();
        var hidden = proxy.MakeHidden(7);

        proxy.HiddenProperty = hidden;

        Assert.Equal(7, proxy.ReadHidden(proxy.HiddenProperty));
    }

    [Fact]
    public void ExternalErasedStaticMethodProducesUsableResult()
    {
        var hidden = ExternalErasureFixtureProxy.CreateHidden(99);
        var proxy = ExternalErasureFixtureProxy.Create();
        Assert.Equal(99, proxy.ReadHidden(hidden));
    }

    [Fact]
    public void ExternalTopLevelTypeItselfIsErasedAndUsable()
    {
        // Pins that the PROXIED type's own name (not just a member's signature type) can require erasure:
        // ExternalErasureFixture is internal with no InternalsVisibleTo, so even ExternalErasureFixtureProxy.Instance
        // is typed as object, and the ctor accessor resolves it purely through the assembly-qualified metadata name.
        var proxy = ExternalErasureFixtureProxy.Create();
        Assert.NotNull(proxy.Instance);
        Assert.Equal(5, proxy.RoundTrip(5));
    }

    [Fact]
    public void ExternalConstructorWithErasedParameterWorksThroughStaticFactories()
    {
        var key = ExternalKeyedFixtureProxy.MakeKey(9);
        var proxy = ExternalKeyedFixtureProxy.Create(key);
        Assert.Equal(9, proxy.KeyValue);
    }

    [Fact]
    public void SameAssemblyAndCrossAssemblyErasureBothWork()
    {
        // Pins the distinction MetadataTypeName.TryBuild draws: same-assembly names omit the assembly qualifier,
        // cross-assembly names include it (", LaquaiLib.Generators.RuntimeTests.External"). Both must resolve at runtime.
        var sameAssembly = new ErasureFixtureProxy(new Fixtures.ErasureFixture());
        var sameSecret = sameAssembly.MakeSecret(1);
        Assert.Equal(1, sameAssembly.ReadSecret(sameSecret));

        var crossAssembly = ExternalErasureFixtureProxy.Create();
        var crossHidden = crossAssembly.MakeHidden(2);
        Assert.Equal(2, crossAssembly.ReadHidden(crossHidden));
    }
}
