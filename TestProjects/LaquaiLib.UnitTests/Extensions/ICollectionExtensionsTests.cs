using System.Collections.ObjectModel;

using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Extensions;

public class ICollectionExtensionTests
{
    #region KeepOnly
    private class CustomCollection<T> : ICollection<T>
    {
        private readonly List<T> _items = [];

        public int Count => _items.Count;
        public bool IsReadOnly => false;

        public void Add(T item) => _items.Add(item);
        public void Clear() => _items.Clear();
        public bool Contains(T item) => _items.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);
        public bool Remove(T item) => _items.Remove(item);
        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _items.GetEnumerator();
    }

    [Fact]
    public void ThrowsExceptionForReadOnlyCollection()
    {
        var readOnlyCollection = new ReadOnlyCollection<int>([1, 2, 3]);
        Assert.Throws<NotSupportedException>(() => readOnlyCollection.KeepOnly(i => i > 1));
    }

    [Fact]
    public void ReturnsUnchangedForEmptyCollection()
    {
        var emptyList = new List<int>();
        emptyList.KeepOnly(static i => i > 0);
        Assert.Empty(emptyList);
    }

    [Fact]
    public void FilterHashSet()
    {
        var hashSet = new HashSet<int> { 1, 2, 3, 4, 5 };
        hashSet.KeepOnly(static i => i % 2 == 0);
        Assert.Equal([2, 4], hashSet);
    }

    [Fact]
    public void FilterISet()
    {
        ISet<string> set = new SortedSet<string> { "apple", "banana", "cherry", "date" };
        set.KeepOnly(static s => s.Length > 5);
        Assert.Equal(new SortedSet<string> { "banana", "cherry" }, set);
    }

    [Fact]
    public void FilterList()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };
        list.KeepOnly(static i => i > 3);
        Assert.Equal([4, 5], list);
    }

    [Fact]
    public void FilterIList()
    {
        IList<char> charList = new Collection<char> { 'a', 'b', 'c', 'd' };
        charList.KeepOnly(static c => c > 'b');
        Assert.Equal(['c', 'd'], charList.ToArray());
    }

    [Fact]
    public void FilterGenericCollection()
    {
        var collection = new CustomCollection<double>
        {
            1.1,
            2.2,
            3.3,
            4.4
        };

        collection.KeepOnly(static d => d < 3.0);

        Assert.Equal(2, collection.Count);
        Assert.Contains(1.1, collection);
        Assert.Contains(2.2, collection);
    }

    [Fact]
    public void KeepAllItems()
    {
        var list = new List<int> { 1, 2, 3 };
        list.KeepOnly(static _ => true);
        Assert.Equal([1, 2, 3], list);
    }

    [Fact]
    public void RemoveAllItems()
    {
        var list = new List<int> { 1, 2, 3 };
        list.KeepOnly(static _ => false);
        Assert.Empty(list);
    }

    [Fact]
    public void ComplexPredicateWorks()
    {
        var numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        numbers.KeepOnly(static n => n is > 3 and < 8);
        Assert.Equal([4, 5, 6, 7], numbers);
    }
    #endregion
}
