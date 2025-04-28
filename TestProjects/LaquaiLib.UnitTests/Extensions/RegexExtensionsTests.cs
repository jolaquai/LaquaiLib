using System.Text.RegularExpressions;

using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Extensions;

public class RegexExtensionsTests
{
    [Fact]
    public void GetRegexReturnsOriginalRegexInstance()
    {
        var regex = new Regex(@"\d+");
        var match = regex.Match("abc123def");

        var result = match.GetRegex();

        Assert.Same(regex, result);
    }

    [Fact]
    public void GetRegexThrowsForNullMatch()
    {
        Match match = null;

        Assert.Throws<ArgumentNullException>(() => match.GetRegex());
    }

    [Fact]
    public void GetRegexReturnsCorrectRegexPattern()
    {
        var pattern = @"\d+";
        var regex = new Regex(pattern);
        var match = regex.Match("abc123def");

        var result = match.GetRegex();

        Assert.Equal(pattern, result.ToString());
    }

    [Fact]
    public void GetRegexWorksWithDifferentRegexOptions()
    {
        var pattern = "abc";
        var regex = new Regex(pattern, RegexOptions.IgnoreCase);
        var match = regex.Match("ABC123");

        var result = match.GetRegex();

        Assert.Equal(RegexOptions.IgnoreCase, result.Options);
    }

    [Fact]
    public void GetOriginalTextReturnsCorrectText()
    {
        var original = "abc123def";
        var regex = new Regex(@"\d+");
        var match = regex.Match(original);

        var result = match.GetOriginalText();

        Assert.Equal(original, result);
    }

    [Fact]
    public void GetOriginalTextWorksWithGroups()
    {
        var original = "abc123def456";
        var regex = new Regex(@"(\d+)def(\d+)");
        var match = regex.Match(original);
        var group = match.Groups[1];

        var result = group.GetOriginalText();

        Assert.Equal(original, result);
    }

    [Fact]
    public void GetOriginalTextWorksWithCaptures()
    {
        var original = "abc123def123ghi";
        var regex = new Regex(@"(\d+)");
        var match = regex.Match(original);
        var group = match.Groups[1];
        var capture = group.Captures[0];

        var result = capture.GetOriginalText();

        Assert.Equal(original, result);
    }

    [Fact]
    public void GetOriginalTextThrowsForNullCapture()
    {
        Capture capture = null;

        Assert.Throws<ArgumentNullException>(() => capture.GetOriginalText());
    }

    [Fact]
    public void GetOriginalTextReturnsSameStringForAllMatchesOnSameInput()
    {
        var original = "abc123def456ghi";
        var regex = new Regex(@"\d+");
        var matches = regex.Matches(original);

        var result1 = matches[0].GetOriginalText();
        var result2 = matches[1].GetOriginalText();

        Assert.Same(result1, result2);
        Assert.Equal(original, result1);
    }
}