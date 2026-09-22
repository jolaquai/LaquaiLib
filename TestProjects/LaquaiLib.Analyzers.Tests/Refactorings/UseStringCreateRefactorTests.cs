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

    #region Interpolated string -> string.Create
    [Fact]
    public Task InterpolationSelected()
        => VerifyRefactoring(
            """
            class C
            {
                string M(string name, int age) => [|$"Hello, {name}! You are {age} years old."|];
            }
            """,
            """
            class C
            {
                string M(string name, int age) => string.Create(null, stackalloc char[64], $"Hello, {name}! You are {age} years old.");
            }
            """
        );

    // The caret sits inside a hole, which the outward walk has to climb out of
    [Fact]
    public Task CaretInsideHole()
        => VerifyRefactoring(
            """
            class C
            {
                string M(string name, int age) => $"Hello, {na[||]me}! You are {age} years old.";
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
        => VerifyRefactoring(
            """
            class C
            {
                string M(double value) => [||]$"{value:N2}";
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
        => VerifyRefactoring(
            """
            class C
            {
                string M(string s) => [||]$"[{s,200}]";
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
        => VerifyRefactoring(
            """
            class C
            {
                string M(string s) => [||]$"{s,4000}";
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
        => VerifyRefactoring(
            """
            class C
            {
                string M(char c) => [||]$"{c}";
            }
            """,
            """
            class C
            {
                string M(char c) => string.Create(null, stackalloc char[32], $"{c}");
            }
            """
        );

    [Fact]
    public Task VerbatimInterpolation()
        => VerifyRefactoring(
            """
            class C
            {
                string M(int i) => [||]$@"a\b{i}";
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
        => VerifyRefactoring(
            """
            class C
            {
                string M(string a, string b) => [||]$"x{a}/{b}y";
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
        => VerifyRefactoring(
            """
            class C
            {
                object M(int i) => [||]$"v={i}";
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
        => VerifyRefactoring(
            """
            using System;
            class C
            {
                void M(int n)
                {
                    for (var i = 0; i < n; i++)
                    {
                        Func<int, string> f = x => [||]$"v={x}";
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
        => VerifyRefactoring(
            """
            class C
            {
                void M(int n)
                {
                    for (var i = 0; i < n; i++)
                    {
                        Local(i);
                    }
                    string Local(int x) => [||]$"v={x}";
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
    #endregion

    #region Not offered
    // Every hole string-typed and unformatted at four operands or fewer is already a String.Concat call
    [Fact]
    public Task AllStringHolesWithinConcatCutoff()
        => VerifyNoRefactoring(
            """
            class C
            {
                string M(string a, string b) => [||]$"{a}/{b}";
            }
            """
        );

    [Fact]
    public Task SingleStringHole()
        => VerifyNoRefactoring(
            """
            class C
            {
                string M(string a) => [||]$"{a}";
            }
            """
        );

    [Fact]
    public Task NoHoles()
        => VerifyNoRefactoring(
            """
            class C
            {
                string M() => [||]$"nothing to interpolate";
            }
            """
        );

    [Fact]
    public Task ConstantContext()
        => VerifyNoRefactoring(
            """
            class C
            {
                const string S = [||]$"a" + "b";
            }
            """
        );

    [Fact]
    public Task ConvertedToFormattableString()
        => VerifyNoRefactoring(
            """
            using System;
            class C
            {
                FormattableString M(int i) => [||]$"v={i}";
            }
            """
        );

    [Fact]
    public Task ConvertedToIFormattable()
        => VerifyNoRefactoring(
            """
            using System;
            class C
            {
                IFormattable M(int i) => [||]$"v={i}";
            }
            """
        );

    // Bound to AppendInterpolatedStringHandler, the interpolation is not producing a string here at all
    [Fact]
    public Task BoundToACustomHandler()
        => VerifyNoRefactoring(
            """
            using System.Text;
            class C
            {
                void M(StringBuilder sb, int i) => sb.Append([||]$"v={i}");
            }
            """
        );

    // CS4007: the handler is a ref struct and cannot survive the await
    [Fact]
    public Task AwaitInHole()
        => VerifyNoRefactoring(
            """
            using System.Threading.Tasks;
            class C
            {
                async Task<string> M(Task<int> t) => [||]$"v={await t}";
            }
            """
        );

    // CS0255
    [Fact]
    public Task InCatchBlock()
        => VerifyNoRefactoring(
            """
            using System;
            class C
            {
                string M(int i)
                {
                    try { return null; }
                    catch (Exception) { return [||]$"v={i}"; }
                }
            }
            """
        );

    // CS0255
    [Fact]
    public Task InFinallyBlock()
        => VerifyNoRefactoring(
            """
            class C
            {
                void M(int i)
                {
                    string s;
                    try { s = null; }
                    finally { s = [||]$"v={i}"; }
                }
            }
            """
        );

    // CA2014: the localloc would grow the frame on every iteration
    [Fact]
    public Task InForLoop()
        => VerifyNoRefactoring(
            """
            class C
            {
                void M(int n)
                {
                    for (var i = 0; i < n; i++)
                    {
                        var s = [||]$"v={i}";
                    }
                }
            }
            """
        );

    [Fact]
    public Task InForEachLoop()
        => VerifyNoRefactoring(
            """
            using System.Collections.Generic;
            class C
            {
                void M(IEnumerable<int> items)
                {
                    foreach (var i in items)
                    {
                        var s = [||]$"v={i}";
                    }
                }
            }
            """
        );

    [Fact]
    public Task InWhileLoop()
        => VerifyNoRefactoring(
            """
            class C
            {
                void M(int n)
                {
                    while (n-- > 0)
                    {
                        var s = [||]$"v={n}";
                    }
                }
            }
            """
        );

    [Fact]
    public Task InDoLoop()
        => VerifyNoRefactoring(
            """
            class C
            {
                void M(int n)
                {
                    do
                    {
                        var s = [||]$"v={n}";
                    }
                    while (n-- > 0);
                }
            }
            """
        );

    // CS8640/CS8952
    [Fact]
    public Task InExpressionTree()
        => VerifyNoRefactoring(
            """
            using System;
            using System.Linq.Expressions;
            class C
            {
                Expression<Func<int, string>> M() => x => [||]$"v={x}";
            }
            """
        );
    #endregion

    #region string.Create -> interpolated string
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
    #endregion
}
