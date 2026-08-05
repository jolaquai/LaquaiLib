using LaquaiLib.Collections.Enumeration;

namespace LaquaiLib.UnitTests.Collections.Enumeration;

public class FilterableEnumerableTests
{
    private static List<T> Drain<T>(FilterableEnumerable<T> enumerable)
    {
        var drained = new List<T>();
        foreach (var item in enumerable)
        {
            drained.Add(item);
        }
        return drained;
    }

    [Fact]
    public void FilterWorks()
    {
        IEnumerable<int> ints = [1, 2, 3, 4, 5];
        var enumerable = new FilterableEnumerable<int>(ints, static i => i % 2 == 0);
        using var enumerator = enumerable.GetEnumerator();

        Assert.True(enumerator.MoveNext());
        Assert.Equal(2, enumerator.Current);
        Assert.True(enumerator.MoveNext());
        Assert.Equal(4, enumerator.Current);
        Assert.False(enumerator.MoveNext());
    }
    [Fact]
    public void NullFilterLeavesEnumeratorUnchanged()
    {
        IEnumerable<int> ints = [1, 2, 3];
        var enumerable = new FilterableEnumerable<int>(ints, (Func<int, bool>)null);
        using var filteredEnumerator = enumerable.GetEnumerator();
        using var enumerator = ints.GetEnumerator();

        Assert.True(filteredEnumerator.MoveNext());
        Assert.True(enumerator.MoveNext());
        Assert.Equal(enumerator.Current, filteredEnumerator.Current);
        Assert.True(filteredEnumerator.MoveNext());
        Assert.True(enumerator.MoveNext());
        Assert.Equal(enumerator.Current, filteredEnumerator.Current);
        Assert.True(filteredEnumerator.MoveNext());
        Assert.True(enumerator.MoveNext());
        Assert.Equal(enumerator.Current, filteredEnumerator.Current);
        Assert.False(filteredEnumerator.MoveNext());
        Assert.False(enumerator.MoveNext());
    }

    [Fact]
    public void IndexedFilterWorks()
    {
        IEnumerable<int> ints = [10, 20, 30, 40, 50];
        var enumerable = new FilterableEnumerable<int>(ints, static (i, index) => index % 2 == 0);

        Assert.Equal([10, 30, 50], Drain(enumerable));
    }
    [Fact]
    public void IndexedFilterReceivesSequentialSourceIndices()
    {
        IEnumerable<string> items = ["a", "b", "c"];
        var seen = new List<(string Item, int Index)>();
        var enumerable = new FilterableEnumerable<string>(items, (item, index) =>
        {
            seen.Add((item, index));
            return true;
        });

        Assert.Equal(["a", "b", "c"], Drain(enumerable));
        (string Item, int Index)[] expected = [("a", 0), ("b", 1), ("c", 2)];
        Assert.Equal(expected, seen);
    }
    [Fact]
    public void NullIndexedFilterLeavesEnumeratorUnchanged()
    {
        IEnumerable<int> ints = [1, 2, 3];
        var enumerable = new FilterableEnumerable<int>(ints, (Func<int, int, bool>)null);

        Assert.Equal([1, 2, 3], Drain(enumerable));
    }

    [Fact]
    public void SingleArgumentConstructorEnumeratesAllItems()
    {
        IEnumerable<int> ints = [1, 2, 3];
        var enumerable = new FilterableEnumerable<int>(ints);

        Assert.Equal([1, 2, 3], Drain(enumerable));
    }
    [Fact]
    public void ParameterlessConstructorEnumeratesNothing()
    {
        var enumerable = new FilterableEnumerable<int>();

        Assert.Empty(Drain(enumerable));
    }
    [Fact]
    public void DefaultInstanceEnumeratesNothing()
    {
        var enumerable = default(FilterableEnumerable<int>);

        Assert.Empty(Drain(enumerable));
    }
    [Fact]
    public void NullSourceWithNullFilterEnumeratesNothing()
    {
        var enumerable = new FilterableEnumerable<int>(null, (Func<int, bool>)null);

        Assert.Empty(Drain(enumerable));
    }
    [Fact]
    public void NullSourceWithFilterThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(static () => _ = new FilterableEnumerable<int>(null, static i => i > 0));
    }

    [Fact]
    public void EmptySourceEnumeratesNothing()
    {
        IEnumerable<int> ints = [];
        var enumerable = new FilterableEnumerable<int>(ints, static i => i > 0);

        Assert.Empty(Drain(enumerable));
    }
    [Fact]
    public void FilterMatchingNothingEnumeratesNothing()
    {
        IEnumerable<int> ints = [1, 2, 3];
        var enumerable = new FilterableEnumerable<int>(ints, static i => i > 100);

        Assert.Empty(Drain(enumerable));
    }

    [Fact]
    public void GetEnumeratorReturnsIndependentEnumerators()
    {
        IEnumerable<int> ints = [1, 2, 3];
        var enumerable = new FilterableEnumerable<int>(ints);
        using var first = enumerable.GetEnumerator();
        using var second = enumerable.GetEnumerator();

        Assert.True(first.MoveNext());
        Assert.True(first.MoveNext());
        Assert.Equal(2, first.Current);
        Assert.True(second.MoveNext());
        Assert.Equal(1, second.Current);
    }
    [Fact]
    public void EnumerationIsNotCachedBetweenCalls()
    {
        var source = new List<int> { 1, 2 };
        var enumerable = new FilterableEnumerable<int>(source);

        Assert.Equal([1, 2], Drain(enumerable));
        source.Add(3);
        Assert.Equal([1, 2, 3], Drain(enumerable));
    }
    [Fact]
    public void FilterIsEvaluatedLazilyOnEachEnumeration()
    {
        var evaluations = 0;
        var source = new List<int> { 1, 2, 3 };
        var enumerable = new FilterableEnumerable<int>(source, i =>
        {
            evaluations++;
            return i % 2 == 1;
        });

        Assert.Equal(0, evaluations);
        Assert.Equal([1, 3], Drain(enumerable));
        Assert.Equal(3, evaluations);
        Assert.Equal([1, 3], Drain(enumerable));
        Assert.Equal(6, evaluations);
    }
}
