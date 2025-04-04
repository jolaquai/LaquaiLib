using LaquaiLib.Collections;

namespace LaquaiLib.UnitTests.Collections;

public class MultiKeyDictionaryTests
{
    [Fact]
    public void ConstructorDefaultInitializesWithNoStorage()
    {
        var dictionary = new MultiKeyDictionary<string>();

        Assert.False(dictionary.TryGetValue(["key"], out _));
    }

    [Fact]
    public void ConstructorWithCapacityAndKeyCountAllocatesStorage()
    {
        var dictionary = new MultiKeyDictionary<string>(10, 1);

        dictionary.Add(["key"], "value");

        Assert.True(dictionary.TryGetValue(["key"], out var value));
        Assert.Equal("value", value);
    }

    [Fact]
    public void AddSingleKeyAddsValue()
    {
        var dictionary = new MultiKeyDictionary<string>();

        dictionary.Add(["key"], "value");

        Assert.Equal("value", dictionary.GetValue(["key"]));
    }

    [Fact]
    public void AddExistingKeyThrowsInvalidOperationException()
    {
        var dictionary = new MultiKeyDictionary<string>();
        dictionary.Add(["key"], "value");

        Assert.Throws<InvalidOperationException>(() => dictionary.Add(["key"], "value2"));
    }

    [Fact]
    public void TryAddSingleKeyReturnsTrue()
    {
        var dictionary = new MultiKeyDictionary<string>();

        var added = dictionary.TryAdd(["key"], "value");

        Assert.True(added);
        Assert.Equal("value", dictionary.GetValue(["key"]));
    }

    [Fact]
    public void TryAddExistingKeyReturnsFalse()
    {
        var dictionary = new MultiKeyDictionary<string>();
        dictionary.Add(["key"], "value");

        var added = dictionary.TryAdd(["key"], "value2");

        Assert.False(added);
        Assert.Equal("value", dictionary.GetValue(["key"]));
    }

    [Fact]
    public void GetValueNonExistentKeyThrowsKeyNotFoundException()
    {
        var dictionary = new MultiKeyDictionary<string>();

        Assert.Throws<KeyNotFoundException>(() => dictionary.GetValue(["nonexistent"]));
    }

    [Fact]
    public void TryGetValueExistingKeyReturnsTrueAndValue()
    {
        var dictionary = new MultiKeyDictionary<string>();
        dictionary.Add(["key"], "value");

        var exists = dictionary.TryGetValue(["key"], out var value);

        Assert.True(exists);
        Assert.Equal("value", value);
    }

    [Fact]
    public void TryGetValueNonExistentKeyReturnsFalseAndDefault()
    {
        var dictionary = new MultiKeyDictionary<string>();

        var exists = dictionary.TryGetValue(["key"], out var value);

        Assert.False(exists);
        Assert.Null(value);
    }

    [Fact]
    public void SetValueExistingKeyUpdatesValue()
    {
        var dictionary = new MultiKeyDictionary<string>();
        dictionary.Add(["key"], "value");

        dictionary.SetValue(["key"], "newValue");

        Assert.Equal("newValue", dictionary.GetValue(["key"]));
    }

    [Fact]
    public void SetValueNonExistentKeyAddsKeyValue()
    {
        var dictionary = new MultiKeyDictionary<string>();

        dictionary.SetValue(["key"], "value");

        Assert.Equal("value", dictionary.GetValue(["key"]));
    }

    [Fact]
    public void AddMultipleKeysAddsValue()
    {
        var dictionary = new MultiKeyDictionary<string>();
        object[] keys = ["key1", "key2", "key3"];

        dictionary.Add(keys, "value");

        Assert.Equal("value", dictionary.GetValue(keys));
    }

    [Fact]
    public void DifferentKeyOrdersStoresDifferentValues()
    {
        var dictionary = new MultiKeyDictionary<string>();

        dictionary.Add(["a", "b"], "ab");
        dictionary.Add(["b", "a"], "ba");

        Assert.Equal("ab", dictionary.GetValue(["a", "b"]));
        Assert.Equal("ba", dictionary.GetValue(["b", "a"]));
    }

    [Fact]
    public void SingleKeyAndMultipleKeysStoreDifferentValues()
    {
        var dictionary = new MultiKeyDictionary<string>();

        dictionary.Add(["key"], "single");
        dictionary.Add(["key", "another"], "multiple");

        Assert.Equal("single", dictionary.GetValue(["key"]));
        Assert.Equal("multiple", dictionary.GetValue(["key", "another"]));
    }

    [Fact]
    public void ManyKeysWorks()
    {
        var dictionary = new MultiKeyDictionary<string>();
        object[] manyKeys = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

        dictionary.Add(manyKeys, "many-keys-value");

        Assert.Equal("many-keys-value", dictionary.GetValue(manyKeys));
    }

    [Fact]
    public void ClearRemovesAllValues()
    {
        var dictionary = new MultiKeyDictionary<string>();
        dictionary.Add(["key1"], "value1");
        dictionary.Add(["key2", "key3"], "value2");

        dictionary.Clear();

        Assert.False(dictionary.TryGetValue(["key1"], out _));
        Assert.False(dictionary.TryGetValue(["key2", "key3"], out _));
    }

    [Fact]
    public void ClearWithKeyCountRemovesOnlyMatchingValues()
    {
        var dictionary = new MultiKeyDictionary<string>();
        dictionary.Add(["key1"], "value1");
        dictionary.Add(["key2", "key3"], "value2");

        dictionary.Clear(1);
        Assert.False(dictionary.TryGetValue(["key1"], out _));
        Assert.True(dictionary.TryGetValue(["key2", "key3"], out _));
    }

    [Fact]
    public void GetOrAddExistingKeyReturnsExistingValue()
    {
        var dictionary = new MultiKeyDictionary<string>();
        object[] keys = ["key"];
        dictionary.Add(keys, "value");

        var result = dictionary.GetOrAdd(keys, "new-value");

        Assert.Equal("value", result);
    }

    [Fact]
    public void GetOrAddNonExistentKeyAddsAndReturnsNewValue()
    {
        var dictionary = new MultiKeyDictionary<string>();
        object[] keys = ["key"];

        var result = dictionary.GetOrAdd(keys, "value");

        Assert.Equal("value", result);
        Assert.Equal("value", dictionary.GetValue(keys));
    }

    [Fact]
    public void GetOrAddWithFactoryNonExistentKeyInvokesFactoryAndAddsResult()
    {
        var dictionary = new MultiKeyDictionary<string>();
        object[] keys = ["key"];
        var factoryInvoked = false;

        var result = dictionary.GetOrAdd(keys, () =>
        {
            factoryInvoked = true;
            return "factory-value";
        });

        Assert.True(factoryInvoked);
        Assert.Equal("factory-value", result);
        Assert.Equal("factory-value", dictionary.GetValue(keys));
    }

    [Fact]
    public void GetOrAddWithOutParameterExistingKeyReturnsTrueAndExistingValue()
    {
        var dictionary = new MultiKeyDictionary<string>();
        object[] keys = ["key"];
        dictionary.Add(keys, "value");

        var existed = dictionary.GetOrAdd(keys, "new-value", out var result);

        Assert.True(existed);
        Assert.Equal("value", result);
    }

    [Fact]
    public void GetOrAddWithOutParameterNonExistentKeyReturnsFalseAndNewValue()
    {
        var dictionary = new MultiKeyDictionary<string>();
        object[] keys = ["key"];

        var existed = dictionary.GetOrAdd(keys, "value", out var result);

        Assert.False(existed);
        Assert.Equal("value", result);
    }

    [Fact]
    public void AddOrUpdateExistingKeyUpdatesValue()
    {
        var dictionary = new MultiKeyDictionary<string>();
        object[] keys = ["key"];
        dictionary.Add(keys, "value");

        dictionary.AddOrUpdate(keys, "new-value", existingValue => existingValue + "-updated");

        Assert.Equal("value-updated", dictionary.GetValue(keys));
    }

    [Fact]
    public void AddOrUpdateNonExistentKeyAddsValue()
    {
        var dictionary = new MultiKeyDictionary<string>();
        object[] keys = ["key"];

        dictionary.AddOrUpdate(keys, "new-value", existingValue => existingValue + "-updated");

        Assert.Equal("new-value", dictionary.GetValue(keys));
    }

    [Fact]
    public void AddOrUpdateWithBothValuesExistingKeyUpdatesWithBothValues()
    {
        var dictionary = new MultiKeyDictionary<string>();
        object[] keys = ["key"];
        dictionary.Add(keys, "old-value");

        dictionary.AddOrUpdate(keys, "add-value", (existing, add) => $"{existing}+{add}");

        Assert.Equal("old-value+add-value", dictionary.GetValue(keys));
    }

    [Fact]
    public void AddOrUpdateWithFactoryNonExistentKeyInvokesAddFactory()
    {
        var dictionary = new MultiKeyDictionary<string>();
        object[] keys = ["key"];
        var addFactoryInvoked = false;
        var updateFactoryInvoked = false;

        dictionary.AddOrUpdate(
            keys,
            () =>
            {
                addFactoryInvoked = true;
                return "add-factory-value";
            },
            existing =>
            {
                updateFactoryInvoked = true;
                return existing + "-updated";
            }
        );

        Assert.True(addFactoryInvoked);
        Assert.False(updateFactoryInvoked);
        Assert.Equal("add-factory-value", dictionary.GetValue(keys));
    }

    [Fact]
    public void GetValueRefOrNullRefExistingKeyReturnsRefAndTrue()
    {
        var dictionary = new MultiKeyDictionary<string>();
        object[] keys = ["key"];
        dictionary.Add(keys, "value");

        ref var refValue = ref dictionary.GetValueRefOrNullRef(keys, out var existed);

        Assert.True(existed);
        Assert.Equal("value", refValue);

        refValue = "changed-via-ref";
        Assert.Equal("changed-via-ref", dictionary.GetValue(keys));
    }

    [Fact]
    public void GetValueRefOrAddDefaultNonExistentKeyAddsDefault()
    {
        var dictionary = new MultiKeyDictionary<int>();
        object[] keys = ["key"];

        ref var refValue = ref dictionary.GetValueRefOrAddDefault(keys, out var existed);

        Assert.False(existed);
        Assert.Equal(0, refValue);
        refValue = 42;
        Assert.Equal(42, dictionary.GetValue(keys));
    }

    [Fact]
    public void GetValueRefOrAddNonExistentKeyAddsSpecifiedValue()
    {
        var dictionary = new MultiKeyDictionary<string>();
        object[] keys = ["key"];

        ref var refValue = ref dictionary.GetValueRefOrAdd(keys, "initial", out var existed);

        Assert.False(existed);
        Assert.Equal("initial", refValue);

        refValue = "changed-via-ref";
        Assert.Equal("changed-via-ref", dictionary.GetValue(keys));
    }

    [Fact]
    public void GetValueRefOrAddWithFactoryExistingKeyDoesNotInvokeFactory()
    {
        var dictionary = new MultiKeyDictionary<string>();
        object[] keys = ["key"];
        dictionary.Add(keys, "value");
        var factoryInvoked = false;

        ref var refValue = ref dictionary.GetValueRefOrAdd(keys, () =>
        {
            factoryInvoked = true;
            return "factory-value";
        }, out var existed);

        Assert.True(existed);
        Assert.False(factoryInvoked);
        Assert.Equal("value", refValue);
    }

    [Fact]
    public void KeysReturnsAllKeys()
    {
        var dictionary = new MultiKeyDictionary<string>();
        dictionary.Add(["key1"], "value1");
        dictionary.Add(["key2", "key3"], "value2");

        var keys = dictionary.Keys;

        Assert.Equal(2, keys.Length);
        Assert.Contains("key1", keys);
        Assert.Contains(keys, k => k is ValueTuple<object, object> tuple && tuple.Item1.Equals("key2") && tuple.Item2.Equals("key3"));
    }

    [Fact]
    public void ValuesReturnsAllValues()
    {
        var dictionary = new MultiKeyDictionary<string>();
        dictionary.Add(["key1"], "value1");
        dictionary.Add(["key2", "key3"], "value2");

        var values = dictionary.Values.ToArray();

        Assert.Equal(2, values.Length);
        Assert.Contains("value1", values);
        Assert.Contains("value2", values);
    }

    [Fact]
    public void StructKeysGetBoxedAndWorkCorrectly()
    {
        var dictionary = new MultiKeyDictionary<string>();

        dictionary.Add([1, 2], "int-keys");
        dictionary.Add([DateTime.Now.Date], "date-key");

        Assert.Equal("int-keys", dictionary.GetValue([1, 2]));
        Assert.Equal("date-key", dictionary.GetValue([DateTime.Now.Date]));
    }

    [Fact]
    public void EmptyKeyArrayThrowsArgumentOutOfRangeException()
    {
        var dictionary = new MultiKeyDictionary<string>();

        Assert.Throws<ArgumentOutOfRangeException>(() => dictionary.GetValue([]));
    }

    [Fact]
    public void KeySumReturnsCorrectCount()
    {
        var dictionary = new MultiKeyDictionary<string>();
        dictionary.Add(["key1"], "value1");
        dictionary.Add(["key2", "key3"], "value2");
        dictionary.Add([1, 2, 3, 4, 5, 6, 7, 8, 9], "many-keys");

        Assert.Equal(3, dictionary.Keys.Length);
    }

    [Fact]
    public void ReadOnlySpanOverloadWorks()
    {
        var dictionary = new MultiKeyDictionary<string>();
        object[] keysArray = ["key1", "key2"];

        dictionary.Add(keysArray.AsSpan(), "span-value");

        Assert.Equal("span-value", dictionary.GetValue(keysArray.AsSpan()));
    }
}
