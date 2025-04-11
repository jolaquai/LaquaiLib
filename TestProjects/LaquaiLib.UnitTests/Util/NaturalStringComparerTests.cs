using LaquaiLib.Util;

namespace LaquaiLib.UnitTests.Util;

public class NaturalStringComparerTests
{
    [Fact]
    public void CompareRegularStrings()
    {
        var comparer = new NaturalStringComparer();

        Assert.Equal(0, comparer.Compare("abc", "abc"));
        Assert.True(comparer.Compare("abc", "def") < 0);
        Assert.True(comparer.Compare("def", "abc") > 0);
        Assert.Equal(0, comparer.Compare("ABC", "abc"));
    }

    [Fact]
    public void CompareArabicNumerals()
    {
        var comparer = new NaturalStringComparer();

        Assert.Equal(0, comparer.Compare("123", "123"));
        Assert.True(comparer.Compare("123", "456") < 0);
        Assert.True(comparer.Compare("456", "123") > 0);
        Assert.True(comparer.Compare("5", "10") < 0);
        Assert.True(comparer.Compare("10", "5") > 0);
    }

    [Fact]
    public void CompareStringsWithNumbers()
    {
        var comparer = new NaturalStringComparer();

        Assert.Equal(0, comparer.Compare("file123", "file123"));
        Assert.True(comparer.Compare("file5", "file10") < 0);
        Assert.True(comparer.Compare("file10", "file5") > 0);
        Assert.True(comparer.Compare("file10", "file10a") < 0);
    }

    [Fact]
    public void CompareLargeNumbers()
    {
        var comparer = new NaturalStringComparer();

        Assert.True(comparer.Compare("file999999999", "file1000000000") < 0);
        Assert.True(comparer.Compare("file1000000000", "file999999999") > 0);

        Assert.True(comparer.Compare("file2147483647", "file2147483648") < 0);
        Assert.True(comparer.Compare("file2147483648", "file2147483647") > 0);
    }

    [Fact]
    public void CompareRomanNumerals()
    {
        var comparer = new NaturalStringComparer();

        Assert.Equal(0, comparer.Compare("III", "III"));
        Assert.True(comparer.Compare("II", "III") < 0);
        Assert.True(comparer.Compare("V", "I") > 0);
        Assert.True(comparer.Compare("X", "IX") > 0);
        Assert.True(comparer.Compare("i", "II") < 0);
    }

    [Fact]
    public void CompareStringsWithRomanNumerals()
    {
        var comparer = new NaturalStringComparer();

        Assert.Equal(0, comparer.Compare("Chapter V", "Chapter V"));
        Assert.True(comparer.Compare("Chapter I", "Chapter II") < 0);
        Assert.True(comparer.Compare("Chapter V", "Chapter I") > 0);
        Assert.True(comparer.Compare("Chapter X", "Chapter IX") > 0);
    }

    [Fact]
    public void CompareValidRomanNumerals()
    {
        var comparer = new NaturalStringComparer();

        Assert.True(comparer.Compare("I", "II") < 0);
        Assert.True(comparer.Compare("IV", "V") < 0);
        Assert.True(comparer.Compare("MCMXCIX", "MM") < 0);
    }

    [Fact]
    public void CompareInvalidRomanNumerals()
    {
        var comparer = new NaturalStringComparer();

        Assert.True(comparer.Compare("IIII", "III") > 0);
        Assert.True(comparer.Compare("IXL", "XL") < 0);
        Assert.True(comparer.Compare("ABC", "DEF") < 0);
    }

    [Fact]
    public void CompareCaseInsensitiveRomanNumerals()
    {
        var comparer = new NaturalStringComparer();

        Assert.True(comparer.Compare("chapter i", "chapter ii") < 0);
        Assert.True(comparer.Compare("chapter I", "chapter ii") < 0);
        Assert.True(comparer.Compare("chapter i", "chapter II") < 0);
        Assert.True(comparer.Compare("CHAPTER I", "chapter II") < 0);
    }

    [Fact]
    public void CompareMixedContent()
    {
        var comparer = new NaturalStringComparer();

        Assert.True(comparer.Compare("a1", "a2") < 0);
        Assert.True(comparer.Compare("a10", "a2") > 0);
        Assert.True(comparer.Compare("a-I", "a-II") < 0);
        Assert.True(comparer.Compare("aIII", "aXI") < 0);
        Assert.True(comparer.Compare("file1", "fileI") < 0);
    }

    [Fact]
    public void CompareMixedContentsWithinStrings()
    {
        var comparer = new NaturalStringComparer();

        Assert.True(comparer.Compare("File1Page2", "File1Page10") < 0);
        Assert.True(comparer.Compare("ChapterISection1", "ChapterISection2") < 0);
        Assert.True(comparer.Compare("ChapterISection10", "ChapterIISection1") < 0);
        Assert.True(comparer.Compare("A1B2C3", "A1B2C10") < 0);
        Assert.True(comparer.Compare("A1B10C3", "A1B2C10") > 0);
    }

    [Fact]
    public void CompareComplexMixedContent()
    {
        var comparer = new NaturalStringComparer();
        var unsorted = new List<string>
            {
                "file10.txt",
                "file1.txt",
                "file2.txt",
                "fileIX.txt",
                "fileV.txt",
                "fileXI.txt",
                "fileI.txt"
            };

        var expected = new List<string>
            {
                "file1.txt",
                "file2.txt",
                "file10.txt",
                "fileI.txt",
                "fileV.txt",
                "fileIX.txt",
                "fileXI.txt"
            };

        unsorted.Sort(comparer);

        Assert.Equal(expected, unsorted);
    }

    [Fact]
    public void CompareEdgeCases()
    {
        var comparer = new NaturalStringComparer();

        Assert.Equal(0, comparer.Compare("", ""));
        Assert.True(comparer.Compare(null, "abc") < 0);
        Assert.True(comparer.Compare("abc", null) > 0);
        Assert.Equal(0, comparer.Compare(null, null));
        Assert.True(comparer.Compare("", "abc") < 0);
        Assert.True(comparer.Compare("abc", "") > 0);
    }

    [Fact]
    public void SortLargeDataset()
    {
        var comparer = new NaturalStringComparer();
        var unsorted = new List<string>
            {
                "z1.txt",
                "z10.txt",
                "z100.txt",
                "z11.txt",
                "z12.txt",
                "z13.txt",
                "z2.txt",
                "z22.txt",
                "z3.txt"
            };

        var expected = new List<string>
            {
                "z1.txt",
                "z2.txt",
                "z3.txt",
                "z10.txt",
                "z11.txt",
                "z12.txt",
                "z13.txt",
                "z22.txt",
                "z100.txt"
            };

        unsorted.Sort(comparer);

        Assert.Equal(expected, unsorted);
    }

    [Fact]
    public void SortPracticalExamples()
    {
        var comparer = new NaturalStringComparer();
        var unsorted = new List<string>
            {
                "Image 1.jpg",
                "Image 10.jpg",
                "Image 2.jpg",
                "Image 3.jpg",
                "Chapter I.docx",
                "Chapter V.docx",
                "Chapter X.docx",
                "Report (1).pdf",
                "Report (10).pdf",
                "Report (2).pdf"
            };

        var expected = new List<string>
            {
                "Chapter I.docx",
                "Chapter V.docx",
                "Chapter X.docx",
                "Image 1.jpg",
                "Image 2.jpg",
                "Image 3.jpg",
                "Image 10.jpg",
                "Report (1).pdf",
                "Report (2).pdf",
                "Report (10).pdf"
            };

        unsorted.Sort(comparer);

        Assert.Equal(expected, unsorted);
    }

    [Fact]
    public void InstanceShouldReturnSingletonInstance()
    {
        var instance1 = NaturalStringComparer.Instance;
        var instance2 = NaturalStringComparer.Instance;

        Assert.NotNull(instance1);
        Assert.Same(instance1, instance2);
        Assert.IsType<NaturalStringComparer>(instance1);
    }

    [Fact]
    public void GetNumberExtractionShouldWork()
    {
        var comparer = new NaturalStringComparer();

        Assert.True(comparer.Compare("file123abc", "file123def") < 0);
        Assert.True(comparer.Compare("file9999999999", "file10000000000") < 0);
    }

    [Fact]
    public void RomanNumeralIdentificationShouldWork()
    {
        var comparer = new NaturalStringComparer();

        Assert.True(comparer.Compare("ChapterI", "ChapterII") < 0);
        Assert.True(comparer.Compare("SectionIV", "SectionV") < 0);
    }

    [Fact]
    public void MalformedRomanNumeralsAreComparedAsStrings()
    {
        var comparer = new NaturalStringComparer();

        Assert.True(comparer.Compare("ChapterXXXXX", "ChapterMMMMM") > 0);
        Assert.True(comparer.Compare("ChapterIIII", "ChapterIV") < 0);
    }

    [Fact]
    public void RomanToIntConversionShouldWork()
    {
        var comparer = new NaturalStringComparer();

        Assert.True(comparer.Compare("I", "II") < 0);
        Assert.True(comparer.Compare("IV", "VI") < 0);
        Assert.True(comparer.Compare("IX", "X") < 0);
        Assert.True(comparer.Compare("XIV", "XVI") < 0);
        Assert.True(comparer.Compare("MCMXCIX", "MM") < 0);
    }

    [Fact]
    public void SpecialRomanNumeralCasesShouldWork()
    {
        var comparer = new NaturalStringComparer();

        Assert.True(comparer.Compare("Chapter III", "Chapter IV") < 0);
        Assert.True(comparer.Compare("Chapter IX", "Chapter X") < 0);
        Assert.True(comparer.Compare("Chapter XIX", "Chapter XX") < 0);

        Assert.True(comparer.Compare("Chapter iV", "Chapter Vi") < 0);
        Assert.True(comparer.Compare("Chapter vIi", "Chapter vIIi") < 0);
    }
}
