namespace LaquaiLib.Generators.RuntimeTests.Runtime;

public class ErasureTests
{
    [Fact]
    public void ErasedMethodResultRoundTripsThroughErasedParameter()
    {
        var proxy = new ErasureFixtureProxy(new ErasureFixture());
        var secret = proxy.MakeSecret(41);
        Assert.NotNull(secret);
        Assert.Equal(41, proxy.ReadSecret(secret));
    }

    [Fact]
    public void ErasedPropertyGetAndSetRoundTrip()
    {
        var proxy = new ErasureFixtureProxy(new ErasureFixture());
        var secret = proxy.MakeSecret(7);

        proxy.SecretProperty = secret;

        Assert.Equal(7, proxy.ReadSecret(proxy.SecretProperty));
    }

    [Fact]
    public void ErasedStaticMethodProducesUsableResult()
    {
        var secret = ErasureFixtureProxy.CreateSecret(99);
        var proxy = new ErasureFixtureProxy(new ErasureFixture());
        Assert.Equal(99, proxy.ReadSecret(secret));
    }

    [Fact]
    public void ErasedResultDeclaredOnBaseTypeTargetsBaseTypeAccessor()
    {
        // Pins that [UnsafeAccessor] targets ErasureBaseFixture (the member's own containing type), not ErasureDerivedFixture.
        var proxy = new ErasureDerivedFixtureProxy(new ErasureDerivedFixture());
        var secret = proxy.GetBaseSecret();
        Assert.NotNull(secret);
        Assert.Equal(7, proxy.ReadBaseSecret(secret));
    }

    [Fact]
    public void ConstructorWithErasedParameterWorksThroughStaticFactories()
    {
        var key = KeyedFixtureProxy.MakeKey(9);
        var proxy = KeyedFixtureProxy.Create(key);
        Assert.Equal(9, proxy.KeyValue);
    }
}
