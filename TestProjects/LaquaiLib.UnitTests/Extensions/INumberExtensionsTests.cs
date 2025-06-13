using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Extensions;

public class NumberExtensionsTests
{
    [Fact]
    public void HasFlagReturnsTrueWhenFlagIsSet()
    {
        var value = 0b1010;
        var flag = 0b0010;

        var result = value.HasFlag(flag);

        Assert.True(result);
    }

    [Fact]
    public void HasFlagReturnsFalseWhenFlagIsNotSet()
    {
        var value = 0b1010;
        var flag = 0b0100;

        var result = value.HasFlag(flag);

        Assert.False(result);
    }

    [Fact]
    public void HasFlagReturnsTrueWhenMultipleFlagsAreSet()
    {
        var value = 0b1111;
        var flags = 0b1010;

        var result = value.HasFlag(flags);

        Assert.True(result);
    }

    [Fact]
    public void HasFlagReturnsFalseWhenNotAllMultipleFlagsAreSet()
    {
        var value = 0b1010;
        var flags = 0b1011;

        var result = value.HasFlag(flags);

        Assert.False(result);
    }

    [Fact]
    public void HasFlagWithLongType()
    {
        var value = 0xFFFF_FFFF_0000_0000;
        var flag = 0x8000_0000_0000_0000;

        var result = value.HasFlag(flag);

        Assert.True(result);
    }

    [Fact]
    public void HasFlagWithUIntType()
    {
        var value = 0xF0F0F0F0;
        var flag = 0xF0000000;

        var result = value.HasFlag(flag);

        Assert.True(result);
    }

    [Fact]
    public void HasFlagWithZeroValueAndZeroFlag()
    {
        var value = 0;
        var flag = 0;

        var result = value.HasFlag(flag);

        Assert.True(result);
    }

    [Fact]
    public void HasFlagWithMaxValueAndAnyFlag()
    {
        var value = int.MaxValue;
        var flag = 0b1010_1010;

        var result = value.HasFlag(flag);

        Assert.True(result);
    }

    [Fact]
    public void AsBinaryReturnsCorrectStringForPositiveInt()
    {
        var value = 42;

        var result = value.AsBinary();

        Assert.Equal("101010", result);
    }

    [Fact]
    public void AsBinaryReturnsCorrectStringForNegativeInt()
    {
        var value = -42;

        var result = value.AsBinary();

        Assert.Equal("11111111111111111111111111010110", result);
    }

    [Fact]
    public void AsBinaryReturnsCorrectStringForUInt()
    {
        uint value = 42;

        var result = value.AsBinary();

        Assert.Equal("101010", result);
    }

    [Fact]
    public void AsBinaryReturnsCorrectStringForMaxValue()
    {
        var value = int.MaxValue;

        var result = value.AsBinary();

        Assert.Equal("1111111111111111111111111111111", result);
    }

    [Fact]
    public void AsBinaryReturnsCorrectStringForMinValue()
    {
        var value = int.MinValue;

        var result = value.AsBinary();

        Assert.Equal("10000000000000000000000000000000", result);
    }

    [Fact]
    public void AsBinaryReturnsCorrectStringForZero()
    {
        var value = 0;

        var result = value.AsBinary();

        Assert.Equal("0", result);
    }

    [Fact]
    public void AsHexReturnsCorrectStringForPositiveInt()
    {
        var value = 42;

        var result = value.AsHex();

        Assert.Equal("2A", result);
    }

    [Fact]
    public void AsHexReturnsCorrectStringForNegativeInt()
    {
        var value = -42;

        var result = value.AsHex();

        Assert.Equal("FFFFFFD6", result);
    }

    [Fact]
    public void AsHexReturnsCorrectStringForUInt()
    {
        var value = 0xDEADBEEF;

        var result = value.AsHex();

        Assert.Equal("DEADBEEF", result);
    }

    [Fact]
    public void AsHexReturnsCorrectStringForMaxValue()
    {
        var value = int.MaxValue;

        var result = value.AsHex();

        Assert.Equal("7FFFFFFF", result);
    }

    [Fact]
    public void AsHexReturnsCorrectStringForMinValue()
    {
        var value = int.MinValue;

        var result = value.AsHex();

        Assert.Equal("80000000", result);
    }

    [Fact]
    public void AsHexReturnsCorrectStringForZero()
    {
        var value = 0;

        var result = value.AsHex();

        Assert.Equal("0", result);
    }

    [Fact]
    public void AsHexWithLongType()
    {
        var value = 0x123456789ABCDEF0;

        var result = value.AsHex();

        Assert.Equal("123456789ABCDEF0", result);
    }

    [Fact]
    public void AsBinaryWithByteType()
    {
        byte value = 15;

        var result = value.AsBinary();

        Assert.Equal("1111", result);
    }
}