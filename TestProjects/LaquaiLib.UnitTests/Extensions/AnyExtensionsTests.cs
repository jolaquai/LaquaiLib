using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Extensions;

public class AnyExtensionsTests
{
    #region As Tests

    [Fact]
    public void AsWithValidCastReturnsCorrectlyTypedObject()
    {
        object obj = "test string";

        var result = obj.As<string>();

        Assert.IsType<string>(result);
        Assert.Equal("test string", result);
    }

    [Fact]
    public void AsWithInheritanceReturnsCorrectlyTypedObject()
    {
        var derived = new Derived();
        object obj = derived;

        var resultBase = obj.As<Base>();
        var resultDerived = obj.As<Derived>();

        Assert.IsType<Derived>(resultBase);
        Assert.IsType<Derived>(resultDerived);
        Assert.Same(derived, resultBase);
        Assert.Same(derived, resultDerived);
    }

    #endregion

    #region Helper Classes

    public class TestObject : IEquatable<TestObject>
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public override bool Equals(object obj) => Equals(obj as TestObject);

        public bool Equals(TestObject other)
        {
            if (other is null)
            {
                return false;
            }

            return Id == other.Id && Name == other.Name;
        }

        public override int GetHashCode() => HashCode.Combine(Id, Name);
    }

    public class Base { }
    public class Derived : Base { }

    #endregion
}