using LaquaiLib.Analyzers.Performance__0XXX_;

namespace LaquaiLib.Analyzers.Tests.Performance;

public class InterpolatedStringCreateAnalyzerTests
{
    private static Task VerifyNoDiagnostic(string source)
        => new CSharpAnalyzerTest<InterpolatedStringCreateAnalyzer, DefaultVerifier>
        {
            TestCode = source,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        }.RunAsync();

    // Every hole string-typed and unformatted at four operands or fewer is already a String.Concat call
    [Fact]
    public Task AllStringHolesWithinConcatCutoff()
        => VerifyNoDiagnostic(
            """
            class C
            {
                string M(string a, string b) => $"{a}/{b}";
            }
            """
        );

    [Fact]
    public Task SingleStringHole()
        => VerifyNoDiagnostic(
            """
            class C
            {
                string M(string a) => $"{a}";
            }
            """
        );

    [Fact]
    public Task NoHoles()
        => VerifyNoDiagnostic(
            """
            class C
            {
                string M() => $"nothing to interpolate";
            }
            """
        );

    [Fact]
    public Task ConstantContext()
        => VerifyNoDiagnostic(
            """
            class C
            {
                const string S = $"a" + "b";
            }
            """
        );

    [Fact]
    public Task ConvertedToFormattableString()
        => VerifyNoDiagnostic(
            """
            using System;
            class C
            {
                FormattableString M(int i) => $"v={i}";
            }
            """
        );

    [Fact]
    public Task ConvertedToIFormattable()
        => VerifyNoDiagnostic(
            """
            using System;
            class C
            {
                IFormattable M(int i) => $"v={i}";
            }
            """
        );

    // Bound to AppendInterpolatedStringHandler, the interpolation is not producing a string here at all
    [Fact]
    public Task BoundToACustomHandler()
        => VerifyNoDiagnostic(
            """
            using System.Text;
            class C
            {
                void M(StringBuilder sb, int i) => sb.Append($"v={i}");
            }
            """
        );

    // CS4007: the handler is a ref struct and cannot survive the await
    [Fact]
    public Task AwaitInHole()
        => VerifyNoDiagnostic(
            """
            using System.Threading.Tasks;
            class C
            {
                async Task<string> M(Task<int> t) => $"v={await t}";
            }
            """
        );

    // CS0255
    [Fact]
    public Task InCatchBlock()
        => VerifyNoDiagnostic(
            """
            using System;
            class C
            {
                string M(int i)
                {
                    try { return null; }
                    catch (Exception) { return $"v={i}"; }
                }
            }
            """
        );

    // CS0255
    [Fact]
    public Task InFinallyBlock()
        => VerifyNoDiagnostic(
            """
            class C
            {
                void M(int i)
                {
                    string s;
                    try { s = null; }
                    finally { s = $"v={i}"; }
                }
            }
            """
        );

    // CA2014: the localloc would grow the frame on every iteration
    [Fact]
    public Task InForLoop()
        => VerifyNoDiagnostic(
            """
            class C
            {
                void M(int n)
                {
                    for (var i = 0; i < n; i++)
                    {
                        var s = $"v={i}";
                    }
                }
            }
            """
        );

    [Fact]
    public Task InForEachLoop()
        => VerifyNoDiagnostic(
            """
            using System.Collections.Generic;
            class C
            {
                void M(IEnumerable<int> items)
                {
                    foreach (var i in items)
                    {
                        var s = $"v={i}";
                    }
                }
            }
            """
        );

    [Fact]
    public Task InWhileLoop()
        => VerifyNoDiagnostic(
            """
            class C
            {
                void M(int n)
                {
                    while (n-- > 0)
                    {
                        var s = $"v={n}";
                    }
                }
            }
            """
        );

    [Fact]
    public Task InDoLoop()
        => VerifyNoDiagnostic(
            """
            class C
            {
                void M(int n)
                {
                    do
                    {
                        var s = $"v={n}";
                    }
                    while (n-- > 0);
                }
            }
            """
        );

    // CS8640/CS8952
    [Fact]
    public Task InExpressionTree()
        => VerifyNoDiagnostic(
            """
            using System;
            using System.Linq.Expressions;
            class C
            {
                Expression<Func<int, string>> M() => x => $"v={x}";
            }
            """
        );
}
