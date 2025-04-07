using LaquaiLib.Collections;

namespace LaquaiLib.UnitTests.Collections;

public class TwoWayLookupTests
{
    #region Constructor and Basic Operations

    [Fact]
    public void ConstructorCreatesEmptyLookup()
    {
        var lookup = new TwoWayLookup<int, string>();

        Assert.Empty(lookup);
    }

    #endregion

    #region Add Forward/Reverse Tests

    [Fact]
    public void AddForwardWithValidKeyValueAddsSuccessfully()
    {
        var lookup = new TwoWayLookup<int, string>();

        lookup.AddForward(1, "One");

        Assert.Single(lookup);
        Assert.Equal("One", lookup.GetForward(1));
        Assert.Equal(1, lookup.GetReverse("One"));
    }

    [Fact]
    public void AddReverseWithValidKeyValueAddsSuccessfully()
    {
        var lookup = new TwoWayLookup<int, string>();

        lookup.AddReverse("One", 1);

        Assert.Single(lookup);
        Assert.Equal("One", lookup.GetForward(1));
        Assert.Equal(1, lookup.GetReverse("One"));
    }

    [Fact]
    public void AddWithValidKeyValueAddsSuccessfully()
    {
        var lookup = new TwoWayLookup<int, string>
        {
            { 1, "One" }
        };

        Assert.Single(lookup);
        Assert.Equal("One", lookup.GetForward(1));
        Assert.Equal(1, lookup.GetReverse("One"));
    }

    [Fact]
    public void AddForwardWithDuplicateKeyThrowsException()
    {
        var lookup = new TwoWayLookup<int, string>();
        lookup.AddForward(1, "One");

        var ex = Assert.Throws<ArgumentException>(() => lookup.AddForward(1, "AnotherOne"));
        Assert.Contains("key", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AddForwardWithDuplicateValueThrowsException()
    {
        var lookup = new TwoWayLookup<int, string>();
        lookup.AddForward(1, "One");

        var ex = Assert.Throws<ArgumentException>(() => lookup.AddForward(2, "One"));
        Assert.Contains("One", ex.Message);
    }

    [Fact]
    public void AddReverseWithDuplicateKeyThrowsException()
    {
        var lookup = new TwoWayLookup<int, string>();
        lookup.AddReverse("One", 1);

        var ex = Assert.Throws<ArgumentException>(() => lookup.AddReverse("One", 2));
        Assert.Contains("key", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AddReverseWithDuplicateValueThrowsException()
    {
        var lookup = new TwoWayLookup<int, string>();
        lookup.AddReverse("One", 1);

        var ex = Assert.Throws<ArgumentException>(() => lookup.AddReverse("Two", 1));
        Assert.Contains("1", ex.Message);
    }

    #endregion

    #region TryAdd Forward/Reverse Tests

    [Fact]
    public void TryAddForwardWithValidKeyValueReturnsTrue()
    {
        var lookup = new TwoWayLookup<int, string>();

        var result = lookup.TryAddForward(1, "One");

        Assert.True(result);
        Assert.Equal("One", lookup.GetForward(1));
        Assert.Equal(1, lookup.GetReverse("One"));
    }

    [Fact]
    public void TryAddForwardWithDuplicateKeyReturnsFalse()
    {
        var lookup = new TwoWayLookup<int, string>();
        lookup.AddForward(1, "One");

        var result = lookup.TryAddForward(1, "AnotherOne");

        Assert.False(result);
        Assert.Equal("One", lookup.GetForward(1));
    }

    [Fact]
    public void TryAddForwardWithDuplicateValueReturnsFalse()
    {
        var lookup = new TwoWayLookup<int, string>();
        lookup.AddForward(1, "One");

        var result = lookup.TryAddForward(2, "One");

        Assert.False(result);
        Assert.Equal(1, lookup.GetReverse("One"));
    }

    [Fact]
    public void TryAddReverseWithValidKeyValueReturnsTrue()
    {
        var lookup = new TwoWayLookup<int, string>();

        var result = lookup.TryAddReverse("One", 1);

        Assert.True(result);
        Assert.Equal("One", lookup.GetForward(1));
        Assert.Equal(1, lookup.GetReverse("One"));
    }

    [Fact]
    public void TryAddReverseWithDuplicateKeyReturnsFalse()
    {
        var lookup = new TwoWayLookup<int, string>();
        lookup.AddReverse("One", 1);

        var result = lookup.TryAddReverse("One", 2);

        Assert.False(result);
        Assert.Equal(1, lookup.GetReverse("One"));
    }

    [Fact]
    public void TryAddReverseWithDuplicateValueReturnsFalse()
    {
        var lookup = new TwoWayLookup<int, string>();
        lookup.AddReverse("One", 1);

        var result = lookup.TryAddReverse("Two", 1);

        Assert.False(result);
        Assert.Equal("One", lookup.GetForward(1));
    }

    #endregion

    #region Generic TryAdd Tests

    [Fact]
    public void TryAddWithCorrectForwardTypesReturnsTrue()
    {
        var lookup = new TwoWayLookup<int, string>();

        var result = lookup.TryAdd(1, "One");

        Assert.True(result);
        Assert.Equal("One", lookup.GetForward(1));
        Assert.Equal(1, lookup.GetReverse("One"));
    }

    [Fact]
    public void TryAddWithCorrectReverseTypesReturnsTrue()
    {
        var lookup = new TwoWayLookup<int, string>();

        var result = lookup.TryAdd("One", 1);

        Assert.True(result);
        Assert.Equal("One", lookup.GetForward(1));
        Assert.Equal(1, lookup.GetReverse("One"));
    }

    [Fact]
    public void TryAddWithDuplicateKeyReturnsFalse()
    {
        var lookup = new TwoWayLookup<int, string>
        {
            { 1, "One" }
        };

        var result = lookup.TryAdd(1, "AnotherOne");

        Assert.False(result);
        Assert.Equal("One", lookup.GetForward(1));
    }

    #endregion

    #region Get Forward/Reverse Tests

    [Fact]
    public void GetForwardWithExistingKeyReturnsCorrectValue()
    {
        var lookup = new TwoWayLookup<int, string>();
        lookup.AddForward(1, "One");
        lookup.AddForward(2, "Two");

        var result = lookup.GetForward(2);

        Assert.Equal("Two", result);
    }

    [Fact]
    public void GetForwardWithNonExistentKeyThrowsKeyNotFoundException()
    {
        var lookup = new TwoWayLookup<int, string>();
        lookup.AddForward(1, "One");

        Assert.Throws<KeyNotFoundException>(() => lookup.GetForward(2));
    }

    [Fact]
    public void GetReverseWithExistingValueReturnsCorrectKey()
    {
        var lookup = new TwoWayLookup<int, string>();
        lookup.AddForward(1, "One");
        lookup.AddForward(2, "Two");

        var result = lookup.GetReverse("Two");

        Assert.Equal(2, result);
    }

    [Fact]
    public void GetReverseWithNonExistentValueThrowsKeyNotFoundException()
    {
        var lookup = new TwoWayLookup<int, string>();
        lookup.AddForward(1, "One");

        Assert.Throws<KeyNotFoundException>(() => lookup.GetReverse("Two"));
    }

    #endregion

    #region TryGet Forward/Reverse Tests

    [Fact]
    public void TryGetForwardWithExistingKeyReturnsTrue()
    {
        var lookup = new TwoWayLookup<int, string>();
        lookup.AddForward(1, "One");

        var result = lookup.TryGetForward(1, out var value);

        Assert.True(result);
        Assert.Equal("One", value);
    }

    [Fact]
    public void TryGetForwardWithNonExistentKeyReturnsFalse()
    {
        var lookup = new TwoWayLookup<int, string>();
        lookup.AddForward(1, "One");

        var result = lookup.TryGetForward(2, out var value);

        Assert.False(result);
        Assert.Equal(default, value);
    }

    [Fact]
    public void TryGetReverseWithExistingValueReturnsTrue()
    {
        var lookup = new TwoWayLookup<int, string>();
        lookup.AddForward(1, "One");

        var result = lookup.TryGetReverse("One", out var key);

        Assert.True(result);
        Assert.Equal(1, key);
    }

    [Fact]
    public void TryGetReverseWithNonExistentValueReturnsFalse()
    {
        var lookup = new TwoWayLookup<int, string>();
        lookup.AddForward(1, "One");

        var result = lookup.TryGetReverse("Two", out var key);

        Assert.False(result);
        Assert.Equal(default, key);
    }

    #endregion

    #region Remove Forward/Reverse Tests

    [Fact]
    public void RemoveForwardWithExistingKeyRemovesEntry()
    {
        var lookup = new TwoWayLookup<int, string>();
        lookup.AddForward(1, "One");
        lookup.AddForward(2, "Two");

        var result = lookup.RemoveForward(1);

        Assert.True(result);
        Assert.Single(lookup);
        Assert.Throws<KeyNotFoundException>(() => lookup.GetForward(1));
        Assert.Throws<KeyNotFoundException>(() => lookup.GetReverse("One"));
    }

    [Fact]
    public void RemoveForwardWithNonExistentKeyThrowsKeyNotFoundException()
    {
        var lookup = new TwoWayLookup<int, string>();
        lookup.AddForward(1, "One");

        Assert.Throws<KeyNotFoundException>(() => lookup.RemoveForward(2));
    }

    [Fact]
    public void RemoveReverseWithExistingValueRemovesEntry()
    {
        var lookup = new TwoWayLookup<int, string>();
        lookup.AddForward(1, "One");
        lookup.AddForward(2, "Two");

        var result = lookup.RemoveReverse("One");

        Assert.True(result);
        Assert.Single(lookup);
        Assert.Throws<KeyNotFoundException>(() => lookup.GetForward(1));
        Assert.Throws<KeyNotFoundException>(() => lookup.GetReverse("One"));
    }

    [Fact]
    public void RemoveReverseWithNonExistentValueThrowsKeyNotFoundException()
    {
        var lookup = new TwoWayLookup<int, string>();
        lookup.AddForward(1, "One");

        Assert.Throws<KeyNotFoundException>(() => lookup.RemoveReverse("Two"));
    }

    #endregion

    #region TryRemove Forward/Reverse Tests

    [Fact]
    public void TryRemoveForwardWithExistingKeyReturnsTrue()
    {
        var lookup = new TwoWayLookup<int, string>();
        lookup.AddForward(1, "One");
        lookup.AddForward(2, "Two");

        var result = lookup.TryRemoveForward(1);

        Assert.True(result);
        Assert.Single(lookup);
        Assert.False(lookup.TryGetForward(1, out _));
        Assert.False(lookup.TryGetReverse("One", out _));
    }

    [Fact]
    public void TryRemoveForwardWithNonExistentKeyReturnsFalse()
    {
        var lookup = new TwoWayLookup<int, string>();
        lookup.AddForward(1, "One");

        var result = lookup.TryRemoveForward(2);

        Assert.False(result);
        Assert.Single(lookup);
    }

    [Fact]
    public void TryRemoveReverseWithExistingValueReturnsTrue()
    {
        var lookup = new TwoWayLookup<int, string>();
        lookup.AddForward(1, "One");
        lookup.AddForward(2, "Two");

        var result = lookup.TryRemoveReverse("One");

        Assert.True(result);
        Assert.Single(lookup);
        Assert.False(lookup.TryGetForward(1, out _));
        Assert.False(lookup.TryGetReverse("One", out _));
    }

    [Fact]
    public void TryRemoveReverseWithNonExistentValueReturnsFalse()
    {
        var lookup = new TwoWayLookup<int, string>();
        lookup.AddForward(1, "One");

        var result = lookup.TryRemoveReverse("Two");

        Assert.False(result);
        Assert.Single(lookup);
    }

    #endregion

    #region Clear Tests

    [Fact]
    public void ClearRemovesAllEntries()
    {
        var lookup = new TwoWayLookup<int, string>();
        lookup.AddForward(1, "One");
        lookup.AddForward(2, "Two");
        lookup.AddForward(3, "Three");

        lookup.Clear();

        Assert.Empty(lookup);
    }

    [Fact]
    public void ClearOnEmptyLookupDoesNothing()
    {
        var lookup = new TwoWayLookup<int, string>();

        lookup.Clear();

        Assert.Empty(lookup);
    }

    #endregion

    #region Enumeration Tests

    [Fact]
    public void GetEnumeratorReturnsAllEntries()
    {
        var lookup = new TwoWayLookup<int, string>();
        lookup.AddForward(1, "One");
        lookup.AddForward(2, "Two");
        lookup.AddForward(3, "Three");

        var entries = lookup.ToList();

        Assert.Equal(3, entries.Count);
        Assert.Contains(entries, static e => e.Key == 1 && e.Value == "One");
        Assert.Contains(entries, static e => e.Key == 2 && e.Value == "Two");
        Assert.Contains(entries, static e => e.Key == 3 && e.Value == "Three");
    }

    [Fact]
    public void GetReverseEnumeratorReturnsAllEntriesReversed()
    {
        var lookup = new TwoWayLookup<int, string>();
        lookup.AddForward(1, "One");
        lookup.AddForward(2, "Two");
        lookup.AddForward(3, "Three");

        var entries = new List<KeyValuePair<string, int>>();
        var enumerator = lookup.GetReverseEnumerator();
        while (enumerator.MoveNext())
        {
            entries.Add(enumerator.Current);
        }

        Assert.Equal(3, entries.Count);
        Assert.Contains(entries, static e => e.Key == "One" && e.Value == 1);
        Assert.Contains(entries, static e => e.Key == "Two" && e.Value == 2);
        Assert.Contains(entries, static e => e.Key == "Three" && e.Value == 3);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void MultipleOperationsMaintainsConsistency()
    {
        var lookup = new TwoWayLookup<int, string>();

        lookup.AddForward(1, "One");
        lookup.AddReverse("Two", 2);
        lookup.Add(3, "Three");
        Assert.Equal(3, lookup.Count);

        Assert.Equal("One", lookup.GetForward(1));
        Assert.Equal(2, lookup.GetReverse("Two"));
        Assert.True(lookup.TryGetForward(3, out var value));
        Assert.Equal("Three", value);

        lookup.RemoveForward(1);
        Assert.Equal(2, lookup.Count);
        Assert.False(lookup.TryGetForward(1, out _));
        Assert.False(lookup.TryGetReverse("One", out _));

        lookup.TryAddForward(4, "Four");
        Assert.Equal(3, lookup.Count);

        lookup.Clear();
        Assert.Empty(lookup);
        Assert.False(lookup.TryGetForward(2, out _));
    }

    [Fact]
    public void ComplexTypeTest()
    {
        var lookup = new TwoWayLookup<Person, Address>();
        var person1 = new Person { Id = 1, Name = "John" };
        var person2 = new Person { Id = 2, Name = "Jane" };
        var address1 = new Address { Id = 1, Street = "123 Main St" };
        var address2 = new Address { Id = 2, Street = "456 Oak Ave" };

        lookup.AddForward(person1, address1);
        lookup.AddForward(person2, address2);

        Assert.Equal(2, lookup.Count);
        Assert.Equal(address1, lookup.GetForward(person1));
        Assert.Equal(person2, lookup.GetReverse(address2));

        lookup.RemoveForward(person1);

        Assert.Single(lookup);
        Assert.Equal(address2, lookup.GetForward(person2));
    }

    #endregion

    #region Helper Classes

    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is Person other)
            {
                return Id == other.Id && Name == other.Name;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name);
        }
    }

    public class Address
    {
        public int Id { get; set; }
        public string Street { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is Address other)
            {
                return Id == other.Id && Street == other.Street;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Street);
        }
    }

    #endregion
}
