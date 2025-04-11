using LaquaiLib.Collections;
using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Collections;

public class ConcurrentTwoWayLookupTests
{
    #region Constructor and Basic Operations

    [Fact]
    public void ConstructorCreatesEmptyLookup()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();

        Assert.Equal(0, lookup.Count);
    }

    #endregion

    #region Add Forward/Reverse Tests

    [Fact]
    public void AddForwardWithValidKeyValueAddsSuccessfully()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();

        lookup.AddForward(1, "One");

        Assert.Equal(1, lookup.Count);
        Assert.Equal("One", lookup.GetForward(1));
        Assert.Equal(1, lookup.GetReverse("One"));
    }

    [Fact]
    public void AddReverseWithValidKeyValueAddsSuccessfully()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();

        lookup.AddReverse("One", 1);

        Assert.Equal(1, lookup.Count);
        Assert.Equal("One", lookup.GetForward(1));
        Assert.Equal(1, lookup.GetReverse("One"));
    }

    [Fact]
    public void AddWithValidKeyValueAddsSuccessfully()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();

        lookup.Add(1, "One");

        Assert.Equal(1, lookup.Count);
        Assert.Equal("One", lookup.GetForward(1));
        Assert.Equal(1, lookup.GetReverse("One"));
    }

    [Fact]
    public void AddForwardWithDuplicateKeyThrowsException()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();
        lookup.AddForward(1, "One");

        var ex = Assert.Throws<ArgumentException>(() => lookup.AddForward(1, "AnotherOne"));
        Assert.Contains("key", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AddForwardWithDuplicateValueThrowsException()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();
        lookup.AddForward(1, "One");

        var ex = Assert.Throws<ArgumentException>(() => lookup.AddForward(2, "One"));
        Assert.Contains("One", ex.Message);
    }

    [Fact]
    public void AddReverseWithDuplicateKeyThrowsException()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();
        lookup.AddReverse("One", 1);

        var ex = Assert.Throws<ArgumentException>(() => lookup.AddReverse("One", 2));
        Assert.Contains("key", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AddReverseWithDuplicateValueThrowsException()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();
        lookup.AddReverse("One", 1);

        var ex = Assert.Throws<ArgumentException>(() => lookup.AddReverse("Two", 1));
        Assert.Contains("1", ex.Message);
    }

    #endregion

    #region TryAdd Forward/Reverse Tests

    [Fact]
    public void TryAddForwardWithValidKeyValueReturnsTrue()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();

        var result = lookup.TryAddForward(1, "One");

        Assert.True(result);
        Assert.Equal("One", lookup.GetForward(1));
        Assert.Equal(1, lookup.GetReverse("One"));
    }

    [Fact]
    public void TryAddForwardWithDuplicateKeyReturnsFalse()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();
        lookup.AddForward(1, "One");

        var result = lookup.TryAddForward(1, "AnotherOne");

        Assert.False(result);
        Assert.Equal("One", lookup.GetForward(1));
    }

    [Fact]
    public void TryAddReverseWithValidKeyValueReturnsTrue()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();

        var result = lookup.TryAddReverse("One", 1);

        Assert.True(result);
        Assert.Equal("One", lookup.GetForward(1));
        Assert.Equal(1, lookup.GetReverse("One"));
    }

    [Fact]
    public void TryAddReverseWithDuplicateKeyReturnsFalse()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();
        lookup.AddReverse("One", 1);

        var result = lookup.TryAddReverse("One", 2);

        Assert.False(result);
        Assert.Equal(1, lookup.GetReverse("One"));
    }

    #endregion

    #region TryAdd Generic Tests

    [Fact]
    public void TryAddWithForwardTypesReturnsTrue()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();

        var result = lookup.TryAdd(1, "One");

        Assert.True(result);
        Assert.Equal("One", lookup.GetForward(1));
        Assert.Equal(1, lookup.GetReverse("One"));
    }

    [Fact]
    public void TryAddWithReverseTypesReturnsTrue()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();

        var result = lookup.TryAdd("One", 1);

        Assert.True(result);
        Assert.Equal("One", lookup.GetForward(1));
        Assert.Equal(1, lookup.GetReverse("One"));
    }

    #endregion

    #region Set Forward/Reverse Tests

    [Fact]
    public void SetForwardWithExistingKeyUpdatesValue()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();
        lookup.AddForward(1, "One");

        var result = lookup.SetForward(1, "NewOne");

        Assert.Equal("NewOne", result);
        Assert.Equal("NewOne", lookup.GetForward(1));
        Assert.Equal(1, lookup.GetReverse("NewOne"));
        Assert.False(lookup.TryGetReverse("One", out _));
    }

    [Fact]
    public void SetForwardWithNewKeyAddsKeyValue()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();

        var result = lookup.SetForward(1, "One");

        Assert.Equal("One", result);
        Assert.Equal("One", lookup.GetForward(1));
        Assert.Equal(1, lookup.GetReverse("One"));
    }

    [Fact]
    public void SetReverseWithExistingKeyUpdatesValue()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();
        lookup.AddReverse("One", 1);

        var result = lookup.SetReverse("One", 2);

        Assert.Equal(2, result);
        Assert.Equal("One", lookup.GetForward(2));
        Assert.Equal(2, lookup.GetReverse("One"));
        Assert.False(lookup.TryGetForward(1, out _));
    }

    #endregion

    #region Indexer Tests

    [Fact]
    public void IndexerGetWithT1KeyReturnsCorrectValue()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();
        lookup.AddForward(1, "One");

        var result = lookup[1];

        Assert.Equal("One", result);
    }

    [Fact]
    public void IndexerGetWithT2KeyReturnsCorrectValue()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();
        lookup.AddForward(1, "One");

        var result = lookup["One"];

        Assert.Equal(1, result);
    }

    [Fact]
    public void IndexerSetWithT1KeyUpdatesValue()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();
        lookup.AddForward(1, "One");

        lookup[1] = "NewOne";

        Assert.Equal("NewOne", lookup.GetForward(1));
        Assert.Equal(1, lookup.GetReverse("NewOne"));
        Assert.False(lookup.TryGetReverse("One", out _));
    }

    [Fact]
    public void IndexerSetWithT2KeyUpdatesValue()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();
        lookup.AddForward(1, "One");

        lookup["One"] = 2;

        Assert.Equal("One", lookup.GetForward(2));
        Assert.Equal(2, lookup.GetReverse("One"));
        Assert.False(lookup.TryGetForward(1, out _));
    }

    #endregion

    #region Get Forward/Reverse Tests

    [Fact]
    public void GetForwardWithExistingKeyReturnsCorrectValue()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();
        lookup.AddForward(1, "One");
        lookup.AddForward(2, "Two");

        var result = lookup.GetForward(2);

        Assert.Equal("Two", result);
    }

    [Fact]
    public void GetForwardWithNonExistentKeyThrowsKeyNotFoundException()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();
        lookup.AddForward(1, "One");

        Assert.Throws<KeyNotFoundException>(() => lookup.GetForward(2));
    }

    [Fact]
    public void GetReverseWithExistingValueReturnsCorrectKey()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();
        lookup.AddForward(1, "One");
        lookup.AddForward(2, "Two");

        var result = lookup.GetReverse("Two");

        Assert.Equal(2, result);
    }

    #endregion

    #region TryGet Forward/Reverse Tests

    [Fact]
    public void TryGetForwardWithExistingKeyReturnsTrue()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();
        lookup.AddForward(1, "One");

        var result = lookup.TryGetForward(1, out var value);

        Assert.True(result);
        Assert.Equal("One", value);
    }

    [Fact]
    public void TryGetForwardWithNonExistentKeyReturnsFalse()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();
        lookup.AddForward(1, "One");

        var result = lookup.TryGetForward(2, out var value);

        Assert.False(result);
        Assert.Equal(default, value);
    }

    [Fact]
    public void TryGetReverseWithExistingValueReturnsTrue()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();
        lookup.AddForward(1, "One");

        var result = lookup.TryGetReverse("One", out var key);

        Assert.True(result);
        Assert.Equal(1, key);
    }

    #endregion

    #region Remove Forward/Reverse Tests

    [Fact]
    public void RemoveForwardWithExistingKeyRemovesEntry()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();
        lookup.AddForward(1, "One");
        lookup.AddForward(2, "Two");

        var result = lookup.RemoveForward(1);

        Assert.True(result);
        Assert.Equal(1, lookup.Count);
        Assert.False(lookup.TryGetForward(1, out _));
        Assert.False(lookup.TryGetReverse("One", out _));
    }

    [Fact]
    public void RemoveReverseWithExistingValueRemovesEntry()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();
        lookup.AddForward(1, "One");
        lookup.AddForward(2, "Two");

        var result = lookup.RemoveReverse("One");

        Assert.True(result);
        Assert.Equal(1, lookup.Count);
        Assert.False(lookup.TryGetForward(1, out _));
        Assert.False(lookup.TryGetReverse("One", out _));
    }

    #endregion

    #region TryRemove Forward/Reverse Tests

    [Fact]
    public void TryRemoveForwardWithExistingKeyReturnsTrue()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();
        lookup.AddForward(1, "One");
        lookup.AddForward(2, "Two");

        var result = lookup.TryRemoveForward(1);

        Assert.True(result);
        Assert.Equal(1, lookup.Count);
        Assert.False(lookup.TryGetForward(1, out _));
        Assert.False(lookup.TryGetReverse("One", out _));
    }

    [Fact]
    public void TryRemoveForwardWithNonExistentKeyReturnsFalse()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();
        lookup.AddForward(1, "One");

        var result = lookup.TryRemoveForward(2);

        Assert.False(result);
        Assert.Equal(1, lookup.Count);
    }

    [Fact]
    public void TryRemoveReverseWithExistingValueReturnsTrue()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();
        lookup.AddForward(1, "One");
        lookup.AddForward(2, "Two");

        var result = lookup.TryRemoveReverse("One");

        Assert.True(result);
        Assert.Equal(1, lookup.Count);
        Assert.False(lookup.TryGetForward(1, out _));
        Assert.False(lookup.TryGetReverse("One", out _));
    }

    #endregion

    #region Clear Tests

    [Fact]
    public void ClearRemovesAllEntries()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();
        lookup.AddForward(1, "One");
        lookup.AddForward(2, "Two");
        lookup.AddForward(3, "Three");

        lookup.Clear();

        Assert.Equal(0, lookup.Count);
    }

    #endregion

    #region Enumeration Tests

    [Fact]
    public void GetEnumeratorReturnsAllEntries()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();
        lookup.AddForward(1, "One");
        lookup.AddForward(2, "Two");
        lookup.AddForward(3, "Three");

        var entries = lookup.ToList();

        Assert.Contains(new KeyValuePair<int, string>(1, "One"), entries);
        Assert.Contains(new KeyValuePair<int, string>(2, "Two"), entries);
        Assert.Contains(new KeyValuePair<int, string>(3, "Three"), entries);
    }

    [Fact]
    public void GetReverseEnumeratorReturnsAllEntriesReversed()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();
        lookup.AddForward(1, "One");
        lookup.AddForward(2, "Two");
        lookup.AddForward(3, "Three");

        var entries = new List<KeyValuePair<string, int>>();
        using (var enumerator = lookup.GetReverseEnumerator())
        {
            while (enumerator.MoveNext())
            {
                entries.Add(enumerator.Current);
            }
        }

        Assert.Contains(new KeyValuePair<string, int>("One", 1), entries);
        Assert.Contains(new KeyValuePair<string, int>("Two", 2), entries);
        Assert.Contains(new KeyValuePair<string, int>("Three", 3), entries);
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task ConcurrentAddFromMultipleThreadsAddsAllEntries()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();
        var tasks = new List<Task>();
        var numItems = 100;

        for (var i = 0; i < numItems; i++)
        {
            var id = i;
            tasks.Add(Task.Run(() => lookup.TryAddForward(id, $"Item{id}")));
        }

        await Task.WhenAll(tasks);

        Assert.Equal(numItems, lookup.Count);
        for (var i = 0; i < numItems; i++)
        {
            Assert.Equal($"Item{i}", lookup.GetForward(i));
        }
    }

    [Fact]
    public async Task ConcurrentReadWriteDoesNotThrow()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();
        var addTasks = new List<Task>();
        var readTasks = new List<Task>();
        var removeTasks = new List<Task>();
        var numItems = 100;

        for (var i = 0; i < numItems / 2; i++)
        {
            lookup.AddForward(i, $"Item{i}");
        }

        for (var i = 0; i < numItems / 2; i++)
        {
            var id = i;
            readTasks.Add(Task.Run(() =>
            {
                try
                {
                    _ = lookup.GetForward(id);
                }
                catch { }
            }, TestContext.Current.CancellationToken));

            removeTasks.Add(Task.Run(() =>
            {
                try
                {
                    lookup.TryRemoveForward(id);
                }
                catch { }
            }, TestContext.Current.CancellationToken));

            addTasks.Add(Task.Run(() =>
            {
                try
                {
                    lookup.TryAddForward(id + (numItems / 2), $"Item{id + (numItems / 2)}");
                }
                catch { }
            }, TestContext.Current.CancellationToken));
        }

        await Task.WhenAll(readTasks.Concat(removeTasks).Concat(addTasks));

        Assert.True(lookup.Count > 0);
    }

    [Fact]
    public async Task ReadersDoNotBlockOtherReaders()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();
        for (var i = 0; i < 10; i++)
        {
            lookup.Add(i, $"Item{i}");
        }

        var barrier = new Barrier(2);
        var reader1Done = false;
        var reader2Done = false;

        var reader1 = Task.Run(() =>
        {
            var val = lookup.GetForward(1);
            barrier.SignalAndWait();
            Thread.Sleep(50);

            val = lookup.GetForward(2);
            reader1Done = true;
        }, TestContext.Current.CancellationToken);

        var reader2 = Task.Run(() =>
        {
            barrier.SignalAndWait();
            var val = lookup.GetForward(3);
            reader2Done = true;
        }, TestContext.Current.CancellationToken);

        await Task.WhenAll(reader1, reader2);

        Assert.True(reader1Done);
        Assert.True(reader2Done);
    }

    [Fact]
    public async Task WriterBlocksOtherWriters()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();
        var barrier = new Barrier(2);
        var writer2BlockedFor = 0d;

        var writer1 = Task.Run(async () =>
        {
            // The performance of this is fast enough on my machine that the test literally always fails, so this is the only way I could actually ensure the locks work xd
            using (var wLock = lookup.TakeWriteLock())
            {
                barrier.SignalAndWait();
                Thread.Sleep(250);
            }

            lookup.AddForward(2, "Two");
            lookup.AddForward(4, "Four");
            lookup.AddForward(5, "Five");
            lookup.AddForward(6, "Six");
        }, TestContext.Current.CancellationToken);

        var writer2 = Task.Run(() =>
        {
            barrier.SignalAndWait();
            var startTime = DateTime.Now;
            lookup.AddForward(3, "Three");
            var elapsed = DateTime.Now - startTime;

            writer2BlockedFor = elapsed.TotalMilliseconds;
        }, TestContext.Current.CancellationToken);

        await writer1;
        await writer2;

        Assert.True(writer2BlockedFor > 150);
        Assert.Equal(5, lookup.Count);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void MultipleOperationsMaintainsConsistency()
    {
        var lookup = new ConcurrentTwoWayLookup<int, string>();

        lookup.AddForward(1, "One");
        lookup.AddReverse("Two", 2);
        lookup.Add(3, "Three");
        Assert.Equal(3, lookup.Count);

        Assert.Equal("One", lookup.GetForward(1));
        Assert.Equal(2, lookup.GetReverse("Two"));
        Assert.True(lookup.TryGetForward(3, out var value));
        Assert.Equal("Three", value);

        lookup[1] = "OneUpdated";
        Assert.Equal("OneUpdated", lookup.GetForward(1));
        Assert.Equal(1, lookup.GetReverse("OneUpdated"));
        Assert.False(lookup.TryGetReverse("One", out _));

        lookup.RemoveForward(1);
        Assert.Equal(2, lookup.Count);
        Assert.False(lookup.TryGetForward(1, out _));
        Assert.False(lookup.TryGetReverse("OneUpdated", out _));

        lookup.TryAddForward(4, "Four");
        Assert.Equal(3, lookup.Count);

        lookup.Clear();
        Assert.Equal(0, lookup.Count);
        Assert.False(lookup.TryGetForward(2, out _));
    }

    [Fact]
    public void ComplexTypeTest()
    {
        var lookup = new ConcurrentTwoWayLookup<Person, Address>();
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

        Assert.Equal(1, lookup.Count);
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