using LaquaiLib.Analyzers.Performance__0XXX_;

namespace LaquaiLib.Analyzers.Tests.Performance;

public class StringConcatenationAnalyzerTests
{
    private static Task VerifyAnalyzer(string source, ReferenceAssemblies referenceAssemblies = null)
        => new CSharpAnalyzerTest<StringConcatenationAnalyzer, DefaultVerifier>
        {
            TestCode = source,
            ReferenceAssemblies = referenceAssemblies ?? ReferenceAssemblies.Net.Net100,
            // Without this the formatter reflows with Environment.NewLine, making the expected sources platform-dependent
            TestState = { AnalyzerConfigFiles = { ("/.editorconfig", "root = true\n\n[*.cs]\nend_of_line = crlf\n") } },
        }.RunAsync();

    private static Task VerifyNoDiagnostic(string source, ReferenceAssemblies referenceAssemblies = null) => VerifyAnalyzer(source, referenceAssemblies);

    #region + chains
    [Fact]
    public Task TwoStringsAreReported()
        => VerifyAnalyzer(
            """
            class C
            {
                string M(string a, string b) => {|LAQ0007:a + b|};
            }
            """
        );

    [Fact]
    public Task FiveStringsAreReported()
        => VerifyAnalyzer(
            """
            class C
            {
                string M(string a, string b, string c, string d, string e) => {|LAQ0007:a + b + c + d + e|};
            }
            """
        );

    [Fact]
    public Task ChainIsReportedOnceAtItsTop()
        => VerifyAnalyzer(
            """
            class C
            {
                string M(string a, string b, string c) => {|LAQ0007:a + b + c|};
            }
            """
        );

    [Fact]
    public Task ParenthesizedInnerLinkIsNotReportedSeparately()
        => VerifyAnalyzer(
            """
            class C
            {
                string M(string a, string b, string c) => {|LAQ0007:(a + b) + c|};
            }
            """
        );

    [Fact]
    public Task NonStringPartIsReported()
        => VerifyAnalyzer(
            """
            class C
            {
                string M(string a, int b) => {|LAQ0007:a + b|};
            }
            """
        );

    [Fact]
    public Task AdjacentLiteralsAreReported()
        => VerifyAnalyzer(
            """
            class C
            {
                string M() => {|LAQ0007:"a" + "b"|};
            }
            """
        );

    [Fact]
    public Task NumericAdditionIsNotReported()
        => VerifyNoDiagnostic(
            """
            class C
            {
                int M(int a, int b) => a + b;
            }
            """
        );

    [Fact]
    public Task ConstantChainIsNotReported()
        => VerifyNoDiagnostic(
            """
            class C
            {
                const string Prefix = "a";
                const string Value = Prefix + "b";
            }
            """
        );

    [Fact]
    public Task SingleStringIsNotReported()
        => VerifyNoDiagnostic(
            """
            class C
            {
                string M(string a) => a;
            }
            """
        );
    #endregion

    #region interpolated strings
    [Fact]
    public Task FourStringPartsAreNotReported()
        => VerifyNoDiagnostic(
            """
            class C
            {
                string M(string a, string b, string c, string d) => $"{a}{b}{c}{d}";
            }
            """
        );

    [Fact]
    public Task FiveStringPartsAreReported()
        => VerifyAnalyzer(
            """
            class C
            {
                string M(string a, string b, string c, string d, string e) => {|LAQ0007:$"{a}{b}{c}{d}{e}"|};
            }
            """
        );

    [Fact]
    public Task TextRunsCountTowardsThePartLimit()
        => VerifyAnalyzer(
            """
            class C
            {
                string M(string a, string b) => {|LAQ0007:$"w{a}x{b}y"|};
            }
            """
        );

    [Fact]
    public Task FiveStringPartsAreNotReportedWithoutTheSpanOfStringOverload()
        => VerifyNoDiagnostic(
            """
            class C
            {
                string M(string a, string b, string c, string d, string e) => $"{a}{b}{c}{d}{e}";
            }
            """,
            ReferenceAssemblies.Net.Net80
        );

    [Fact]
    public Task TwoSpanPartsAreReported()
        => VerifyAnalyzer(
            """
            using System;
            class C
            {
                string M(ReadOnlySpan<char> a, ReadOnlySpan<char> b) => {|LAQ0007:$"{a}{b}"|};
            }
            """
        );

    [Fact]
    public Task FourMixedSpanAndStringPartsAreReported()
        => VerifyAnalyzer(
            """
            using System;
            class C
            {
                string M(ReadOnlySpan<char> a, string b) => {|LAQ0007:$"[{a}{b}]"|};
            }
            """
        );

    [Fact]
    public Task FiveSpanPartsAreNotReported()
        => VerifyNoDiagnostic(
            """
            using System;
            class C
            {
                string M(ReadOnlySpan<char> a, ReadOnlySpan<char> b, ReadOnlySpan<char> c, ReadOnlySpan<char> d, ReadOnlySpan<char> e) => $"{a}{b}{c}{d}{e}";
            }
            """
        );

    [Fact]
    public Task FormatSpecifierIsNotReported()
        => VerifyNoDiagnostic(
            """
            class C
            {
                string M(System.DateTime a, string b, string c, string d, string e) => $"{a:yyyy}{b}{c}{d}{e}";
            }
            """
        );

    [Fact]
    public Task AlignmentSpecifierIsNotReported()
        => VerifyNoDiagnostic(
            """
            class C
            {
                string M(string a, string b, string c, string d, string e) => $"{a,5}{b}{c}{d}{e}";
            }
            """
        );

    [Fact]
    public Task NonStringHoleIsNotReported()
        => VerifyNoDiagnostic(
            """
            class C
            {
                string M(string a, int b, string c, string d, string e) => $"{a}{b}{c}{d}{e}";
            }
            """
        );

    [Fact]
    public Task ChainInsideAHoleIsNotReportedSeparately()
        => VerifyNoDiagnostic(
            """
            class C
            {
                string M(string a, string b, string c) => $"{a + b}{c}";
            }
            """
        );

    [Fact]
    public Task ChainInsideAHoleCountsTowardsThePartLimit()
        => VerifyAnalyzer(
            """
            class C
            {
                string M(string a, string b, string c, string d) => {|LAQ0007:$"{a + b}{c}{d}x"|};
            }
            """
        );

    [Fact]
    public Task ChainInsideAClauseBearingHoleIsStillReported()
        => VerifyAnalyzer(
            """
            class C
            {
                string M(string a, int b) => $"{{|LAQ0007:a + b|},5}";
            }
            """
        );

    [Fact]
    public Task NestedInterpolatedStringIsReportedOnlyAtItsTop()
        => VerifyAnalyzer(
            """
            class C
            {
                string M(string a, int b, string c) => {|LAQ0007:$"{$"{a}{b}"}{c}"|};
            }
            """
        );

    [Fact]
    public Task RawInterpolatedStringWithTextIsNotReported()
        => VerifyNoDiagnostic(
            """"
            class C
            {
                string M(string a, string b, string c, string d) => $"""w{a}x{b}y{c}z{d}""";
            }
            """"
        );

    [Fact]
    public Task PlainInterpolatedStringIsNotReported()
        => VerifyNoDiagnostic(
            """
            class C
            {
                string M(string a) => $"x{a}y";
            }
            """
        );

    [Fact]
    public Task ConstantInterpolatedStringIsNotReported()
        => VerifyNoDiagnostic(
            """
            class C
            {
                const string A = "a";
                const string Value = $"{A}{A}{A}{A}{A}";
            }
            """
        );
    #endregion
}
