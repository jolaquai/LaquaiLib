using LaquaiLib.UnsafeUtils.Accessors;

namespace LaquaiLib.UnitTests.UnsafeUtils.Accessors;

public class ListAccessorsTests
{
    [Fact]
    public void ItemsMatchesBackingArrayLength()
    {
        var list = new List<int>([1, 2, 3]);

        ref var items = ref ListAccessors<int>._items(list);

        Assert.Equal(list.Capacity, items.Length);
    }

    [Fact]
    public void SizeMatchesCount()
    {
        var list = new List<int>([1, 2, 3, 4]);

        Assert.Equal(list.Count, ListAccessors<int>._size(list));
    }

    [Fact]
    public void VersionChangesOnStructuralMutation()
    {
        var list = new List<int>([1, 2, 3]);

        var before = ListAccessors<int>._version(list);
        list.Add(4);
        var after = ListAccessors<int>._version(list);

        Assert.NotEqual(before, after);
    }
}
