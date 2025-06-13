using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Extensions;

public class CharExtensionsTests
{
    [Fact]
    public void IsVowelReturnsTrueForLowerCaseVowels()
    {
        Assert.True('a'.IsVowel());
        Assert.True('e'.IsVowel());
        Assert.True('i'.IsVowel());
        Assert.True('o'.IsVowel());
        Assert.True('u'.IsVowel());
        Assert.True('y'.IsVowel());
    }

    [Fact]
    public void IsVowelReturnsTrueForUpperCaseVowels()
    {
        Assert.True('A'.IsVowel());
        Assert.True('E'.IsVowel());
        Assert.True('I'.IsVowel());
        Assert.True('O'.IsVowel());
        Assert.True('U'.IsVowel());
        Assert.True('Y'.IsVowel());
    }

    [Fact]
    public void IsVowelReturnsFalseForConsonants()
    {
        Assert.False('b'.IsVowel());
        Assert.False('c'.IsVowel());
        Assert.False('d'.IsVowel());
        Assert.False('f'.IsVowel());
        Assert.False('g'.IsVowel());
        Assert.False('z'.IsVowel());
    }

    [Fact]
    public void IsVowelReturnsFalseForNonLetters()
    {
        Assert.False('1'.IsVowel());
        Assert.False('@'.IsVowel());
        Assert.False(' '.IsVowel());
        Assert.False('\n'.IsVowel());
    }

    [Fact]
    public void IsConsonantReturnsTrueForLowerCaseConsonants()
    {
        Assert.True('b'.IsConsonant());
        Assert.True('c'.IsConsonant());
        Assert.True('d'.IsConsonant());
        Assert.True('f'.IsConsonant());
        Assert.True('g'.IsConsonant());
        Assert.True('z'.IsConsonant());
    }

    [Fact]
    public void IsConsonantReturnsTrueForUpperCaseConsonants()
    {
        Assert.True('B'.IsConsonant());
        Assert.True('C'.IsConsonant());
        Assert.True('D'.IsConsonant());
        Assert.True('F'.IsConsonant());
        Assert.True('G'.IsConsonant());
        Assert.True('Z'.IsConsonant());
    }

    [Fact]
    public void IsConsonantReturnsFalseForVowels()
    {
        Assert.False('a'.IsConsonant());
        Assert.False('e'.IsConsonant());
        Assert.False('i'.IsConsonant());
        Assert.False('o'.IsConsonant());
        Assert.False('u'.IsConsonant());
        Assert.False('y'.IsConsonant());
        Assert.False('A'.IsConsonant());
        Assert.False('E'.IsConsonant());
    }

    [Fact]
    public void IsConsonantReturnsFalseForNonLetters()
    {
        Assert.False('1'.IsConsonant());
        Assert.False('@'.IsConsonant());
        Assert.False(' '.IsConsonant());
        Assert.False('\n'.IsConsonant());
    }
}