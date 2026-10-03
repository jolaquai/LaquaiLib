using LaquaiLib.Analyzers.Fixes.Fixes;
using LaquaiLib.Analyzers.Performance__0XXX_;

namespace LaquaiLib.Analyzers.Tests.Fixes;

public class InterpolatedStringCreateFixerTests
{
    private static Task VerifyFix(string source, string fixedSource)
        => new CSharpCodeFixTest<InterpolatedStringCreateAnalyzer, InterpolatedStringCreateFixer, DefaultVerifier>
        {
            TestCode = source.ReplaceLineEndings("\r\n"),
            FixedCode = fixedSource.ReplaceLineEndings("\r\n"),
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            // Without this the formatter reflows with Environment.NewLine, making the expected sources platform-dependent
            TestState = { AnalyzerConfigFiles = { ("/.editorconfig", "root = true\n\n[*.cs]\nend_of_line = crlf\n") } },
        }.RunAsync();

    [Fact]
    public Task InterpolationSelected()
        => VerifyFix(
            """
            class C
            {
                string M(string name, int age) => {|LAQ0009:$"Hello, {name}! You are {age} years old."|};
            }
            """,
            """
            class C
            {
                string M(string name, int age) => string.Create(null, stackalloc char[64], $"Hello, {name}! You are {age} years old.");
            }
            """
        );

    // A format specifier can expand a value well past its default rendering, so the estimate stops trusting the hole's type
    [Fact]
    public Task FormatSpecifierSizesBufferConservatively()
        => VerifyFix(
            """
            class C
            {
                string M(double value) => {|LAQ0009:$"{value:N2}"|};
            }
            """,
            """
            class C
            {
                string M(double value) => string.Create(null, stackalloc char[32], $"{value:N2}");
            }
            """
        );

    [Fact]
    public Task AlignmentWidensBuffer()
        => VerifyFix(
            """
            class C
            {
                string M(string s) => {|LAQ0009:$"[{s,200}]"|};
            }
            """,
            """
            class C
            {
                string M(string s) => string.Create(null, stackalloc char[208], $"[{s,200}]");
            }
            """
        );

    [Fact]
    public Task BufferIsClampedToStackBudget()
        => VerifyFix(
            """
            class C
            {
                string M(string s) => {|LAQ0009:$"{s,4000}"|};
            }
            """,
            """
            class C
            {
                string M(string s) => string.Create(null, stackalloc char[1024], $"{s,4000}");
            }
            """
        );

    [Fact]
    public Task BufferHasAFloor()
        => VerifyFix(
            """
            class C
            {
                string M(int i) => {|LAQ0009:$"{i}"|};
            }
            """,
            """
            class C
            {
                string M(int i) => string.Create(null, stackalloc char[32], $"{i}");
            }
            """
        );

    [Fact]
    public Task ExactSizeForChars()
        => VerifyFix(
            """
            class C
            {
                string M(char a, char b) => {|LAQ0009:$"<{a}{b}>"|};
            }
            """,
            """
            class C
            {
                string M(char a, char b) => string.Create(null, stackalloc char[4], $"<{a}{b}>");
            }
            """
        );

    [Fact]
    public Task ExactSizeCountsEscapedBraces()
        => VerifyFix(
            """
            class C
            {
                string M(char c) => {|LAQ0009:$"{{{c}}}"|};
            }
            """,
            """
            class C
            {
                string M(char c) => string.Create(null, stackalloc char[3], $"{{{c}}}");
            }
            """
        );

    [Fact]
    public Task ExactSizeForGuid()
        => VerifyFix(
            """
            using System;
            class C
            {
                string M(Guid g) => {|LAQ0009:$"id-{g}/{g:N}"|};
            }
            """,
            """
            using System;
            class C
            {
                string M(Guid g) => string.Create(null, stackalloc char[72], $"id-{g}/{g:N}");
            }
            """
        );

    [Fact]
    public Task ExactSizeForConstantHoleAndAlignment()
        => VerifyFix(
            """
            class C
            {
                const string P = "abc";
                string M(char c) => {|LAQ0009:$"{P}/{c,5}"|};
            }
            """,
            """
            class C
            {
                const string P = "abc";
                string M(char c) => string.Create(null, stackalloc char[9], $"{P}/{c,5}");
            }
            """
        );

    [Fact]
    public Task ExactSizeForConstantNumbersAndBools()
        => VerifyFix(
            """
            class C
            {
                const int N = 42;
                const bool B = true;
                string M(char c) => {|LAQ0009:$"{N}/{B}/{N:X4}/{c}"|};
            }
            """,
            """
            class C
            {
                const int N = 42;
                const bool B = true;
                string M(char c) => string.Create(null, stackalloc char[14], $"{N}/{B}/{N:X4}/{c}");
            }
            """
        );

    [Fact]
    public Task NegativeConstantFallsBackToEstimate()
        => VerifyFix(
            """
            class C
            {
                const int N = -1;
                string M(char c) => {|LAQ0009:$"{N}{c}"|};
            }
            """,
            """
            class C
            {
                const int N = -1;
                string M(char c) => string.Create(null, stackalloc char[32], $"{N}{c}");
            }
            """
        );

    [Fact]
    public Task OneUnknownHoleFallsBackToEstimate()
        => VerifyFix(
            """
            class C
            {
                string M(char c, int i) => {|LAQ0009:$"{c}{i}"|};
            }
            """,
            """
            class C
            {
                string M(char c, int i) => string.Create(null, stackalloc char[32], $"{c}{i}");
            }
            """
        );

    [Fact]
    public Task VerbatimInterpolation()
        => VerifyFix(
            """
            class C
            {
                string M(int i) => {|LAQ0009:$@"a\b{i}"|};
            }
            """,
            """
            class C
            {
                string M(int i) => string.Create(null, stackalloc char[32], $@"a\b{i}");
            }
            """
        );

    // Five operands is past the four-argument Concat overload, so this one really is on the handler today
    [Fact]
    public Task StringHolesPastTheConcatCutoff()
        => VerifyFix(
            """
            class C
            {
                string M(string a, string b) => {|LAQ0009:$"x{a}/{b}y"|};
            }
            """,
            """
            class C
            {
                string M(string a, string b) => string.Create(null, stackalloc char[48], $"x{a}/{b}y");
            }
            """
        );

    [Fact]
    public Task ConvertedToObject()
        => VerifyFix(
            """
            class C
            {
                object M(int i) => {|LAQ0009:$"v={i}"|};
            }
            """,
            """
            class C
            {
                object M(int i) => string.Create(null, stackalloc char[32], $"v={i}");
            }
            """
        );

    // The lambda body is its own frame, so the enclosing loop never accumulates the localloc
    [Fact]
    public Task LambdaInsideLoop()
        => VerifyFix(
            """
            using System;
            class C
            {
                void M(int n)
                {
                    for (var i = 0; i < n; i++)
                    {
                        Func<int, string> f = x => {|LAQ0009:$"v={x}"|};
                    }
                }
            }
            """,
            """
            using System;
            class C
            {
                void M(int n)
                {
                    for (var i = 0; i < n; i++)
                    {
                        Func<int, string> f = x => string.Create(null, stackalloc char[32], $"v={x}");
                    }
                }
            }
            """
        );

    [Fact]
    public Task InsideLocalFunctionInLoop()
        => VerifyFix(
            """
            class C
            {
                void M(int n)
                {
                    for (var i = 0; i < n; i++)
                    {
                        Local(i);
                    }
                    string Local(int x) => {|LAQ0009:$"v={x}"|};
                }
            }
            """,
            """
            class C
            {
                void M(int n)
                {
                    for (var i = 0; i < n; i++)
                    {
                        Local(i);
                    }
                    string Local(int x) => string.Create(null, stackalloc char[32], $"v={x}");
                }
            }
            """
        );
}
