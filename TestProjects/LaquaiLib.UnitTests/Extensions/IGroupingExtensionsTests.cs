using System.Collections.ObjectModel;

using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Extensions;

public class IGroupingExtensionsTests
{
    private class TestGrouping<TKey, TElement>(TKey key, IEnumerable<TElement> elements) : List<TElement>(elements), IGrouping<TKey, TElement>
    {
        public TKey Key { get; } = key;
    }

    [Fact]
    public void DeconstructSeparatesKeyAndElements()
    {
        var grouping = new TestGrouping<int, string>(1, ["a", "b", "c"]);

        grouping.Deconstruct(out var key, out var elements);

        Assert.Equal(1, key);
        Assert.Equal(["a", "b", "c"], elements);
    }

    [Fact]
    public void DeconstructWithTupleDeconstructorWorks()
    {
        var grouping = new TestGrouping<int, string>(1, ["a", "b", "c"]);

        (var key, var elements) = grouping;

        Assert.Equal(1, key);
        Assert.Equal(["a", "b", "c"], elements);
    }

    [Fact]
    public void ToListDictionaryCreatesCorrectDictionary()
    {
        var groupings = new List<IGrouping<int, string>>
        {
            new TestGrouping<int, string>(1, ["a", "b", "c"]),
            new TestGrouping<int, string>(2, ["d", "e"]),
            new TestGrouping<int, string>(3, ["f"])
        };

        var dictionary = groupings.ToListDictionary();

        Assert.Equal(3, dictionary.Count);
        Assert.Equal(new[] { "a", "b", "c" }, dictionary[1]);
        Assert.Equal(new[] { "d", "e" }, dictionary[2]);
        Assert.Equal(new[] { "f" }, dictionary[3]);
    }

    [Fact]
    public void ToListDictionaryWithEmptySource()
    {
        var groupings = new List<IGrouping<int, string>>();

        var dictionary = groupings.ToListDictionary();

        Assert.Empty(dictionary);
    }

    [Fact]
    public void ToListDictionaryReturnsAddableList()
    {
        var groupings = new List<IGrouping<int, string>>
        {
            new TestGrouping<int, string>(1, ["a", "b", "c"])
        };
        var dictionary = groupings.ToListDictionary();

        dictionary[1].Add("new");

        Assert.Equal(4, dictionary[1].Count);
        Assert.Contains("new", dictionary[1]);
    }

    [Fact]
    public void ToArrayDictionaryCreatesCorrectDictionary()
    {
        var groupings = new List<IGrouping<int, string>>
        {
            new TestGrouping<int, string>(1, ["a", "b", "c"]),
            new TestGrouping<int, string>(2, ["d", "e"]),
            new TestGrouping<int, string>(3, ["f"])
        };

        var dictionary = groupings.ToArrayDictionary();

        Assert.Equal(3, dictionary.Count);
        Assert.Equal(new[] { "a", "b", "c" }, dictionary[1]);
        Assert.Equal(new[] { "d", "e" }, dictionary[2]);
        Assert.Equal(new[] { "f" }, dictionary[3]);
    }

    [Fact]
    public void ToArrayDictionaryWithEmptySource()
    {
        var groupings = new List<IGrouping<int, string>>();

        var dictionary = groupings.ToArrayDictionary();

        Assert.Empty(dictionary);
    }

    [Fact]
    public void ToDictionaryWithListCreatesCorrectDictionary()
    {
        var groupings = new List<IGrouping<int, string>>
        {
            new TestGrouping<int, string>(1, ["a", "b", "c"]),
            new TestGrouping<int, string>(2, ["d", "e"]),
            new TestGrouping<int, string>(3, ["f"])
        };

        var dictionary = groupings.ToDictionary<int, List<string>, string>();

        Assert.Equal(3, dictionary.Count);
        Assert.Equal(new[] { "a", "b", "c" }, dictionary[1]);
        Assert.Equal(new[] { "d", "e" }, dictionary[2]);
        Assert.Equal(new[] { "f" }, dictionary[3]);
    }

    [Fact]
    public void ToDictionaryWithHashSetCreatesCorrectDictionary()
    {
        var groupings = new List<IGrouping<int, string>>
        {
            new TestGrouping<int, string>(1, ["a", "b", "c"]),
            new TestGrouping<int, string>(2, ["d", "e"]),
            new TestGrouping<int, string>(3, ["f"])
        };

        var dictionary = groupings.ToDictionary<int, HashSet<string>, string>();

        Assert.Equal(3, dictionary.Count);
        Assert.Equal(["a", "b", "c"], dictionary[1]);
        Assert.Equal(["d", "e"], dictionary[2]);
        Assert.Equal(["f"], dictionary[3]);
    }

    [Fact]
    public void ToDictionaryWithObservableCollection()
    {
        var groupings = new List<IGrouping<string, int>>
        {
            new TestGrouping<string, int>("one", [1, 2, 3]),
            new TestGrouping<string, int>("two", [4, 5]),
            new TestGrouping<string, int>("three", [6])
        };

        var dictionary = groupings.ToDictionary<string, ObservableCollection<int>, int>();

        Assert.Equal(3, dictionary.Count);
        Assert.Equal(new[] { 1, 2, 3 }, dictionary["one"]);
        Assert.Equal(new[] { 4, 5 }, dictionary["two"]);
        Assert.Equal(new[] { 6 }, dictionary["three"]);
    }

    [Fact]
    public void ToDictionaryWithEmptySource()
    {
        var groupings = new List<IGrouping<int, string>>();

        var dictionary = groupings.ToDictionary<int, List<string>, string>();

        Assert.Empty(dictionary);
    }

    [Fact]
    public void ToListDictionaryWithSelector()
    {
        var groupings = new List<IGrouping<int, string>>
        {
            new TestGrouping<int, string>(1, ["a", "b", "c"]),
            new TestGrouping<int, string>(2, ["d", "e"]),
            new TestGrouping<int, string>(3, ["f"])
        };

        var dictionary = groupings.ToListDictionary(s => s.ToUpper());

        Assert.Equal(3, dictionary.Count);
        Assert.Equal(new[] { "A", "B", "C" }, dictionary[1]);
        Assert.Equal(new[] { "D", "E" }, dictionary[2]);
        Assert.Equal(new[] { "F" }, dictionary[3]);
    }

    [Fact]
    public void ToListDictionaryWithIntSelector()
    {
        var groupings = new List<IGrouping<int, string>>
        {
            new TestGrouping<int, string>(1, ["a", "b", "c"]),
            new TestGrouping<int, string>(2, ["d", "e"]),
            new TestGrouping<int, string>(3, ["f"])
        };

        var dictionary = groupings.ToListDictionary(s => s.Length);

        Assert.Equal(3, dictionary.Count);
        Assert.Equal(new[] { 1, 1, 1 }, dictionary[1]);
        Assert.Equal(new[] { 1, 1 }, dictionary[2]);
        Assert.Equal(new[] { 1 }, dictionary[3]);
    }

    [Fact]
    public void ToListDictionaryWithSelectorAndEmptySource()
    {
        var groupings = new List<IGrouping<int, string>>();

        var dictionary = groupings.ToListDictionary(s => s.ToUpper());

        Assert.Empty(dictionary);
    }

    [Fact]
    public void ToArrayDictionaryWithSelector()
    {
        var groupings = new List<IGrouping<int, string>>
        {
            new TestGrouping<int, string>(1, ["a", "b", "c"]),
            new TestGrouping<int, string>(2, ["d", "e"]),
            new TestGrouping<int, string>(3, ["f"])
        };

        var dictionary = groupings.ToArrayDictionary(s => s.ToUpper());

        Assert.Equal(3, dictionary.Count);
        Assert.Equal(new[] { "A", "B", "C" }, dictionary[1]);
        Assert.Equal(new[] { "D", "E" }, dictionary[2]);
        Assert.Equal(new[] { "F" }, dictionary[3]);
    }

    [Fact]
    public void ToArrayDictionaryWithIntSelector()
    {
        var groupings = new List<IGrouping<int, string>>
        {
            new TestGrouping<int, string>(1, ["a", "b", "c"]),
            new TestGrouping<int, string>(2, ["d", "e"]),
            new TestGrouping<int, string>(3, ["f"])
        };

        var dictionary = groupings.ToArrayDictionary(s => s.Length);

        Assert.Equal(3, dictionary.Count);
        Assert.Equal([1, 1, 1], dictionary[1]);
        Assert.Equal([1, 1], dictionary[2]);
        Assert.Equal([1], dictionary[3]);
    }

    [Fact]
    public void ToArrayDictionaryWithSelectorAndEmptySource()
    {
        var groupings = new List<IGrouping<int, string>>();

        var dictionary = groupings.ToArrayDictionary(s => s.ToUpper());

        Assert.Empty(dictionary);
    }

    [Fact]
    public void ToDictionaryWithSelector()
    {
        var groupings = new List<IGrouping<int, string>>
        {
            new TestGrouping<int, string>(1, ["a", "b", "c"]),
            new TestGrouping<int, string>(2, ["d", "e"]),
            new TestGrouping<int, string>(3, ["f"])
        };

        var dictionary = groupings.ToDictionary<int, List<string>, string, string>(s => s.ToUpper());

        Assert.Equal(3, dictionary.Count);
        Assert.Equal(new[] { "A", "B", "C" }, dictionary[1]);
        Assert.Equal(new[] { "D", "E" }, dictionary[2]);
        Assert.Equal(new[] { "F" }, dictionary[3]);
    }

    [Fact]
    public void ToDictionaryWithHashSetAndSelector()
    {
        var groupings = new List<IGrouping<int, string>>
        {
            new TestGrouping<int, string>(1, ["a", "b", "c"]),
            new TestGrouping<int, string>(2, ["d", "e"]),
            new TestGrouping<int, string>(3, ["f"])
        };

        var dictionary = groupings.ToDictionary<int, HashSet<string>, string, string>(s => s.ToUpper());

        Assert.Equal(3, dictionary.Count);
        Assert.Equal(["A", "B", "C"], dictionary[1]);
        Assert.Equal(["D", "E"], dictionary[2]);
        Assert.Equal(["F"], dictionary[3]);
    }

    [Fact]
    public void ToDictionaryWithObservableCollectionAndSelector()
    {
        var groupings = new List<IGrouping<string, int>>
        {
            new TestGrouping<string, int>("one", [1, 2, 3]),
            new TestGrouping<string, int>("two", [4, 5]),
            new TestGrouping<string, int>("three", [6])
        };

        var dictionary = groupings.ToDictionary<string, ObservableCollection<string>, int, string>(i => i.ToString());

        Assert.Equal(3, dictionary.Count);
        Assert.Equal(new[] { "1", "2", "3" }, dictionary["one"]);
        Assert.Equal(new[] { "4", "5" }, dictionary["two"]);
        Assert.Equal(new[] { "6" }, dictionary["three"]);
    }

    [Fact]
    public void ToDictionaryWithSelectorAndEmptySource()
    {
        var groupings = new List<IGrouping<int, string>>();

        var dictionary = groupings.ToDictionary<int, List<string>, string, string>(s => s.ToUpper());

        Assert.Empty(dictionary);
    }

    [Fact]
    public void DictionaryEntriesCorrespondToGroupings()
    {
        var groupings = new List<IGrouping<int, string>>
        {
            new TestGrouping<int, string>(1, ["a", "b", "c"]),
            new TestGrouping<int, string>(2, ["d", "e"]),
            new TestGrouping<int, string>(3, ["f"])
        };

        var dictionary = groupings.ToListDictionary();

        Assert.Equal(groupings.Count, dictionary.Count);
        foreach (var grouping in groupings)
        {
            Assert.True(dictionary.ContainsKey(grouping.Key));
            Assert.Equal(grouping, dictionary[grouping.Key]);
        }
    }

    [Fact]
    public void ToDictionaryWithDuplicateStringsInHashSet()
    {
        var groupings = new List<IGrouping<int, string>>
        {
            new TestGrouping<int, string>(1, ["a", "a", "b"])
        };

        var dictionary = groupings.ToDictionary<int, HashSet<string>, string>();

        Assert.Equal(1, dictionary.Count);
        Assert.Equal(2, dictionary[1].Count);
        Assert.Contains("a", dictionary[1]);
        Assert.Contains("b", dictionary[1]);
    }

    [Fact]
    public void ToListDictionaryWithNullableElements()
    {
        var groupings = new List<IGrouping<int, string>>
        {
            new TestGrouping<int, string>(1, ["a", null, "c"])
        };

        var dictionary = groupings.ToListDictionary();

        Assert.Equal(3, dictionary[1].Count);
        Assert.Equal("a", dictionary[1][0]);
        Assert.Null(dictionary[1][1]);
        Assert.Equal("c", dictionary[1][2]);
    }
}
