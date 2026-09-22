namespace LaquaiLib.Generators.RuntimeTests.Runtime;

public class DerivedFixtureProxyTests
{
    [Fact]
    public void PrivateInstanceMethodOverloadsResolveToCorrectSignature()
    {
        var proxy = new DerivedFixtureProxy(new DerivedFixture());
        Assert.Equal(5, proxy.Add(2, 3));
        Assert.Equal(9, proxy.Add(2, 3, 4));
    }

    [Fact]
    public void PrivateStaticMethodComputesCorrectValue() => Assert.Equal(7, DerivedFixtureProxy.StaticAdd(3, 4));

    [Fact]
    public void PrivateInstanceFieldRoundTripsThroughRealStorage()
    {
        var target = new DerivedFixture();
        var proxy = new DerivedFixtureProxy(target);

        proxy._field = 1234;

        Assert.Equal(1234, proxy._field);
        var fieldInfo = typeof(DerivedFixture).GetField("_field", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.Equal(1234, (int)fieldInfo.GetValue(target));
    }

    [Fact]
    public void PrivateReadonlyFieldIsWritableThroughUnsafeAccessor()
    {
        // UnsafeAccessor bypasses the compile-time readonly check entirely (it's not a runtime concept for instance fields).
        var target = new DerivedFixture();
        var proxy = new DerivedFixtureProxy(target);

        proxy._readonlyField = 999;

        Assert.Equal(999, proxy._readonlyField);
        var fieldInfo = typeof(DerivedFixture).GetField("_readonlyField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.Equal(999, (int)fieldInfo.GetValue(target));
    }

    [Fact]
    public void PrivateStaticFieldMutationIsVisibleAcrossProxyInstancesAndRealType()
    {
        _ = new DerivedFixtureProxy(new DerivedFixture());
        _ = new DerivedFixtureProxy(new DerivedFixture());

        DerivedFixtureProxy.s_staticField = 4242;

        Assert.Equal(4242, DerivedFixtureProxy.s_staticField);
        var fieldInfo = typeof(DerivedFixture).GetField("s_staticField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert.Equal(4242, (int)fieldInfo.GetValue(null));
    }

    [Fact]
    public void InheritedPrivateBaseFieldIsReachableAndMutable()
    {
        var target = new DerivedFixture();
        var proxy = new DerivedFixtureProxy(target);

        Assert.Equal(11, proxy._baseField);
        proxy._baseField = 55;
        var fieldInfo = typeof(BaseFixture).GetField("_baseField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.Equal(55, (int)fieldInfo.GetValue(target));
    }

    [Fact]
    public void InheritedPrivateBaseMethodIsReachable()
    {
        var proxy = new DerivedFixtureProxy(new DerivedFixture());
        Assert.Equal(99, proxy.BaseOnlyMethod());
    }

    [Fact]
    public void VirtualMethodOverrideDispatchesToDerivedImplementationNotBase()
    {
        // UnsafeAccessor binding is non-virtual; this only works because the generator's dedup keeps the
        // DerivedFixture override (ContainingType == DerivedFixture) instead of BaseFixture's declaration.
        var proxy = new DerivedFixtureProxy(new DerivedFixture());
        Assert.Equal("derived-greeting", proxy.VirtualGreeting());
    }

    [Fact]
    public void PropertyGetSetRoundTrips()
    {
        var proxy = new DerivedFixtureProxy(new DerivedFixture());
        proxy.PropGetSet = 777;
        Assert.Equal(777, proxy.PropGetSet);
    }

    [Fact]
    public void GetOnlyPropertyReturnsComputedValue()
    {
        var proxy = new DerivedFixtureProxy(new DerivedFixture());
        Assert.Equal(123, proxy.PropGetOnly);
    }

    [Fact]
    public void InitOnlyPropertyIsProxiedAsGetOnly()
    {
        var proxy = new DerivedFixtureProxy(new DerivedFixture());
        Assert.Equal(456, proxy.PropInitOnly);
        // No setter should exist on the proxy for an init-only property.
        var propertyInfo = typeof(DerivedFixtureProxy).GetProperty("PropInitOnly");
        Assert.Null(propertyInfo.SetMethod);
    }

    [Fact]
    public void RefOutInParametersRoundTripCorrectly()
    {
        var proxy = new DerivedFixtureProxy(new DerivedFixture());
        var r = 10;
        var inVal = 5;
        var result = proxy.TryProcess(ref r, out var o, in inVal);

        Assert.True(result);
        Assert.Equal(15, r);
        Assert.Equal(10, o);
    }

    [Fact]
    public void ExplicitInterfaceImplementationDispatchesThroughCastInterface()
    {
        var proxy = new DerivedFixtureProxy(new DerivedFixture());
        Assert.Equal("hello-from-derived", ((IGreeter)proxy).Greet());
    }

    [Fact]
    public void StaticConstructorProxyCreatesWorkingInstance()
    {
        var proxy = DerivedFixtureProxy.Create();
        Assert.Equal(123, proxy.PropGetOnly);
    }

    // GENERATOR DEFECT: WriteEventProxy emits "Accessors.add_Notified(null, value)" for BOTH the static and
    // instance branches (copy-paste bug), instead of passing "_instance" in the instance branch. Subscribing
    // through the proxy therefore invokes the private instance add_Notified accessor with a null target and
    // throws a NullReferenceException instead of actually subscribing. This test documents the expected,
    // correct behavior and is left failing on purpose.
    [Fact]
    public void EventAddRemoveAndRaiseReachesHandler()
    {
        var target = new DerivedFixture();
        var proxy = new DerivedFixtureProxy(target);
        var received = -1;
        void Handler(object sender, int value) => received = value;

        proxy.Notified += Handler;
        target.RaiseNotified(7);
        Assert.Equal(7, received);

        proxy.Notified -= Handler;
        target.RaiseNotified(9);
        Assert.Equal(7, received);
    }
}
