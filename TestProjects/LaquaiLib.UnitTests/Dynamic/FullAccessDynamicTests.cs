using LaquaiLib.Dynamic;

using Microsoft.CSharp.RuntimeBinder;

namespace LaquaiLib.UnitTests.Dynamic;

public class FullAccessDynamicTests
{
    #region Test Fixture Classes

    public class DummyClass { }

    private class TestClass
    {
        public string PublicProperty { get; set; } = "PublicValue";
        private string PrivateProperty { get; set; } = "PrivateValue";
        protected string ProtectedProperty { get; set; } = "ProtectedValue";
        internal string InternalProperty { get; set; } = "InternalValue";

        public string PublicField = "PublicFieldValue";
        private string PrivateField = "PrivateFieldValue";
        protected string ProtectedField = "ProtectedFieldValue";
        internal string InternalField = "InternalFieldValue";

        public string PublicMethod() => "PublicMethodResult";
        private string PrivateMethod() => "PrivateMethodResult";
        protected string ProtectedMethod() => "ProtectedMethodResult";
        internal string InternalMethod() => "InternalMethodResult";

        public string MethodWithParameters(string param1, int param2) => $"{param1}{param2}";
        private string PrivateMethodWithParameters(string param1, int param2) => $"Private{param1}{param2}";

        public static string StaticProperty { get; set; } = "StaticPropertyValue";
        private static string PrivateStaticProperty { get; set; } = "PrivateStaticPropertyValue";
        public static string StaticField = "StaticFieldValue";
        private static string PrivateStaticField = "PrivateStaticFieldValue";
        public static string StaticMethod() => "StaticMethodResult";
        private static string PrivateStaticMethod() => "PrivateStaticMethodResult";

        public string NullReturningMethod() => null;

        public DummyClass NullProperty { get; set; } = null;
        public DummyClass GetNullObject() => null;
        public DummyClass NonNullProperty { get; set; } = new DummyClass();

        public override string ToString() => "TestClassToString";
    }

    private class IndexerTestClass
    {
        private readonly Dictionary<string, string> dictionary = new Dictionary<string, string>();

        public string this[string key]
        {
            get => dictionary.TryGetValue(key, out var value) ? value : null;
            set => dictionary[key] = value;
        }

        private string this[int index]
        {
            get => index.ToString();
            set { /* Do nothing */ }
        }
    }

    private class DelegateTestClass
    {
        public Func<string, string> PublicDelegate { get; set; }
        private Func<string, string> PrivateDelegate { get; set; }

        public DelegateTestClass()
        {
            PublicDelegate = (s) => $"Public: {s}";
            PrivateDelegate = (s) => $"Private: {s}";
        }
    }

    private class BaseClass
    {
        public string BasePublicProperty { get; set; } = "BasePublicValue";
        protected string BaseProtectedProperty { get; set; } = "BaseProtectedValue";
        private string BasePrivateProperty { get; set; } = "BasePrivateValue";

        public string BasePublicMethod() => "BasePublicMethodResult";
        protected string BaseProtectedMethod() => "BaseProtectedMethodResult";
        private string BasePrivateMethod() => "BasePrivateMethodResult";
    }

    private class DerivedClass : BaseClass
    {
        public string DerivedPublicProperty { get; set; } = "DerivedPublicValue";
        private string DerivedPrivateMethod() => "DerivedPrivateMethodResult";
    }

    #endregion

    #region Property Access Tests

    [Fact]
    public void CanAccessPublicProperty()
    {
        var testObj = new TestClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        string value = dynamicObj.PublicProperty;

        Assert.Equal("PublicValue", value);
    }

    [Fact]
    public void CanAccessPrivateProperty()
    {
        var testObj = new TestClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        string value = dynamicObj.PrivateProperty;

        Assert.Equal("PrivateValue", value);
    }

    [Fact]
    public void CanAccessProtectedProperty()
    {
        var testObj = new TestClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        string value = dynamicObj.ProtectedProperty;

        Assert.Equal("ProtectedValue", value);
    }

    [Fact]
    public void CanAccessInternalProperty()
    {
        var testObj = new TestClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        string value = dynamicObj.InternalProperty;

        Assert.Equal("InternalValue", value);
    }

    [Fact]
    public void CanSetPublicProperty()
    {
        var testObj = new TestClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        dynamicObj.PublicProperty = "NewPublicValue";

        Assert.Equal("NewPublicValue", testObj.PublicProperty);
    }

    [Fact]
    public void CanSetPrivateProperty()
    {
        var testObj = new TestClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        dynamicObj.PrivateProperty = "NewPrivateValue";

        Assert.Equal("NewPrivateValue", (string)dynamicObj.PrivateProperty);
    }

    [Fact]
    public void CanAccessStaticProperty()
    {
        dynamic dynamicObj = FullAccessDynamicFactory.Create(typeof(TestClass), null);

        string value = dynamicObj.StaticProperty;

        Assert.Equal("StaticPropertyValue", value);
    }

    [Fact]
    public void CanAccessPrivateStaticProperty()
    {
        dynamic dynamicObj = FullAccessDynamicFactory.Create(typeof(TestClass), null);

        string value = dynamicObj.PrivateStaticProperty;

        Assert.Equal("PrivateStaticPropertyValue", value);
    }

    [Fact]
    public void CanSetStaticProperty()
    {
        TestClass.StaticProperty = "OriginalStaticValue";
        dynamic dynamicObj = FullAccessDynamicFactory.Create(typeof(TestClass), null);

        dynamicObj.StaticProperty = "NewStaticValue";

        Assert.Equal("NewStaticValue", TestClass.StaticProperty);

        TestClass.StaticProperty = "StaticPropertyValue";
    }

    #endregion

    #region Field Access Tests

    [Fact]
    public void CanAccessPublicField()
    {
        var testObj = new TestClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        string value = dynamicObj.PublicField;

        Assert.Equal("PublicFieldValue", value);
    }

    [Fact]
    public void CanAccessPrivateField()
    {
        var testObj = new TestClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        string value = dynamicObj.PrivateField;

        Assert.Equal("PrivateFieldValue", value);
    }

    [Fact]
    public void CanSetPrivateField()
    {
        var testObj = new TestClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        dynamicObj.PrivateField = "NewPrivateFieldValue";

        Assert.Equal("NewPrivateFieldValue", (string)dynamicObj.PrivateField);
    }

    [Fact]
    public void CanAccessStaticField()
    {
        dynamic dynamicObj = FullAccessDynamicFactory.Create(typeof(TestClass), null);

        string value = dynamicObj.StaticField;

        Assert.Equal("StaticFieldValue", value);
    }

    [Fact]
    public void CanAccessPrivateStaticField()
    {
        dynamic dynamicObj = FullAccessDynamicFactory.Create(typeof(TestClass), null);

        string value = dynamicObj.PrivateStaticField;

        Assert.Equal("PrivateStaticFieldValue", value);
    }

    #endregion

    #region Method Invocation Tests

    [Fact]
    public void CanInvokePublicMethod()
    {
        var testObj = new TestClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        string result = dynamicObj.PublicMethod();

        Assert.Equal("PublicMethodResult", result);
    }

    [Fact]
    public void CanInvokePrivateMethod()
    {
        var testObj = new TestClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        string result = dynamicObj.PrivateMethod();

        Assert.Equal("PrivateMethodResult", result);
    }

    [Fact]
    public void CanInvokeProtectedMethod()
    {
        var testObj = new TestClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        string result = dynamicObj.ProtectedMethod();

        Assert.Equal("ProtectedMethodResult", result);
    }

    [Fact]
    public void CanInvokeMethodWithParameters()
    {
        var testObj = new TestClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        string result = dynamicObj.MethodWithParameters("Test", 123);

        Assert.Equal("Test123", result);
    }

    [Fact]
    public void CanInvokePrivateMethodWithParameters()
    {
        var testObj = new TestClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        string result = dynamicObj.PrivateMethodWithParameters("Test", 123);

        Assert.Equal("PrivateTest123", result);
    }

    [Fact]
    public void CanInvokeStaticMethod()
    {
        dynamic dynamicObj = FullAccessDynamicFactory.Create(typeof(TestClass), null);

        string result = dynamicObj.StaticMethod();

        Assert.Equal("StaticMethodResult", result);
    }

    [Fact]
    public void CanInvokePrivateStaticMethod()
    {
        dynamic dynamicObj = FullAccessDynamicFactory.Create(typeof(TestClass), null);

        string result = dynamicObj.PrivateStaticMethod();

        Assert.Equal("PrivateStaticMethodResult", result);
    }

    #endregion

    #region Indexer Tests

    [Fact]
    public void CanAccessPublicIndexer()
    {
        var testObj = new IndexerTestClass();
        testObj["TestKey"] = "TestValue";
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        string value = dynamicObj["TestKey"];

        Assert.Equal("TestValue", value);
    }

    [Fact]
    public void CanSetPublicIndexer()
    {
        var testObj = new IndexerTestClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        dynamicObj["TestKey"] = "TestValue";

        Assert.Equal("TestValue", testObj["TestKey"]);
    }

    [Fact]
    public void CanAccessPrivateIndexer()
    {
        var testObj = new IndexerTestClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        string value = dynamicObj[5];

        Assert.Equal("5", value);
    }

    #endregion

    #region Null Handling Tests

    [Fact]
    public void HandleNullInstanceGracefully()
    {
        TestClass testObj = null;
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        Assert.Null(dynamicObj.PublicProperty);
    }

    [Fact]
    public void NullReturnValueIsProperlyHandled()
    {
        var testObj = new TestClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        var result = dynamicObj.NullReturningMethod();

        Assert.Null(result);
    }

    [Fact]
    public void CanUnwrapOriginalObject()
    {
        var testObj = new TestClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        var unwrapped = dynamicObj.Unwrap();

        Assert.Same(testObj, unwrapped);
    }

    [Fact]
    public void CanHandleNullPropertyChain()
    {
        var testObj = new TestClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        var result = dynamicObj.NullProperty?.PublicProperty;
        Assert.Null(result);
    }

    [Fact]
    public void CanHandleNullMethodReturn()
    {
        var testObj = new TestClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        var result = dynamicObj.GetNullObject()?.PublicProperty;
        Assert.Null(result);
    }

    [Fact]
    public void CanAccessPropertyChain()
    {
        var testObj = new TestClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        var result = dynamicObj.NonNullProperty.PublicProperty;

        Assert.Equal("PublicValue", result);
    }

    #endregion

    #region Type Conversion Tests

    [Fact]
    public void CanConvertToOriginalType()
    {
        var testObj = new TestClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        TestClass converted = dynamicObj;

        Assert.Same(testObj, converted);
    }

    [Fact]
    public void ThrowsOnInvalidTypeConversion()
    {
        var testObj = new TestClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        Assert.Throws<InvalidCastException>(() => {
            int converted = dynamicObj;
        });
    }

    #endregion

    #region Delegate Invocation Tests

    [Fact]
    public void CanInvokePublicDelegate()
    {
        var testObj = new DelegateTestClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        var result = dynamicObj.PublicDelegate("Test");

        Assert.Equal("Public: Test", result);
    }

    [Fact]
    public void CanInvokePrivateDelegate()
    {
        var testObj = new DelegateTestClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        var result = dynamicObj.PrivateDelegate("Test");

        Assert.Equal("Private: Test", result);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void EqualsOperatorWorksCorrectly()
    {
        var testObj = new TestClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        Assert.True(dynamicObj == testObj);
        Assert.False(dynamicObj == new TestClass());
    }

    [Fact]
    public void NotEqualsOperatorWorksCorrectly()
    {
        var testObj = new TestClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        Assert.False(dynamicObj != testObj);
        Assert.True(dynamicObj != new TestClass());
    }

    [Fact]
    public void GetHashCodeReturnsUnderlyingObjectHashCode()
    {
        var testObj = new TestClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        Assert.Equal(testObj.GetHashCode(), dynamicObj.GetHashCode());
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void CanAccessBaseClassPublicProperty()
    {
        var testObj = new DerivedClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        string value = dynamicObj.BasePublicProperty;

        Assert.Equal("BasePublicValue", value);
    }

    [Fact]
    public void CanAccessBaseClassProtectedProperty()
    {
        var testObj = new DerivedClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        string value = dynamicObj.BaseProtectedProperty;

        Assert.Equal("BaseProtectedValue", value);
    }

    [Fact]
    public void CanAccessBaseClassPrivateProperty()
    {
        var testObj = new DerivedClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        string value = dynamicObj.BasePrivateProperty;

        Assert.Equal("BasePrivateValue", value);
    }

    [Fact]
    public void CanInvokeBaseClassPublicMethod()
    {
        var testObj = new DerivedClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        string result = dynamicObj.BasePublicMethod();

        Assert.Equal("BasePublicMethodResult", result);
    }

    [Fact]
    public void CanInvokeBaseClassProtectedMethod()
    {
        var testObj = new DerivedClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        string result = dynamicObj.BaseProtectedMethod();

        Assert.Equal("BaseProtectedMethodResult", result);
    }

    [Fact]
    public void CanInvokeBaseClassPrivateMethod()
    {
        var testObj = new DerivedClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        string result = dynamicObj.BasePrivateMethod();

        Assert.Equal("BasePrivateMethodResult", result);
    }

    #endregion

    #region Caching Tests

    [Fact]
    public void MemberCachingWorks()
    {
        var testObj = new TestClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        var value1 = dynamicObj.PrivateProperty;
        var value2 = dynamicObj.PrivateProperty;

        Assert.Equal(value1, value2);
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void DefaultConstructorCreatesInstance()
    {
        dynamic dynamicObj = FullAccessDynamicFactory.Create<TestClass>();

        Assert.NotNull(dynamicObj.Unwrap());
        Assert.IsType<TestClass>(dynamicObj.Unwrap());
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void ToStringReturnsUnderlyingToString()
    {
        var testObj = new TestClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        string result = dynamicObj.ToString();

        Assert.Equal(testObj.ToString(), result);
    }

    [Fact]
    public void EqualsMethodWorksCorrectly()
    {
        var testObj = new TestClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        Assert.True(dynamicObj.Equals(testObj));
        Assert.False(dynamicObj.Equals(new TestClass()));
    }

    [Fact]
    public void GetTypeReturnsUnderlyingType()
    {
        var testObj = new TestClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        Type type = dynamicObj.GetType();

        Assert.Equal(typeof(TestClass), type);
    }

    [Fact]
    public void GetDynamicMemberNamesReturnsPropertyNames()
    {
        var testObj = new TestClass();
        var dynamicObj = (FullAccessDynamic<TestClass>)FullAccessDynamicFactory.Create(testObj);

        var memberNames = dynamicObj.GetDynamicMemberNames();

        Assert.Contains("PublicProperty", memberNames);
        Assert.Contains("PrivateProperty", memberNames);
        Assert.Contains("ProtectedProperty", memberNames);
        Assert.Contains("InternalProperty", memberNames);
    }

    [Fact]
    public void NonExistentPropertyReturnsNull()
    {
        var testObj = new TestClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        Assert.Throws<RuntimeBinderException>(() => dynamicObj.NonExistentProperty);
    }

    [Fact]
    public void NonExistentMethodThrowsMemberNotFoundException()
    {
        var testObj = new TestClass();
        dynamic dynamicObj = FullAccessDynamicFactory.Create(testObj);

        Assert.ThrowsAny<Exception>(() => dynamicObj.NonExistentMethod());
    }

    #endregion
}
