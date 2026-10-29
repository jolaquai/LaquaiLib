using System.Text.RegularExpressions;

using LaquaiLib.UnsafeUtils.Accessors;

namespace LaquaiLib.UnitTests.UnsafeUtils.Accessors;

public class RegexAccessorsTests
{
    [Fact]
    public void RegexFieldReflectsTheMatchingPattern()
    {
        var match = Regex.Match("abc", "b");

        ref var regex = ref RegexAccessors._regex(match);

        Assert.Equal("b", regex.ToString());
    }

    [Fact]
    public void GetTextReturnsTheOriginalInputAndAgreesWithValue()
    {
        var match = Regex.Match("abc", "b");
        Capture capture = match;

        var text = RegexAccessors.get_Text(capture);

        Assert.Equal("abc", text);
        Assert.Equal(match.Value, text.Substring(match.Index, match.Length));
    }
}
