using LaquaiLib.Collections.Enumeration;

namespace LaquaiLib.UnitTests.Collections.Enumeration;

public class FilterableEnumerableTests
{
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
}
