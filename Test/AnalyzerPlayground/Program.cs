using LaquaiLib.Analyzers.Fixes.Performance;
using LaquaiLib.Analyzers.Performance__0XXX_;

using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

using Xunit;

namespace AnalyzerPlayground;

internal static class Program
{
    public static async Task Main(string[] args)
    {
    }
}

public class TestClass2;

public class TestClass
{
    [Fact]
    public async Task TestAnalyzer()
    {
        var analyzerTest = new CSharpAnalyzerTest<SealClassAnalyzer, DefaultVerifier>
        {
            ExpectedDiagnostics =
            {
                new DiagnosticResult(SealClassAnalyzer.Descriptor)
                    .WithLocation(3, 8),
                new DiagnosticResult(SealClassAnalyzer.Descriptor)
                    .WithLocation(4, 8),
            },
            TestCode = """
            public class A;
            public class B : A;
            public class C : B;
            public class D;
            """,
        };
        await analyzerTest.RunAsync(TestContext.Current.CancellationToken);
    }
    [Fact]
    public async Task TestFix()
    {
        var fixTest = new CSharpCodeFixTest<SealClassAnalyzer, SealClassAnalyzerFix, DefaultVerifier>
        {
            ExpectedDiagnostics =
            {
                new DiagnosticResult(SealClassAnalyzer.Descriptor)
                    .WithLocation(3, 8),
                new DiagnosticResult(SealClassAnalyzer.Descriptor)
                    .WithLocation(4, 8),
            },
            TestCode = """
            public class A;
            public class B : A;
            public class C : B;
            public class D;
            """,
            FixedCode = """
            public class A;
            public class B : A;
            public sealed class C : B;
            public sealed class D;
            """,
        };
        await fixTest.RunAsync(TestContext.Current.CancellationToken);
    }
}
