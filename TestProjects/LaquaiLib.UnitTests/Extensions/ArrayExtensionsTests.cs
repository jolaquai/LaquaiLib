using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Extensions;

public class ArrayExtensionsTests
{
    #region AsEnumerable Tests

    [Fact]
    public void AsEnumerableOneDimensionalArrayReturnsCorrectEnumeration()
    {
        Array array = new int[] { 1, 2, 3, 4, 5 };

        var enumerable = array.AsEnumerable<int>();

        Assert.Equal([1, 2, 3, 4, 5], enumerable.ToArray());
    }

    [Fact]
    public void AsEnumerableTwoDimensionalArrayReturnsCorrectEnumeration()
    {
        var array = new int[,] { { 1, 2, 3 }, { 4, 5, 6 } };

        var enumerable = array.AsEnumerable<int>();

        Assert.Equal([1, 2, 3, 4, 5, 6], enumerable.ToArray());
    }

    [Fact]
    public void AsEnumerableThreeDimensionalArrayReturnsCorrectEnumeration()
    {
        var array = new int[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } };

        var enumerable = array.AsEnumerable<int>();

        Assert.Equal([1, 2, 3, 4, 5, 6, 7, 8], enumerable.ToArray());
    }

    [Fact]
    public void AsEnumerableEmptyArrayReturnsEmptyEnumeration()
    {
        var array = Array.CreateInstance(typeof(int), 0);

        var enumerable = array.AsEnumerable<int>();

        Assert.Empty(enumerable);
    }

    #endregion

    #region SpanProvider Tests

    [Fact]
    public void GetSpanProviderOneDimensionalArrayReturnsValidProvider()
    {
        Array array = new int[] { 1, 2, 3, 4, 5 };

        using var provider = array.GetSpanProvider<int>();
        var span = provider.Span;

        Assert.Equal(5, span.Length);
        Assert.Equal(1, span[0]);
        Assert.Equal(5, span[4]);
    }

    [Fact]
    public void GetReadOnlySpanProviderTwoDimensionalArrayReturnsValidProvider()
    {
        var array = new int[,] { { 1, 2, 3 }, { 4, 5, 6 } };

        using var provider = array.GetReadOnlySpanProvider<int>();
        var span = provider.ReadOnlySpan;

        Assert.Equal(6, span.Length);
        Assert.Equal(1, span[0]);
        Assert.Equal(6, span[5]);
    }

    [Fact]
    public void TryGetSpanOneDimensionalArrayReturnsSpanWithoutProvider()
    {
        Array array = new int[] { 1, 2, 3, 4, 5 };

        var result = array.TryGetSpan<int>(out var provider, out var span);

        Assert.True(result);
        Assert.Null(provider);
        Assert.Equal(5, span.Length);
        Assert.Equal(1, span[0]);
        Assert.Equal(5, span[4]);
    }

    [Fact]
    public void TryGetSpanTwoDimensionalArrayReturnsSpanWithProvider()
    {
        var array = new int[,] { { 1, 2, 3 }, { 4, 5, 6 } };

        var result = array.TryGetSpan<int>(out var provider, out var span);

        try
        {
            Assert.True(result);
            Assert.NotNull(provider);
            Assert.Equal(6, span.Length);
            Assert.Equal(1, span[0]);
            Assert.Equal(6, span[5]);
        }
        finally
        {
            provider?.Dispose();
        }
    }

    [Fact]
    public void TryGetReadOnlySpanOneDimensionalArrayReturnsSpanWithoutProvider()
    {
        Array array = new int[] { 1, 2, 3, 4, 5 };

        var result = array.TryGetReadOnlySpan<int>(out var provider, out var span);

        Assert.True(result);
        Assert.Null(provider);
        Assert.Equal(5, span.Length);
        Assert.Equal(1, span[0]);
        Assert.Equal(5, span[4]);
    }

    [Fact]
    public void TryGetReadOnlySpanTwoDimensionalArrayReturnsSpanWithProvider()
    {
        var array = new int[,] { { 1, 2, 3 }, { 4, 5, 6 } };

        var result = array.TryGetReadOnlySpan<int>(out var provider, out var span);

        try
        {
            Assert.True(result);
            Assert.NotNull(provider);
            Assert.Equal(6, span.Length);
            Assert.Equal(1, span[0]);
            Assert.Equal(6, span[5]);
        }
        finally
        {
            provider?.Dispose();
        }
    }

    [Fact]
    public void TryGetSpanInvalidTypeCastReturnsFalse()
    {
        Array array = new string[] { "one", "two", "three" };

        var result = array.TryGetSpan<int>(out var provider, out var span);

        Assert.False(result);
        Assert.Null(provider);
        Assert.Equal(default, span);
    }

    #endregion

    #region MultiDimCopyTo Tests

    [Fact]
    public void MultiDimCopyToOneDimensionalArraysCopiesCorrectly()
    {
        int[] source = [1, 2, 3, 4, 5];
        var destination = new int[5];

        source.MultiDimCopyTo<int>(destination);

        Assert.Equal([1, 2, 3, 4, 5], destination);
    }

    [Fact]
    public void MultiDimCopyToWithIndicesCopiesCorrectly()
    {
        int[] source = [1, 2, 3, 4, 5];
        var destination = new int[5];

        source.MultiDimCopyTo<int>(1, destination, 2, 2);

        Assert.Equal([0, 0, 2, 3, 0], destination);
    }

    [Fact]
    public void MultiDimCopyToTwoDimensionalArraysCopiesCorrectly()
    {
        int[,] source = { { 1, 2, 3 }, { 4, 5, 6 } };
        var destination = new int[2, 3];

        source.MultiDimCopyTo<int>(destination);

        Assert.Equal(1, destination[0, 0]);
        Assert.Equal(2, destination[0, 1]);
        Assert.Equal(3, destination[0, 2]);
        Assert.Equal(4, destination[1, 0]);
        Assert.Equal(5, destination[1, 1]);
        Assert.Equal(6, destination[1, 2]);
    }

    [Fact]
    public void MultiDimCopyToInvalidArraysThrowsArgumentException()
    {
        int[] source = [1, 2, 3, 4, 5];
        var destination = new string[5];

        Assert.Throws<ArgumentException>(() => source.MultiDimCopyTo<int>(destination));
    }

    [Fact]
    public void MultiDimCopyToInvalidTypeThrowsArgumentException()
    {
        int[] source = [1, 2, 3, 4, 5];
        var destination = new int[5];

        Assert.Throws<ArgumentException>(() => source.MultiDimCopyTo<string>(destination));
    }

    [Fact]
    public void MultiDimCopyToNegativeSourceIndexThrowsArgumentOutOfRangeException()
    {
        int[] source = [1, 2, 3, 4, 5];
        var destination = new int[5];

        Assert.Throws<ArgumentOutOfRangeException>(() => source.MultiDimCopyTo<int>(-1, destination, 0, 3));
    }

    [Fact]
    public void MultiDimCopyToNegativeDestinationIndexThrowsArgumentOutOfRangeException()
    {
        int[] source = [1, 2, 3, 4, 5];
        var destination = new int[5];

        Assert.Throws<ArgumentOutOfRangeException>(() => source.MultiDimCopyTo<int>(0, destination, -1, 3));
    }

    [Fact]
    public void MultiDimCopyToSourceRangeExceedsArrayThrowsArgumentOutOfRangeException()
    {
        int[] source = [1, 2, 3];
        var destination = new int[5];

        Assert.Throws<ArgumentOutOfRangeException>(() => source.MultiDimCopyTo<int>(1, destination, 0, 3));
    }

    [Fact]
    public void MultiDimCopyToDestinationRangeExceedsArrayThrowsArgumentOutOfRangeException()
    {
        int[] source = [1, 2, 3, 4, 5];
        var destination = new int[3];

        Assert.Throws<ArgumentOutOfRangeException>(() => source.MultiDimCopyTo<int>(0, destination, 1, 3));
    }

    [Fact]
    public void MultiDimCopyToLengthExceedsSourceThrowsArgumentOutOfRangeException()
    {
        int[] source = [1, 2, 3];
        var destination = new int[5];

        Assert.Throws<ArgumentOutOfRangeException>(() => source.MultiDimCopyTo<int>(0, destination, 0, 4));
    }

    [Fact]
    public void MultiDimCopyToLengthExceedsDestinationThrowsArgumentOutOfRangeException()
    {
        int[] source = [1, 2, 3, 4, 5];
        var destination = new int[3];

        Assert.Throws<ArgumentOutOfRangeException>(() => source.MultiDimCopyTo<int>(0, destination, 0, 4));
    }

    #endregion

    #region MultiDimConstrainedCopyTo Tests

    [Fact]
    public void MultiDimConstrainedCopyToOneDimensionalArraysCopiesCorrectly()
    {
        int[] source = [1, 2, 3, 4, 5];
        var destination = new int[5];

        source.MultiDimConstrainedCopyTo<int>(destination);

        Assert.Equal([1, 2, 3, 4, 5], destination);
    }

    [Fact]
    public void MultiDimConstrainedCopyToWhenExceptionThrownRestoresOriginalData()
    {

        int[] source = [1, 2, 3, 4, 5];
        int[] destination = [10, 20, 30, 40, 50];

        Assert.Throws<ArgumentOutOfRangeException>(() => source.MultiDimConstrainedCopyTo<int>(4, destination, 0, 2));

        Assert.Equal([10, 20, 30, 40, 50], destination);
    }

    #endregion

    #region TryGetAt Tests

    [Fact]
    public void TryGetAtValidIndexReturnsTrue()
    {
        int[] array = [1, 2, 3, 4, 5];

        var result = array.TryGetAt(2, out var value);

        Assert.True(result);
        Assert.Equal(3, value);
    }

    [Fact]
    public void TryGetAtNegativeIndexReturnsFalse()
    {
        int[] array = [1, 2, 3, 4, 5];

        var result = array.TryGetAt(-1, out var value);

        Assert.False(result);
        Assert.Equal(default, value);
    }

    [Fact]
    public void TryGetAtIndexOutOfRangeReturnsFalse()
    {
        int[] array = [1, 2, 3, 4, 5];

        var result = array.TryGetAt(5, out var value);

        Assert.False(result);
        Assert.Equal(default, value);
    }

    [Fact]
    public void TryGetAtEmptyArrayReturnsFalse()
    {
        var array = Array.Empty<int>();

        var result = array.TryGetAt(0, out var value);

        Assert.False(result);
        Assert.Equal(default, value);
    }

    [Fact]
    public void TryGetAtWithReferenceTypeGetsCorrectValueOrDefault()
    {
        string[] array = ["one", "two", "three"];

        var validResult = array.TryGetAt(1, out var validValue);
        var invalidResult = array.TryGetAt(3, out var invalidValue);

        Assert.True(validResult);
        Assert.Equal("two", validValue);

        Assert.False(invalidResult);
        Assert.Null(invalidValue);
    }

    #endregion

    #region GetAtOrDefault Tests

    [Fact]
    public void GetAtOrDefaultValidIndexReturnsValue()
    {
        int[] array = [1, 2, 3, 4, 5];

        var value = array.GetAtOrDefault(2);

        Assert.Equal(3, value);
    }

    [Fact]
    public void GetAtOrDefaultInvalidIndexReturnsDefaultValueType()
    {
        int[] array = [1, 2, 3, 4, 5];

        var value = array.GetAtOrDefault(10);

        Assert.Equal(default, value);
    }

    [Fact]
    public void GetAtOrDefaultInvalidIndexReturnsSpecifiedDefault()
    {
        int[] array = [1, 2, 3, 4, 5];

        var value = array.GetAtOrDefault(10, 999);

        Assert.Equal(999, value);
    }

    [Fact]
    public void GetAtOrDefaultWithReferenceTypeReturnsCorrectValue()
    {
        string[] array = ["one", "two", "three"];

        var validValue = array.GetAtOrDefault(1);
        var invalidValue = array.GetAtOrDefault(3);
        var customDefault = array.GetAtOrDefault(3, "custom");

        Assert.Equal("two", validValue);
        Assert.Null(invalidValue);
        Assert.Equal("custom", customDefault);
    }

    #endregion

    #region SequenceEqual Tests

    [Fact]
    public void SequenceEqualIdenticalArraysReturnsTrue()
    {
        int[] first = [1, 2, 3, 4, 5];
        int[] second = [1, 2, 3, 4, 5];

        var result = first.SequenceEqual<int>(second);

        Assert.True(result);
    }

    [Fact]
    public void SequenceEqualDifferentArraysReturnsFalse()
    {
        int[] first = [1, 2, 3, 4, 5];
        int[] second = [1, 2, 3, 5, 4];

        var result = first.SequenceEqual<int>(second);

        Assert.False(result);
    }

    [Fact]
    public void SequenceEqualDifferentLengthArraysReturnsFalse()
    {
        int[] first = [1, 2, 3, 4, 5];
        int[] second = [1, 2, 3, 4];

        var result = first.SequenceEqual<int>(second);

        Assert.False(result);
    }

    [Fact]
    public void SequenceEqualDifferentTypeArraysReturnsFalse()
    {
        int[] first = [1, 2, 3];
        double[] second = [1.0, 2.0, 3.0];

        var result = first.SequenceEqual<int>(second);

        Assert.False(result);
    }

    [Fact]
    public void SequenceEqualMultidimensionalSameElementsReturnsTrue()
    {
        int[,] first = { { 1, 2 }, { 3, 4 } };
        int[,] second = { { 1, 2 }, { 3, 4 } };

        var result = first.SequenceEqual<int>(second);

        Assert.True(result);
    }

    [Fact]
    public void SequenceEqualMultidimensionalDifferentElementsReturnsFalse()
    {
        int[,] first = { { 1, 2 }, { 3, 4 } };
        int[,] second = { { 1, 2 }, { 3, 5 } };

        var result = first.SequenceEqual<int>(second);

        Assert.False(result);
    }

    [Fact]
    public void SequenceEqualDifferentDimensionsButSameElementsReturnsTrue()
    {
        var first = new int[2, 2] { { 1, 2 }, { 3, 4 } };
        var second = new int[1, 4] { { 1, 2, 3, 4 } };

        var result = first.SequenceEqual<int>(second);

        Assert.True(result);
    }

    [Fact]
    public void SequenceEqualWithCustomComparerUsesComparer()
    {
        string[] first = ["a", "b", "c"];
        string[] second = ["A", "B", "C"];
        var comparer = StringComparer.OrdinalIgnoreCase;

        var result = first.SequenceEqual<string>(second, comparer);

        Assert.True(result);
    }

    #endregion
}