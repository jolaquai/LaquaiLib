using BitArray = LaquaiLib.Numerics.BitArray;

namespace LaquaiLib.UnitTests.Wrappers;

public class BitArrayTests
{
    private static BitArray U(params ulong[] words) => new(words);
    private static BitArray S(params ulong[] words)
    {
        var b = new BitArray(words);
        b.Signed = true;
        return b;
    }

    [Fact]
    public void CreateWithCapacityRoundsUpToWordSize()
    {
        Assert.Equal(64, BitArray.CreateWithCapacity(1).Capacity);
        Assert.Equal(64, BitArray.CreateWithCapacity(64).Capacity);
        Assert.Equal(128, BitArray.CreateWithCapacity(65).Capacity);
        Assert.Equal(192, BitArray.CreateWithCapacity(129).Capacity);
    }

    [Fact]
    public void CreateWithCapacityRejectsNonPositive()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => BitArray.CreateWithCapacity(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => BitArray.CreateWithCapacity(-1));
    }

    [Fact]
    public void WithLowerNSetSetsLeastSignificantBits()
    {
        var ba = BitArray.WithLowerNSet(3);
        Assert.Equal(7, ba.As<int>());
        Assert.Equal(3, ba.PopCount());
        Assert.Equal(0, ba.FindFirstSetBit());
        Assert.Equal(2, ba.FindLastSetBit());
    }

    [Fact]
    public void WithUpperNSetSetsMostSignificantBitsOfRoundedCapacity()
    {
        var ba = BitArray.WithUpperNSet(3);
        Assert.Equal(64, ba.Capacity);
        Assert.Equal(3, ba.PopCount());
        Assert.Equal(61, ba.FindFirstSetBit());
        Assert.Equal(63, ba.FindLastSetBit());
    }

    [Fact]
    public void IndexerGetAndSetRoundTrips()
    {
        var ba = U(0);
        ba[5] = true;
        Assert.True(ba[5]);
        Assert.False(ba[4]);
        Assert.Equal(1, ba.PopCount());
        ba[5] = false;
        Assert.False(ba[5]);
        Assert.Equal(0, ba.PopCount());
    }

    [Fact]
    public void IndexerGetOutOfRangeThrows()
    {
        var ba = U(0);
        Assert.Throws<ArgumentOutOfRangeException>(() => { _ = ba[64]; });
    }

    [Fact]
    public void IndexerSetBeyondCapacityGrows()
    {
        var ba = U(0);
        ba[100] = true;
        Assert.Equal(128, ba.Capacity);
        Assert.True(ba[100]);
        Assert.Equal(101, ba.Length);
    }

    [Fact]
    public void IndexerFromEndResolvesAgainstCapacity()
    {
        var ba = BitArray.CreateWithCapacity(64);
        ba[^1] = true;
        Assert.True(ba[63]);
        Assert.Equal(63, ba.FindLastSetBit());
    }

    [Fact]
    public void TryGetRespectsCapacityWithoutGrowing()
    {
        var ba = U(0);
        Assert.True(ba.TryGet(63, out var inRange));
        Assert.False(inRange);
        Assert.False(ba.TryGet(64, out var outOfRange));
        Assert.False(outOfRange);
        Assert.Equal(64, ba.Capacity);
    }

    [Fact]
    public void TrySetRespectsCapacityWithoutGrowing()
    {
        var ba = U(0);
        Assert.True(ba.TrySet(63, true));
        Assert.True(ba[63]);
        Assert.False(ba.TrySet(64, true));
        Assert.Equal(64, ba.Capacity);
    }

    [Fact]
    public void RangeGetterExtractsPackedBits()
    {
        var ba = U(0xF0);
        var slice = ba[4..8];
        Assert.Equal(15, slice.As<int>());
    }

    [Fact]
    public void AndOperatorIntersectsBits()
    {
        var result = U(0b1100) & U(0b1010);
        Assert.Equal(0b1000, result.As<int>());
    }

    [Fact]
    public void OrOperatorUnionsBits()
    {
        var result = U(0b1100) | U(0b1010);
        Assert.Equal(0b1110, result.As<int>());
    }

    [Fact]
    public void XorOperatorTogglesBits()
    {
        var result = U(0b1100) ^ U(0b1010);
        Assert.Equal(0b0110, result.As<int>());
    }

    [Fact]
    public void AndClearsWordsBeyondShorterOperand()
    {
        var result = U(0xFFFFFFFFFFFFFFFF, 0xFFFFFFFFFFFFFFFF) & U(0x0F);
        Assert.Equal(128, result.Capacity);
        Assert.Equal(15, result.As<int>());
        Assert.False(result[64]);
    }

    [Fact]
    public void OrWidensToLargerOperandCapacity()
    {
        var result = U(0x0F) | U(0x00, 0x01);
        Assert.Equal(128, result.Capacity);
        Assert.True(result[64]);
        Assert.True(result[0]);
    }

    [Fact]
    public void BitwiseOperatorResultSignedIsDisjunction()
    {
        Assert.True((U(0b1100) | S(0b1010)).Signed);
        Assert.True((S(0b1100) & U(0b1010)).Signed);
        Assert.False((U(0b1100) ^ U(0b1010)).Signed);
    }

    [Fact]
    public void NotOperatorComplementsWithinCapacity()
    {
        var result = ~U(0);
        Assert.True(result.IsAllSet());
        Assert.Equal(64, result.PopCount());
    }

    [Fact]
    public void NotOfLowerBitsFlipsOnlyThoseBits()
    {
        var ba = U(0);
        ba.Not(4);
        Assert.Equal(15, ba.As<int>());
    }

    [Fact]
    public void NotOfLowerBitsGrowsWhenNeeded()
    {
        var ba = U(0);
        ba.Not(70);
        Assert.Equal(128, ba.Capacity);
        Assert.Equal(70, ba.PopCount());
        Assert.True(ba[69]);
        Assert.False(ba[70]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(31)]
    public void ShiftLeftMovesBitsTowardMsb(int n)
    {
        var ba = U(1);
        ba.ShiftLeft(n);
        Assert.Equal(1UL << n, ba.As<ulong>());
    }

    [Fact]
    public void ShiftLeftAcrossWordBoundary()
    {
        var ba = BitArray.CreateWithCapacity(128);
        ba[0] = true;
        ba.ShiftLeft(64);
        Assert.True(ba[64]);
        Assert.False(ba[0]);
    }

    [Fact]
    public void ShiftRightMovesBitsTowardLsb()
    {
        var ba = U(0x20);
        ba.ShiftRight(5);
        Assert.Equal(1UL, ba.As<ulong>());
    }

    [Fact]
    public void ShiftBeyondCapacityClears()
    {
        var ba = U(0xFFFFFFFFFFFFFFFF);
        ba.ShiftLeft(64);
        Assert.True(ba.IsAllZero());
    }

    [Fact]
    public void ShiftRightLogicalZeroFillsHighBits()
    {
        var ba = U(0x8000000000000000);
        ba.ShiftRight(4);
        Assert.Equal(0x0800000000000000UL, ba.As<ulong>());
    }

    [Fact]
    public void ShiftRightArithmeticSignFillsForSignedNegative()
    {
        var ba = S(0x8000000000000000);
        ba.ShiftRightArithmetic(4);
        Assert.Equal(0xF800000000000000UL, ba.As<ulong>());
    }

    [Fact]
    public void ShiftRightArithmeticZeroFillsForUnsigned()
    {
        var ba = U(0x8000000000000000);
        ba.ShiftRightArithmetic(4);
        Assert.Equal(0x0800000000000000UL, ba.As<ulong>());
    }

    [Fact]
    public void ShiftOperatorsDoNotMutateOriginal()
    {
        var ba = U(1);
        var shifted = ba << 5;
        Assert.Equal(1UL, ba.As<ulong>());
        Assert.Equal(32UL, shifted.As<ulong>());
    }

    [Fact]
    public void RotateLeftWrapsAroundCapacity()
    {
        var ba = U(0x8000000000000000);
        ba.RotateLeft(1);
        Assert.Equal(1UL, ba.As<ulong>());
    }

    [Fact]
    public void RotateRightWrapsAroundCapacity()
    {
        var ba = U(1);
        ba.RotateRight(1);
        Assert.Equal(0x8000000000000000UL, ba.As<ulong>());
    }

    [Fact]
    public void SetRangeAcrossWordBoundary()
    {
        var ba = BitArray.CreateWithCapacity(128);
        ba.Set(60, 8, true);
        Assert.Equal(8, ba.PopCount());
        Assert.False(ba[59]);
        Assert.True(ba[60]);
        Assert.True(ba[67]);
        Assert.False(ba[68]);
    }

    [Fact]
    public void SetAllAndClear()
    {
        var ba = U(0);
        ba.SetAll(true);
        Assert.True(ba.IsAllSet());
        ba.Clear();
        Assert.True(ba.IsAllZero());
    }

    [Fact]
    public void SetUpperAndSetLower()
    {
        var ba = BitArray.CreateWithCapacity(64);
        ba.SetUpper(3, true);
        ba.SetLower(2, true);
        Assert.True(ba[0]);
        Assert.True(ba[1]);
        Assert.True(ba[61]);
        Assert.True(ba[63]);
        Assert.Equal(5, ba.PopCount());
    }

    [Fact]
    public void PredicatesOnZeroValue()
    {
        var ba = U(0);
        Assert.True(ba.IsAllZero());
        Assert.True(ba.None());
        Assert.False(ba.Any());
        Assert.Equal(-1, ba.FindFirstSetBit());
        Assert.Equal(-1, ba.FindLastSetBit());
        Assert.Equal(0, ba.Length);
        Assert.Equal(0, ba.PopCount());
    }

    [Fact]
    public void GrowthSignExtendsForSignedNegative()
    {
        var ba = S(0x8000000000000000);
        ba[70] = true;
        Assert.Equal(128, ba.Capacity);
        Assert.True(ba[64]);
        Assert.True(ba[100]);
    }

    [Fact]
    public void GrowthZeroExtendsForUnsigned()
    {
        var ba = U(0xFFFFFFFFFFFFFFFF);
        ba[70] = true;
        Assert.Equal(128, ba.Capacity);
        Assert.False(ba[64]);
        Assert.False(ba[100]);
    }

    [Fact]
    public void IsNegativeReflectsSignBitAndSignedFlag()
    {
        Assert.True(S(0xFFFFFFFFFFFFFFFF).IsNegative);
        Assert.False(S(5).IsNegative);
        Assert.False(U(0xFFFFFFFFFFFFFFFF).IsNegative);
    }

    [Fact]
    public void EqualsIgnoresSignedWhenNonNegative()
    {
        Assert.True(S(5).Equals(U(5)));
        Assert.True(U(5).Equals(S(5)));
        Assert.Equal(S(5).GetHashCode(), U(5).GetHashCode());
    }

    [Fact]
    public void EqualsDistinguishesNegativeFromUnsignedMagnitude()
    {
        Assert.False(S(0xFFFFFFFFFFFFFFFB).Equals(U(0xFFFFFFFFFFFFFFFB)));
    }

    [Fact]
    public void EqualsIgnoresTrailingCapacity()
    {
        Assert.True(U(5).Equals(U(5, 0)));
        Assert.Equal(U(5).GetHashCode(), U(5, 0).GetHashCode());
    }

    [Fact]
    public void CompareToOrdersByNumericValue()
    {
        Assert.Equal(0, S(5).CompareTo(U(5)));
        Assert.True(S(0xFFFFFFFFFFFFFFFB).CompareTo(U(5)) < 0);
        Assert.True(U(0xFFFFFFFFFFFFFFFB).CompareTo(S(0xFFFFFFFFFFFFFFFB)) > 0);
        Assert.Equal(1, U(5).CompareTo(null));
    }

    [Fact]
    public void ComparisonOperators()
    {
        Assert.True(U(3) < U(5));
        Assert.True(U(5) > U(3));
        Assert.True(U(5) <= S(5));
        Assert.True(U(5) >= S(5));
        Assert.True(U(5) == S(5));
        Assert.True(U(5) != U(6));
    }

    [Fact]
    public void EqualityOperatorHandlesNull()
    {
        BitArray nullRef = null;
        Assert.True(nullRef == null);
        Assert.False(U(5) == null);
        Assert.True(U(5) != null);
    }

    [Fact]
    public void CloneIsIndependentAndCopiesSigned()
    {
        var original = S(0x0F);
        var clone = original.Clone();
        original[10] = true;
        Assert.False(clone[10]);
        Assert.True(clone.Signed);
        Assert.Equal(0x0F, clone.As<int>());
    }

    [Fact]
    public void AsReinterpretsLowBytes()
    {
        Assert.Equal(5, U(5).As<int>());
        Assert.Equal(-1, U(0xFFFFFFFF).As<int>());
        Assert.Equal(int.MinValue, U(0x80000000).As<int>());
        Assert.Equal(uint.MaxValue, U(0xFFFFFFFF).As<uint>());
        Assert.Equal(1.0, U(0x3FF0000000000000).As<double>());
    }

    [Fact]
    public void AsReinterpretsAcrossTwoWords()
    {
        Assert.Equal((Int128)5, U(5, 0).As<Int128>());
        Assert.Equal((Int128)(-1), U(0xFFFFFFFFFFFFFFFF, 0xFFFFFFFFFFFFFFFF).As<Int128>());
        Assert.Equal((Int128)5, U(5).As<Int128>());
    }

    [Fact]
    public void AsExactReturnsValueWhenItFits()
    {
        Assert.Equal(uint.MaxValue, U(0xFFFFFFFF).AsExact<uint>());
        Assert.Equal(0, U(0).AsExact<int>());
    }

    [Fact]
    public void AsExactThrowsWhenBitsExceedTargetWidth()
    {
        Assert.Throws<OverflowException>(() => { new BitArray(0x100UL).AsExact<byte>(); });
        Assert.Throws<OverflowException>(() => { new BitArray(0x1FFFFFFFFUL).AsExact<int>(); });
    }

    [Fact]
    public void AsExactIsSignAgnosticForNegativeValues()
    {
        Assert.Throws<OverflowException>(() => { S(0xFFFFFFFFFFFFFFFB).AsExact<int>(); });
    }

    [Fact]
    public void ParseAndToStringRoundTrip()
    {
        Assert.Equal("255", BitArray.Parse("255").ToString());
        Assert.Equal("-5", BitArray.Parse("-5").ToString());
        Assert.Equal("255", BitArray.Parse("0xFF").ToString());
        Assert.Equal("10", BitArray.Parse("0b1010").ToString());
    }

    [Fact]
    public void ParseRejectsInvalidLiterals()
    {
        Assert.Throws<FormatException>(() => { BitArray.Parse("0xZZ"); });
        Assert.Throws<FormatException>(() => { BitArray.Parse("0x"); });
        Assert.Throws<FormatException>(() => { BitArray.Parse(""); });
    }
}
