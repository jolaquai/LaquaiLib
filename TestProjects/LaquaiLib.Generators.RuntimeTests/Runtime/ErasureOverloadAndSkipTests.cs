namespace LaquaiLib.Generators.RuntimeTests.Runtime;

public class ErasureOverloadAndSkipTests
{
    [Fact]
    public void CollidingErasedOverloadsCollapseToExactlyOneMethod()
    {
        var methods = typeof(WhichFixtureProxy).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(m => m.Name == "Which")
            .ToArray();
        Assert.Single(methods);
    }

    [Fact]
    public void SurvivingErasedOverloadIsCallableWithARealInstance()
    {
        var proxy = new WhichFixtureProxy(new WhichFixture());
        var a = WhichFixtureProxy.MakeA();
        Assert.Equal("A", proxy.Which(a));
    }

    [Fact]
    public void UnsupportedFieldEventAndStructReturningMethodAreNotExposedOnProxy()
    {
        var type = typeof(SkippedMembersFixtureProxy);
        var flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly;

        Assert.Null(type.GetField("_hiddenField", flags));
        Assert.Null(type.GetEvent("HiddenEvent", flags));
        Assert.Null(type.GetMethod("GetHiddenStruct", flags));
    }

    [Fact]
    public void ProxyRemainsUsableDespiteSkippedMembers()
    {
        var proxy = new SkippedMembersFixtureProxy(new SkippedMembersFixture());
        Assert.Equal(123, proxy.Marker());
    }
}
