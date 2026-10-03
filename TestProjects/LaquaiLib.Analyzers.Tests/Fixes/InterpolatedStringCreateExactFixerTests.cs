using LaquaiLib.Analyzers.Fixes.Fixes;
using LaquaiLib.Analyzers.Performance__0XXX_;

namespace LaquaiLib.Analyzers.Tests.Fixes;

public class InterpolatedStringCreateExactFixerTests
{
    private static Task VerifyFix(string source, string fixedSource)
        => new CSharpCodeFixTest<InterpolatedStringCreateAnalyzer, InterpolatedStringCreateExactFixer, DefaultVerifier>
        {
            TestCode = source.ReplaceLineEndings("\r\n"),
            FixedCode = fixedSource.ReplaceLineEndings("\r\n"),
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            // Without this the formatter reflows with Environment.NewLine, making the expected sources platform-dependent
            TestState = { AnalyzerConfigFiles = { ("/.editorconfig", "root = true\n\n[*.cs]\nend_of_line = crlf\n") } },
        }.RunAsync();

    private static Task VerifyNoFix(string source)
        => new CSharpCodeFixTest<InterpolatedStringCreateAnalyzer, InterpolatedStringCreateExactFixer, DefaultVerifier>
        {
            TestCode = source.ReplaceLineEndings("\r\n"),
            FixedCode = source.ReplaceLineEndings("\r\n"),
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestState = { AnalyzerConfigFiles = { ("/.editorconfig", "root = true\n\n[*.cs]\nend_of_line = crlf\n") } },
        }.RunAsync();

    [Fact]
    public Task TwoCharHoles()
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
                string M(char a, char b) => string.Create(4, (a, b), static (span, state) =>
                {
                    span[0] = '<';
                    span[1] = state.Item1;
                    span[2] = state.Item2;
                    span[3] = '>';
                });
            }
            """
        );

    // No stackalloc fix is available inside a loop, but nothing stops string.Create<TState>
    [Fact]
    public Task InsideLoop()
        => VerifyFix(
            """
            class C
            {
                void M(char c, int n)
                {
                    for (var i = 0; i < n; i++)
                    {
                        var s = {|LAQ0009:$"a{c}"|};
                    }
                }
            }
            """,
            """
            class C
            {
                void M(char c, int n)
                {
                    for (var i = 0; i < n; i++)
                    {
                        var s = string.Create(2, c, static (span, state) =>
                        {
                            span[0] = 'a';
                            span[1] = state;
                        });
                    }
                }
            }
            """
        );

    [Fact]
    public Task GuidWithAlignmentAndFormat()
        => VerifyFix(
            """
            using System;
            class C
            {
                string M(Guid g) => {|LAQ0009:$"[{g,-34:N}]"|};
            }
            """,
            """
            using System;
            class C
            {
                string M(Guid g) => string.Create(36, g, static (span, state) =>
                {
                    span[0] = '[';
                    state.TryFormat(span.Slice(1, 32), out _, "N");
                    span.Slice(33, 2).Fill(' ');
                    span[35] = ']';
                });
            }
            """
        );

    [Fact]
    public Task ConstantHolesAreFoldedIntoText()
        => VerifyFix(
            """
            class C
            {
                const int N = 7;
                string M(char c) => {|LAQ0009:$"n={N,3}/{c}"|};
            }
            """,
            """
            class C
            {
                const int N = 7;
                string M(char c) => string.Create(7, c, static (span, state) =>
                {
                    "n=  7/".CopyTo(span);
                    span[6] = state;
                });
            }
            """
        );

    [Fact]
    public Task NameClashesAreAvoided()
        => VerifyFix(
            """
            class C
            {
                string M(char span, char state) => {|LAQ0009:$"{span}{state}"|};
            }
            """,
            """
            class C
            {
                string M(char span, char state) => string.Create(2, (span, state), static (span2, state2) =>
                {
                    span2[0] = state2.Item1;
                    span2[1] = state2.Item2;
                });
            }
            """
        );

    [Fact]
    public Task FixAllRewritesEveryOccurrence()
        => VerifyFix(
            """
            class C
            {
                string A(char c) => {|LAQ0009:$"a{c}"|};
                string B(char c) => {|LAQ0009:$"b{c}"|};
            }
            """,
            """
            class C
            {
                string A(char c) => string.Create(2, c, static (span, state) =>
                {
                    span[0] = 'a';
                    span[1] = state;
                });
                string B(char c) => string.Create(2, c, static (span, state) =>
                {
                    span[0] = 'b';
                    span[1] = state;
                });
            }
            """
        );

    [Fact]
    public Task NotOfferedForUnknownHole()
        => VerifyNoFix(
            """
            class C
            {
                string M(char c, int i) => {|LAQ0009:$"{c}{i}"|};
            }
            """
        );
}
