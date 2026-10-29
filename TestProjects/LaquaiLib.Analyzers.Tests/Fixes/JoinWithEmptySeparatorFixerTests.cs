using LaquaiLib.Analyzers.Fixes.Fixes;
using LaquaiLib.Analyzers.Performance__0XXX_;

namespace LaquaiLib.Analyzers.Tests.Fixes;

public class JoinWithEmptySeparatorFixerTests
{
    private static Task VerifyFix(string source, string fixedSource)
        => new CSharpCodeFixTest<JoinWithEmptySeparatorAnalyzer, JoinWithEmptySeparatorFixer, DefaultVerifier>
        {
            // Raw string literals normalize embedded newlines to '\n'; the formatter honors the CRLF .editorconfig below for any
            // line break it introduces, so both sides need to agree on CRLF explicitly rather than through file-level line endings
            TestCode = source.ReplaceLineEndings("\r\n"),
            FixedCode = fixedSource.ReplaceLineEndings("\r\n"),
            ReferenceAssemblies = ReferenceAssemblies.Net.Net100,
            // Without this the formatter reflows with Environment.NewLine, making the expected sources platform-dependent
            TestState = { AnalyzerConfigFiles = { ("/.editorconfig", "root = true\n\n[*.cs]\nend_of_line = crlf\n") } },
        }.RunAsync();

    [Fact]
    public Task EmptyLiteralSeparatorIsDropped()
        => VerifyFix(
            """
            class C
            {
                string M(string[] values) => {|LAQ0008:string.Join("", values)|};
            }
            """,
            """
            class C
            {
                string M(string[] values) => string.Concat(values);
            }
            """
        );

    [Fact]
    public Task StringEmptySeparatorIsDropped()
        => VerifyFix(
            """
            class C
            {
                string M(string[] values) => {|LAQ0008:string.Join(string.Empty, values)|};
            }
            """,
            """
            class C
            {
                string M(string[] values) => string.Concat(values);
            }
            """
        );

    [Fact]
    public Task UsingStaticInvocationKeepsItsQualification()
        => VerifyFix(
            """
            using static System.String;
            class C
            {
                string M(string[] values) => {|LAQ0008:Join("", values)|};
            }
            """,
            """
            using static System.String;
            class C
            {
                string M(string[] values) => Concat(values);
            }
            """
        );

    [Fact]
    public Task NamedSeparatorArgumentIsDropped()
        => VerifyFix(
            """
            class C
            {
                string M(string[] values) => {|LAQ0008:string.Join(values: values, separator: "")|};
            }
            """,
            """
            class C
            {
                string M(string[] values) => string.Concat(values: values);
            }
            """
        );

    [Fact]
    public Task MultiLineCallKeepsItsLayout()
        => VerifyFix(
            """
            class C
            {
                string M(string[] values) => {|LAQ0008:string.Join(
                    "",
                    values)|};
            }
            """,
            """
            class C
            {
                string M(string[] values) => string.Concat(
                    values);
            }
            """
        );
}
