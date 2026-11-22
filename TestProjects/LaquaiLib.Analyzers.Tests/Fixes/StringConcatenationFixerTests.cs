using LaquaiLib.Analyzers.Fixes.Fixes;
using LaquaiLib.Analyzers.Performance__0XXX_;

namespace LaquaiLib.Analyzers.Tests.Fixes;

public class StringConcatenationFixerTests
{
    private static Task VerifyFix(string source, string fixedSource)
        => new CSharpCodeFixTest<StringConcatenationAnalyzer, StringConcatenationFixer, DefaultVerifier>
        {
            // Raw string literals normalize embedded newlines to '\n'; the formatter honors the CRLF .editorconfig below for any
            // line break it introduces, so both sides need to agree on CRLF explicitly rather than through file-level line endings
            TestCode = source.ReplaceLineEndings("\r\n"),
            FixedCode = fixedSource.ReplaceLineEndings("\r\n"),
            ReferenceAssemblies = ReferenceAssemblies.Net.Net100,
            // Without this the formatter reflows with Environment.NewLine, making the expected sources platform-dependent
            TestState = { AnalyzerConfigFiles = { ("/.editorconfig", "root = true\n\n[*.cs]\nend_of_line = crlf\n") } },
        }.RunAsync();

    #region string.Concat
    [Fact]
    public Task TwoStringsBecomeConcat()
        => VerifyFix(
            """
            class C
            {
                string M(string a, string b) => {|LAQ0007:a + b|};
            }
            """,
            """
            class C
            {
                string M(string a, string b) => string.Concat(a, b);
            }
            """
        );

    [Fact]
    public Task FiveStringsBecomeConcat()
        => VerifyFix(
            """
            class C
            {
                string M(string a, string b, string c, string d, string e) => {|LAQ0007:a + b + c + d + e|};
            }
            """,
            """
            class C
            {
                string M(string a, string b, string c, string d, string e) => string.Concat(a, b, c, d, e);
            }
            """
        );

    [Fact]
    public Task LiteralsAroundAHoleSurviveAsArguments()
        => VerifyFix(
            """
            class C
            {
                string M(string a) => {|LAQ0007:"x" + a + "y"|};
            }
            """,
            """
            class C
            {
                string M(string a) => string.Concat("x", a, "y");
            }
            """
        );

    [Fact]
    public Task SpanHolesBecomeConcat()
        => VerifyFix(
            """
            using System;
            class C
            {
                string M(ReadOnlySpan<char> a, ReadOnlySpan<char> b) => {|LAQ0007:$"{a}{b}"|};
            }
            """,
            """
            using System;
            class C
            {
                string M(ReadOnlySpan<char> a, ReadOnlySpan<char> b) => string.Concat(a, b);
            }
            """
        );

    [Fact]
    public Task TextAroundASpanHoleBecomesLiteralArguments()
        => VerifyFix(
            """
            using System;
            class C
            {
                string M(ReadOnlySpan<char> a, string b) => {|LAQ0007:$"[{a}{b}]"|};
            }
            """,
            """
            using System;
            class C
            {
                string M(ReadOnlySpan<char> a, string b) => string.Concat("[", a, b, "]");
            }
            """
        );

    [Fact]
    public Task FiveInterpolatedPartsBecomeConcat()
        => VerifyFix(
            """
            class C
            {
                string M(string a, string b, string c, string d, string e) => {|LAQ0007:$"{a}{b}{c}{d}{e}"|};
            }
            """,
            """
            class C
            {
                string M(string a, string b, string c, string d, string e) => string.Concat(a, b, c, d, e);
            }
            """
        );

    [Fact]
    public Task ChainInsideAHoleIsFlattenedIntoConcat()
        => VerifyFix(
            """
            class C
            {
                string M(string a, string b, string c, string d) => {|LAQ0007:$"{a + b}{c}{d}x"|};
            }
            """,
            """
            class C
            {
                string M(string a, string b, string c, string d) => string.Concat(a, b, c, d, "x");
            }
            """
        );
    [Fact]
    public Task EscapedBracesRoundTripIntoALiteralArgument()
        => VerifyFix(
            """
            class C
            {
                string M(string a, string b, string c, string d) => {|LAQ0007:$"{{x}}{a}{b}{c}{d}"|};
            }
            """,
            """
            class C
            {
                string M(string a, string b, string c, string d) => string.Concat("{x}", a, b, c, d);
            }
            """
        );
    #endregion

    #region interpolated string
    [Fact]
    public Task NonStringPartBecomesAHole()
        => VerifyFix(
            """
            class C
            {
                string M(string a, int b) => {|LAQ0007:a + b|};
            }
            """,
            """
            class C
            {
                string M(string a, int b) => $"{a}{b}";
            }
            """
        );

    [Fact]
    public Task LiteralsBecomeTextRuns()
        => VerifyFix(
            """
            class C
            {
                string M(int a) => {|LAQ0007:"x" + a + "y"|};
            }
            """,
            """
            class C
            {
                string M(int a) => $"x{a}y";
            }
            """
        );

    // The markup parser reads {{ and }} as escaped braces
    [Fact]
    public Task BracesInLiteralsAreEscaped()
        => VerifyFix(
            """
            class C
            {
                string M(int a) => {|LAQ0007:"{x}" + a|};
            }
            """,
            """
            class C
            {
                string M(int a) => $"{{x}}{a}";
            }
            """
        );

    [Fact]
    public Task LowPrecedenceOperandIsParenthesized()
        => VerifyFix(
            """
            class C
            {
                string M(string a, object b) => {|LAQ0007:a + (b ?? "x")|};
            }
            """,
            """
            class C
            {
                string M(string a, object b) => $"{a}{(b ?? "x")}";
            }
            """
        );

    [Fact]
    public Task ConditionalOperandIsParenthesized()
        => VerifyFix(
            """
            class C
            {
                string M(bool a, int b, int c) => {|LAQ0007:"x" + (a ? b : c)|};
            }
            """,
            """
            class C
            {
                string M(bool a, int b, int c) => $"x{(a ? b : c)}";
            }
            """
        );

    [Fact]
    public Task VerbatimLiteralKeepsItsForm()
        => VerifyFix(
            """
            class C
            {
                string M(int a) => {|LAQ0007:@"x\y" + a|};
            }
            """,
            """
            class C
            {
                string M(int a) => $@"x\y{a}";
            }
            """
        );

    [Fact]
    public Task FormatSpecifierInANestedStringSurvives()
        => VerifyFix(
            """
            class C
            {
                string M(System.DateTime a, object b) => {|LAQ0007:$"{a:yyyy}" + b|};
            }
            """,
            """
            class C
            {
                string M(System.DateTime a, object b) => $"{a:yyyy}{b}";
            }
            """
        );

    [Fact]
    public Task NestedInterpolatedStringIsInlined()
        => VerifyFix(
            """
            class C
            {
                string M(string a, int b, string c) => {|LAQ0007:$"{$"{a}{b}"}{c}"|};
            }
            """,
            """
            class C
            {
                string M(string a, int b, string c) => $"{a}{b}{c}";
            }
            """
        );
    #endregion

    #region merged literal
    [Fact]
    public Task AdjacentLiteralsMerge()
        => VerifyFix(
            """
            class C
            {
                string M() => {|LAQ0007:"a" + "b"|};
            }
            """,
            """
            class C
            {
                string M() => "ab";
            }
            """
        );

    [Fact]
    public Task MergedLiteralEscapesWhatItHasTo()
        => VerifyFix(
            """
            class C
            {
                string M() => {|LAQ0007:"a\n" + "b"|};
            }
            """,
            """
            class C
            {
                string M() => "a\nb";
            }
            """
        );

    [Fact]
    public Task MergedVerbatimLiteralStaysVerbatim()
        => VerifyFix(
            """
            class C
            {
                string M() => {|LAQ0007:@"a\" + @"b\"|};
            }
            """,
            """
            class C
            {
                string M() => @"a\b\";
            }
            """
        );
    #endregion
}
