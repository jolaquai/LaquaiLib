using System.Collections.ObjectModel;

using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Extensions;

public class IEnumerableExtensionsIntoTests
{
    [Fact]
    public void IntoArrayWithListCopiesAllElements()
    {
        var source = new List<int> { 1, 2, 3 };
        var target = new int[3];

        var result = source.Into(target);

        Assert.Equal(3, result);
        Assert.Equal([1, 2, 3], target);
    }

    [Fact]
    public void IntoArrayWithListAndStartIndexCopiesAllElements()
    {
        var source = new List<int> { 1, 2, 3 };
        var target = new int[5];

        var result = source.Into(target, 2);

        Assert.Equal(3, result);
        Assert.Equal([0, 0, 1, 2, 3], target);
    }

    [Fact]
    public void IntoArrayWithListThrowsWhenTargetTooSmall()
    {
        var source = new List<int> { 1, 2, 3 };
        var target = new int[2];

        Assert.Throws<ArgumentException>(() => source.Into(target));
    }

    [Fact]
    public void IntoArrayWithICollectionCopiesAllElements()
    {
        ICollection<int> source = new Collection<int> { 1, 2, 3 };
        var target = new int[3];

        var result = source.Into(target);

        Assert.Equal(3, result);
        Assert.Equal([1, 2, 3], target);
    }

    [Fact]
    public void IntoArrayWithICollectionAndStartIndexCopiesAllElements()
    {
        ICollection<int> source = new Collection<int> { 1, 2, 3 };
        var target = new int[5];

        var result = source.Into(target, 2);

        Assert.Equal(3, result);
        Assert.Equal([0, 0, 1, 2, 3], target);
    }

    [Fact]
    public void IntoArrayWithICollectionThrowsWhenTargetTooSmall()
    {
        ICollection<int> source = new Collection<int> { 1, 2, 3 };
        var target = new int[2];

        Assert.Throws<ArgumentException>(() => source.Into(target));
    }

    [Fact]
    public void IntoArrayWithIReadOnlyListCopiesAllElements()
    {
        IReadOnlyList<int> source = new ReadOnlyCollection<int>([1, 2, 3]);
        var target = new int[3];

        var result = source.Into(target);

        Assert.Equal(3, result);
        Assert.Equal([1, 2, 3], target);
    }

    [Fact]
    public void IntoArrayWithIReadOnlyListAndStartIndexCopiesAllElements()
    {
        IReadOnlyList<int> source = new ReadOnlyCollection<int>([1, 2, 3]);
        var target = new int[5];

        var result = source.Into(target, 2);

        Assert.Equal(3, result);
        Assert.Equal([0, 0, 1, 2, 3], target);
    }

    [Fact]
    public void IntoArrayWithIReadOnlyListThrowsWhenTargetTooSmall()
    {
        IReadOnlyList<int> source = new ReadOnlyCollection<int>([1, 2, 3]);
        var target = new int[2];

        Assert.Throws<ArgumentException>(() => source.Into(target));
    }

    [Fact]
    public void IntoArrayWithIReadOnlyCollectionCopiesAllElements()
    {
        IReadOnlyCollection<int> source = new ReadOnlyCollection<int>([1, 2, 3]);
        var target = new int[3];

        var result = source.Into(target);

        Assert.Equal(3, result);
        Assert.Equal([1, 2, 3], target);
    }

    [Fact]
    public void IntoArrayWithIReadOnlyCollectionAndStartIndexCopiesAllElements()
    {
        IReadOnlyCollection<int> source = new ReadOnlyCollection<int>([1, 2, 3]);
        var target = new int[5];

        var result = source.Into(target, 2);

        Assert.Equal(3, result);
        Assert.Equal([0, 0, 1, 2, 3], target);
    }

    [Fact]
    public void IntoArrayWithIReadOnlyCollectionThrowsWhenTargetTooSmall()
    {
        IReadOnlyCollection<int> source = new ReadOnlyCollection<int>([1, 2, 3]);
        var target = new int[2];

        Assert.Throws<ArgumentException>(() => source.Into(target));
    }

    [Fact]
    public void IntoArrayWithGenericEnumerableCopiesAllElements()
    {
        var source = Enumerable.Range(1, 3);
        var target = new int[3];

        var result = source.Into(target);

        Assert.Equal(3, result);
        Assert.Equal([1, 2, 3], target);
    }

    [Fact]
    public void IntoArrayWithGenericEnumerableAndStartIndexCopiesAllElements()
    {
        var source = Enumerable.Range(1, 3);
        var target = new int[5];

        var result = source.Into(target, 2);

        Assert.Equal(3, result);
        Assert.Equal([0, 0, 1, 2, 3], target);
    }

    [Fact]
    public void IntoArrayWithGenericEnumerableThrowsWhenTargetTooSmall()
    {
        var source = Enumerable.Range(1, 3);
        var target = new int[2];

        Assert.Throws<ArgumentException>(() => source.Into(target));
    }

    [Fact]
    public void IntoArrayWithGenericEnumerableAndAllowUnsafeMutationCopiesElements()
    {
        var source = Enumerable.Range(1, 3);
        var target = new int[5];

        var result = source.Into(target, 1, true);

        Assert.Equal(3, result);
        Assert.Equal([0, 1, 2, 3, 0], target);
    }

    [Fact]
    public void IntoArrayWithGenericEnumerableAndAllowUnsafeMutationThrowsWhenTargetTooSmall()
    {
        var source = Enumerable.Range(1, 3);
        var target = new int[2];

        Assert.Throws<ArgumentException>(() => source.Into(target, 0, true));
    }

    [Fact]
    public void IntoListWithListCopiesAllElements()
    {
        var source = new List<int> { 1, 2, 3 };
        var target = new List<int> { 0, 0, 0 };

        var result = source.Into(target);

        Assert.Equal(3, result);
        Assert.Equal(new[] { 1, 2, 3 }, target);
    }

    [Fact]
    public void IntoListWithListAndStartIndexCopiesAllElements()
    {
        var source = new List<int> { 1, 2, 3 };
        var target = new List<int> { 9, 9, 9, 9, 9 };

        var result = source.Into(target, 2);

        Assert.Equal(3, result);
        Assert.Equal(new[] { 9, 9, 1, 2, 3 }, target);
    }

    [Fact]
    public void IntoListWithListResizesTargetWhenNeeded()
    {
        var source = new List<int> { 1, 2, 3 };
        var target = new List<int> { 0 };

        var result = source.Into(target);

        Assert.Equal(3, result);
        Assert.Equal(new[] { 1, 2, 3 }, target);
    }

    [Fact]
    public void IntoListWithListAndStartIndexResizesTargetWhenNeeded()
    {
        var source = new List<int> { 1, 2, 3 };
        var target = new List<int> { 9, 9 };

        var result = source.Into(target, 2);

        Assert.Equal(3, result);
        Assert.Equal(new[] { 9, 9, 1, 2, 3 }, target);
    }

    [Fact]
    public void IntoListWithIReadOnlyListCopiesAllElements()
    {
        IReadOnlyList<int> source = new ReadOnlyCollection<int>([1, 2, 3]);
        var target = new List<int> { 0, 0, 0 };

        var result = source.Into(target);

        Assert.Equal(3, result);
        Assert.Equal(new[] { 1, 2, 3 }, target);
    }

    [Fact]
    public void IntoListWithIReadOnlyListAndStartIndexCopiesAllElements()
    {
        IReadOnlyList<int> source = new ReadOnlyCollection<int>([1, 2, 3]);
        var target = new List<int> { 9, 9, 9, 9, 9 };

        var result = source.Into(target, 2);

        Assert.Equal(3, result);
        Assert.Equal(new[] { 9, 9, 1, 2, 3 }, target);
    }

    [Fact]
    public void IntoListWithIReadOnlyListResizesTargetWhenNeeded()
    {
        IReadOnlyList<int> source = new ReadOnlyCollection<int>([1, 2, 3]);
        var target = new List<int> { 0 };

        var result = source.Into(target);

        Assert.Equal(3, result);
        Assert.Equal(new[] { 1, 2, 3 }, target);
    }

    [Fact]
    public void IntoListWithIReadOnlyCollectionCopiesAllElements()
    {
        IReadOnlyCollection<int> source = new ReadOnlyCollection<int>([1, 2, 3]);
        var target = new List<int> { 0, 0, 0 };

        var result = source.Into(target);

        Assert.Equal(3, result);
        Assert.Equal(new[] { 1, 2, 3 }, target);
    }

    [Fact]
    public void IntoListWithIReadOnlyCollectionAndStartIndexCopiesAllElements()
    {
        IReadOnlyCollection<int> source = new ReadOnlyCollection<int>([1, 2, 3]);
        var target = new List<int> { 9, 9, 9, 9, 9 };

        var result = source.Into(target, 2);

        Assert.Equal(3, result);
        Assert.Equal(new[] { 9, 9, 1, 2, 3 }, target);
    }

    [Fact]
    public void IntoListWithIReadOnlyCollectionResizesTargetWhenNeeded()
    {
        IReadOnlyCollection<int> source = new ReadOnlyCollection<int>([1, 2, 3]);
        var target = new List<int> { 0 };

        var result = source.Into(target);

        Assert.Equal(3, result);
        Assert.Equal(new[] { 1, 2, 3 }, target);
    }

    [Fact]
    public void IntoListWithGenericEnumerableCopiesAllElements()
    {
        var source = Enumerable.Range(1, 3);
        var target = new List<int> { 0, 0, 0 };

        var result = source.Into(target);

        Assert.Equal(3, result);
        Assert.Equal(new[] { 1, 2, 3 }, target);
    }

    [Fact]
    public void IntoListWithGenericEnumerableAndStartIndexCopiesAllElements()
    {
        var source = Enumerable.Range(1, 3);
        var target = new List<int> { 9, 9, 9, 9, 9 };

        var result = source.Into(target, 2);

        Assert.Equal(3, result);
        Assert.Equal(new[] { 9, 9, 1, 2, 3 }, target);
    }

    [Fact]
    public void IntoListWithGenericEnumerableResizesTargetWhenNeeded()
    {
        var source = Enumerable.Range(1, 3);
        var target = new List<int> { 0 };

        var result = source.Into(target);

        Assert.Equal(3, result);
        Assert.Equal(new[] { 1, 2, 3 }, target);
    }

    [Fact]
    public void IntoListWithGenericEnumerableWithKnownCountCopiesAllElements()
    {
        IEnumerable<int> source = [1, 2, 3];
        var target = new List<int> { 0, 0, 0 };

        var result = source.Into(target);

        Assert.Equal(3, result);
        Assert.Equal(new[] { 1, 2, 3 }, target);
    }

    [Fact]
    public void AddToListAppendsAllElements()
    {
        var source = new List<int> { 1, 2, 3 };
        var target = new List<int> { 4, 5 };

        var result = source.AddTo(target);

        Assert.Equal(3, result);
        Assert.Equal(new[] { 4, 5, 1, 2, 3 }, target);
    }

    [Fact]
    public void AddToListWithEmptyTargetAppendsAllElements()
    {
        var source = new List<int> { 1, 2, 3 };
        var target = new List<int>();

        var result = source.AddTo(target);

        Assert.Equal(3, result);
        Assert.Equal(new[] { 1, 2, 3 }, target);
    }

    [Fact]
    public void IntoSpanWithListCopiesAllElements()
    {
        var source = new List<int> { 1, 2, 3 };
        var targetArray = new int[3];
        var target = targetArray.AsSpan();

        var result = source.Into(target);

        Assert.Equal(3, result);
        Assert.Equal([1, 2, 3], targetArray);
    }

    [Fact]
    public void IntoSpanWithListAndStartIndexCopiesAllElements()
    {
        var source = new List<int> { 1, 2, 3 };
        var targetArray = new int[5];
        var target = targetArray.AsSpan();

        var result = source.Into(target, 2);

        Assert.Equal(3, result);
        Assert.Equal([0, 0, 1, 2, 3], targetArray);
    }

    [Fact]
    public void IntoSpanWithListThrowsWhenTargetTooSmall()
    {
        var source = new List<int> { 1, 2, 3 };
        var targetArray = new int[2];

        Assert.Throws<ArgumentException>(() =>
        {
            var target = targetArray.AsSpan();
            return source.Into(target);
        });
    }

    [Fact]
    public void IntoSpanWithIReadOnlyListCopiesAllElements()
    {
        IReadOnlyList<int> source = new ReadOnlyCollection<int>([1, 2, 3]);
        var targetArray = new int[3];
        var target = targetArray.AsSpan();

        var result = source.Into(target);

        Assert.Equal(3, result);
        Assert.Equal([1, 2, 3], targetArray);
    }

    [Fact]
    public void IntoSpanWithIReadOnlyListAndStartIndexCopiesAllElements()
    {
        IReadOnlyList<int> source = new ReadOnlyCollection<int>([1, 2, 3]);
        var targetArray = new int[5];
        var target = targetArray.AsSpan();

        var result = source.Into(target, 2);

        Assert.Equal(3, result);
        Assert.Equal([0, 0, 1, 2, 3], targetArray);
    }

    [Fact]
    public void IntoSpanWithIReadOnlyListThrowsWhenTargetTooSmall()
    {
        IReadOnlyList<int> source = new ReadOnlyCollection<int>([1, 2, 3]);
        var targetArray = new int[2];

        Assert.Throws<ArgumentException>(() =>
        {
            var target = targetArray.AsSpan();
            return source.Into(target);
        });
    }

    [Fact]
    public void IntoSpanWithIReadOnlyCollectionCopiesAllElements()
    {
        IReadOnlyCollection<int> source = new ReadOnlyCollection<int>([1, 2, 3]);
        var targetArray = new int[3];
        var target = targetArray.AsSpan();

        var result = source.Into(target);

        Assert.Equal(3, result);
        Assert.Equal([1, 2, 3], targetArray);
    }

    [Fact]
    public void IntoSpanWithIReadOnlyCollectionAndStartIndexCopiesAllElements()
    {
        IReadOnlyCollection<int> source = new ReadOnlyCollection<int>([1, 2, 3]);
        var targetArray = new int[5];
        var target = targetArray.AsSpan();

        var result = source.Into(target, 2);

        Assert.Equal(3, result);
        Assert.Equal([0, 0, 1, 2, 3], targetArray);
    }

    [Fact]
    public void IntoSpanWithIReadOnlyCollectionThrowsWhenTargetTooSmall()
    {
        IReadOnlyCollection<int> source = new ReadOnlyCollection<int>([1, 2, 3]);
        var targetArray = new int[2];

        Assert.Throws<ArgumentException>(() =>
        {
            var target = targetArray.AsSpan();
            return source.Into(target);
        });
    }

    [Fact]
    public void IntoSpanWithGenericEnumerableCopiesAllElements()
    {
        var source = Enumerable.Range(1, 3);
        var targetArray = new int[3];
        var target = targetArray.AsSpan();

        var result = source.Into(target);

        Assert.Equal(3, result);
        Assert.Equal([1, 2, 3], targetArray);
    }

    [Fact]
    public void IntoSpanWithGenericEnumerableAndStartIndexCopiesAllElements()
    {
        var source = Enumerable.Range(1, 3);
        var targetArray = new int[5];
        var target = targetArray.AsSpan();

        var result = source.Into(target, 2);

        Assert.Equal(3, result);
        Assert.Equal([0, 0, 1, 2, 3], targetArray);
    }

    [Fact]
    public void IntoSpanWithGenericEnumerableThrowsWhenTargetTooSmall()
    {
        var source = Enumerable.Range(1, 3);
        var targetArray = new int[2];

        Assert.Throws<ArgumentException>(() =>
        {
            var target = targetArray.AsSpan();
            return source.Into(target);
        });
    }

    [Fact]
    public void IntoSpanWithGenericEnumerableAndAllowUnsafeMutationCopiesElements()
    {
        var source = Enumerable.Range(1, 3);
        var targetArray = new int[5];
        var target = targetArray.AsSpan();

        var result = source.Into(target, 1, true);

        Assert.Equal(3, result);
        Assert.Equal([0, 1, 2, 3, 0], targetArray);
    }

    [Fact]
    public void IntoSpanWithGenericEnumerableAndAllowUnsafeMutationThrowsWhenTargetTooSmall()
    {
        var source = Enumerable.Range(1, 3);
        var targetArray = new int[2];

        Assert.ThrowsAny<ArgumentException>(() =>
        {
            var target = targetArray.AsSpan();
            return source.Into(target, 0, true);
        });
    }

    [Fact]
    public void IntoDictionaryWithValueFactoryAddsAllElements()
    {
        var source = new List<string> { "one", "two", "three" };
        var target = new Dictionary<string, int>();

        var result = source.Into(target, static key => key.Length);

        Assert.Equal(3, result);
        Assert.Equal(3, target.Count);
        Assert.Equal(3, target["one"]);
        Assert.Equal(3, target["two"]);
        Assert.Equal(5, target["three"]);
    }

    [Fact]
    public void IntoDictionaryWithValueFactoryThrowsOnDuplicateKeys()
    {
        var source = new List<string> { "one", "two", "one" };
        var target = new Dictionary<string, int>();

        Assert.Throws<InvalidOperationException>(() => source.Into(target, key => key.Length));
    }

    [Fact]
    public void IntoDictionaryWithValueFactoryWithOverwriteOverwritesExistingValues()
    {
        var source = new List<string> { "one", "two", "one" };
        var target = new Dictionary<string, int>();

        var result = source.Into(target, static key => key.Length, true);

        Assert.Equal(3, result);
        Assert.Equal(2, target.Count);
        Assert.Equal(3, target["one"]);
        Assert.Equal(3, target["two"]);
    }

    [Fact]
    public void IntoDictionaryWithValueFactoryWithNonDictionaryTargetAddsAllElements()
    {
        var source = new List<string> { "one", "two", "three" };
        IDictionary<string, int> target = new SortedDictionary<string, int>();

        var result = source.Into(target, static key => key.Length);

        Assert.Equal(3, result);
        Assert.Equal(3, target.Count);
        Assert.Equal(3, target["one"]);
        Assert.Equal(3, target["two"]);
        Assert.Equal(5, target["three"]);
    }

    [Fact]
    public void IntoDictionaryWithKeyFactoryAddsAllElements()
    {
        var source = new List<int> { 1, 2, 3 };
        var target = new Dictionary<string, int>();

        var result = source.Into(target, static value => value.ToString());

        Assert.Equal(3, result);
        Assert.Equal(3, target.Count);
        Assert.Equal(1, target["1"]);
        Assert.Equal(2, target["2"]);
        Assert.Equal(3, target["3"]);
    }

    [Fact]
    public void IntoDictionaryWithKeyFactoryThrowsOnDuplicateKeys()
    {
        var source = new List<int> { 1, 2, 1 };
        var target = new Dictionary<string, int>();

        Assert.Throws<InvalidOperationException>(() => source.Into(target, value => value.ToString()));
    }

    [Fact]
    public void IntoDictionaryWithKeyFactoryWithOverwriteOverwritesExistingValues()
    {
        var source = new List<int> { 1, 2, 1 };
        var target = new Dictionary<string, int>();

        var result = source.Into(target, static value => value.ToString(), true);

        Assert.Equal(3, result);
        Assert.Equal(2, target.Count);
        Assert.Equal(1, target["1"]);
        Assert.Equal(2, target["2"]);
    }

    [Fact]
    public void IntoDictionaryWithKeyFactoryWithNonDictionaryTargetAddsAllElements()
    {
        var source = new List<int> { 1, 2, 3 };
        IDictionary<string, int> target = new SortedDictionary<string, int>();

        var result = source.Into(target, static value => value.ToString());

        Assert.Equal(3, result);
        Assert.Equal(3, target.Count);
        Assert.Equal(1, target["1"]);
        Assert.Equal(2, target["2"]);
        Assert.Equal(3, target["3"]);
    }

    [Fact]
    public void IntoICollectionWithArrayTargetCallsArrayOverload()
    {
        var source = new List<int> { 1, 2, 3 };
        ICollection<int> target = new int[3];

        var result = source.Into(target);

        Assert.Equal(3, result);
        Assert.Equal([1, 2, 3], target);
    }

    [Fact]
    public void IntoICollectionWithListTargetCallsListOverload()
    {
        var source = new List<int> { 1, 2, 3 };
        ICollection<int> target = [0, 0, 0];

        var result = source.Into(target);

        Assert.Equal(3, result);
        Assert.Equal([1, 2, 3], target);
    }

    [Fact]
    public void IntoICollectionWithGenericCollectionAddsAllElements()
    {
        var source = new List<int> { 1, 2, 3 };
        ICollection<int> target = new HashSet<int>();

        var result = source.Into(target);

        Assert.Equal(3, result);
        Assert.Equal(3, target.Count);
        Assert.Contains(1, target);
        Assert.Contains(2, target);
        Assert.Contains(3, target);
    }

    [Fact]
    public void IntoICollectionWithReadOnlyCollectionThrowsInvalidOperationException()
    {
        var source = new List<int> { 1, 2, 3 };
        ICollection<int> target = new ReadOnlyCollection<int>([]);

        Assert.Throws<InvalidOperationException>(() => source.Into(target));
    }
}
