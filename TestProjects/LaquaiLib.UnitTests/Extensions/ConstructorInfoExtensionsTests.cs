using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Extensions;

public class ConstructorInfoExtensionsTests
{
    public class SimpleClass
    {
        public int Value { get; }

        public SimpleClass() => Value = 0;
        public SimpleClass(int value) => Value = value;
        public SimpleClass(int value1, string value2) => Value = value1;
    }

    [Fact]
    public void CreateDelegateWithParameterlessConstructorCreatesWorkingDelegate()
    {
        var ctor = typeof(SimpleClass).GetConstructor(Type.EmptyTypes);
        var factory = ctor.CreateDelegate<Func<SimpleClass>>();

        var instance = factory();

        Assert.NotNull(instance);
        Assert.Equal(0, instance.Value);
    }

    [Fact]
    public void CreateDelegateWithSingleParameterConstructorCreatesWorkingDelegate()
    {
        var ctor = typeof(SimpleClass).GetConstructor([typeof(int)]);
        var factory = ctor.CreateDelegate<Func<int, SimpleClass>>();

        var instance = factory(42);

        Assert.NotNull(instance);
        Assert.Equal(42, instance.Value);
    }

    [Fact]
    public void CreateDelegateWithMultipleParametersConstructorCreatesWorkingDelegate()
    {
        var ctor = typeof(SimpleClass).GetConstructor([typeof(int), typeof(string)]);
        var factory = ctor.CreateDelegate<Func<int, string, SimpleClass>>();

        var instance = factory(42, "test");

        Assert.NotNull(instance);
        Assert.Equal(42, instance.Value);
    }

    [Fact]
    public void CreateDelegateWithNonFuncDelegateThrowsArgumentException()
    {
        var ctor = typeof(SimpleClass).GetConstructor(Type.EmptyTypes);

        var exception = Assert.Throws<ArgumentException>(ctor.CreateDelegate<Action>);

        Assert.Contains("is not a System.Func overload", exception.Message);
        Assert.Equal("TDelegate", exception.ParamName);
    }

    [Fact]
    public void CreateDelegateWithMismatchedReturnTypeThrowsArgumentException()
    {
        var ctor = typeof(SimpleClass).GetConstructor(Type.EmptyTypes);

        var exception = Assert.Throws<ArgumentException>(ctor.CreateDelegate<Func<string>>);

        Assert.Contains("does not match the constructor's signature", exception.Message);
        Assert.Contains("Delegate:", exception.Message);
        Assert.Contains("Constructor:", exception.Message);
        Assert.Equal("TDelegate", exception.ParamName);
    }

    [Fact]
    public void CreateDelegateWithMismatchedParameterCountThrowsArgumentException()
    {
        var ctor = typeof(SimpleClass).GetConstructor([typeof(int)]);

        var exception = Assert.Throws<ArgumentException>(ctor.CreateDelegate<Func<SimpleClass>>);

        Assert.Contains("does not match the constructor's signature", exception.Message);
        Assert.Contains("Delegate:", exception.Message);
        Assert.Contains("Constructor:", exception.Message);
        Assert.Equal("TDelegate", exception.ParamName);
    }

    [Fact]
    public void CreateDelegateWithMismatchedParameterTypesThrowsArgumentException()
    {
        var ctor = typeof(SimpleClass).GetConstructor([typeof(int)]);

        var exception = Assert.Throws<ArgumentException>(ctor.CreateDelegate<Func<string, SimpleClass>>);

        Assert.Contains("does not match the constructor's signature", exception.Message);
        Assert.Contains("Delegate:", exception.Message);
        Assert.Contains("Constructor:", exception.Message);
        Assert.Equal("TDelegate", exception.ParamName);
    }

    [Fact]
    public void CreateDelegateWithGenericTypesConstructorCreatesWorkingDelegate()
    {
        var ctor = typeof(KeyValuePair<int, string>).GetConstructor([typeof(int), typeof(string)]);
        var factory = ctor.CreateDelegate<Func<int, string, KeyValuePair<int, string>>>();

        var instance = factory(42, "test");

        Assert.Equal(42, instance.Key);
        Assert.Equal("test", instance.Value);
    }

    [Fact]
    public void CreateDelegateWithValueTypeConstructorCreatesWorkingDelegate()
    {
        var ctor = typeof(DateTime).GetConstructor([typeof(int), typeof(int), typeof(int)]);
        var factory = ctor.CreateDelegate<Func<int, int, int, DateTime>>();

        var instance = factory(2023, 12, 31);

        Assert.Equal(2023, instance.Year);
        Assert.Equal(12, instance.Month);
        Assert.Equal(31, instance.Day);
    }

    [Fact]
    public void CreateDelegatePerformanceCreatingDelegateIsFasterThanActivator()
    {
        var ctor = typeof(SimpleClass).GetConstructor([typeof(int)]);
        var factory = ctor.CreateDelegate<Func<int, SimpleClass>>();

        for (var i = 0; i < 100; i++)
        {
            factory(i);
            Activator.CreateInstance(typeof(SimpleClass), i);
        }

        const int iterations = 100000;

        var delegateWatch = System.Diagnostics.Stopwatch.StartNew();
        for (var i = 0; i < iterations; i++)
        {
            factory(i);
        }
        delegateWatch.Stop();

        var activatorWatch = System.Diagnostics.Stopwatch.StartNew();
        for (var i = 0; i < iterations; i++)
        {
            Activator.CreateInstance(typeof(SimpleClass), i);
        }
        activatorWatch.Stop();

        Assert.True(delegateWatch.ElapsedTicks < activatorWatch.ElapsedTicks);
    }
}
