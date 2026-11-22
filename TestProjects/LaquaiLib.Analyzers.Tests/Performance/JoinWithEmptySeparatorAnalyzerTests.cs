using LaquaiLib.Analyzers.Performance__0XXX_;

namespace LaquaiLib.Analyzers.Tests.Performance;

public class JoinWithEmptySeparatorAnalyzerTests
{
    private static Task VerifyAnalyzer(string source)
        => new CSharpAnalyzerTest<JoinWithEmptySeparatorAnalyzer, DefaultVerifier>
        {
            TestCode = source,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net100,
            // Without this the formatter reflows with Environment.NewLine, making the expected sources platform-dependent
            TestState = { AnalyzerConfigFiles = { ("/.editorconfig", "root = true\n\n[*.cs]\nend_of_line = crlf\n") } },
        }.RunAsync();

    private static Task VerifyNoDiagnostic(string source) => VerifyAnalyzer(source);

    #region offered
    [Fact]
    public Task EmptyLiteralSeparatorIsReported()
        => VerifyAnalyzer(
            """
            class C
            {
                string M(string[] values) => {|LAQ0008:string.Join("", values)|};
            }
            """
        );

    [Fact]
    public Task StringEmptySeparatorIsReported()
        => VerifyAnalyzer(
            """
            class C
            {
                string M(string[] values) => {|LAQ0008:string.Join(string.Empty, values)|};
            }
            """
        );

    [Fact]
    public Task ConstantSeparatorThatFoldsToEmptyIsReported()
        => VerifyAnalyzer(
            """
            class C
            {
                const string Separator = "";
                string M(string[] values) => {|LAQ0008:string.Join(Separator, values)|};
            }
            """
        );

    [Fact]
    public Task EnumerableOverloadIsReported()
        => VerifyAnalyzer(
            """
            using System.Collections.Generic;
            class C
            {
                string M(IEnumerable<string> values) => {|LAQ0008:string.Join("", values)|};
            }
            """
        );

    [Fact]
    public Task GenericEnumerableOverloadIsReported()
        => VerifyAnalyzer(
            """
            using System.Collections.Generic;
            class C
            {
                string M(IEnumerable<int> values) => {|LAQ0008:string.Join("", values)|};
            }
            """
        );

    [Fact]
    public Task UsingStaticInvocationIsReported()
        => VerifyAnalyzer(
            """
            using static System.String;
            class C
            {
                string M(string[] values) => {|LAQ0008:Join("", values)|};
            }
            """
        );

    [Fact]
    public Task NamedSeparatorArgumentIsReported()
        => VerifyAnalyzer(
            """
            class C
            {
                string M(string[] values) => {|LAQ0008:string.Join(values: values, separator: "")|};
            }
            """
        );
    #endregion

    #region not offered
    [Fact]
    public Task NonEmptySeparatorIsNotReported()
        => VerifyNoDiagnostic(
            """
            class C
            {
                string M(string[] values) => string.Join(", ", values);
            }
            """
        );

    [Fact]
    public Task NonConstantSeparatorIsNotReported()
        => VerifyNoDiagnostic(
            """
            class C
            {
                string M(string separator, string[] values) => string.Join(separator, values);
            }
            """
        );

    [Fact]
    public Task CharSeparatorIsNotReported()
        => VerifyNoDiagnostic(
            """
            class C
            {
                string M(string[] values) => string.Join(',', values);
            }
            """
        );

    [Fact]
    public Task RangeOverloadIsNotReported()
        => VerifyNoDiagnostic(
            """
            class C
            {
                string M(string[] values) => string.Join("", values, 0, 1);
            }
            """
        );

    [Fact]
    public Task UnrelatedJoinIsNotReported()
        => VerifyNoDiagnostic(
            """
            class C
            {
                static string Join(string separator, string[] values) => null;
                string M(string[] values) => Join("", values);
            }
            """
        );
    #endregion
}
