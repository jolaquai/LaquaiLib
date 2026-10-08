using LaquaiLib.Wrappers;

namespace LaquaiLib.UnitTests.Wrappers;

public class NonBoxingUnionsTests
{
    private sealed class NullToString
    {
        public override string ToString() => null;
    }

    private sealed class Named(string name)
    {
        public string Name { get; } = name;
        public override string ToString() => Name;
    }

    private static Result<T> R<T>(T value) => new Result<T>(value);

    [Fact]
    public void NoValueConstantIsEmpty()
    {
        Assert.Equal("Empty", LaquaiLibUnion.NoValue);
    }

    [Fact]
    public void ToStringWithNullReferenceReturnsNoValue()
    {
        string value = null;
        Assert.Equal(LaquaiLibUnion.NoValue, LaquaiLibUnion.ToString(in value));
    }

    [Fact]
    public void ToStringWithNullableStructWithoutValueReturnsNoValue()
    {
        int? value = null;
        Assert.Equal(LaquaiLibUnion.NoValue, LaquaiLibUnion.ToString(in value));
    }

    [Fact]
    public void ToStringWithNullReturningOverrideReturnsNoValue()
    {
        var value = new NullToString();
        Assert.Equal(LaquaiLibUnion.NoValue, LaquaiLibUnion.ToString(in value));
    }

    [Fact]
    public void ToStringWithValueTypeReturnsUnderlyingText()
    {
        var value = 42;
        Assert.Equal("42", LaquaiLibUnion.ToString(in value));
    }

    [Fact]
    public void ToStringWithReferenceTypeReturnsUnderlyingText()
    {
        var value = new Named("named");
        Assert.Equal("named", LaquaiLibUnion.ToString(in value));
    }

    [Fact]
    public void ThrowNotHeldThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => { _ = LaquaiLibUnion.ThrowNotHeld<int>(); });
    }

    [Fact]
    public void ThrowNotHeldMessageNamesTheRequestedType()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => { _ = LaquaiLibUnion.ThrowNotHeld<Guid>(); });
        Assert.Contains(typeof(Guid).ToString(), ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ResultConstructedWithValueHoldsIt()
    {
        var r = R(42);
        Assert.True(r.HasValue);
        Assert.Equal(42, r.AsValue());
        Assert.Equal((object)42, r.Value);
    }

    [Fact]
    public void ResultValueBoxesTheUnderlyingType()
    {
        var r = R(42);
        Assert.IsType<int>(r.Value);
    }

    [Fact]
    public void ResultConstructedWithReferenceHoldsIt()
    {
        var named = new Named("payload");
        var r = R(named);
        Assert.True(r.HasValue);
        Assert.Same(named, r.AsValue());
        Assert.Same(named, r.Value);
    }

    [Fact]
    public void ResultDefaultHasNoValue()
    {
        var r = default(Result<int>);
        Assert.False(r.HasValue);
        Assert.Null(r.Value);
    }

    [Fact]
    public void ResultDefaultAsValueThrows()
    {
        var r = default(Result<int>);
        Assert.Throws<InvalidOperationException>(() => { _ = r.AsValue(); });
    }

    [Fact]
    public void ResultDefaultToStringReturnsNoValue()
    {
        Assert.Equal(LaquaiLibUnion.NoValue, default(Result<int>).ToString());
    }

    [Fact]
    public void ResultDefaultGetHashCodeIsZero()
    {
        Assert.Equal(0, default(Result<int>).GetHashCode());
    }

    [Fact]
    public void ResultConstructedWithNullReferenceReportsNoValue()
    {
        var r = R<string>(null);
        Assert.False(r.HasValue);
        Assert.Null(r.Value);
    }

    [Fact]
    public void ResultConstructedWithNullReferenceStillReturnsNullFromAsValue()
    {
        var r = R<string>(null);
        Assert.Null(r.AsValue());
    }

    [Fact]
    public void ResultConstructedWithNullReferenceToStringReturnsNoValue()
    {
        Assert.Equal(LaquaiLibUnion.NoValue, R<string>(null).ToString());
    }

    [Fact]
    public void ResultConstructedWithNullableStructWithoutValueReportsNoValue()
    {
        var r = R<int?>(null);
        Assert.False(r.HasValue);
        Assert.Null(r.Value);
        Assert.Equal(LaquaiLibUnion.NoValue, r.ToString());
    }

    [Fact]
    public void ResultConstructedWithNullableStructWithValueReportsHasValue()
    {
        var r = R<int?>(7);
        Assert.True(r.HasValue);
        Assert.Equal(7, r.AsValue());
        Assert.Equal("7", r.ToString());
    }

    [Fact]
    public void ResultToStringReturnsUnderlyingText()
    {
        Assert.Equal("42", R(42).ToString());
        Assert.Equal("payload", R(new Named("payload")).ToString());
    }

    [Fact]
    public void ResultToStringWithNullReturningOverrideReturnsNoValue()
    {
        Assert.Equal(LaquaiLibUnion.NoValue, R(new NullToString()).ToString());
    }

    [Fact]
    public void ResultTryGetValueOnHeldValueReturnsTrue()
    {
        Assert.True(R(42).TryGetValue(out var value));
        Assert.Equal(42, value);
    }

    [Fact]
    public void ResultTryGetValueOnDefaultReturnsFalse()
    {
        Assert.False(default(Result<int>).TryGetValue(out var value));
        Assert.Equal(0, value);
    }

    [Fact]
    public void ResultTryGetValueOnNullReferenceReturnsFalse()
    {
        Assert.False(R<string>(null).TryGetValue(out var value));
        Assert.Null(value);
    }

    [Fact]
    public void ResultGetHashCodeMatchesUnderlyingHash()
    {
        Assert.Equal(EqualityComparer<int>.Default.GetHashCode(42), R(42).GetHashCode());
    }

    [Fact]
    public void ResultGetHashCodeIsStableAcrossEqualInstances()
    {
        Assert.Equal(R("text").GetHashCode(), R("text").GetHashCode());
    }

    [Fact]
    public void ResultGetHashCodeOfNullReferenceMatchesComparer()
    {
        Assert.Equal(EqualityComparer<string>.Default.GetHashCode(null), R<string>(null).GetHashCode());
    }

    [Fact]
    public void ResultEqualsSameValueReturnsTrue()
    {
        Assert.True(R(42).Equals(R(42)));
    }

    [Fact]
    public void ResultEqualsDifferentValueReturnsFalse()
    {
        Assert.False(R(42).Equals(R(43)));
    }

    [Fact]
    public void ResultDefaultsAreEqual()
    {
        Assert.True(default(Result<int>).Equals(default(Result<int>)));
    }

    [Fact]
    public void ResultWithValueDoesNotEqualDefault()
    {
        Assert.False(R(0).Equals(default(Result<int>)));
        Assert.False(default(Result<int>).Equals(R(0)));
    }

    [Fact]
    public void ResultEqualsRawValueReturnsTrueWhenHeld()
    {
        Assert.True(R(42).Equals(42));
    }

    [Fact]
    public void ResultEqualsRawValueReturnsFalseForDifferentValue()
    {
        Assert.False(R(42).Equals(43));
    }

    [Fact]
    public void ResultDefaultEqualsRawNonNullValueReturnsFalse()
    {
        Assert.False(default(Result<int>).Equals(0));
    }

    [Fact]
    public void ResultDefaultEqualsRawNullReturnsTrue()
    {
        Assert.True(default(Result<string>).Equals((string)null));
    }

    [Fact]
    public void ResultEqualsObjectHoldingResultDelegatesToTypedOverload()
    {
        Assert.True(R(42).Equals((object)R(42)));
        Assert.False(R(42).Equals((object)R(43)));
    }

    [Fact]
    public void ResultEqualsObjectHoldingRawValueDelegatesToTypedOverload()
    {
        Assert.True(R(42).Equals((object)42));
        Assert.False(R(42).Equals((object)43));
    }

    [Fact]
    public void ResultEqualsObjectOfUnrelatedTypeReturnsFalse()
    {
        Assert.False(R(42).Equals((object)"42"));
    }

    [Fact]
    public void ResultEqualsObjectNullReturnsFalse()
    {
        Assert.False(R(42).Equals((object)null));
        Assert.False(R("text").Equals((object)null));
    }

    [Fact]
    public void ResultEqualityOperatorComparesTwoResults()
    {
        Assert.True(R(42) == R(42));
        Assert.False(R(42) == R(43));
    }

    [Fact]
    public void ResultInequalityOperatorComparesTwoResults()
    {
        Assert.False(R(42) != R(42));
        Assert.True(R(42) != R(43));
    }

    [Fact]
    public void ResultEqualityOperatorComparesAgainstRawValueOnTheRight()
    {
        Assert.True(R(42) == 42);
        Assert.False(R(42) == 43);
    }

    [Fact]
    public void ResultEqualityOperatorComparesAgainstRawValueOnTheLeft()
    {
        Assert.True(42 == R(42));
        Assert.False(43 == R(42));
    }

    [Fact]
    public void ResultInequalityOperatorComparesAgainstRawValue()
    {
        Assert.True(R(42) != 43);
        Assert.False(R(42) != 42);
        Assert.True(43 != R(42));
        Assert.False(42 != R(42));
    }

    [Fact]
    public void ResultDefaultEqualityOperatorAgainstRawNull()
    {
        Assert.True(default(Result<string>) == null);
        Assert.False(default(Result<string>) != null);
    }

    [Fact]
    public void ResultImplementsIEquatableOfResult()
    {
        IEquatable<Result<int>> equatable = R(42);
        Assert.True(equatable.Equals(R(42)));
        Assert.False(equatable.Equals(R(43)));
    }

    [Fact]
    public void ResultImplementsIEquatableOfValue()
    {
        IEquatable<int> equatable = R(42);
        Assert.True(equatable.Equals(42));
        Assert.False(equatable.Equals(43));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void ResultRoundTripsArbitraryIntegers(int value)
    {
        var r = R(value);
        Assert.True(r.HasValue);
        Assert.Equal(value, r.AsValue());
        Assert.True(r.TryGetValue(out var actual));
        Assert.Equal(value, actual);
        Assert.Equal(value.ToString(), r.ToString());
    }

    [Fact]
    public void ResultOfStructTypeRoundTrips()
    {
        var value = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var r = R(value);
        Assert.True(r.HasValue);
        Assert.Equal(value, r.AsValue());
        Assert.Equal(value.ToString(), r.ToString());
        Assert.True(r == value);
    }

    [Fact]
    public void ResultOfEmptyStringIsStillHeld()
    {
        var r = R("");
        Assert.True(r.HasValue);
        Assert.Equal("", r.AsValue());
        Assert.Equal("", r.ToString());
    }
}
