using System.Text;

using LaquaiLib.Text;

namespace LaquaiLib.UnitTests.Text;

public class MSBuildTests
{
    #region Location shapes - BuildError
    [Fact]
    public void BuildErrorWithLineLocation()
    {
        var result = MSBuild.BuildError("Main.cs", new Line(17), "message");
        Assert.Equal("Main.cs(17): error: message", result);
    }

    [Fact]
    public void BuildErrorWithLineRangeLocation()
    {
        var result = MSBuild.BuildError("Main.cs", new LineRange(3, 9), "message");
        Assert.Equal("Main.cs(3-9): error: message", result);
    }

    [Fact]
    public void BuildErrorWithLineAndColumnLocation()
    {
        var result = MSBuild.BuildError("Main.cs", new LineAndColumn(17, 5), "message");
        Assert.Equal("Main.cs(17,5): error: message", result);
    }

    [Fact]
    public void BuildErrorWithLineAndColumnRangeLocation()
    {
        var result = MSBuild.BuildError("Main.cs", new LineAndColumnRange(17, 5, 12), "message");
        Assert.Equal("Main.cs(17,5-12): error: message", result);
    }

    [Fact]
    public void BuildErrorWithTextRangeLocation()
    {
        var result = MSBuild.BuildError("Main.cs", new TextRange(17, 19, 5, 8), "message");
        Assert.Equal("Main.cs(17,5,19,8): error: message", result);
    }
    #endregion

    #region Location shapes - BuildWarning
    [Fact]
    public void BuildWarningWithLineLocation()
    {
        var result = MSBuild.BuildWarning("Main.cs", new Line(17), "message");
        Assert.Equal("Main.cs(17): warning: message", result);
    }

    [Fact]
    public void BuildWarningWithLineRangeLocation()
    {
        var result = MSBuild.BuildWarning("Main.cs", new LineRange(3, 9), "message");
        Assert.Equal("Main.cs(3-9): warning: message", result);
    }

    [Fact]
    public void BuildWarningWithLineAndColumnLocation()
    {
        var result = MSBuild.BuildWarning("Main.cs", new LineAndColumn(17, 5), "message");
        Assert.Equal("Main.cs(17,5): warning: message", result);
    }

    [Fact]
    public void BuildWarningWithLineAndColumnRangeLocation()
    {
        var result = MSBuild.BuildWarning("Main.cs", new LineAndColumnRange(17, 5, 12), "message");
        Assert.Equal("Main.cs(17,5-12): warning: message", result);
    }

    [Fact]
    public void BuildWarningWithTextRangeLocation()
    {
        var result = MSBuild.BuildWarning("Main.cs", new TextRange(17, 19, 5, 8), "message");
        Assert.Equal("Main.cs(17,5,19,8): warning: message", result);
    }
    #endregion

    #region No location
    [Fact]
    public void BuildErrorWithBlankOrigin()
    {
        var result = MSBuild.BuildError("message");
        Assert.Equal("error: message", result);
    }

    [Fact]
    public void BuildErrorWithToolOrigin()
    {
        var result = MSBuild.BuildError("message", origin: "cl");
        Assert.Equal("cl: error: message", result);
    }
    #endregion

    #region Subcategory and code
    [Fact]
    public void BuildErrorIncludesCode()
    {
        var result = MSBuild.BuildError("message", code: "CS0168");
        Assert.Equal("error CS0168: message", result);
    }

    [Fact]
    public void BuildErrorIncludesSubcategory()
    {
        var result = MSBuild.BuildError("message", subcategory: "Command line");
        Assert.Equal("Command line error: message", result);
    }

    [Fact]
    public void BuildErrorIncludesOriginSubcategoryAndCode()
    {
        var result = MSBuild.BuildError("message", origin: "cl", subcategory: "Command line", code: "D4024");
        Assert.Equal("cl: Command line error D4024: message", result);
    }
    #endregion

    #region TextWriter sink
    [Fact]
    public void BuildErrorWritesToTextWriterUsingWriterNewLine()
    {
        var writer = new BufferTextWriter();
        writer.NewLine = "\n";
        MSBuild.WriteError(writer, "Main.cs", new LineAndColumn(17, 5), "message");
        Assert.Equal("Main.cs(17,5): error: message\n", new string(writer.Span));
    }

    [Fact]
    public void BuildWarningWritesToTextWriterWithCode()
    {
        var writer = new BufferTextWriter();
        writer.NewLine = "\n";
        MSBuild.WriteWarning(writer, "message", code: "CS0168");
        Assert.Equal("warning CS0168: message\n", new string(writer.Span));
    }
    #endregion

    #region Stream sink
    [Fact]
    public void BuildErrorWritesUtf8ToStreamWithCrlf()
    {
        using var stream = new MemoryStream();
        MSBuild.WriteError(stream, "Main.cs", new Line(17), "message");
        Assert.Equal("Main.cs(17): error: message\r\n", Encoding.UTF8.GetString(stream.ToArray()));
    }

    [Fact]
    public void BuildWarningWritesUtf8ToStreamWithCode()
    {
        using var stream = new MemoryStream();
        MSBuild.WriteWarning(stream, "message", code: "CS0168");
        Assert.Equal("warning CS0168: message\r\n", Encoding.UTF8.GetString(stream.ToArray()));
    }
    #endregion

    #region Validation
    [Fact]
    public void BuildErrorThrowsOnNullText()
    {
        Assert.Throws<ArgumentNullException>(() => MSBuild.BuildError(null));
    }

    [Fact]
    public void BuildErrorThrowsOnEmbeddedNewlineInText()
    {
        Assert.Throws<ArgumentException>(() => MSBuild.BuildError("line one\nline two"));
    }

    [Fact]
    public void BuildErrorThrowsOnCodeContainingSpace()
    {
        Assert.Throws<ArgumentException>(() => MSBuild.BuildError("message", code: "CS 0168"));
    }

    [Fact]
    public void BuildErrorThrowsOnCodeContainingColon()
    {
        Assert.Throws<ArgumentException>(() => MSBuild.BuildError("message", code: "CS:0168"));
    }
    #endregion
}
