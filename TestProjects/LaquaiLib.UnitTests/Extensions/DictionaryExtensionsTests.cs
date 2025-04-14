using System.Collections.Concurrent;
using System.Collections.ObjectModel;

using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Extensions;

public class DictionaryExtensionsTests
{
    #region CloneTests

    [Fact]
    public void CloneReturnsNewDictionaryWithSameContents()
    {
        var source = new Dictionary<string, int> { { "one", 1 }, { "two", 2 }, { "three", 3 } };

        var result = source.Clone();

        Assert.NotSame(source, result);
        Assert.Equal(source.Count, result.Count);
        Assert.All(source, kvp => Assert.Equal(kvp.Value, result[kvp.Key]));
    }

    [Fact]
    public void CloneWorksWithEmptyDictionary()
    {
        var source = new Dictionary<string, int>();

        var result = source.Clone();

        Assert.NotSame(source, result);
        Assert.Empty(result);
    }

    [Fact]
    public void CloneWithReferenceTypeCreatesShallowCopy()
    {
        var list1 = new List<int> { 1, 2, 3 };
        var list2 = new List<int> { 4, 5, 6 };
        var source = new Dictionary<string, List<int>> { { "one", list1 }, { "two", list2 } };

        var result = source.Clone();

        Assert.NotSame(source, result);
        Assert.Same(list1, result["one"]);
        Assert.Same(list2, result["two"]);
    }
    #endregion

    #region GetOrAddTests

    [Fact]
    public void GetOrAddReturnsExistingValue()
    {
        var source = new Dictionary<string, int> { { "one", 1 } };

        var result = source.GetOrAdd("one", 100);

        Assert.Equal(1, result);
        Assert.Equal(1, source["one"]);
    }

    [Fact]
    public void GetOrAddAddsNewKeyValue()
    {
        var source = new Dictionary<string, int> { { "one", 1 } };

        var result = source.GetOrAdd("two", 2);

        Assert.Equal(2, result);
        Assert.Equal(2, source["two"]);
    }

    [Fact]
    public void GetOrAddThrowsForReadOnlyDictionary()
    {
        var readOnly = new ReadOnlyDictionary<string, int>(new Dictionary<string, int> { { "one", 1 } });

        var exception = Assert.Throws<ArgumentException>(() => readOnly.GetOrAdd("two", 2));
        Assert.Contains("mutable", exception.Message);
    }

    [Fact]
    public void GetOrAddThrowsForNullKey()
    {
        var source = new Dictionary<string, int>();

        Assert.Throws<ArgumentNullException>(() => source.GetOrAdd(null, 1));
    }

    [Fact]
    public void GetOrAddThrowsForNullSource()
    {
        Dictionary<string, int> source = null;

        Assert.Throws<ArgumentNullException>(() => source.GetOrAdd("one", 1));
    }

    [Fact]
    public void GetOrAddWithFactoryReturnsExistingValue()
    {
        var source = new Dictionary<string, int> { { "one", 1 } };
        bool factoryCalled = false;

        var result = source.GetOrAdd("one", () => { factoryCalled = true; return 100; });

        Assert.Equal(1, result);
        Assert.False(factoryCalled);
    }

    [Fact]
    public void GetOrAddWithFactoryAddsNewValue()
    {
        var source = new Dictionary<string, int> { { "one", 1 } };

        var result = source.GetOrAdd("two", () => 2);

        Assert.Equal(2, result);
        Assert.Equal(2, source["two"]);
    }

    [Fact]
    public void GetOrAddWithFactoryThrowsForNullFactory()
    {
        var source = new Dictionary<string, int>();
        Func<int> factory = null;

        Assert.Throws<ArgumentNullException>(() => source.GetOrAdd("two", factory));
    }

    [Fact]
    public void GetOrAddWithConcurrentDictionaryUsesNativeMethod()
    {
        var source = new ConcurrentDictionary<string, int>();
        source.TryAdd("one", 1);

        var result = source.GetOrAdd("one", 100);
        Assert.Equal(1, result);

        result = source.GetOrAdd("two", 2);
        Assert.Equal(2, result);
        Assert.Equal(2, source["two"]);
    }

    [Fact]
    public void GetOrAddWithFactoryAndConcurrentDictionaryUsesNativeMethod()
    {
        var source = new ConcurrentDictionary<string, int>();
        source.TryAdd("one", 1);
        bool factoryCalled = false;

        var result = source.GetOrAdd("one", () => { factoryCalled = true; return 100; });
        Assert.Equal(1, result);
        Assert.False(factoryCalled);

        result = source.GetOrAdd("two", () => 2);
        Assert.Equal(2, result);
        Assert.Equal(2, source["two"]);
    }

    [Fact]
    public void GetOrAddWithOutParameterReturnsTrueForExistingKey()
    {
        var source = new Dictionary<string, int> { { "one", 1 } };

        bool found = source.GetOrAdd("one", 100, out var value);

        Assert.True(found);
        Assert.Equal(1, value);
    }

    [Fact]
    public void GetOrAddWithOutParameterReturnsFalseForNewKey()
    {
        var source = new Dictionary<string, int> { { "one", 1 } };

        bool found = source.GetOrAdd("two", 2, out var value);

        Assert.False(found);
        Assert.Equal(2, value);
        Assert.Equal(2, source["two"]);
    }

    [Fact]
    public void GetOrAddWithOutParameterAndFactoryReturnsTrueForExistingKey()
    {
        var source = new Dictionary<string, int> { { "one", 1 } };
        bool factoryCalled = false;

        bool found = source.GetOrAdd("one", () => { factoryCalled = true; return 100; }, out var value);

        Assert.True(found);
        Assert.Equal(1, value);
        Assert.False(factoryCalled);
    }

    [Fact]
    public void GetOrAddWithOutParameterAndFactoryReturnsFalseForNewKey()
    {
        var source = new Dictionary<string, int> { { "one", 1 } };

        bool found = source.GetOrAdd("two", () => 2, out var value);

        Assert.False(found);
        Assert.Equal(2, value);
        Assert.Equal(2, source["two"]);
    }

    [Fact]
    public void GetOrAddWithOutParameterThrowsForReadOnlyDictionary()
    {
        var readOnly = new ReadOnlyDictionary<string, int>(new Dictionary<string, int> { { "one", 1 } });

        Assert.Throws<ArgumentException>(() => readOnly.GetOrAdd("one", 100, out _));
    }

    [Fact]
    public void GetOrAddWithOutParameterAndFactoryThrowsForReadOnlyDictionary()
    {
        var readOnly = new ReadOnlyDictionary<string, int>(new Dictionary<string, int> { { "one", 1 } });

        Assert.Throws<ArgumentException>(() => readOnly.GetOrAdd("one", () => 100, out _));
    }
    #endregion

    #region AddOrUpdateTests

    [Fact]
    public void AddOrUpdateAddsNewKeyValue()
    {
        var source = new Dictionary<string, int> { { "one", 1 } };

        source.AddOrUpdate("two", 2, oldValue => oldValue * 10);

        Assert.Equal(2, source["two"]);
    }

    [Fact]
    public void AddOrUpdateUpdatesExistingValue()
    {
        var source = new Dictionary<string, int> { { "one", 1 } };

        source.AddOrUpdate("one", 100, oldValue => oldValue * 10);

        Assert.Equal(10, source["one"]);
    }

    [Fact]
    public void AddOrUpdateThrowsForReadOnlyDictionary()
    {
        var readOnly = new ReadOnlyDictionary<string, int>(new Dictionary<string, int> { { "one", 1 } });

        var exception = Assert.Throws<ArgumentException>(() =>
                readOnly.AddOrUpdate("one", 100, oldValue => oldValue * 10));
        Assert.Contains("mutable", exception.Message);
    }

    [Fact]
    public void AddOrUpdateThrowsForNullKey()
    {
        var source = new Dictionary<string, int>();

        Assert.Throws<ArgumentNullException>(() =>
            source.AddOrUpdate(null, 1, oldValue => oldValue * 10));
    }

    [Fact]
    public void AddOrUpdateThrowsForNullSource()
    {
        Dictionary<string, int> source = null;

        Assert.Throws<ArgumentNullException>(() =>
            source.AddOrUpdate("one", 1, oldValue => oldValue * 10));
    }

    [Fact]
    public void AddOrUpdateWithExistingKeyThrowsForNullUpdateFactory()
    {
        var source = new Dictionary<string, int> { { "one", 1 } };
        Func<int, int> updateFactory = null;

        Assert.Throws<ArgumentNullException>(() =>
            source.AddOrUpdate("one", 100, updateFactory));
    }

    [Fact]
    public void AddOrUpdateWithTwoValueFactoryAddsNewValue()
    {
        var source = new Dictionary<string, int> { { "one", 1 } };

        source.AddOrUpdate("two", 2, (oldValue, newValue) => oldValue + newValue);

        Assert.Equal(2, source["two"]);
    }

    [Fact]
    public void AddOrUpdateWithTwoValueFactoryUpdatesExistingValue()
    {
        var source = new Dictionary<string, int> { { "one", 1 } };

        source.AddOrUpdate("one", 10, (oldValue, newValue) => oldValue + newValue);

        Assert.Equal(11, source["one"]);
    }

    [Fact]
    public void AddOrUpdateWithTwoValueFactoryThrowsForNullUpdateFactory()
    {
        var source = new Dictionary<string, int> { { "one", 1 } };
        Func<int, int, int> updateFactory = null;

        Assert.Throws<ArgumentNullException>(() =>
            source.AddOrUpdate("one", 100, updateFactory));
    }

    [Fact]
    public void AddOrUpdateWithValueFactoryAddsNewValue()
    {
        var source = new Dictionary<string, int> { { "one", 1 } };
        bool addFactoryCalled = false;

        source.AddOrUpdate("two", () => { addFactoryCalled = true; return 2; }, oldValue => oldValue * 10);

        Assert.True(addFactoryCalled);
        Assert.Equal(2, source["two"]);
    }

    [Fact]
    public void AddOrUpdateWithValueFactoryUpdatesExistingValue()
    {
        var source = new Dictionary<string, int> { { "one", 1 } };
        bool addFactoryCalled = false;

        source.AddOrUpdate("one", () => { addFactoryCalled = true; return 100; }, oldValue => oldValue * 10);

        Assert.False(addFactoryCalled);
        Assert.Equal(10, source["one"]);
    }

    [Fact]
    public void AddOrUpdateWithValueFactoryThrowsForNullAddFactory()
    {
        var source = new Dictionary<string, int>();
        Func<int> addFactory = null;

        Assert.Throws<ArgumentNullException>(() =>
            source.AddOrUpdate("two", addFactory, oldValue => oldValue * 10));
    }

    [Fact]
    public void AddOrUpdateWithConcurrentDictionaryUsesNativeMethod()
    {
        var source = new ConcurrentDictionary<string, int>();
        source.TryAdd("one", 1);

        source.AddOrUpdate("one", 100, oldValue => oldValue * 10);
        Assert.Equal(10, source["one"]);

        source.AddOrUpdate("two", 2, oldValue => oldValue * 10);
        Assert.Equal(2, source["two"]);
    }

    [Fact]
    public void AddOrUpdateWithTwoValueFactoryAndConcurrentDictionaryUsesNativeMethod()
    {
        var source = new ConcurrentDictionary<string, int>();
        source.TryAdd("one", 1);

        source.AddOrUpdate("one", 10, (oldValue, newValue) => oldValue + newValue);
        Assert.Equal(11, source["one"]);

        source.AddOrUpdate("two", 2, (oldValue, newValue) => oldValue + newValue);
        Assert.Equal(2, source["two"]);
    }

    [Fact]
    public void AddOrUpdateWithValueFactoryAndConcurrentDictionaryUsesNativeMethod()
    {
        var source = new ConcurrentDictionary<string, int>();
        source.TryAdd("one", 1);

        source.AddOrUpdate("one", () => 100, oldValue => oldValue * 10);
        Assert.Equal(10, source["one"]);

        source.AddOrUpdate("two", () => 2, oldValue => oldValue * 10);
        Assert.Equal(2, source["two"]);
    }
    #endregion

    #region GetValueRefTests

    [Fact]
    public void GetValueRefOrNullRefWithOutParameterReturnsTrueForExistingKey()
    {
        var source = new Dictionary<string, int> { { "one", 1 } };

        ref var valueRef = ref source.GetValueRefOrNullRef("one", out bool existed);

        Assert.True(existed);
        Assert.Equal(1, valueRef);
    }

    [Fact]
    public void GetValueRefOrNullRefWithOutParameterReturnsFalseForMissingKey()
    {
        var source = new Dictionary<string, int> { { "one", 1 } };

        ref var valueRef = ref source.GetValueRefOrNullRef("two", out bool existed);

        Assert.False(existed);
    }

    [Fact]
    public void GetValueRefOrNullRefWithOutParameterThrowsForNullDictionary()
    {
        Dictionary<string, int> source = null;

        Assert.Throws<ArgumentNullException>(() => source.GetValueRefOrNullRef("one", out _));
    }

    [Fact]
    public void GetValueRefOrNullRefWithOutParameterThrowsForNullKey()
    {
        var source = new Dictionary<string, int> { { "one", 1 } };

        Assert.Throws<ArgumentNullException>(() => source.GetValueRefOrNullRef(null, out _));
    }

    [Fact]
    public void GetValueRefOrAddReturnsRefToExistingValue()
    {
        var source = new Dictionary<string, int> { { "one", 1 } };

        ref var valueRef = ref source.GetValueRefOrAdd("one", 100, out bool existed);

        Assert.True(existed);
        Assert.Equal(1, valueRef);

        valueRef = 10;
        Assert.Equal(10, source["one"]);
    }

    [Fact]
    public void GetValueRefOrAddAddsAndReturnsRefToNewValue()
    {
        var source = new Dictionary<string, int> { { "one", 1 } };

        ref var valueRef = ref source.GetValueRefOrAdd("two", 2, out bool existed);

        Assert.False(existed);
        Assert.Equal(2, valueRef);
        Assert.Equal(2, source["two"]);

        valueRef = 20;
        Assert.Equal(20, source["two"]);
    }

    [Fact]
    public void GetValueRefOrAddThrowsForNullDictionary()
    {
        Dictionary<string, int> source = null;

        Assert.Throws<ArgumentNullException>(() => source.GetValueRefOrAdd("one", 1, out _));
    }

    [Fact]
    public void GetValueRefOrAddThrowsForNullKey()
    {
        var source = new Dictionary<string, int> { { "one", 1 } };

        Assert.Throws<ArgumentNullException>(() => source.GetValueRefOrAdd(null, 2, out _));
    }

    [Fact]
    public void GetValueRefOrAddWithFactoryReturnsRefToExistingValue()
    {
        var source = new Dictionary<string, int> { { "one", 1 } };
        bool factoryCalled = false;

        ref var valueRef = ref source.GetValueRefOrAdd("one", () => { factoryCalled = true; return 100; }, out bool existed);

        Assert.True(existed);
        Assert.False(factoryCalled);
        Assert.Equal(1, valueRef);

        valueRef = 10;
        Assert.Equal(10, source["one"]);
    }

    [Fact]
    public void GetValueRefOrAddWithFactoryAddsAndReturnsRefToNewValue()
    {
        var source = new Dictionary<string, int> { { "one", 1 } };

        ref var valueRef = ref source.GetValueRefOrAdd("two", () => 2, out bool existed);

        Assert.False(existed);
        Assert.Equal(2, valueRef);
        Assert.Equal(2, source["two"]);

        valueRef = 20;
        Assert.Equal(20, source["two"]);
    }

    [Fact]
    public void GetValueRefOrAddWithFactoryThrowsForNullDictionary()
    {
        Dictionary<string, int> source = null;

        Assert.Throws<ArgumentNullException>(() => source.GetValueRefOrAdd("one", () => 1, out _));
    }

    [Fact]
    public void GetValueRefOrAddWithFactoryThrowsForNullKey()
    {
        var source = new Dictionary<string, int> { { "one", 1 } };

        Assert.Throws<ArgumentNullException>(() => source.GetValueRefOrAdd(null, () => 2, out _));
    }

    [Fact]
    public void GetValueRefOrAddWithFactoryThrowsForNullFactory()
    {
        var source = new Dictionary<string, int> { { "one", 1 } };
        Func<int> factory = null;

        Assert.Throws<ArgumentNullException>(() => source.GetValueRefOrAdd("two", factory, out _));
    }

    [Fact]
    public void CollectionsMarshalWrappersWorkCorrectly()
    {
        var source = new Dictionary<string, int> { { "one", 1 } };

        ref var valueRef = ref source.GetValueRefOrNullRef("one");
        Assert.Equal(1, valueRef);

        ref var valueRef2 = ref source.GetValueRefOrAddDefault("two", out bool existed);
        Assert.False(existed);
        valueRef2 = 2;
        Assert.Equal(2, source["two"]);
    }
    #endregion
}