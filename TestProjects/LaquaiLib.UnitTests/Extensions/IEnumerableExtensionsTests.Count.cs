using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Extensions;

public class IEnumerableCountsMethodTests
{
    [Fact]
    public void CountsBuildsCorrectDictionary()
    {
        var items = new[] { "a", "b", "a", "c", "b", "a" };
        var counts = items.Counts();

        Assert.Equal(3, counts.Count);
        Assert.Equal(3, counts["a"]);
        Assert.Equal(2, counts["b"]);
        Assert.Equal(1, counts["c"]);
    }

    [Fact]
    public void CountsHandlesEmptyCollection()
    {
        var empty = Array.Empty<string>();
        var counts = empty.Counts();

        Assert.Empty(counts);
    }

    [Fact]
    public void CountsRespectsCustomComparer()
    {
        var items = new[] { "A", "b", "a", "C", "B", "a" };
        var counts = items.Counts(StringComparer.OrdinalIgnoreCase);

        Assert.Equal(3, counts.Count);
        Assert.Equal(3, counts["a"]); // Case insensitive, so "A" and "a" are counted together
        Assert.Equal(2, counts["b"]); // Case insensitive, so "b" and "B" are counted together
        Assert.Equal(1, counts["c"]); // Case insensitive, so "C" is counted
    }

    [Fact]
    public void CountsWithNullComparer()
    {
        var items = new[] { "A", "b", "a", "C", "B", "a" };
        var counts = items.Counts(null); // Should use default comparer

        Assert.Equal(5, counts.Count);
        Assert.Equal(2, counts["a"]);
        Assert.Equal(1, counts["A"]);
        Assert.Equal(1, counts["b"]);
        Assert.Equal(1, counts["B"]);
        Assert.Equal(1, counts["C"]);
    }

    [Fact]
    public void CountsWithCustomType()
    {
        var items = new[]
        {
            new CustomItem(1, "one"),
            new CustomItem(2, "two"),
            new CustomItem(1, "ONE"),
            new CustomItem(3, "three"),
            new CustomItem(2, "TWO")
        };

        var counts = items.Counts(new CustomItemComparer());

        Assert.Equal(3, counts.Count);
        Assert.Equal(2, counts[new CustomItem(1, "any")]);
        Assert.Equal(2, counts[new CustomItem(2, "any")]);
        Assert.Equal(1, counts[new CustomItem(3, "any")]);
    }

    [Fact]
    public void CountsHandlesDuplicateValues()
    {
        var items = new[] { 5, 5, 5, 5, 5 };
        var counts = items.Counts();

        Assert.Single(counts);
        Assert.Equal(5, counts[5]);
    }

    [Fact]
    public void CountsWithCustomEnumerable()
    {
        var customCollection = new CustomEnumerable<string>(["x", "y", "x", "z", "y", "x"]);
        var counts = customCollection.Counts();

        Assert.Equal(3, counts.Count);
        Assert.Equal(3, counts["x"]);
        Assert.Equal(2, counts["y"]);
        Assert.Equal(1, counts["z"]);
    }

    private class CustomItem(int id, string name)
    {
        public int Id { get; } = id;
        public string Name { get; } = name;
    }

    private class CustomItemComparer : IEqualityComparer<CustomItem>
    {
        public bool Equals(CustomItem x, CustomItem y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return x.Id == y.Id;
        }

        public int GetHashCode(CustomItem obj) => obj.Id.GetHashCode();
    }

    private class CustomEnumerable<T>(T[] items) : IEnumerable<T>
    {
        private readonly T[] _items = items;

        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_items).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
    }
}
