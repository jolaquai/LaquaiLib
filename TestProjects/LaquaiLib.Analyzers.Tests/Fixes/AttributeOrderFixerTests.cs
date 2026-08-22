using LaquaiLib.Analyzers.Fixes.Fixes;
using LaquaiLib.Analyzers.Quality__1XXX_;

namespace LaquaiLib.Analyzers.Tests.Fixes;

public class AttributeOrderFixerTests
{
    private static Task VerifyFix(string source, string fixedSource)
        => new CSharpCodeFixTest<AttributeOrderAnalyzer, AttributeOrderFixer, DefaultVerifier>
        {
            // Raw string literals normalize embedded newlines to '\n'; the formatter honors the CRLF .editorconfig below for any
            // line break it introduces, so both sides need to agree on CRLF explicitly rather than through file-level line endings
            TestCode = source.ReplaceLineEndings("\r\n"),
            FixedCode = fixedSource.ReplaceLineEndings("\r\n"),
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            // Without this the formatter reflows with Environment.NewLine, making the expected sources platform-dependent
            TestState = { AnalyzerConfigFiles = { ("/.editorconfig", "root = true\n\n[*.cs]\nend_of_line = crlf\n") } },
        }.RunAsync();

    private const string Attributes =
        """
        using System.Diagnostics.CodeAnalysis;
        using System.Runtime.CompilerServices;

        """;

    // Already alphabetical - untouched
    [Fact]
    public Task AlreadyOrderedIsLeftAlone()
        => VerifyFix(
            Attributes + """
            class C
            {
                [DoesNotReturn][MethodImpl(MethodImplOptions.NoInlining)]
                static void M() => throw null;
            }
            """,
            Attributes + """
            class C
            {
                [DoesNotReturn][MethodImpl(MethodImplOptions.NoInlining)]
                static void M() => throw null;
            }
            """
        );

    // Two single-attribute lists just swap places, each keeping its own brackets
    [Fact]
    public Task ReversedSeparateListsSwapAsBlocks()
        => VerifyFix(
            Attributes + """
            class C
            {
                {|LAQ1001:[MethodImpl(MethodImplOptions.NoInlining)][DoesNotReturn]|}
                static void M() => throw null;
            }
            """,
            Attributes + """
            class C
            {
                [DoesNotReturn][MethodImpl(MethodImplOptions.NoInlining)]
                static void M() => throw null;
            }
            """
        );

    // A single list's own attributes are reordered in place, without splitting the list
    [Fact]
    public Task ReversedSingleListReordersInPlace()
        => VerifyFix(
            Attributes + """
            class C
            {
                {|LAQ1001:[MethodImpl(MethodImplOptions.NoInlining), DoesNotReturn]|}
                static void M() => throw null;
            }
            """,
            Attributes + """
            class C
            {
                [DoesNotReturn][MethodImpl(MethodImplOptions.NoInlining)]
                static void M() => throw null;
            }
            """
        );

    // DoesNotReturn and MethodImpl can't stay adjacent once ExcludeFromCodeCoverage sorts between them, so every
    // attribute is split into its own list
    [Fact]
    public Task InterleavingAttributesAreSplitIntoIndividualLists()
        => VerifyFix(
            Attributes + """
            class C
            {
                {|LAQ1001:[MethodImpl(MethodImplOptions.NoInlining), DoesNotReturn][ExcludeFromCodeCoverage]|}
                static void M() => throw null;
            }
            """,
            Attributes + """
            class C
            {
                [DoesNotReturn]
                [ExcludeFromCodeCoverage]
                [MethodImpl(MethodImplOptions.NoInlining)]
                static void M() => throw null;
            }
            """
        );

    [Fact]
    public Task ParameterAttributesAreReordered()
        => VerifyFix(
            Attributes + """
            class C
            {
                static void M({|LAQ1001:[DisallowNull, AllowNull]|} object x) { }
            }
            """,
            Attributes + """
            class C
            {
                static void M([AllowNull, DisallowNull] object x) { }
            }
            """
        );
}
