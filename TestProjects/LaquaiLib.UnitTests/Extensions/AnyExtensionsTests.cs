using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Extensions;

public class AnyExtensionsTests
{
    #region AllEqual Tests

    [Fact]
    public void AllEqualWithSpanEmptyCollectionReturnsTrue()
    {
        var source = 42;
        ReadOnlySpan<int> other = [];

        var result = source.AllEqual(other);

        Assert.True(result);
    }

    [Fact]
    public void AllEqualWithSpanAllElementsEqualReturnsTrue()
    {
        var source = 42;
        ReadOnlySpan<int> other = [42, 42, 42];

        var result = source.AllEqual(other);

        Assert.True(result);
    }

    [Fact]
    public void AllEqualWithSpanSomeElementsNotEqualReturnsFalse()
    {
        var source = 42;
        ReadOnlySpan<int> other = [42, 43, 42];

        var result = source.AllEqual(other);

        Assert.False(result);
    }

    [Fact]
    public void AllEqualWithSpanSourceNullAllOthersNullReturnsTrue()
    {
        string source = null;
        ReadOnlySpan<string> other = [null, null];

        var result = source.AllEqual(other);

        Assert.True(result);
    }

    [Fact]
    public void AllEqualWithSpanSourceNullSomeOthersNotNullReturnsFalse()
    {
        string source = null;
        ReadOnlySpan<string> other = [null, "test"];

        var result = source.AllEqual(other);

        Assert.False(result);
    }

    [Fact]
    public void AllEqualWithSpanCustomObjectsReturnsTrue()
    {
        var source = new TestObject { Id = 1, Name = "Test" };
        ReadOnlySpan<TestObject> other =
            [
                new TestObject { Id = 1, Name = "Test" },
                new TestObject { Id = 1, Name = "Test" }
            ];

        var result = source.AllEqual(other);

        Assert.True(result);
    }

    [Fact]
    public void AllEqualWithEnumerableEmptyCollectionReturnsTrue()
    {
        var source = 42;
        IEnumerable<int> other = [];

        var result = source.AllEqual(other);

        Assert.True(result);
    }

    [Fact]
    public void AllEqualWithEnumerableAllElementsEqualReturnsTrue()
    {
        var source = 42;
        IEnumerable<int> other = [42, 42, 42];

        var result = source.AllEqual(other);

        Assert.True(result);
    }

    [Fact]
    public void AllEqualWithEnumerableSomeElementsNotEqualReturnsFalse()
    {
        var source = 42;
        IEnumerable<int> other = [42, 43, 42];

        var result = source.AllEqual(other);

        Assert.False(result);
    }

    [Fact]
    public void AllEqualWithEnumerableStopsEnumerationOnFirstNonEqual()
    {
        var source = 42;
        var enumerableWithTracking = new TrackingEnumerable<int>([42, 43, 42]);

        var result = source.AllEqual(enumerableWithTracking);

        Assert.False(result);
        Assert.Equal(2, enumerableWithTracking.EnumeratedCount);
    }

    #endregion

    #region EqualBy Tests

    [Fact]
    public void EqualByWithSpanEmptyCollectionReturnsTrue()
    {
        var source = "test";
        ReadOnlySpan<string> other = [];
        static int transform(string s) => s?.Length ?? 0;

        var result = source.EqualBy(transform, other);

        Assert.True(result);
    }

    [Fact]
    public void EqualByWithSpanAllElementsEqualAfterTransformReturnsTrue()
    {
        var source = "test";
        ReadOnlySpan<string> other = ["1234", "abcd", "wxyz"];
        static int transform(string s) => s.Length;

        var result = source.EqualBy(transform, other);

        Assert.True(result);
    }

    [Fact]
    public void EqualByWithSpanSomeElementsNotEqualAfterTransformReturnsFalse()
    {
        var source = "test";
        ReadOnlySpan<string> other = ["1234", "abc", "wxyz"];
        static int transform(string s) => s.Length;

        var result = source.EqualBy(transform, other);

        Assert.False(result);
    }

    [Fact]
    public void EqualByWithSpanTransformReturnsNullHandlesProperly()
    {
        var source = new TestObject { Id = 1, Name = "Test" };
        ReadOnlySpan<TestObject> other =
            [
                new TestObject { Id = 2, Name = null },
                new TestObject { Id = 3, Name = null }
            ];
        static string transform(TestObject o) => o.Name;

        var result = source.EqualBy(transform, other);

        Assert.False(result);
    }

    [Fact]
    public void EqualByWithEnumerableEmptyCollectionReturnsTrue()
    {
        var source = "test";
        IEnumerable<string> other = [];
        static int transform(string s) => s?.Length ?? 0;

        var result = source.EqualBy(transform, other);

        Assert.True(result);
    }

    [Fact]
    public void EqualByWithEnumerableAllElementsEqualAfterTransformReturnsTrue()
    {
        var source = new TestObject { Id = 1, Name = "Test" };
        IEnumerable<TestObject> other =
            [
                new TestObject { Id = 2, Name = "Test" },
                new TestObject { Id = 3, Name = "Test" }
            ];
        static string transform(TestObject o) => o.Name;

        var result = source.EqualBy(transform, other);

        Assert.True(result);
    }

    [Fact]
    public void EqualByWithEnumerableSomeElementsNotEqualAfterTransformReturnsFalse()
    {
        var source = new TestObject { Id = 1, Name = "Test" };
        IEnumerable<TestObject> other =
            [
                new TestObject { Id = 2, Name = "Test" },
                new TestObject { Id = 3, Name = "Different" }
            ];
        static string transform(TestObject o) => o.Name;

        var result = source.EqualBy(transform, other);

        Assert.False(result);
    }

    [Fact]
    public void EqualByWithEnumerableStopsEnumerationOnFirstNonEqual()
    {
        var source = new TestObject { Id = 1, Name = "Test" };
        var otherObjects = new[]
            {
                new TestObject { Id = 2, Name = "Test" },
                new TestObject { Id = 3, Name = "Different" },
                new TestObject { Id = 4, Name = "Test" }
            };
        var enumerableWithTracking = new TrackingEnumerable<TestObject>(otherObjects);
        static string transform(TestObject o) => o.Name;

        var result = source.EqualBy(transform, enumerableWithTracking);

        Assert.False(result);
        Assert.Equal(2, enumerableWithTracking.EnumeratedCount);
    }

    #endregion

    #region IsNull Tests

    [Fact]
    public void IsNullWithNullObjectReturnsTrue()
    {
        string obj = null;

        var result = obj.IsNull();

        Assert.True(result);
    }

    [Fact]
    public void IsNullWithNonNullObjectReturnsFalse()
    {
        var obj = "test";

        var result = obj.IsNull();

        Assert.False(result);
    }

    [Fact]
    public void IsNullWithNullableValueTypeReturnsExpectedResult()
    {
        int? nullableInt = null;
        int? nonNullInt = 42;

        var nullResult = nullableInt.IsNull();
        var nonNullResult = nonNullInt.IsNull();

        Assert.True(nullResult);
        Assert.False(nonNullResult);
    }

    #endregion

    #region With Tests

    [Fact]
    public void WithSynchronousActionExecutesAction()
    {
        var obj = new TestObject { Id = 1, Name = "Initial" };
        var actionExecuted = false;

        var result = obj.With(o =>
        {
            o.Name = "Modified";
            actionExecuted = true;
        });

        Assert.True(actionExecuted);
        Assert.Equal("Modified", obj.Name);
        Assert.Same(obj, result);
    }

    [Fact]
    public void WithSynchronousActionSupportsChaining()
    {
        var obj = new TestObject { Id = 1, Name = "Initial" };

        var result = obj
                .With(o => o.Id = 2)
                .With(o => o.Name = "Modified");

        Assert.Equal(2, obj.Id);
        Assert.Equal("Modified", obj.Name);
        Assert.Same(obj, result);
    }

    [Fact]
    public async Task WithAsynchronousActionExecutesAction()
    {
        var obj = new TestObject { Id = 1, Name = "Initial" };
        var actionExecuted = false;

        var result = await obj.With(async o =>
        {
            await Task.Delay(10).ConfigureAwait(false);
            o.Name = "Modified";
            actionExecuted = true;
        });

        Assert.True(actionExecuted);
        Assert.Equal("Modified", obj.Name);
        Assert.Same(obj, result);
    }

    [Fact]
    public async Task WithAsynchronousActionSupportsChaining()
    {
        var obj = new TestObject { Id = 1, Name = "Initial" };

        var result = await obj
                .With(async o =>
                {
                    await Task.Delay(10).ConfigureAwait(false);
                    o.Id = 2;
                })
                .With(async o =>
                {
                    await Task.Delay(10).ConfigureAwait(false);
                    o.Name = "Modified";
                });

        Assert.Equal(2, obj.Id);
        Assert.Equal("Modified", obj.Name);
        Assert.Same(obj, result);
    }

    #endregion

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
                return false;

            return Id == other.Id && Name == other.Name;
        }

        public override int GetHashCode() => HashCode.Combine(Id, Name);
    }

    public class TrackingEnumerable<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> _source;
        public int EnumeratedCount { get; private set; }

        public TrackingEnumerable(IEnumerable<T> source)
        {
            _source = source;
            EnumeratedCount = 0;
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in _source)
            {
                EnumeratedCount++;
                yield return item;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class Base { }
    public class Derived : Base { }

    #endregion
}