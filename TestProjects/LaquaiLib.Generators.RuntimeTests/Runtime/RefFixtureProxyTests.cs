namespace LaquaiLib.Generators.RuntimeTests.Runtime;

public class RefFixtureProxyTests
{
    [Fact]
    public void RefReturningPropertyAliasesUnderlyingField()
    {
        var target = new RefFixture();
        var proxy = new RefFixtureProxy(target);

        ref var aliased = ref proxy.ValueRef;
        aliased = 4321;

        var fieldInfo = typeof(RefFixture).GetField("_value", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.Equal(4321, (int)fieldInfo.GetValue(target));
    }
}
