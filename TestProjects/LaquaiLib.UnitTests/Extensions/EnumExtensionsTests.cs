using System.ComponentModel;

using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Extensions;

public class EnumExtensionsTests
{
    #region Test Enums

    private enum TestEnum
    {
        [Description("First Value")]
        First = 1,

        Second = 2,

        [Description("Third Value")]
        Third = 3
    }

    [Flags]
    private enum TestFlagsEnum
    {
        None = 0,
        Flag1 = 1,
        Flag2 = 2,
        Flag3 = 4,
        Flag4 = 8,
        Flag1And2 = Flag1 | Flag2,
        Flag3And4 = Flag3 | Flag4,
        All = Flag1 | Flag2 | Flag3 | Flag4
    }

    private enum NonFlagsEnum
    {
        Value1 = 1,
        Value2 = 2,
        Value3 = 3
    }

    #endregion

    #region GetDescriptionTests

    [Fact]
    public void GetDescriptionWithDescriptionAttributeReturnsDescription()
    {
        var value = TestEnum.First;

        var result = value.Description;

        Assert.Equal("First Value", result);
    }

    [Fact]
    public void GetDescriptionWithoutDescriptionAttributeReturnsToString()
    {
        var value = TestEnum.Second;

        var result = value.Description;

        Assert.Equal("Second", result);
    }

    [Fact]
    public void GetDescriptionWithFlagsEnumReturnsToString()
    {
        var value = TestFlagsEnum.Flag1And2;

        var result = value.Description;

        Assert.Equal("Flag1And2", result);
    }

    #endregion

    #region GetFlagsTests

    [Fact]
    public void GetFlagsWithSingleFlagReturnsEmptyArray()
    {
        var value = TestFlagsEnum.Flag1;

        var result = value.Flags;

        Assert.Equal(2, result.Length);
        Assert.Contains(TestFlagsEnum.None, result);
        Assert.Contains(TestFlagsEnum.Flag1, result);
    }

    [Fact]
    public void GetFlagsWithMultipleFlagsReturnsFlags()
    {
        var value = TestFlagsEnum.Flag1And2;

        var result = value.Flags;

        Assert.Equal(4, result.Length);
        Assert.Contains(TestFlagsEnum.None, result);
        Assert.Contains(TestFlagsEnum.Flag1, result);
        Assert.Contains(TestFlagsEnum.Flag2, result);
        Assert.Contains(TestFlagsEnum.Flag1And2, result);
    }

    [Fact]
    public void GetFlagsWithAllFlagsReturnsAllFlags()
    {
        var value = TestFlagsEnum.All;

        var result = value.Flags;

        Assert.Equal(8, result.Length);
        Assert.Contains(TestFlagsEnum.None, result);
        Assert.Contains(TestFlagsEnum.Flag1, result);
        Assert.Contains(TestFlagsEnum.Flag2, result);
        Assert.Contains(TestFlagsEnum.Flag3, result);
        Assert.Contains(TestFlagsEnum.Flag4, result);
        Assert.Contains(TestFlagsEnum.Flag1And2, result);
        Assert.Contains(TestFlagsEnum.Flag3And4, result);
        Assert.Contains(TestFlagsEnum.All, result);
    }

    [Fact]
    public void GetFlagsWithNoneFlagReturnsEmptyArray()
    {
        var value = TestFlagsEnum.None;

        var result = value.Flags;

        Assert.Contains(TestFlagsEnum.None, result);
    }

    [Fact]
    public void GetFlagsWithNonFlagsEnumThrowsArgumentException()
    {
        var value = NonFlagsEnum.Value1;

        var exception = Assert.Throws<ArgumentException>(() => value.Flags);
        Assert.Contains("is not marked with [FlagsAttribute]", exception.Message);
    }

    #endregion

    #region HasValueTests

    [Fact]
    public void HasValueWithZeroValueReturnsFalse()
    {
        var value = TestFlagsEnum.None;

        var result = value.HasValue();

        Assert.False(result);
    }

    [Fact]
    public void HasValueWithSingleFlagReturnsTrue()
    {
        var value = TestFlagsEnum.Flag1;

        var result = value.HasValue();

        Assert.True(result);
    }

    [Fact]
    public void HasValueWithMultipleFlagsReturnsTrue()
    {
        var value = TestFlagsEnum.Flag1And2;

        var result = value.HasValue();

        Assert.True(result);
    }

    [Fact]
    public void HasValueWithCommonFlagReturnsTrue()
    {
        var value1 = TestFlagsEnum.Flag1And2;
        var value2 = TestFlagsEnum.Flag1;

        var result = value1.HasValue(value2);

        Assert.True(result);
    }

    [Fact]
    public void HasValueWithNoCommonFlagReturnsFalse()
    {
        var value1 = TestFlagsEnum.Flag1And2;
        var value2 = TestFlagsEnum.Flag3And4;

        var result = value1.HasValue(value2);

        Assert.False(result);
    }

    [Fact]
    public void HasValueWithZeroAndNonZeroReturnsFalse()
    {
        var value1 = TestFlagsEnum.None;
        var value2 = TestFlagsEnum.Flag1;

        var result = value1.HasValue(value2);

        Assert.False(result);
    }

    #endregion
}
