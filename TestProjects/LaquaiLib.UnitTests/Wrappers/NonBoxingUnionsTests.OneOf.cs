using LaquaiLib.Wrappers;

namespace LaquaiLib.UnitTests.Wrappers;

public class NonBoxingUnionsOneOfTests
{
    private sealed class NullToString
    {
        public override string ToString() => null;
    }

    [Fact]
    public void OneOfTwoWithFirstCaseTracksState()
    {
        var u = new OneOf<int, string>(1);
        Assert.True(u.IsT1);
        Assert.False(u.IsT2);
        Assert.True(u.HasValue);
        Assert.Equal(1, u.AsT1());
        Assert.Equal((object)1, u.Value);
        Assert.Equal("1", u.ToString());
    }

    [Fact]
    public void OneOfTwoWithSecondCaseTracksState()
    {
        var u = new OneOf<int, string>("two");
        Assert.False(u.IsT1);
        Assert.True(u.IsT2);
        Assert.True(u.HasValue);
        Assert.Equal("two", u.AsT2());
        Assert.Equal((object)"two", u.Value);
        Assert.Equal("two", u.ToString());
    }

    [Fact]
    public void OneOfTwoValueBoxesTheHeldCaseType()
    {
        Assert.IsType<int>(new OneOf<int, string>(1).Value);
        Assert.IsType<string>(new OneOf<int, string>("two").Value);
    }

    [Fact]
    public void OneOfTwoAsThrowsForCaseNotHeld()
    {
        var first = new OneOf<int, string>(1);
        var second = new OneOf<int, string>("two");
        Assert.Throws<InvalidOperationException>(() => { _ = first.AsT2(); });
        Assert.Throws<InvalidOperationException>(() => { _ = second.AsT1(); });
    }

    [Fact]
    public void OneOfTwoTryGetValueSucceedsOnlyForHeldCase()
    {
        var u = new OneOf<int, string>(1);
        Assert.True(u.TryGetValue(out int held));
        Assert.Equal(1, held);
        Assert.False(u.TryGetValue(out string other));
        Assert.Null(other);
    }

    [Fact]
    public void OneOfTwoTryGetValueAssignsFieldEvenWhenCaseNotHeld()
    {
        var u = new OneOf<int, string>("two");
        Assert.False(u.TryGetValue(out int notHeld));
        Assert.Equal(0, notHeld);
        Assert.True(u.TryGetValue(out string held));
        Assert.Equal("two", held);
    }

    [Fact]
    public void OneOfTwoWithDefaultValueTypeStillHasValue()
    {
        var u = new OneOf<int, string>(0);
        Assert.True(u.IsT1);
        Assert.True(u.HasValue);
        Assert.Equal(0, u.AsT1());
        Assert.True(u.TryGetValue(out int held));
        Assert.Equal(0, held);
    }

    [Fact]
    public void OneOfTwoWithNullReferenceKeepsTagButReportsNoValue()
    {
        var u = new OneOf<int, string>((string)null);
        Assert.True(u.IsT2);
        Assert.False(u.HasValue);
        Assert.Null(u.Value);
        Assert.Null(u.AsT2());
        Assert.Equal(LaquaiLibUnion.NoValue, u.ToString());
        Assert.False(u.TryGetValue(out string held));
        Assert.Null(held);
    }

    [Fact]
    public void OneOfTwoWithNullableStructWithoutValueReportsNoValue()
    {
        var u = new OneOf<int?, string>((int?)null);
        Assert.True(u.IsT1);
        Assert.False(u.HasValue);
        Assert.Null(u.Value);
        Assert.Equal(LaquaiLibUnion.NoValue, u.ToString());
        Assert.False(u.TryGetValue(out int? held));
        Assert.Null(held);
    }

    [Fact]
    public void OneOfTwoWithNullReturningOverrideReturnsNoValueFromToString()
    {
        var u = new OneOf<int, NullToString>(new NullToString());
        Assert.True(u.IsT2);
        Assert.True(u.HasValue);
        Assert.Equal(LaquaiLibUnion.NoValue, u.ToString());
    }

    [Fact]
    public void OneOfTwoDefaultIsEmpty()
    {
        var u = default(OneOf<int, string>);
        Assert.False(u.IsT1);
        Assert.False(u.IsT2);
        Assert.False(u.HasValue);
        Assert.Null(u.Value);
        Assert.Equal(LaquaiLibUnion.NoValue, u.ToString());
        Assert.Equal(0, u.GetHashCode());
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT1(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT2(); });
        Assert.False(u.TryGetValue(out int first));
        Assert.Equal(0, first);
        Assert.False(u.TryGetValue(out string second));
        Assert.Null(second);
    }

    [Fact]
    public void OneOfTwoGetHashCodeMatchesHeldValueHash()
    {
        Assert.Equal(EqualityComparer<int>.Default.GetHashCode(1), new OneOf<int, string>(1).GetHashCode());
        Assert.Equal(EqualityComparer<string>.Default.GetHashCode("two"), new OneOf<int, string>("two").GetHashCode());
    }

    [Fact]
    public void OneOfTwoGetHashCodeIsStableAcrossEqualInstances()
    {
        Assert.Equal(new OneOf<int, string>("two").GetHashCode(), new OneOf<int, string>("two").GetHashCode());
    }

    [Fact]
    public void OneOfTwoEqualsSameCaseSameValueReturnsTrue()
    {
        Assert.True(new OneOf<int, string>(1).Equals(new OneOf<int, string>(1)));
        Assert.True(new OneOf<int, string>("two").Equals(new OneOf<int, string>("two")));
    }

    [Fact]
    public void OneOfTwoEqualsSameCaseDifferentValueReturnsFalse()
    {
        Assert.False(new OneOf<int, string>(1).Equals(new OneOf<int, string>(2)));
    }

    [Fact]
    public void OneOfTwoEqualsDifferentCaseReturnsFalse()
    {
        Assert.False(new OneOf<int, string>(1).Equals(new OneOf<int, string>("two")));
    }

    [Fact]
    public void OneOfTwoDefaultsAreEqual()
    {
        Assert.True(default(OneOf<int, string>).Equals(default(OneOf<int, string>)));
    }

    [Fact]
    public void OneOfTwoWithValueDoesNotEqualDefault()
    {
        Assert.False(new OneOf<int, string>(0).Equals(default(OneOf<int, string>)));
        Assert.False(default(OneOf<int, string>).Equals(new OneOf<int, string>(0)));
    }

    [Fact]
    public void OneOfTwoEqualsRawValueOfHeldCaseReturnsTrue()
    {
        Assert.True(new OneOf<int, string>(1).Equals(1));
        Assert.True(new OneOf<int, string>("two").Equals("two"));
    }

    [Fact]
    public void OneOfTwoEqualsRawValueOfOtherCaseReturnsFalse()
    {
        Assert.False(new OneOf<int, string>(1).Equals("two"));
        Assert.False(new OneOf<int, string>("two").Equals(1));
    }

    [Fact]
    public void OneOfTwoDefaultEqualsRawValueReturnsFalse()
    {
        Assert.False(default(OneOf<int, string>).Equals(0));
        Assert.False(default(OneOf<int, string>).Equals((string)null));
    }

    [Fact]
    public void OneOfTwoEqualsObjectHoldingUnionDelegatesToTypedOverload()
    {
        Assert.True(new OneOf<int, string>(1).Equals((object)new OneOf<int, string>(1)));
        Assert.False(new OneOf<int, string>(1).Equals((object)new OneOf<int, string>(2)));
    }

    [Fact]
    public void OneOfTwoEqualsObjectHoldingRawValueDelegatesToTypedOverload()
    {
        Assert.True(new OneOf<int, string>(1).Equals((object)1));
        Assert.True(new OneOf<int, string>("two").Equals((object)"two"));
        Assert.False(new OneOf<int, string>(1).Equals((object)"two"));
    }

    [Fact]
    public void OneOfTwoEqualsObjectOfUnrelatedTypeReturnsFalse()
    {
        Assert.False(new OneOf<int, string>(1).Equals((object)1d));
    }

    [Fact]
    public void OneOfTwoEqualsObjectOfDifferentUnionTypeReturnsFalse()
    {
        Assert.False(new OneOf<int, string>(1).Equals((object)new OneOf<int, string, double>(1)));
    }

    [Fact]
    public void OneOfTwoEqualsObjectNullReturnsFalse()
    {
        Assert.False(new OneOf<int, string>(1).Equals((object)null));
        Assert.False(new OneOf<int, string>("two").Equals((object)null));
    }

    [Fact]
    public void OneOfTwoImplementsIEquatable()
    {
        IEquatable<OneOf<int, string>> equatable = new OneOf<int, string>(1);
        Assert.True(equatable.Equals(new OneOf<int, string>(1)));
        Assert.False(equatable.Equals(new OneOf<int, string>("two")));
    }

    [Fact]
    public void OneOfTwoIsCopiedByValue()
    {
        var original = new OneOf<int, string>(1);
        var copy = original;
        Assert.True(copy.Equals(original));
        Assert.Equal(1, copy.AsT1());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void OneOfTwoRoundTripsArbitraryIntegers(int value)
    {
        var u = new OneOf<int, string>(value);
        Assert.True(u.IsT1);
        Assert.Equal(value, u.AsT1());
        Assert.Equal(value.ToString(), u.ToString());
        Assert.True(u.Equals(value));
    }

    [Fact]
    public void OneOfThreeWithFirstCaseTracksState()
    {
        var u = new OneOf<int, string, double>(1);
        Assert.True(u.IsT1);
        Assert.False(u.IsT2);
        Assert.False(u.IsT3);
        Assert.True(u.HasValue);
        Assert.Equal(1, u.AsT1());
        Assert.Equal((object)1, u.Value);
        Assert.Equal("1", u.ToString());
        Assert.True(u.TryGetValue(out int held));
        Assert.Equal(1, held);
    }

    [Fact]
    public void OneOfThreeWithSecondCaseTracksState()
    {
        var u = new OneOf<int, string, double>("two");
        Assert.False(u.IsT1);
        Assert.True(u.IsT2);
        Assert.False(u.IsT3);
        Assert.True(u.HasValue);
        Assert.Equal("two", u.AsT2());
        Assert.Equal((object)"two", u.Value);
        Assert.Equal("two", u.ToString());
        Assert.True(u.TryGetValue(out string held));
        Assert.Equal("two", held);
    }

    [Fact]
    public void OneOfThreeWithThirdCaseTracksState()
    {
        var u = new OneOf<int, string, double>(3d);
        Assert.False(u.IsT1);
        Assert.False(u.IsT2);
        Assert.True(u.IsT3);
        Assert.True(u.HasValue);
        Assert.Equal(3d, u.AsT3());
        Assert.Equal((object)3d, u.Value);
        Assert.Equal((3d).ToString(), u.ToString());
        Assert.True(u.TryGetValue(out double held));
        Assert.Equal(3d, held);
    }

    [Fact]
    public void OneOfThreeAsThrowsForEveryCaseNotHeld()
    {
        var first = new OneOf<int, string, double>(1);
        var second = new OneOf<int, string, double>("two");
        var third = new OneOf<int, string, double>(3d);
        Assert.Throws<InvalidOperationException>(() => { _ = first.AsT2(); });
        Assert.Throws<InvalidOperationException>(() => { _ = first.AsT3(); });
        Assert.Throws<InvalidOperationException>(() => { _ = second.AsT1(); });
        Assert.Throws<InvalidOperationException>(() => { _ = second.AsT3(); });
        Assert.Throws<InvalidOperationException>(() => { _ = third.AsT1(); });
        Assert.Throws<InvalidOperationException>(() => { _ = third.AsT2(); });
    }

    [Fact]
    public void OneOfThreeTryGetValueFailsForEveryCaseNotHeld()
    {
        var u = new OneOf<int, string, double>("two");
        Assert.False(u.TryGetValue(out int first));
        Assert.Equal(0, first);
        Assert.False(u.TryGetValue(out double third));
        Assert.Equal(0d, third);
    }

    [Fact]
    public void OneOfThreeDefaultIsEmpty()
    {
        var u = default(OneOf<int, string, double>);
        Assert.False(u.IsT1);
        Assert.False(u.IsT2);
        Assert.False(u.IsT3);
        Assert.False(u.HasValue);
        Assert.Null(u.Value);
        Assert.Equal(LaquaiLibUnion.NoValue, u.ToString());
        Assert.Equal(0, u.GetHashCode());
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT1(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT2(); });
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT3(); });
        Assert.False(u.TryGetValue(out int first));
        Assert.False(u.TryGetValue(out string second));
        Assert.False(u.TryGetValue(out double third));
    }

    [Fact]
    public void OneOfThreeWithNullReferenceKeepsTagButReportsNoValue()
    {
        var u = new OneOf<int, string, double>((string)null);
        Assert.True(u.IsT2);
        Assert.False(u.HasValue);
        Assert.Null(u.Value);
        Assert.Null(u.AsT2());
        Assert.Equal(LaquaiLibUnion.NoValue, u.ToString());
    }

    [Fact]
    public void OneOfThreeGetHashCodeMatchesHeldValueHash()
    {
        Assert.Equal(EqualityComparer<int>.Default.GetHashCode(1), new OneOf<int, string, double>(1).GetHashCode());
        Assert.Equal(EqualityComparer<string>.Default.GetHashCode("two"), new OneOf<int, string, double>("two").GetHashCode());
        Assert.Equal(EqualityComparer<double>.Default.GetHashCode(3d), new OneOf<int, string, double>(3d).GetHashCode());
    }

    [Fact]
    public void OneOfThreeEqualsSameCaseSameValueReturnsTrue()
    {
        Assert.True(new OneOf<int, string, double>(3d).Equals(new OneOf<int, string, double>(3d)));
    }

    [Fact]
    public void OneOfThreeEqualsDifferentCaseReturnsFalse()
    {
        Assert.False(new OneOf<int, string, double>(1).Equals(new OneOf<int, string, double>(3d)));
        Assert.False(new OneOf<int, string, double>("two").Equals(new OneOf<int, string, double>(3d)));
    }

    [Fact]
    public void OneOfThreeDefaultsAreEqual()
    {
        Assert.True(default(OneOf<int, string, double>).Equals(default(OneOf<int, string, double>)));
    }

    [Fact]
    public void OneOfThreeEqualsRawValueOfHeldCaseReturnsTrue()
    {
        Assert.True(new OneOf<int, string, double>(1).Equals(1));
        Assert.True(new OneOf<int, string, double>("two").Equals("two"));
        Assert.True(new OneOf<int, string, double>(3d).Equals(3d));
    }

    [Fact]
    public void OneOfThreeEqualsRawValueOfOtherCaseReturnsFalse()
    {
        var u = new OneOf<int, string, double>(1);
        Assert.False(u.Equals("two"));
        Assert.False(u.Equals(3d));
    }

    [Fact]
    public void OneOfThreeEqualsObjectHoldingRawValueDelegatesToTypedOverload()
    {
        Assert.True(new OneOf<int, string, double>(3d).Equals((object)3d));
        Assert.False(new OneOf<int, string, double>(3d).Equals((object)1));
    }

    [Fact]
    public void OneOfThreeEqualsObjectNullReturnsFalse()
    {
        Assert.False(new OneOf<int, string, double>(3d).Equals((object)null));
    }

    [Fact]
    public void OneOfThreeImplementsIEquatable()
    {
        IEquatable<OneOf<int, string, double>> equatable = new OneOf<int, string, double>(3d);
        Assert.True(equatable.Equals(new OneOf<int, string, double>(3d)));
        Assert.False(equatable.Equals(new OneOf<int, string, double>(1)));
    }

    [Fact]
    public void OneOfThreeReassignmentReplacesHeldCase()
    {
        var u = new OneOf<int, string, double>(1);
        u = new OneOf<int, string, double>("two");
        Assert.False(u.IsT1);
        Assert.True(u.IsT2);
        Assert.Equal("two", u.AsT2());
        Assert.Throws<InvalidOperationException>(() => { _ = u.AsT1(); });
    }

    [Fact]
    public void OneOfThreeStoredInArrayPreservesEachCase()
    {
        var values = new[]
        {
            new OneOf<int, string, double>(1),
            new OneOf<int, string, double>("two"),
            new OneOf<int, string, double>(3d)
        };
        Assert.True(values[0].IsT1);
        Assert.True(values[1].IsT2);
        Assert.True(values[2].IsT3);
        Assert.Equal([(object)1, "two", 3d], values.Select(static v => v.Value));
    }
}
