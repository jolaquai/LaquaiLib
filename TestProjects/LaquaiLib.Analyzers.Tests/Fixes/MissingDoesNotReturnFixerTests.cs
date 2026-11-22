using LaquaiLib.Analyzers.Fixes.Fixes;
using LaquaiLib.Analyzers.Quality__1XXX_;

namespace LaquaiLib.Analyzers.Tests.Fixes;

public class MissingDoesNotReturnFixerTests
{
    private static Task VerifyFix(string source, string fixedSource)
        => new CSharpCodeFixTest<MissingDoesNotReturnAnalyzer, MissingDoesNotReturnFixer, DefaultVerifier>
        {
            TestCode = source.ReplaceLineEndings("\r\n"),
            FixedCode = fixedSource.ReplaceLineEndings("\r\n"),
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestState = { AnalyzerConfigFiles = { ("/.editorconfig", "root = true\n\n[*.cs]\nend_of_line = crlf\n") } },
        }.RunAsync();

    private const string Using =
        """
        using System;
        using System.Diagnostics.CodeAnalysis;

        """;

    [Fact]
    public Task ExpressionBodiedMethodGetsAttributePrepended()
        => VerifyFix(
            Using + """
            class C
            {
                [DoesNotReturn] static void Throw() => throw new Exception();
                static void {|LAQ1002:M|}() => Throw();
            }
            """,
            Using + """
            class C
            {
                [DoesNotReturn] static void Throw() => throw new Exception();
                [DoesNotReturn]
                static void M() => Throw();
            }
            """
        );

    [Fact]
    public Task BlockBodiedMethodGetsAttributePrepended()
        => VerifyFix(
            Using + """
            class C
            {
                [DoesNotReturn] static void Throw() => throw new Exception();
                static void {|LAQ1002:M|}()
                {
                    Throw();
                }
            }
            """,
            Using + """
            class C
            {
                [DoesNotReturn] static void Throw() => throw new Exception();

                [DoesNotReturn]
                static void M()
                {
                    Throw();
                }
            }
            """
        );

    [Fact]
    public Task GetAccessorGetsAttributePrepended()
        => VerifyFix(
            Using + """
            class C
            {
                [DoesNotReturn] static void ThrowHelper() => throw new Exception();
                long Length
                {
                    {|LAQ1002:get|}
                    {
                        ThrowHelper();
                        return default;
                    }
                }
            }
            """,
            Using + """
            class C
            {
                [DoesNotReturn] static void ThrowHelper() => throw new Exception();
                long Length
                {
                    [DoesNotReturn]
                    get
                    {
                        ThrowHelper();
                        return default;
                    }
                }
            }
            """
        );

    // [DoesNotReturn] only ever targets a method (CS0657 on any other target specifier), so an expression-bodied
    // property has no accessor of its own to carry it; the fixer expands the arrow body into an explicit 'get'
    [Fact]
    public Task ExpressionBodiedPropertyExpandsToAttributedGetAccessor()
        => VerifyFix(
            Using + """
            class C
            {
                long Position
                {
                    [DoesNotReturn] get => throw new NotSupportedException();
                }
                long {|LAQ1002:Other|} => Position;
            }
            """,
            Using + """
            class C
            {
                long Position
                {
                    [DoesNotReturn] get => throw new NotSupportedException();
                }
                long Other { [DoesNotReturn] get => Position; }
            }
            """
        );

    [Fact]
    public Task ExpressionBodiedIndexerExpandsToAttributedGetAccessor()
        => VerifyFix(
            Using + """
            class C
            {
                [DoesNotReturn] static int ThrowHelper() => throw new Exception();
                int {|LAQ1002:this|}[int i] => ThrowHelper();
            }
            """,
            Using + """
            class C
            {
                [DoesNotReturn] static int ThrowHelper() => throw new Exception();
                int this[int i] { [DoesNotReturn] get => ThrowHelper(); }
            }
            """
        );
}
