using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Extensions;

public class IEnumerableExtensionsOrderTests
{
    private class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }

        public override string ToString() => $"{Name} ({Age})";
    }

    private class ReverseComparer<T> : IComparer<T> where T : IComparable<T>
    {
        public int Compare(T x, T y) => y.CompareTo(x);
    }

    [Fact]
    public void OrderByWithIndexCallsKeySelectorWithCorrectIndices()
    {
        var source = new[] { 5, 3, 1, 2, 4 };
        var indices = new List<int>();

        var result = source.OrderBy((x, i) => { indices.Add(i); return x; }).ToArray();

        Assert.Equal(new[] { 0, 1, 2, 3, 4 }, indices);
        Assert.Equal([1, 2, 3, 4, 5], result);
    }

    [Fact]
    public void OrderByWithIndexProducesCorrectOrder()
    {
        var source = new[] { "apple", "banana", "cherry", "date", "elderberry" };

        var result = source.OrderBy(static (s, i) => s.Length + i).ToArray();
        // -> 5, 7, 8, 7, 14
        // -> apple banana date cherry elderberry

        Assert.Equal(new[] { "apple", "banana", "date", "cherry", "elderberry" }, result);
    }

    [Fact]
    public void OrderByWithIndexAndComparerProducesCorrectOrder()
    {
        var source = new[] { 5, 3, 1, 2, 4 };

        var result = source.OrderBy(static (x, i) => x, new ReverseComparer<int>()).ToArray();

        Assert.Equal([5, 4, 3, 2, 1], result);
    }

    [Fact]
    public void OrderByWithIndexThrowsForNullSource()
    {
        IEnumerable<int> source = null;

        Assert.Throws<ArgumentNullException>(() => source.OrderBy((x, i) => x).ToArray());
    }

    [Fact]
    public void OrderByWithIndexThrowsForNullKeySelector()
    {
        var source = new[] { 1, 2, 3 };
        Func<int, int, int> keySelector = null;

        Assert.Throws<ArgumentNullException>(() => source.OrderBy(keySelector).ToArray());
    }

    [Fact]
    public void ThenByWithIndexCallsKeySelectorWithCorrectIndices()
    {
        var source = new[] {
            new Person { Name = "Alice", Age = 30 },
            new Person { Name = "Bob", Age = 25 },
            new Person { Name = "Charlie", Age = 30 },
            new Person { Name = "Dave", Age = 25 }
        };
        var indices = new List<int>();

        var result = source
            .OrderBy(p => p.Age)
            .ThenBy((p, i) => { indices.Add(i); return p.Name; })
            .ToArray();

        Assert.Equal(new[] { 0, 1, 2, 3 }, indices);
        Assert.Equal(new[] { "Bob", "Dave", "Alice", "Charlie" }, result.Select(p => p.Name).ToArray());
    }

    [Fact]
    public void ThenByWithIndexProducesCorrectOrder()
    {
        var source = new[] {
            new Person { Name = "Alice", Age = 30 },
            new Person { Name = "Bob", Age = 25 },
            new Person { Name = "Charlie", Age = 30 },
            new Person { Name = "Dave", Age = 25 }
        };

        var result = source
            .OrderBy(static p => p.Age)
            .ThenBy(static (p, i) => p.Name.Length + i)
            .ToArray();

        Assert.Equal(new[] { "Bob", "Dave", "Alice", "Charlie" }, result.Select(static p => p.Name).ToArray());
    }

    [Fact]
    public void ThenByWithIndexAndComparerProducesCorrectOrder()
    {
        var source = new[] {
            new Person { Name = "Alice", Age = 30 },
            new Person { Name = "Bob", Age = 25 },
            new Person { Name = "Charlie", Age = 30 },
            new Person { Name = "Dave", Age = 25 }
        };

        var result = source
            .OrderBy(static p => p.Age)
            .ThenBy(static (p, i) => p.Name, new ReverseComparer<string>())
            .ToArray();

        Assert.Equal(new[] { "Dave", "Bob", "Charlie", "Alice" }, result.Select(static p => p.Name).ToArray());
    }

    [Fact]
    public void ThenByWithIndexThrowsForNullSource()
    {
        IOrderedEnumerable<int> source = null;

        Assert.Throws<ArgumentNullException>(() => source.ThenBy((x, i) => x).ToArray());
    }

    [Fact]
    public void ThenByWithIndexThrowsForNullKeySelector()
    {
        var source = new[] { 1, 2, 3 }.OrderBy(x => x);
        Func<int, int, int> keySelector = null;

        Assert.Throws<ArgumentNullException>(() => source.ThenBy(keySelector).ToArray());
    }

    [Fact]
    public void OrderByDescendingWithIndexCallsKeySelectorWithCorrectIndices()
    {
        var source = new[] { 5, 3, 1, 2, 4 };
        var indices = new List<int>();

        var result = source.OrderByDescending((x, i) => { indices.Add(i); return x; }).ToArray();

        Assert.Equal(new[] { 0, 1, 2, 3, 4 }, indices);
        Assert.Equal([5, 4, 3, 2, 1], result);
    }

    [Fact]
    public void OrderByDescendingWithIndexProducesCorrectOrder()
    {
        var source = new[] { "apple", "banana", "cherry", "date", "elderberry" };

        var result = source.OrderByDescending(static (s, i) => s.Length + i).ToArray();

        Assert.Equal(new[] { "elderberry", "cherry", "banana", "date", "apple" }, result);
    }

    [Fact]
    public void OrderByDescendingWithIndexAndComparerProducesCorrectOrder()
    {
        var source = new[] { 5, 3, 1, 2, 4 };

        var result = source.OrderByDescending(static (x, i) => x, new ReverseComparer<int>()).ToArray();

        Assert.Equal([1, 2, 3, 4, 5], result);
    }

    [Fact]
    public void OrderByDescendingWithIndexThrowsForNullSource()
    {
        IEnumerable<int> source = null;

        Assert.Throws<ArgumentNullException>(() => source.OrderByDescending((x, i) => x).ToArray());
    }

    [Fact]
    public void OrderByDescendingWithIndexThrowsForNullKeySelector()
    {
        var source = new[] { 1, 2, 3 };
        Func<int, int, int> keySelector = null;

        Assert.Throws<ArgumentNullException>(() => source.OrderByDescending(keySelector).ToArray());
    }

    [Fact]
    public void ThenByDescendingWithIndexCallsKeySelectorWithCorrectIndices()
    {
        var source = new[] {
            new Person { Name = "Alice", Age = 30 },
            new Person { Name = "Bob", Age = 25 },
            new Person { Name = "Charlie", Age = 30 },
            new Person { Name = "Dave", Age = 25 }
        };
        var indices = new List<int>();

        var result = source
            .OrderBy(p => p.Age)
            .ThenByDescending((p, i) => { indices.Add(i); return p.Name; })
            .ToArray();

        Assert.Equal(new[] { 0, 1, 2, 3 }, indices);
        Assert.Equal(new[] { "Dave", "Bob", "Charlie", "Alice" }, result.Select(p => p.Name).ToArray());
    }

    [Fact]
    public void ThenByDescendingWithIndexProducesCorrectOrder()
    {
        var source = new[] {
            new Person { Name = "Alice", Age = 30 },
            new Person { Name = "Bob", Age = 25 },
            new Person { Name = "Charlie", Age = 30 },
            new Person { Name = "Dave", Age = 25 }
        };

        var result = source
            .OrderBy(static p => p.Age)
            .ThenByDescending(static (p, i) => p.Name.Length + i)
            .ToArray();

        Assert.Equal(new[] { "Dave", "Bob", "Charlie", "Alice" }, result.Select(static p => p.Name).ToArray());
    }

    [Fact]
    public void ThenByDescendingWithIndexAndComparerProducesCorrectOrder()
    {
        var source = new[] {
            new Person { Name = "Alice", Age = 30 },
            new Person { Name = "Bob", Age = 25 },
            new Person { Name = "Charlie", Age = 30 },
            new Person { Name = "Dave", Age = 25 }
        };

        var result = source
            .OrderBy(static p => p.Age)
            .ThenByDescending(static (p, i) => p.Name, new ReverseComparer<string>())
            .ToArray();

        Assert.Equal(new[] { "Bob", "Dave", "Alice", "Charlie" }, result.Select(static p => p.Name).ToArray());
    }

    [Fact]
    public void ThenByDescendingWithIndexThrowsForNullSource()
    {
        IOrderedEnumerable<int> source = null;

        Assert.Throws<ArgumentNullException>(() => source.ThenByDescending((x, i) => x).ToArray());
    }

    [Fact]
    public void ThenByDescendingWithIndexThrowsForNullKeySelector()
    {
        var source = new[] { 1, 2, 3 }.OrderBy(x => x);
        Func<int, int, int> keySelector = null;

        Assert.Throws<ArgumentNullException>(() => source.ThenByDescending(keySelector).ToArray());
    }

    [Fact]
    public void OrderByWithKeysProducesCorrectOrder()
    {
        var source = new[] { "apple", "banana", "cherry", "date" };
        var keys = new[] { 3, 1, 4, 2 };

        var result = source.OrderBy(keys).ToArray();

        Assert.Equal(new[] { "banana", "date", "apple", "cherry" }, result);
    }

    [Fact]
    public void OrderByWithKeysAndComparerProducesCorrectOrder()
    {
        var source = new[] { "apple", "banana", "cherry", "date" };
        var keys = new[] { 3, 1, 4, 2 };

        var result = source.OrderBy(keys, new ReverseComparer<int>()).ToArray();

        Assert.Equal(new[] { "cherry", "apple", "date", "banana" }, result);
    }

    [Fact]
    public void OrderByWithKeysThrowsForNullSource()
    {
        IEnumerable<string> source = null;
        var keys = new[] { 1, 2, 3 };

        Assert.Throws<ArgumentNullException>(() => source.OrderBy(keys).ToArray());
    }

    [Fact]
    public void OrderByWithKeysThrowsForNullKeys()
    {
        var source = new[] { "a", "b", "c" };
        IEnumerable<int> keys = null;

        Assert.Throws<ArgumentNullException>(() => source.OrderBy(keys).ToArray());
    }

    [Fact]
    public void ThenByWithKeysProducesCorrectOrder()
    {
        var source = new[] {
            new Person { Name = "Alice", Age = 30 },
            new Person { Name = "Bob", Age = 25 },
            new Person { Name = "Charlie", Age = 30 },
            new Person { Name = "Dave", Age = 25 }
        };
        var keys = new[] { 3, 1, 4, 2 };

        var result = source
            .OrderBy(static p => p.Age)
            .ThenBy(keys)
            .ToArray();

        Assert.Equal(new[] { "Bob", "Dave", "Alice", "Charlie" }, result.Select(static p => p.Name).ToArray());
    }

    [Fact]
    public void ThenByWithKeysAndComparerProducesCorrectOrder()
    {
        var source = new[] {
            new Person { Name = "Alice", Age = 30 },
            new Person { Name = "Bob", Age = 25 },
            new Person { Name = "Charlie", Age = 30 },
            new Person { Name = "Dave", Age = 25 }
        };
        var keys = new[] { 3, 1, 4, 2 };

        var result = source
            .OrderBy(static p => p.Age)
            .ThenBy(keys, new ReverseComparer<int>())
            .ToArray();

        Assert.Equal(new[] { "Dave", "Bob", "Charlie", "Alice" }, result.Select(static p => p.Name).ToArray());
    }

    [Fact]
    public void ThenByWithKeysThrowsForNullSource()
    {
        IOrderedEnumerable<string> source = null;
        var keys = new[] { 1, 2, 3 };

        Assert.Throws<ArgumentNullException>(() => source.ThenBy(keys).ToArray());
    }

    [Fact]
    public void ThenByWithKeysThrowsForNullKeys()
    {
        var source = new[] { "a", "b", "c" }.OrderBy(x => x);
        IEnumerable<int> keys = null;

        Assert.Throws<ArgumentNullException>(() => source.ThenBy(keys).ToArray());
    }

    [Fact]
    public void OrderByDescendingWithKeysProducesCorrectOrder()
    {
        var source = new[] { "apple", "banana", "cherry", "date" };
        var keys = new[] { 3, 1, 4, 2 };

        var result = source.OrderByDescending(keys).ToArray();

        Assert.Equal(new[] { "cherry", "apple", "date", "banana" }, result);
    }

    [Fact]
    public void OrderByDescendingWithKeysAndComparerProducesCorrectOrder()
    {
        var source = new[] { "apple", "banana", "cherry", "date" };
        var keys = new[] { 3, 1, 4, 2 };

        var result = source.OrderByDescending(keys, new ReverseComparer<int>()).ToArray();

        Assert.Equal(new[] { "banana", "date", "apple", "cherry" }, result);
    }

    [Fact]
    public void OrderByDescendingWithKeysThrowsForNullSource()
    {
        IEnumerable<string> source = null;
        var keys = new[] { 1, 2, 3 };

        Assert.Throws<ArgumentNullException>(() => source.OrderByDescending(keys).ToArray());
    }

    [Fact]
    public void OrderByDescendingWithKeysThrowsForNullKeys()
    {
        var source = new[] { "a", "b", "c" };
        IEnumerable<int> keys = null;

        Assert.Throws<ArgumentNullException>(() => source.OrderByDescending(keys).ToArray());
    }

    [Fact]
    public void ThenByDescendingWithKeysProducesCorrectOrder()
    {
        var source = new[] {
            new Person { Name = "Alice", Age = 30 },
            new Person { Name = "Bob", Age = 25 },
            new Person { Name = "Charlie", Age = 30 },
            new Person { Name = "Dave", Age = 25 }
        };
        var keys = new[] { 3, 1, 4, 2 };

        var result = source
            .OrderBy(static p => p.Age)
            .ThenByDescending(keys)
            .ToArray();

        Assert.Equal(new[] { "Dave", "Bob", "Charlie", "Alice" }, result.Select(static p => p.Name).ToArray());
    }

    [Fact]
    public void ThenByDescendingWithKeysAndComparerProducesCorrectOrder()
    {
        var source = new[] {
            new Person { Name = "Alice", Age = 30 },
            new Person { Name = "Bob", Age = 25 },
            new Person { Name = "Charlie", Age = 30 },
            new Person { Name = "Dave", Age = 25 }
        };
        var keys = new[] { 3, 1, 4, 2 };

        var result = source
            .OrderBy(static p => p.Age)
            .ThenByDescending(keys, new ReverseComparer<int>())
            .ToArray();

        Assert.Equal(new[] { "Bob", "Dave", "Alice", "Charlie" }, result.Select(static p => p.Name).ToArray());
    }

    [Fact]
    public void ThenByDescendingWithKeysThrowsForNullSource()
    {
        IOrderedEnumerable<string> source = null;
        var keys = new[] { 1, 2, 3 };

        Assert.Throws<ArgumentNullException>(() => source.ThenByDescending(keys).ToArray());
    }

    [Fact]
    public void ThenByDescendingWithKeysThrowsForNullKeys()
    {
        var source = new[] { "a", "b", "c" }.OrderBy(x => x);
        IEnumerable<int> keys = null;

        Assert.Throws<ArgumentNullException>(() => source.ThenByDescending(keys).ToArray());
    }

    [Fact]
    public void OrderByGenericWithIndexProducesCorrectOrder()
    {
        var source = new[] {
            new Person { Name = "Alice", Age = 30 },
            new Person { Name = "Bob", Age = 25 },
            new Person { Name = "Charlie", Age = 35 }
        };

        var result = source.OrderBy<Person, string>(static (p, i) => p.Name + i).ToArray();

        Assert.Equal(new[] { "Alice", "Bob", "Charlie" }, result.Select(static p => p.Name).ToArray());
    }

    [Fact]
    public void OrderByGenericWithIndexAndComparerProducesCorrectOrder()
    {
        var source = new[] {
            new Person { Name = "Alice", Age = 30 },
            new Person { Name = "Bob", Age = 25 },
            new Person { Name = "Charlie", Age = 35 }
        };

        var result = source.OrderBy<Person, string>(static (p, i) => p.Name, new ReverseComparer<string>()).ToArray();

        Assert.Equal(new[] { "Charlie", "Bob", "Alice" }, result.Select(static p => p.Name).ToArray());
    }

    [Fact]
    public void ThenByGenericWithIndexProducesCorrectOrder()
    {
        var source = new[] {
            new Person { Name = "Alice", Age = 30 },
            new Person { Name = "Bob", Age = 25 },
            new Person { Name = "Charlie", Age = 30 },
            new Person { Name = "Dave", Age = 25 }
        };

        var result = source
            .OrderBy(static p => p.Age)
            .ThenBy<Person, string>(static (p, i) => p.Name + i)
            .ToArray();

        Assert.Equal(new[] { "Bob", "Dave", "Alice", "Charlie" }, result.Select(static p => p.Name).ToArray());
    }

    [Fact]
    public void ThenByGenericWithIndexAndComparerProducesCorrectOrder()
    {
        var source = new[] {
            new Person { Name = "Alice", Age = 30 },
            new Person { Name = "Bob", Age = 25 },
            new Person { Name = "Charlie", Age = 30 },
            new Person { Name = "Dave", Age = 25 }
        };

        var result = source
            .OrderBy(static p => p.Age)
            .ThenBy<Person, string>(static (p, i) => p.Name, new ReverseComparer<string>())
            .ToArray();

        Assert.Equal(new[] { "Dave", "Bob", "Charlie", "Alice" }, result.Select(static p => p.Name).ToArray());
    }

    [Fact]
    public void OrderByDescendingGenericWithIndexProducesCorrectOrder()
    {
        var source = new[] {
            new Person { Name = "Alice", Age = 30 },
            new Person { Name = "Bob", Age = 25 },
            new Person { Name = "Charlie", Age = 35 }
        };

        var result = source.OrderByDescending<Person, string>(static (p, i) => p.Name + i).ToArray();

        Assert.Equal(new[] { "Charlie", "Bob", "Alice" }, result.Select(static p => p.Name).ToArray());
    }

    [Fact]
    public void OrderByDescendingGenericWithIndexAndComparerProducesCorrectOrder()
    {
        var source = new[] {
            new Person { Name = "Alice", Age = 30 },
            new Person { Name = "Bob", Age = 25 },
            new Person { Name = "Charlie", Age = 35 }
        };

        var result = source.OrderByDescending<Person, string>(static (p, i) => p.Name, new ReverseComparer<string>()).ToArray();

        Assert.Equal(new[] { "Alice", "Bob", "Charlie" }, result.Select(static p => p.Name).ToArray());
    }

    [Fact]
    public void ThenByDescendingGenericWithIndexProducesCorrectOrder()
    {
        var source = new[] {
            new Person { Name = "Alice", Age = 30 },
            new Person { Name = "Bob", Age = 25 },
            new Person { Name = "Charlie", Age = 30 },
            new Person { Name = "Dave", Age = 25 }
        };

        var result = source
            .OrderBy(static p => p.Age)
            .ThenByDescending<Person, string>(static (p, i) => p.Name + i)
            .ToArray();

        Assert.Equal(new[] { "Dave", "Bob", "Charlie", "Alice" }, result.Select(static p => p.Name).ToArray());
    }

    [Fact]
    public void ThenByDescendingGenericWithIndexAndComparerProducesCorrectOrder()
    {
        var source = new[] {
            new Person { Name = "Alice", Age = 30 },
            new Person { Name = "Bob", Age = 25 },
            new Person { Name = "Charlie", Age = 30 },
            new Person { Name = "Dave", Age = 25 }
        };

        var result = source
            .OrderBy(static p => p.Age)
            .ThenByDescending<Person, string>(static (p, i) => p.Name, new ReverseComparer<string>())
            .ToArray();

        Assert.Equal(new[] { "Bob", "Dave", "Alice", "Charlie" }, result.Select(static p => p.Name).ToArray());
    }

    [Fact]
    public void OrderByGenericWithKeysProducesCorrectOrder()
    {
        var source = new[] {
            new Person { Name = "Alice", Age = 30 },
            new Person { Name = "Bob", Age = 25 },
            new Person { Name = "Charlie", Age = 35 }
        };
        var keys = new[] { "Z", "X", "Y" };

        var result = source.OrderBy<Person, string>(keys).ToArray();

        Assert.Equal(new[] { "Bob", "Charlie", "Alice" }, result.Select(static p => p.Name).ToArray());
    }

    [Fact]
    public void OrderByGenericWithKeysAndComparerProducesCorrectOrder()
    {
        var source = new[] {
            new Person { Name = "Alice", Age = 30 },
            new Person { Name = "Bob", Age = 25 },
            new Person { Name = "Charlie", Age = 35 }
        };
        var keys = new[] { "Z", "X", "Y" };

        var result = source.OrderBy<Person, string>(keys, new ReverseComparer<string>()).ToArray();

        Assert.Equal(new[] { "Alice", "Charlie", "Bob" }, result.Select(static p => p.Name).ToArray());
    }

    [Fact]
    public void ThenByGenericWithKeysProducesCorrectOrder()
    {
        var source = new[] {
            new Person { Name = "Alice", Age = 30 },
            new Person { Name = "Bob", Age = 25 },
            new Person { Name = "Charlie", Age = 30 },
            new Person { Name = "Dave", Age = 25 }
        };
        var keys = new[] { "C", "A", "D", "B" };

        var result = source
            .OrderBy(static p => p.Age)
            .ThenBy<Person, string>(keys)
            .ToArray();

        Assert.Equal(new[] { "Bob", "Dave", "Alice", "Charlie" }, result.Select(static p => p.Name).ToArray());
    }

    [Fact]
    public void ThenByGenericWithKeysAndComparerProducesCorrectOrder()
    {
        var source = new[] {
            new Person { Name = "Alice", Age = 30 },
            new Person { Name = "Bob", Age = 25 },
            new Person { Name = "Charlie", Age = 30 },
            new Person { Name = "Dave", Age = 25 }
        };
        var keys = new[] { "C", "A", "D", "B" };

        var result = source
            .OrderBy(static p => p.Age)
            .ThenBy<Person, string>(keys, new ReverseComparer<string>())
            .ToArray();

        Assert.Equal(new[] { "Dave", "Bob", "Charlie", "Alice" }, result.Select(static p => p.Name).ToArray());
    }

    [Fact]
    public void OrderByDescendingGenericWithKeysProducesCorrectOrder()
    {
        var source = new[] {
            new Person { Name = "Alice", Age = 30 },
            new Person { Name = "Bob", Age = 25 },
            new Person { Name = "Charlie", Age = 35 }
        };
        var keys = new[] { "Z", "X", "Y" };

        var result = source.OrderByDescending<Person, string>(keys).ToArray();

        Assert.Equal(new[] { "Alice", "Charlie", "Bob" }, result.Select(static p => p.Name).ToArray());
    }

    [Fact]
    public void OrderByDescendingGenericWithKeysAndComparerProducesCorrectOrder()
    {
        var source = new[] {
            new Person { Name = "Alice", Age = 30 },
            new Person { Name = "Bob", Age = 25 },
            new Person { Name = "Charlie", Age = 35 }
        };
        var keys = new[] { "Z", "X", "Y" };

        var result = source.OrderByDescending<Person, string>(keys, new ReverseComparer<string>()).ToArray();

        Assert.Equal(new[] { "Bob", "Charlie", "Alice" }, result.Select(static p => p.Name).ToArray());
    }

    [Fact]
    public void ThenByDescendingGenericWithKeysProducesCorrectOrder()
    {
        var source = new[] {
            new Person { Name = "Alice", Age = 30 },
            new Person { Name = "Bob", Age = 25 },
            new Person { Name = "Charlie", Age = 30 },
            new Person { Name = "Dave", Age = 25 }
        };
        var keys = new[] { "C", "A", "D", "B" };

        var result = source
            .OrderBy(static p => p.Age)
            .ThenByDescending<Person, string>(keys)
            .ToArray();

        Assert.Equal(new[] { "Dave", "Bob", "Charlie", "Alice" }, result.Select(static p => p.Name).ToArray());
    }

    [Fact]
    public void ThenByDescendingGenericWithKeysAndComparerProducesCorrectOrder()
    {
        var source = new[] {
            new Person { Name = "Alice", Age = 30 },
            new Person { Name = "Bob", Age = 25 },
            new Person { Name = "Charlie", Age = 30 },
            new Person { Name = "Dave", Age = 25 }
        };
        var keys = new[] { "C", "A", "D", "B" };

        var result = source
            .OrderBy(static p => p.Age)
            .ThenByDescending<Person, string>(keys, new ReverseComparer<string>())
            .ToArray();

        Assert.Equal(new[] { "Bob", "Dave", "Alice", "Charlie" }, result.Select(static p => p.Name).ToArray());
    }

    [Fact]
    public void OrderByWithEmptySourceReturnsEmptySequence()
    {
        var source = Array.Empty<int>();

        var result = source.OrderBy(static (x, i) => x).ToArray();

        Assert.Empty(result);
    }

    [Fact]
    public void OrderByWithSingleElementReturnsOriginalSequence()
    {
        var source = new[] { 42 };

        var result = source.OrderBy(static (x, i) => x).ToArray();

        Assert.Equal([42], result);
    }

    [Fact]
    public void OrderByHandlesEqualKeys()
    {
        var source = new[] { 2, 1, 2, 1, 3 };

        var result = source.OrderBy(static (x, i) => x).ToArray();

        Assert.Equal([1, 1, 2, 2, 3], result);
    }
}
