using LaquaiLib.Text;

namespace LaquaiLib.UnitTests.Text;

public class StringHelperTests
{
    [Fact]
    public void ZeroLengthReturnsEmptyString()
    {
        var str = StringHelpers.AllocString(0);
        Assert.NotNull(str);
        Assert.Equal("", str);
    }
    [Fact]
    public void OneLengthReturnsOneLengthString()
    {
        var str = StringHelpers.AllocString(1);
        Assert.NotNull(str);
        Assert.Equal(1, str.Length);
    }
    [Fact]
    public void GetSpanReturnsMutableSpan()
    {
        var str = "mystring";
        var span = StringHelpers.GetSpan(str);

        Assert.Equal(str.Length, span.Length);

        span[0] = 'M';
        Assert.Equal("Mystring", str);
    }
}
