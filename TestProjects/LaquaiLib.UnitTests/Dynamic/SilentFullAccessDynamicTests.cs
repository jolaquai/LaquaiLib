using LaquaiLib.Dynamic;

namespace LaquaiLib.UnitTests.Dynamic;

// Test classes for different scenarios
public class TestClass
{
    public string PublicField = "PublicField";
    private string privateField = "PrivateField";
    protected string ProtectedField = "ProtectedField";
    internal string InternalField = "InternalField";
    public static string StaticField = "StaticField";

    public string PublicProperty { get; set; } = "PublicProperty";
    private string PrivateProperty { get; set; } = "PrivateProperty";
    protected string ProtectedProperty { get; set; } = "ProtectedProperty";
    internal string InternalProperty { get; set; } = "InternalProperty";
    public static string StaticProperty { get; set; } = "StaticProperty";

    public string PublicMethod() => "PublicMethod";
    private string PrivateMethod() => "PrivateMethod";
    protected string ProtectedMethod() => "ProtectedMethod";
    internal string InternalMethod() => "InternalMethod";
    public static string StaticMethod() => "StaticMethod";

    public string ParameterizedMethod(string param1, int param2) => $"Params: {param1}, {param2}";

    public string NullReturningMethod() => null;

    public string NullProperty { get; set; } = null;

    public string this[int index]
    {
        get => $"Index: {index}";
        set { /* Do nothing for test */ }
    }

    public AnotherTestClass GetAnotherObject() => new AnotherTestClass();

    public override string ToString() => "TestClass.ToString()";
}

public class AnotherTestClass
{
    public string AnotherPublicProperty { get; set; } = "AnotherPublicProperty";
    private string anotherPrivateProperty { get; set; } = "AnotherPrivateProperty";

    public string AnotherPublicMethod() => "AnotherPublicMethod";
    private string AnotherPrivateMethod() => "AnotherPrivateMethod";
}

public class TestClassWithEquals
{
    public int Id { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is TestClassWithEquals other)
            return Id == other.Id;
        return false;
    }

    public override int GetHashCode() => Id.GetHashCode();
}

public class DelegateTestClass
{
    public Func<string, int, string> MyDelegate { get; set; }

    public DelegateTestClass() => MyDelegate = static (s, i) => $"{s}: {i}";
}

public class SilentFullAccessDynamicTests
{
    #region Creation Tests

    [Fact]
    public void CreateWithNullInstanceReturnsWrapperWithNullUnwrap()
    {
        var wrapper = new SilentFullAccessDynamic<TestClass>(null);

        Assert.Null(wrapper.Unwrap());
    }

    [Fact]
    public void CreateWithInstanceReturnsWrapperWithSameInstance()
    {
        var testObject = new TestClass();

        var wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        Assert.Same(testObject, wrapper.Unwrap());
    }

    [Fact]
    public void DefaultConstructorCreatesEmptyInstance()
    {
        var wrapper = new SilentFullAccessDynamic<TestClass>();
        var instance = wrapper.Unwrap();

        Assert.NotNull(instance);
        Assert.IsType<TestClass>(instance);
    }

    #endregion

    #region Property Access Tests

    [Fact]
    public void TryGetMemberWithPublicPropertyReturnsValue()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var result = wrapper.PublicProperty;

        Assert.Equal("PublicProperty", result);
    }

    [Fact]
    public void TryGetMemberWithPrivatePropertyReturnsValue()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var result = wrapper.PrivateProperty;

        Assert.Equal("PrivateProperty", result);
    }

    [Fact]
    public void TryGetMemberWithProtectedPropertyReturnsValue()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var result = wrapper.ProtectedProperty;

        Assert.Equal("ProtectedProperty", result);
    }

    [Fact]
    public void TryGetMemberWithInternalPropertyReturnsValue()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var result = wrapper.InternalProperty;

        Assert.Equal("InternalProperty", result);
    }

    [Fact]
    public void TryGetMemberWithStaticPropertyReturnsValue()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var result = wrapper.StaticProperty;

        Assert.Equal("StaticProperty", result);
    }

    [Fact]
    public void TryGetMemberWithNullInstanceReturnsNull()
    {
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(null);

        var result = wrapper.PublicProperty;

        Assert.Null(result);
    }

    [Fact]
    public void TryGetMemberWithNonExistentPropertyReturnsNull()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var result = wrapper.NonExistentProperty;

        Assert.Null(result);
    }

    #endregion

    #region Field Access Tests

    [Fact]
    public void TryGetMemberWithPublicFieldReturnsValue()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var result = wrapper.PublicField;

        Assert.Equal("PublicField", result);
    }

    [Fact]
    public void TryGetMemberWithPrivateFieldReturnsValue()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var result = wrapper.privateField;

        Assert.Equal("PrivateField", result);
    }

    [Fact]
    public void TryGetMemberWithProtectedFieldReturnsValue()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var result = wrapper.ProtectedField;

        Assert.Equal("ProtectedField", result);
    }

    [Fact]
    public void TryGetMemberWithInternalFieldReturnsValue()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var result = wrapper.InternalField;

        Assert.Equal("InternalField", result);
    }

    [Fact]
    public void TryGetMemberWithStaticFieldReturnsValue()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var result = wrapper.StaticField;

        Assert.Equal("StaticField", result);
    }

    #endregion

    #region Method Invocation Tests

    [Fact]
    public void TryInvokeMemberWithPublicMethodReturnsValue()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var result = wrapper.PublicMethod();

        Assert.Equal("PublicMethod", result);
    }

    [Fact]
    public void TryInvokeMemberWithPrivateMethodReturnsValue()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var result = wrapper.PrivateMethod();

        Assert.Equal("PrivateMethod", result);
    }

    [Fact]
    public void TryInvokeMemberWithProtectedMethodReturnsValue()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var result = wrapper.ProtectedMethod();

        Assert.Equal("ProtectedMethod", result);
    }

    [Fact]
    public void TryInvokeMemberWithInternalMethodReturnsValue()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var result = wrapper.InternalMethod();

        Assert.Equal("InternalMethod", result);
    }

    [Fact]
    public void TryInvokeMemberWithStaticMethodReturnsValue()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var result = wrapper.StaticMethod();

        Assert.Equal("StaticMethod", result);
    }

    [Fact]
    public void TryInvokeMemberWithParameterizedMethodReturnsValue()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var result = wrapper.ParameterizedMethod("hello", 42);

        Assert.Equal("Params: hello, 42", result);
    }

    [Fact]
    public void TryInvokeMemberWithNullInstanceReturnsNull()
    {
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(null);

        var result = wrapper.PublicMethod();

        Assert.Null(result);
    }

    [Fact]
    public void TryInvokeMemberWithNonExistentMethodReturnsNull()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var result = wrapper.NonExistentMethod();

        Assert.Null(result);
    }

    #endregion

    #region Inherited Methods Tests

    [Fact]
    public void TryInvokeMemberToStringCallsUnderlyingMethod()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var result = wrapper.ToString();

        Assert.Equal("TestClass.ToString()", result);
    }

    [Fact]
    public void TryInvokeMemberGetHashCodeCallsUnderlyingMethod()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var result = wrapper.GetHashCode();

        Assert.Equal(testObject.GetHashCode(), result);
    }

    [Fact]
    public void TryInvokeMemberGetTypeCallsUnderlyingMethod()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var result = wrapper.GetType();

        Assert.Equal(testObject.GetType(), result);
    }

    [Fact]
    public void TryInvokeMemberEqualsCallsUnderlyingMethod()
    {
        var testObject1 = new TestClassWithEquals { Id = 1 };
        var testObject2 = new TestClassWithEquals { Id = 1 };
        dynamic wrapper = new SilentFullAccessDynamic<TestClassWithEquals>(testObject1);

        var result = wrapper.Equals(testObject2);

        Assert.True(result);
    }

    [Fact]
    public void TryInvokeMemberUnwrapReturnsUnderlyingInstance()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var result = wrapper.Unwrap();

        Assert.Same(testObject, result);
    }

    #endregion

    #region Indexer Tests

    [Fact]
    public void TryGetIndexWithValidIndexReturnsValue()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var result = wrapper[5];

        Assert.Equal("Index: 5", result);
    }

    [Fact]
    public void TrySetIndexWithValidIndexDoesNotThrow()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var exception = Record.Exception(() => wrapper[5] = "Test");
        Assert.Null(exception);
    }

    [Fact]
    public void TryGetIndexWithNullInstanceReturnsNull()
    {
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(null);

        var result = wrapper[5];

        Assert.Null(result);
    }

    #endregion

    #region Member Assignment Tests

    [Fact]
    public void TrySetMemberWithPublicPropertySetsValue()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        wrapper.PublicProperty = "Modified";

        Assert.Equal("Modified", testObject.PublicProperty);
    }

    [Fact]
    public void TrySetMemberWithPrivatePropertySetsValue()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        wrapper.PrivateProperty = "Modified";

        Assert.Equal("Modified", wrapper.PrivateProperty);
    }

    [Fact]
    public void TrySetMemberWithPublicFieldSetsValue()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        wrapper.PublicField = "Modified";

        Assert.Equal("Modified", testObject.PublicField);
    }

    [Fact]
    public void TrySetMemberWithPrivateFieldSetsValue()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        wrapper.privateField = "Modified";

        Assert.Equal("Modified", wrapper.privateField);
    }

    [Fact]
    public void TrySetMemberWithNullInstanceDoesNotThrow()
    {
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(null);

        var exception = Record.Exception(() => wrapper.PublicProperty = "Test");
        Assert.Null(exception);
    }

    [Fact]
    public void TrySetMemberWithNonExistentPropertyDoesNotThrow()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var exception = Record.Exception(() => wrapper.NonExistentProperty = "Test");
        Assert.Null(exception);
    }

    #endregion

    #region Chained Access Tests

    [Fact]
    public void ChainedAccessToPublicMembersReturnsValue()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var result = wrapper.GetAnotherObject().AnotherPublicProperty;

        Assert.Equal("AnotherPublicProperty", result);
    }

    [Fact]
    public void ChainedAccessToPrivateMembersReturnsValue()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var result = wrapper.GetAnotherObject().anotherPrivateProperty;

        Assert.Equal("AnotherPrivateProperty", result);
    }

    [Fact]
    public void ChainedMethodInvocationReturnsValue()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var result = wrapper.GetAnotherObject().AnotherPublicMethod();

        Assert.Equal("AnotherPublicMethod", result);
    }

    [Fact]
    public void ChainedPrivateMethodInvocationReturnsValue()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var result = wrapper.GetAnotherObject().AnotherPrivateMethod();

        Assert.Equal("AnotherPrivateMethod", result);
    }

    #endregion

    #region Null Propagation Tests

    [Fact]
    public void NullPropertyChainedAccessReturnsNull()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var result = wrapper.NullProperty?.ToString();

        Assert.Null(result);
    }

    [Fact]
    public void NullReturningMethodChainedAccessReturnsNull()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var result = wrapper.NullReturningMethod()?.ToString();

        Assert.Null(result);
    }

    #endregion

    #region Type Conversion Tests

    [Fact]
    public void TryConvertToAssignableTypeReturnsInstance()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        TestClass converted = wrapper;

        Assert.Same(testObject, converted);
    }

    [Fact]
    public void TryConvertToNonAssignableTypeReturnsNull()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        int? converted = wrapper;
        Assert.Null(converted);
    }

    #endregion

    #region Operator Tests

    [Fact]
    public void EqualsOperatorWithSameInstanceReturnsTrue()
    {
        var testObject = new TestClassWithEquals { Id = 1 };
        var wrapper = new SilentFullAccessDynamic<TestClassWithEquals>(testObject);
        var testObject2 = new TestClassWithEquals { Id = 1 };

        dynamic dynamicWrapper = wrapper;
        Assert.True(dynamicWrapper == testObject2);
    }

    [Fact]
    public void NotEqualsOperatorWithDifferentInstanceReturnsTrue()
    {
        var testObject = new TestClassWithEquals { Id = 1 };
        var wrapper = new SilentFullAccessDynamic<TestClassWithEquals>(testObject);
        var testObject2 = new TestClassWithEquals { Id = 2 };

        dynamic dynamicWrapper = wrapper;
        Assert.True(dynamicWrapper != testObject2);
    }

    #endregion

    #region Dynamic Member Names Test

    [Fact]
    public void GetDynamicMemberNamesReturnsPropertyNames()
    {
        var testObject = new TestClass();
        var wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var memberNames = wrapper.GetDynamicMemberNames().ToList();

        Assert.Contains("PublicProperty", memberNames);
    }

    #endregion

    #region Delegate Invocation Tests

    [Fact]
    public void TryInvokeWithDelegateInvokesDelegate()
    {
        var testDelegate = new Func<string, int, string>(static (s, i) => $"{s}: {i}");
        dynamic wrapper = new SilentFullAccessDynamic<Func<string, int, string>>(testDelegate);

        var result = wrapper("Hello", 42);

        Assert.Equal("Hello: 42", result);
    }

    #endregion

    #region Method Caching Tests

    [Fact]
    public void MethodCacheRepeatInvocationUsesCachedMethod()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var result1 = wrapper.PublicMethod();
        var result2 = wrapper.PublicMethod();

        Assert.Equal("PublicMethod", result1);
        Assert.Equal("PublicMethod", result2);
    }

    [Fact]
    public void PropertyCacheRepeatAccessUsesCachedProperty()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var result1 = wrapper.PublicProperty;
        var result2 = wrapper.PublicProperty;

        Assert.Equal("PublicProperty", result1);
        Assert.Equal("PublicProperty", result2);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void NonWrappedTypeDoesNotLoseOriginalBehavior()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var type = wrapper.GetType();
        var unwrappedObj = wrapper.Unwrap();

        Assert.Equal(testObject.GetType(), type);
        Assert.Same(testObject, unwrappedObj);
    }

    [Fact]
    public void MethodInvocationWithNullArgumentHandlesNullProperly()
    {
        var testObject = new TestClass();
        dynamic wrapper = new SilentFullAccessDynamic<TestClass>(testObject);

        var result = wrapper.ParameterizedMethod(null, 42);

        Assert.Equal("Params: , 42", result);
    }

    #endregion
}
