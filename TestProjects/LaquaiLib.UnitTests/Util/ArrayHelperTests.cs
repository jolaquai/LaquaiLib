using LaquaiLib.Extensions;
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

    [Fact]
    public void SortDescendingWithSingleElementArraysSortsCorrectly()
    {
        int[] keys = [5];
        int[] values = [10];

        ArrayHelper.SortDescending(keys, values);

        Assert.Equal([5], keys);
        Assert.Equal([10], values);
    }

    [Fact]
    public void SortWithTwoElementArraysSortsCorrectly()
    {
        int[] keys = [2, 1];
        int[] values = [20, 10];

        ArrayHelper.Sort(keys, values);

        Assert.Equal([1, 2], keys);
        Assert.Equal([10, 20], values);
    }

    [Fact]
    public void SortDescendingWithTwoDescendingKeysLeavesArraysUnchanged()
    {
        int[] keys = [2, 1];
        int[] values = [20, 10];

        ArrayHelper.SortDescending(keys, values);

        Assert.Equal([2, 1], keys);
        Assert.Equal([20, 10], values);
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

    [Fact]
    public void SortWithDoubleKeysSortsArraysCorrectly()
    {
        double[] keys = [3.5, 1.1, 2.7];
        string[] values = ["three", "one", "two"];

        ArrayHelper.Sort(keys, values);

        Assert.Equal([1.1, 2.7, 3.5], keys);
        Assert.Equal(["one", "two", "three"], values);
    }

    [Fact]
    public void SortWithDateTimeKeysSortsArraysCorrectly()
    {
        DateTime[] keys =
        [
            new DateTime(2024, 6, 1),
            new DateTime(2023, 1, 1),
            new DateTime(2025, 3, 15)
        ];
        string[] values = ["mid", "early", "late"];

        ArrayHelper.Sort(keys, values);

        Assert.Equal(new DateTime(2023, 1, 1), keys[0]);
        Assert.Equal(new DateTime(2024, 6, 1), keys[1]);
        Assert.Equal(new DateTime(2025, 3, 15), keys[2]);
        Assert.Equal(["early", "mid", "late"], values);
    }

    [Fact]
    public void SortWithReferenceTypeValuesPreservesReferences()
    {
        int[] keys = [3, 1, 2];
        var a = new List<int> { 1 };
        var b = new List<int> { 2 };
        var c = new List<int> { 3 };
        List<int>[] values = [a, b, c];

        ArrayHelper.Sort(keys, values);

        Assert.Same(b, values[0]);
        Assert.Same(c, values[1]);
        Assert.Same(a, values[2]);
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

        IComparer comparer = StringComparer.OrdinalIgnoreCase;

        ArrayHelper.Sort(keys, comparer, values);

        Assert.Equal((string[])["apple", "banana", "cherry"], keys);
        Assert.Equal((int[])[1, 3, 2], values);
    }

    [Fact]
    public void SortDescendingWithNonGenericCustomComparerSortsArraysCorrectly()
    {
        Array keys = new string[] { "banana", "apple", "cherry" };
        Array values = new int[] { 3, 1, 2 };

        IComparer comparer = StringComparer.OrdinalIgnoreCase;

        ArrayHelper.SortDescending(keys, comparer, values);

        Assert.Equal((string[])["cherry", "banana", "apple"], keys);
        Assert.Equal((int[])[2, 3, 1], values);
    }

    [Fact]
    public void SortWithExplicitNullComparerUsesDefaultComparer()
    {
        int[] keys = [3, 1, 2];
        int[] values = [30, 10, 20];

        ArrayHelper.Sort(keys, (IComparer<int>)null, values);

        Assert.Equal([1, 2, 3], keys);
        Assert.Equal([10, 20, 30], values);
    }

    [Fact]
    public void SortDescendingWithExplicitNullComparerUsesDefaultComparer()
    {
        int[] keys = [3, 1, 2];
        int[] values = [30, 10, 20];

        ArrayHelper.SortDescending(keys, (IComparer<int>)null, values);

        Assert.Equal([3, 2, 1], keys);
        Assert.Equal([30, 20, 10], values);
    }

    [Fact]
    public void SortWithNonGenericExplicitNullComparerUsesDefaultComparer()
    {
        Array keys = new int[] { 3, 1, 2 };
        Array values = new int[] { 30, 10, 20 };

        ArrayHelper.Sort(keys, (IComparer)null, values);

        Assert.Equal((int[])[1, 2, 3], keys);
        Assert.Equal((int[])[10, 20, 30], values);
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
    public void SortDescendingWithSelectorAndComparerSortsArraysCorrectly()
    {
        var people = new Person[]
        {
            new Person { Name = "Charlie", Age = 30 },
            new Person { Name = "alice", Age = 25 },
            new Person { Name = "Bob", Age = 35 }
        };

        int[] values = [1, 2, 3];

        ArrayHelper.SortDescending(people, static p => p.Name, StringComparer.OrdinalIgnoreCase, values);

        Assert.Equal("Charlie", people[0].Name);
        Assert.Equal("Bob", people[1].Name);
        Assert.Equal("alice", people[2].Name);

        Assert.Equal([1, 3, 2], values);
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

    [Fact]
    public void SortDescendingWithNonGenericSelectorSortsArraysCorrectly()
    {
        Array people = new Person[]
        {
            new Person { Name = "Charlie", Age = 30 },
            new Person { Name = "Alice", Age = 25 },
            new Person { Name = "Bob", Age = 35 }
        };

        Array values = new int[] { 1, 2, 3 };

        ArrayHelper.SortDescending(people, static p => ((Person)p).Name, values);

        Assert.Equal("Charlie", ((Person)people.GetValue(0)).Name);
        Assert.Equal("Bob", ((Person)people.GetValue(1)).Name);
        Assert.Equal("Alice", ((Person)people.GetValue(2)).Name);

        Assert.Equal(1, values.GetValue(0));
        Assert.Equal(3, values.GetValue(1));
        Assert.Equal(2, values.GetValue(2));
    }

    [Fact]
    public void SortWithNonGenericSelectorAndComparerSortsArraysCorrectly()
    {
        Array people = new Person[]
        {
            new Person { Name = "Charlie", Age = 30 },
            new Person { Name = "alice", Age = 25 },
            new Person { Name = "Bob", Age = 35 }
        };

        Array values = new int[] { 1, 2, 3 };

        IComparer comparer = StringComparer.OrdinalIgnoreCase;

        ArrayHelper.Sort(people, static p => ((Person)p).Name, comparer, values);

        Assert.Equal("alice", ((Person)people.GetValue(0)).Name);
        Assert.Equal("Bob", ((Person)people.GetValue(1)).Name);
        Assert.Equal("Charlie", ((Person)people.GetValue(2)).Name);

        Assert.Equal(2, values.GetValue(0));
        Assert.Equal(3, values.GetValue(1));
        Assert.Equal(1, values.GetValue(2));
    }

    [Fact]
    public void SortDescendingWithNonGenericSelectorAndComparerSortsArraysCorrectly()
    {
        Array people = new Person[]
        {
            new Person { Name = "aaaa", Age = 1 },
            new Person { Name = "bb", Age = 2 },
            new Person { Name = "c", Age = 3 },
            new Person { Name = "ddd", Age = 4 }
        };

        Array values = new int[] { 1, 2, 3, 4 };

        IComparer comparer = Comparer<string>.Create(static (x, y) => x.Length.CompareTo(y.Length));

        ArrayHelper.SortDescending(people, static p => ((Person)p).Name, comparer, values);

        Assert.Equal(["aaaa", "ddd", "bb", "c"], ((Person[])people).Select(static p => p.Name).ToArray());
        Assert.Equal((int[])[1, 4, 2, 3], values);
    }

    [Fact]
    public void SortWithSelectorAndNoExtraItemsArraysSortsItemsInPlace()
    {
        var people = new Person[]
        {
            new Person { Name = "Charlie", Age = 30 },
            new Person { Name = "Alice", Age = 25 },
            new Person { Name = "Bob", Age = 35 }
        };

        ArrayHelper.Sort<Person, string, object>(people, static p => p.Name);

        Assert.Equal(["Alice", "Bob", "Charlie"], people.Select(static p => p.Name).ToArray());
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

    [Fact]
    public void SortWithSelectorNullItemsThrowsArgumentNullException()
    {
        int[] values = [1, 2, 3];

        Assert.Throws<ArgumentNullException>(() => ArrayHelper.Sort<int, int, int>(null, static x => x, values));
    }

    [Fact]
    public void SortWithSelectorNullItemsArraysThrowsArgumentNullException()
    {
        int[] items = [1, 2, 3];

        Assert.Throws<ArgumentNullException>(() => ArrayHelper.Sort(items, static x => x, (int[][])null));
    }

    [Fact]
    public void SortWithNonGenericNullKeysThrowsArgumentNullException()
    {
        Array keys = null;
        Array values = new int[] { 1, 2, 3 };

        Assert.Throws<ArgumentNullException>(() => ArrayHelper.Sort(keys, values));
    }

    [Fact]
    public void SortWithNonGenericNullItemsArraysThrowsArgumentNullException()
    {
        Array keys = new int[] { 1, 2, 3 };

        Assert.Throws<ArgumentNullException>(() => ArrayHelper.Sort(keys, (Array[])null));
    }

    [Fact]
    public void SortWithNonGenericDifferentLengthArraysThrowsArgumentException()
    {
        Array keys = new int[] { 1, 2, 3 };
        Array values = new int[] { 1, 2 };

        Assert.Throws<ArgumentException>(() => ArrayHelper.Sort(keys, values));
    }

    [Fact]
    public void SortWithNonGenericSelectorNullKeysThrowsArgumentNullException()
    {
        Array keys = null;
        Array values = new int[] { 1, 2, 3 };

        Assert.Throws<ArgumentNullException>(() => ArrayHelper.Sort(keys, static x => x, values));
    }

    [Fact]
    public void SortWithNonGenericSelectorNullSelectorThrowsArgumentNullException()
    {
        Array keys = new int[] { 1, 2, 3 };
        Array values = new int[] { 1, 2, 3 };
        Func<object, object> selector = null;

        Assert.Throws<ArgumentNullException>(() => ArrayHelper.Sort(keys, selector, values));
    }

    [Fact]
    public void SortWithNonGenericSelectorNullItemsArraysThrowsArgumentNullException()
    {
        Array keys = new int[] { 1, 2, 3 };

        Assert.Throws<ArgumentNullException>(() => ArrayHelper.Sort(keys, static x => x, (Array[])null));
    }

    [Fact]
    public void SortDescendingWithNullKeysThrowsArgumentNullException()
    {
        int[] values = [1, 2, 3];

        Assert.Throws<ArgumentNullException>(() => ArrayHelper.SortDescending<int, int>(null, values));
    }

    [Fact]
    public void SortDescendingWithDifferentLengthArraysThrowsArgumentException()
    {
        int[] keys = [1, 2, 3];
        int[] values = [1, 2];

        Assert.Throws<ArgumentException>(() => ArrayHelper.SortDescending(keys, values));
    }

    [Fact]
    public void SortDescendingWithEmptyItemsArraysReturnsWithoutSorting()
    {
        int[] keys = [3, 1, 2];
        int[][] emptyArray = [];

        ArrayHelper.SortDescending(keys, emptyArray);

        Assert.Equal([3, 1, 2], keys);
    }

    [Fact]
    public void SortDescendingWithNullSelectorThrowsArgumentNullException()
    {
        int[] keys = [1, 2, 3];
        int[] values = [1, 2, 3];
        Func<int, int> selector = null;

        Assert.Throws<ArgumentNullException>(() => ArrayHelper.SortDescending(keys, selector, values));
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

    [Fact]
    public void SortWithNonGenericHeterogeneousArrayTypesSortsAllArraysCorrectly()
    {
        Array keys = new int[] { 3, 1, 2 };
        Array names = new string[] { "three", "one", "two" };
        Array scores = new double[] { 3.3, 1.1, 2.2 };

        ArrayHelper.Sort(keys, names, scores);

        Assert.Equal((int[])[1, 2, 3], keys);
        Assert.Equal((string[])["one", "two", "three"], names);
        Assert.Equal((double[])[1.1, 2.2, 3.3], scores);
    }

    #endregion

    #region Stability And Duplicate Keys Tests

    [Fact]
    public void SortWithDuplicateKeysPreservesKeyValuePairings()
    {
        int[] keys = [2, 1, 2, 1, 3];
        string[] values = ["a", "b", "c", "d", "e"];
        var originalKeys = keys.Copy();
        var originalValues = values.Copy();

        ArrayHelper.Sort(keys, values);

        AssertAscending(keys);
        AssertPairingsPreserved(originalKeys, originalValues, keys, values);
    }

    [Fact]
    public void SortDescendingWithDuplicateKeysPreservesKeyValuePairings()
    {
        int[] keys = [2, 1, 2, 1, 3];
        string[] values = ["a", "b", "c", "d", "e"];
        var originalKeys = keys.Copy();
        var originalValues = values.Copy();

        ArrayHelper.SortDescending(keys, values);

        AssertDescending(keys);
        AssertPairingsPreserved(originalKeys, originalValues, keys, values);
    }

    [Fact]
    public void SortWithAlreadyDescendingKeysSortsToAscending()
    {
        int[] keys = [4, 3, 2, 1];
        int[] values = [40, 30, 20, 10];

        ArrayHelper.Sort(keys, values);

        Assert.Equal([1, 2, 3, 4], keys);
        Assert.Equal([10, 20, 30, 40], values);
    }

    [Fact]
    public void SortDescendingWithAlreadyDescendingKeysLeavesArraysUnchanged()
    {
        int[] keys = [4, 3, 2, 1];
        int[] values = [40, 30, 20, 10];

        ArrayHelper.SortDescending(keys, values);

        Assert.Equal([4, 3, 2, 1], keys);
        Assert.Equal([40, 30, 20, 10], values);
    }

    #endregion

    #region Random Fuzz Tests

    [Fact]
    public void SortWithLargeRandomArrayProducesSortedResultWithPreservedPairings()
    {
        var random = new Random(20260818);
        var keys = new int[500];
        var values = new int[500];
        for (var i = 0; i < keys.Length; i++)
        {
            keys[i] = random.Next(-100, 100);
            values[i] = i;
        }
        var originalKeys = keys.Copy();
        var originalValues = values.Copy();

        ArrayHelper.Sort(keys, values);

        AssertAscending(keys);
        AssertPairingsPreserved(originalKeys, originalValues, keys, values);
    }

    [Fact]
    public void SortDescendingWithLargeRandomArrayProducesSortedResultWithPreservedPairings()
    {
        var random = new Random(20260818);
        var keys = new int[500];
        var values = new int[500];
        for (var i = 0; i < keys.Length; i++)
        {
            keys[i] = random.Next(-100, 100);
            values[i] = i;
        }
        var originalKeys = keys.Copy();
        var originalValues = values.Copy();

        ArrayHelper.SortDescending(keys, values);

        AssertDescending(keys);
        AssertPairingsPreserved(originalKeys, originalValues, keys, values);
    }

    #endregion

    #region SortDescending Already-Ascending Regression Tests

    [Fact]
    public void SortDescendingWithTwoAscendingKeysReversesToDescendingOrder()
    {
        int[] keys = [1, 2];
        int[] values = [10, 20];

        ArrayHelper.SortDescending(keys, values);

        Assert.Equal([2, 1], keys);
        Assert.Equal([20, 10], values);
    }

    [Fact]
    public void SortDescendingWithSelectorAndAlreadyAscendingKeysReversesToDescendingOrder()
    {
        var people = new Person[]
        {
            new Person { Name = "Alice", Age = 20 },
            new Person { Name = "Bob", Age = 30 },
            new Person { Name = "Charlie", Age = 40 }
        };

        ArrayHelper.SortDescending<Person, int, object>(people, static p => p.Age);

        Assert.Equal(["Charlie", "Bob", "Alice"], people.Select(static p => p.Name).ToArray());
    }

    [Fact]
    public void SortWithNonGenericDescendingAlreadyAscendingKeysReversesToDescendingOrder()
    {
        Array keys = new int[] { 1, 2, 3, 4 };
        Array values = new int[] { 10, 20, 30, 40 };

        ArrayHelper.SortDescending(keys, values);

        Assert.Equal((int[])[4, 3, 2, 1], keys);
        Assert.Equal((int[])[40, 30, 20, 10], values);
    }

    #endregion

    #region Test Helpers

    private static void AssertAscending<T>(T[] array, IComparer<T> comparer = null)
    {
        comparer ??= Comparer<T>.Default;
        for (var i = 1; i < array.Length; i++)
            Assert.True(comparer.Compare(array[i - 1], array[i]) <= 0);
    }

    private static void AssertDescending<T>(T[] array, IComparer<T> comparer = null)
    {
        comparer ??= Comparer<T>.Default;
        for (var i = 1; i < array.Length; i++)
            Assert.True(comparer.Compare(array[i - 1], array[i]) >= 0);
    }

    private static void AssertPairingsPreserved<TKey, TValue>(TKey[] originalKeys, TValue[] originalValues, TKey[] sortedKeys, TValue[] sortedValues)
    {
        var buckets = new Dictionary<TKey, List<TValue>>();
        for (var i = 0; i < originalKeys.Length; i++)
        {
            if (!buckets.TryGetValue(originalKeys[i], out var list))
                buckets[originalKeys[i]] = list = [];
            list.Add(originalValues[i]);
        }
        for (var i = 0; i < sortedKeys.Length; i++)
            Assert.True(buckets[sortedKeys[i]].Remove(sortedValues[i]));
        Assert.All(buckets.Values, static list => Assert.Empty(list));
    }

    #endregion
}

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
