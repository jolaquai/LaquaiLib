using System.Collections.Concurrent;

using LaquaiLib.Collections;

namespace LaquaiLib.UnitTests.Collections;

public sealed class ConcurrentSetTests
{
    [Fact]
    public void ConstructorDefaultInitializesEmpty()
    {
        var set = new ConcurrentSet<int>();
        Assert.Empty(set);
        Assert.False(set.IsReadOnly);
    }

    [Fact]
    public void ConstructorWithComparerUsesComparer()
    {
        var comparer = StringComparer.OrdinalIgnoreCase;
        var set = new ConcurrentSet<string>(comparer)
        {
            "TEST"
        };
        Assert.Contains("test", set);
    }

    [Fact]
    public void AddReturnsTrueForNewItem()
    {
        var set = new ConcurrentSet<int>();
        Assert.True(set.Add(1));
        Assert.Single(set);
    }

    [Fact]
    public void AddReturnsFalseForExistingItem()
    {
        var set = new ConcurrentSet<int>
        {
            1
        };
        Assert.False(set.Add(1));
        Assert.Single(set);
    }

    [Fact]
    public void ContainsReturnsTrueForExistingItem()
    {
        var set = new ConcurrentSet<int>
        {
            42
        };
        Assert.Contains(42, set);
    }

    [Fact]
    public void ContainsReturnsFalseForNonExistingItem()
    {
        var set = new ConcurrentSet<int>();
        Assert.DoesNotContain(42, set);
    }

    [Fact]
    public void RemoveReturnsTrueForExistingItem()
    {
        var set = new ConcurrentSet<int>
        {
            1
        };
        Assert.True(set.Remove(1));
        Assert.Empty(set);
        Assert.DoesNotContain(1, set);
    }

    [Fact]
    public void RemoveReturnsFalseForNonExistingItem()
    {
        var set = new ConcurrentSet<int>();
        Assert.False(set.Remove(1));
    }

    [Fact]
    public void ClearRemovesAllItems()
    {
        var set = new ConcurrentSet<int>();
        for (int i = 0; i < 100; i++)
        {
            set.Add(i);
        }

        set.Clear();
        Assert.Empty(set);
        Assert.DoesNotContain(50, set);
    }

    [Fact]
    public void CopyToCopiesAllItems()
    {
        var set = new ConcurrentSet<int>();
        var values = new[] { 1, 2, 3, 4, 5 };
        foreach (var v in values)
        {
            set.Add(v);
        }

        var array = new int[10];
        set.CopyTo(array, 2);

        var copied = new HashSet<int>(array.Skip(2).Take(5));
        Assert.Equal([with(values)], copied);
    }

    [Fact]
    public void ExceptWithRemovesSpecifiedItems()
    {
        var set = new ConcurrentSet<int>();
        for (int i = 1; i <= 5; i++)
        {
            set.Add(i);
        }

        set.ExceptWith(new[] { 2, 4 });

        Assert.Equal(3, set.Count);
        Assert.Contains(1, set);
        Assert.DoesNotContain(2, set);
        Assert.Contains(3, set);
        Assert.DoesNotContain(4, set);
        Assert.Contains(5, set);
    }

    [Fact]
    public void IntersectWithKeepsOnlyCommonItems()
    {
        var set = new ConcurrentSet<int>();
        for (int i = 1; i <= 5; i++)
        {
            set.Add(i);
        }

        set.IntersectWith(new[] { 2, 3, 6 });

        Assert.Equal(2, set.Count);
        Assert.Contains(2, set);
        Assert.Contains(3, set);
    }

    [Fact]
    public void UnionWithAddsAllItems()
    {
        var set = new ConcurrentSet<int>
        {
            1,
            2
        };

        set.UnionWith(new[] { 2, 3, 4 });

        Assert.Equal(4, set.Count);
        for (int i = 1; i <= 4; i++)
        {
            Assert.Contains(i, set);
        }
    }

    [Fact]
    public void SymmetricExceptWithTogglesItems()
    {
        var set = new ConcurrentSet<int>
        {
            1,
            2
        };

        set.SymmetricExceptWith(new[] { 2, 3 });

        Assert.Equal(2, set.Count);
        Assert.Contains(1, set);
        Assert.DoesNotContain(2, set);
        Assert.Contains(3, set);
    }

    [Theory]
    [InlineData(new[] { 1, 2 }, new[] { 1, 2, 3 }, true)]
    [InlineData(new[] { 1, 2, 3 }, new[] { 1, 2 }, false)]
    [InlineData(new[] { 1, 2 }, new[] { 1, 2 }, true)]
    public void IsSubsetOfWorksCorrectly(int[] setItems, int[] otherItems, bool expected)
    {
        var set = new ConcurrentSet<int>();
        foreach (var item in setItems)
        {
            set.Add(item);
        }

        Assert.Equal(expected, set.IsSubsetOf(otherItems));
    }

    [Theory]
    [InlineData(new[] { 1, 2, 3 }, new[] { 1, 2 }, true)]
    [InlineData(new[] { 1, 2 }, new[] { 1, 2, 3 }, false)]
    [InlineData(new[] { 1, 2 }, new[] { 1, 2 }, true)]
    public void IsSupersetOfWorksCorrectly(int[] setItems, int[] otherItems, bool expected)
    {
        var set = new ConcurrentSet<int>();
        foreach (var item in setItems)
        {
            set.Add(item);
        }

        Assert.Equal(expected, set.IsSupersetOf(otherItems));
    }

    [Theory]
    [InlineData(new[] { 1, 2 }, new[] { 1, 2, 3 }, true)]
    [InlineData(new[] { 1, 2, 3 }, new[] { 1, 2 }, false)]
    [InlineData(new[] { 1, 2 }, new[] { 1, 2 }, false)]
    public void IsProperSubsetOfWorksCorrectly(int[] setItems, int[] otherItems, bool expected)
    {
        var set = new ConcurrentSet<int>();
        foreach (var item in setItems)
        {
            set.Add(item);
        }

        Assert.Equal(expected, set.IsProperSubsetOf(otherItems));
    }

    [Theory]
    [InlineData(new[] { 1, 2, 3 }, new[] { 1, 2 }, true)]
    [InlineData(new[] { 1, 2 }, new[] { 1, 2, 3 }, false)]
    [InlineData(new[] { 1, 2 }, new[] { 1, 2 }, false)]
    public void IsProperSupersetOfWorksCorrectly(int[] setItems, int[] otherItems, bool expected)
    {
        var set = new ConcurrentSet<int>();
        foreach (var item in setItems)
        {
            set.Add(item);
        }

        Assert.Equal(expected, set.IsProperSupersetOf(otherItems));
    }

    [Theory]
    [InlineData(new[] { 1, 2, 3 }, new[] { 3, 4, 5 }, true)]
    [InlineData(new[] { 1, 2, 3 }, new[] { 4, 5, 6 }, false)]
    public void OverlapsWorksCorrectly(int[] setItems, int[] otherItems, bool expected)
    {
        var set = new ConcurrentSet<int>();
        foreach (var item in setItems)
        {
            set.Add(item);
        }

        Assert.Equal(expected, set.Overlaps(otherItems));
    }

    [Theory]
    [InlineData(new[] { 1, 2, 3 }, new[] { 3, 2, 1 }, true)]
    [InlineData(new[] { 1, 2, 3 }, new[] { 1, 2, 3, 4 }, false)]
    [InlineData(new[] { 1, 2, 3 }, new[] { 1, 2 }, false)]
    public void SetEqualsWorksCorrectly(int[] setItems, int[] otherItems, bool expected)
    {
        var set = new ConcurrentSet<int>();
        foreach (var item in setItems)
        {
            set.Add(item);
        }

        Assert.Equal(expected, set.SetEquals(otherItems));
    }

    [Fact]
    public void EnumeratorReturnsAllItems()
    {
        var set = new ConcurrentSet<int>();
        var expected = new HashSet<int> { 1, 2, 3, 4, 5 };
        foreach (var item in expected)
        {
            set.Add(item);
        }

        var actual = new HashSet<int>();
        foreach (var item in set)
        {
            actual.Add(item);
        }

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void NonGenericEnumeratorWorks()
    {
        var set = new ConcurrentSet<int>
        {
            1,
            2
        };

        var count = 0;
        IEnumerable enumerable = set;
        foreach (var item in enumerable)
        {
            count++;
        }

        Assert.Equal(2, count);
    }

    [Fact]
    public void ICollectionAddWorks()
    {
        ICollection<int> set = new ConcurrentSet<int>
        {
            1
        };
        Assert.Single(set);
    }

    [Fact]
    public async Task ThreadSafetyAddFromMultipleThreads()
    {
        var set = new ConcurrentSet<int>();
        var tasks = new Task[10];

        for (int i = 0; i < tasks.Length; i++)
        {
            var start = i * 100;
            tasks[i] = Task.Run(() =>
            {
                for (int j = 0; j < 100; j++)
                {
                    set.Add(start + j);
                }
            }, TestContext.Current.CancellationToken);
        }

        await Task.WhenAll(tasks);

        Assert.Equal(1000, set.Count);
        for (int i = 0; i < 1000; i++)
        {
            Assert.Contains(i, set);
        }
    }

    [Fact]
    public void ThreadSafetyMixedOperations()
    {
        var set = new ConcurrentSet<int>();
        using var barrier = new Barrier(3);
        var errors = new ConcurrentBag<Exception>();

        for (int i = 0; i < 100; i++)
        {
            set.Add(i);
        }

        var t1 = Task.Run(() =>
        {
            try
            {
                barrier.SignalAndWait();
                for (int i = 0; i < 50; i++)
                {
                    set.Remove(i * 2);
                }
            }
            catch (Exception ex) { errors.Add(ex); }
        }, TestContext.Current.CancellationToken);

        var t2 = Task.Run(() =>
        {
            try
            {
                barrier.SignalAndWait();
                for (int i = 100; i < 200; i++)
                {
                    set.Add(i);
                }
            }
            catch (Exception ex) { errors.Add(ex); }
        }, TestContext.Current.CancellationToken);

        var t3 = Task.Run(() =>
        {
            try
            {
                barrier.SignalAndWait();
                for (int i = 0; i < 100; i++)
                {
                    set.Contains(i);
                }
            }
            catch (Exception ex) { errors.Add(ex); }
        }, TestContext.Current.CancellationToken);

        Task.WaitAll(t1, t2, t3);

        Assert.Empty(errors);
        Assert.Equal(150, set.Count);
    }

    [Fact]
    public void HandlesHashCollisions()
    {
        var set = new ConcurrentSet<HashCollider>();
        var items = Enumerable.Range(0, 100).Select(i => new HashCollider(i)).ToArray();

        foreach (var item in items)
        {
            Assert.True(set.Add(item));
        }

        Assert.Equal(100, set.Count);

        foreach (var item in items)
        {
            Assert.Contains(item, set);
        }
    }

    private sealed class HashCollider : IEquatable<HashCollider>
    {
        private readonly int value;

        public HashCollider(int value) => this.value = value;

        public override int GetHashCode() => value % 10;

        public bool Equals(HashCollider? other) => other != null && value == other.value;

        public override bool Equals(object? obj) => Equals(obj as HashCollider);
    }
}
