using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Extensions;

public class IEnumerableExtensionsBoolTests
{
    [Fact]
    public void AllReturnsTrue_WhenAllElementsAreTrue()
    {
        var booleans = new[] { true, true, true };
        Assert.True(booleans.All());
    }

    [Theory]
    [InlineData(new[] { false })]
    [InlineData(new[] { true, false })]
    [InlineData(new[] { false, true, false })]
    [InlineData(new[] { true, true, false })]
    public void AllReturnsFalse_WhenAnyElementIsFalse(bool[] booleans) => Assert.False(booleans.All());

    [Fact]
    public void AllReturnsTrue_ForEmptyCollection()
    {
        var emptyCollection = Array.Empty<bool>();
        Assert.True(emptyCollection.All());
    }

    [Fact]
    public void AllAcceptsCustomIEnumerableImplementation()
    {
        var customCollection = new CustomBoolCollection([true, true, true]);
        Assert.True(customCollection.All());

        var customCollectionWithFalse = new CustomBoolCollection([true, false, true]);
        Assert.False(customCollectionWithFalse.All());
    }

    private class CustomBoolCollection(bool[] values) : IEnumerable<bool>
    {
        private readonly bool[] _values = values;

        public IEnumerator<bool> GetEnumerator() => ((IEnumerable<bool>)_values).GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _values.GetEnumerator();
    }
}
