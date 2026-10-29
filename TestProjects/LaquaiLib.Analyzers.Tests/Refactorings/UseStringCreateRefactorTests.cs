namespace LaquaiLib.Analyzers.Tests.Refactorings;

public class UseStringCreateRefactorTests
{
    private static Task VerifyRefactoring(string source, string fixedSource)
        => new CSharpCodeRefactoringTest<UseStringCreateRefactor, DefaultVerifier>
        {
            TestCode = source,
            FixedCode = fixedSource,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            // Without this the formatter reflows with Environment.NewLine, making the expected sources platform-dependent
            TestState = { AnalyzerConfigFiles = { ("/.editorconfig", "root = true\n\n[*.cs]\nend_of_line = crlf\n") } },
        }.RunAsync();

    private static Task VerifyNoRefactoring(string source)
        => new CSharpCodeRefactoringTest<UseStringCreateRefactor, DefaultVerifier>
        {
            TestCode = source,
            FixedCode = source.Replace("[||]", "").Replace("[|", "").Replace("|]", ""),
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        }.RunAsync();

    [Fact]
    public Task UnwrapFromInvocation()
        => VerifyRefactoring(
            """
            class C
            {
                string M(int i) => [|string.Create(null, stackalloc char[32], $"v={i}")|];
            }
            """,
            """
            class C
            {
                string M(int i) => $"v={i}";
            }
            """
        );

    // The caret inside an interpolation this refactoring already wrapped offers the way back rather than a second wrap
    [Fact]
    public Task UnwrapWithCaretInsideInterpolation()
        => VerifyRefactoring(
            """
            class C
            {
                string M(int i) => string.Create(null, stackalloc char[32], $"v=[||]{i}");
            }
            """,
            """
            class C
            {
                string M(int i) => $"v={i}";
            }
            """
        );

    // A non-null provider is a culture the bare interpolation would silently drop
    [Fact]
    public Task NoUnwrapWithExplicitProvider()
        => VerifyNoRefactoring(
            """
            using System.Globalization;
            class C
            {
                string M(double d) => [|string.Create(CultureInfo.InvariantCulture, stackalloc char[32], $"v={d}")|];
            }
            """
        );

    [Fact]
    public Task NoUnwrapOfUnrelatedInvocation()
        => VerifyNoRefactoring(
            """
            class C
            {
                string M(int i) => [|Other(null, $"v={i}")|];
                static string Other(object o, string s) => s;
            }
            """
        );
}
