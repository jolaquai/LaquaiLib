namespace LaquaiLib.Generators.RuntimeTests.Runtime;

public class InternalAccessibilityTests
{
    [Fact]
    public void SameAssemblyInternalTypeCanBeProxiedAtAll()
    {
        // Pins capability (A): before real accessibility checks, an internal proxied type couldn't be proxied at all.
        var proxy = new InternalOnlyFixtureProxy(new InternalOnlyFixture());
        Assert.Equal(55, proxy.GetValue());
    }

    [Fact]
    public void MemberMentioningInternalTypeIsClampedToInternalNotPublic()
    {
        var flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var methodInfo = typeof(InternalMemberFixtureProxy).GetMethod("GetInternal", flags);

        Assert.NotNull(methodInfo);
        Assert.False(methodInfo.IsPublic);

        var proxy = new InternalMemberFixtureProxy(new InternalMemberFixture());
        Assert.Equal(88, proxy.GetInternal().Payload);
    }
}
