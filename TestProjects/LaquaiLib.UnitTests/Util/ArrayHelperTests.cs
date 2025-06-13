using LaquaiLib.Util;

namespace LaquaiLib.UnitTests.Util;

public class ArrayHelperTests
{
    #region Basic Sort Tests

    [Fact]
    public void SortWithIntKeysAndValuesSortsArraysCorrectly()
    {
        int[] keys = [3, 1, 4, 2];
        int[] values1 = [10, 20, 30, 40];
        int[] values2 = [100, 200, 300, 400];

        ArrayHelper.Sort(keys, values1, values2);

        Assert.Equal([1, 2, 3, 4], keys);
        Assert.Equal([20, 40, 10, 30], values1);
        Assert.Equal([200, 400, 100, 300], values2);
    }

    [Fact]
    public void SortDescendingWithIntKeysAndValuesSortsArraysCorrectly()
    {
        int[] keys = [3, 1, 4, 2];
        int[] values1 = [10, 20, 30, 40];
        int[] values2 = [100, 200, 300, 400];

        ArrayHelper.SortDescending(keys, values1, values2);

        Assert.Equal([4, 3, 2, 1], keys);
        Assert.Equal([30, 10, 40, 20], values1);
        Assert.Equal([300, 100, 400, 200], values2);
    }

    [Fact]
    public void SortWithPreSortedKeysReturnsEarly()
    {
        int[] keys = [1, 2, 3, 4];
        int[] values = [10, 20, 30, 40];
        var originalValues = values.ToArray();

        ArrayHelper.Sort(keys, values);

        Assert.Equal(originalValues, values);
    }

    [Fact]
    public void SortWithEmptyArraysReturnsWithoutError()
    {
        int[] keys = [];
        int[] values = [];

        ArrayHelper.Sort(keys, values);
    }

    [Fact]
    public void SortWithSingleElementArraysSortsCorrectly()
    {
        int[] keys = [5];
        int[] values = [10];

        ArrayHelper.Sort(keys, values);

        Assert.Equal([5], keys);
        Assert.Equal([10], values);
    }

    #endregion

    #region Different Data Types Tests

    [Fact]
    public void SortWithStringKeysAndValuesSortsArraysCorrectly()
    {
        string[] keys = ["banana", "apple", "cherry"];
        string[] values = ["yellow", "red", "red"];

        ArrayHelper.Sort(keys, values);

        Assert.Equal(new[] { "apple", "banana", "cherry" }, keys);
        Assert.Equal(new[] { "red", "yellow", "red" }, values);
    }

    [Fact]
    public void SortWithCustomObjectsSortsUsingDefaultComparer()
    {
        var keys = new ComparableObject[]
        {
                new ComparableObject { Value = 3 },
                new ComparableObject { Value = 1 },
                new ComparableObject { Value = 2 }
        };

        string[] values = ["three", "one", "two"];

        ArrayHelper.Sort(keys, values);

        Assert.Equal(1, keys[0].Value);
        Assert.Equal(2, keys[1].Value);
        Assert.Equal(3, keys[2].Value);

        Assert.Equal(new[] { "one", "two", "three" }, values);
    }

    [Fact]
    public void SortWithNonGenericMethodSortsArraysCorrectly()
    {
        Array keys = new int[] { 3, 1, 4, 2 };
        Array values = new string[] { "three", "one", "four", "two" };

        ArrayHelper.Sort(keys, values);

        Assert.Equal(new int[] { 1, 2, 3, 4 }, keys);
        Assert.Equal(new string[] { "one", "two", "three", "four" }, values);
    }

    #endregion

    #region Custom Comparer Tests

    [Fact]
    public void SortWithCustomComparerSortsArraysCorrectly()
    {
        string[] keys = ["A", "a", "B", "b"];
        int[] values = [1, 2, 3, 4];

        ArrayHelper.Sort(keys, StringComparer.OrdinalIgnoreCase, values);

        Assert.Equal(new[] { "A", "a", "B", "b" }, keys);
        Assert.Equal([1, 2, 3, 4], values);
    }

    [Fact]
    public void SortDescendingWithCustomComparerSortsArraysCorrectly()
    {
        string[] keys = ["aaaa", "bb", "c", "ddd"];
        int[] values = [1, 3, 2, 4];

        var comparer = Comparer<string>.Create(static (x, y) => x.Length.CompareTo(y.Length));

        ArrayHelper.SortDescending(keys, comparer, values);

        Assert.Equal(new[] { "aaaa", "ddd", "bb", "c" }, keys);
        Assert.Equal([1, 4, 3, 2], values);
    }

    [Fact]
    public void SortWithNonGenericCustomComparerSortsArraysCorrectly()
    {
        Array keys = new string[] { "banana", "apple", "cherry" };
        Array values = new int[] { 3, 1, 2 };

        System.Collections.IComparer comparer = StringComparer.OrdinalIgnoreCase;

        ArrayHelper.Sort(keys, comparer, values);

        Assert.Equal((string[])["apple", "banana", "cherry"], keys);
        Assert.Equal((int[])[1, 3, 2], values);
    }

    #endregion

    #region Selector Tests

    [Fact]
    public void SortWithSelectorSortsArraysCorrectly()
    {
        var people = new Person[]
        {
            new Person { Name = "Charlie", Age = 30 },
            new Person { Name = "Alice", Age = 25 },
            new Person { Name = "Bob", Age = 35 }
        };

        int[] values = [1, 2, 3];

        ArrayHelper.Sort(people, static p => p.Name, values);

        Assert.Equal("Alice", people[0].Name);
        Assert.Equal("Bob", people[1].Name);
        Assert.Equal("Charlie", people[2].Name);

        Assert.Equal([2, 3, 1], values);
    }

    [Fact]
    public void SortDescendingWithSelectorSortsArraysCorrectly()
    {
        var people = new Person[]
        {
            new Person { Name = "Charlie", Age = 30 },
            new Person { Name = "Alice", Age = 25 },
            new Person { Name = "Bob", Age = 35 }
        };

        int[] values = [1, 2, 3];

        ArrayHelper.SortDescending(people, static p => p.Age, values);

        Assert.Equal("Bob", people[0].Name);
        Assert.Equal("Charlie", people[1].Name);
        Assert.Equal("Alice", people[2].Name);

        Assert.Equal([3, 1, 2], values);
    }

    [Fact]
    public void SortWithSelectorAndComparerSortsArraysCorrectly()
    {
        var people = new Person[]
        {
            new Person { Name = "Charlie", Age = 30 },
            new Person { Name = "alice", Age = 25 },
            new Person { Name = "Bob", Age = 35 }
        };

        int[] values = [1, 2, 3];

        ArrayHelper.Sort(people, static p => p.Name, StringComparer.OrdinalIgnoreCase, values);

        Assert.Equal("alice", people[0].Name);
        Assert.Equal("Bob", people[1].Name);
        Assert.Equal("Charlie", people[2].Name);

        Assert.Equal([2, 3, 1], values);
    }

    [Fact]
    public void SortWithNonGenericSelectorSortsArraysCorrectly()
    {
        Array people = new Person[]
        {
            new Person { Name = "Charlie", Age = 30 },
            new Person { Name = "Alice", Age = 25 },
            new Person { Name = "Bob", Age = 35 }
        };

        Array values = new int[] { 1, 2, 3 };

        ArrayHelper.Sort(people, static p => ((Person)p).Name, values);

        Assert.Equal("Alice", ((Person)people.GetValue(0)).Name);
        Assert.Equal("Bob", ((Person)people.GetValue(1)).Name);
        Assert.Equal("Charlie", ((Person)people.GetValue(2)).Name);

        Assert.Equal(2, values.GetValue(0));
        Assert.Equal(3, values.GetValue(1));
        Assert.Equal(1, values.GetValue(2));
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public void SortWithNullKeysThrowsArgumentNullException()
    {
        int[] values = [1, 2, 3];

        Assert.Throws<ArgumentNullException>(() => ArrayHelper.Sort<int, int>(null, values));
    }

    [Fact]
    public void SortWithNullItemsArraysThrowsArgumentNullException()
    {
        int[] keys = [1, 2, 3];

        Assert.Throws<ArgumentNullException>(() => ArrayHelper.Sort(keys, (int[][])null));
    }

    [Fact]
    public void SortWithDifferentLengthArraysThrowsArgumentException()
    {
        int[] keys = [1, 2, 3];
        int[] values = [1, 2];
        Assert.Throws<ArgumentException>(() => ArrayHelper.Sort(keys, values));
    }

    [Fact]
    public void SortWithEmptyItemsArraysReturnsWithoutSorting()
    {
        int[] keys = [3, 1, 2];
        int[][] emptyArray = [];

        ArrayHelper.Sort(keys, emptyArray);

        Assert.Equal([3, 1, 2], keys);
    }

    [Fact]
    public void SortWithNullSelectorThrowsArgumentNullException()
    {
        int[] keys = [1, 2, 3];
        int[] values = [1, 2, 3];
        Func<int, int> selector = null;

        Assert.Throws<ArgumentNullException>(() => ArrayHelper.Sort(keys, selector, values));
    }

    #endregion

    #region Multiple Arrays Tests

    [Fact]
    public void SortWithMultipleArraysSortsAllArraysCorrectly()
    {
        int[] keys = [3, 1, 4, 2];
        int[] values1 = [10, 20, 30, 40];
        int[] values2 = [100, 200, 300, 400];
        int[] values3 = [1000, 2000, 3000, 4000];

        ArrayHelper.Sort(keys, values1, values2, values3);

        Assert.Equal([1, 2, 3, 4], keys);
        Assert.Equal([20, 40, 10, 30], values1);
        Assert.Equal([200, 400, 100, 300], values2);
        Assert.Equal([2000, 4000, 1000, 3000], values3);
    }

    [Fact]
    public void SortDescendingWithMultipleArraysSortsAllArraysCorrectly()
    {
        int[] keys = [3, 1, 4, 2];
        int[] values1 = [10, 20, 30, 40];
        int[] values2 = [100, 200, 300, 400];
        int[] values3 = [1000, 2000, 3000, 4000];

        ArrayHelper.SortDescending(keys, values1, values2, values3);

        Assert.Equal([4, 3, 2, 1], keys);
        Assert.Equal([30, 10, 40, 20], values1);
        Assert.Equal([300, 100, 400, 200], values2);
        Assert.Equal([3000, 1000, 4000, 2000], values3);
    }

    #endregion
}

// Helper classes for testing
public class ComparableObject : IComparable<ComparableObject>
{
    public int Value { get; set; }

    public int CompareTo(ComparableObject other) => Value.CompareTo(other.Value);
}

public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
}
