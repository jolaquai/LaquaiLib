using LaquaiLib.Analyzers.Fixes.Refactorings;
using LaquaiLib.Analyzers.Refactorings__4XXX_;

using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

using Xunit;

namespace AnalyzerPlayground;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        for (var i = 0; i < 10; i++)
        {
        }

        IEnumerable<int> ts = [2, 3, 4, 5, 6];
        foreach (var t in ts)
        {
        }

        for (var i = 0; i < 10; i++)
        {
            await Task.Delay(100);
        }

        foreach (var t in ts)
        {
            await Task.Delay(100);
        }
    }
}

public class TestClass
{
    [Fact]
    public async Task TestMethod()
    {
        var fixTest = new CSharpCodeFixTest<ParallelizeLoopAnalyzer, ParallelizeLoopAnalyzerFix, DefaultVerifier>
        {
            ExpectedDiagnostics =
            {
                new DiagnosticResult(ParallelizeLoopAnalyzer.Descriptor)
                    .WithArguments("Parallel.For")
                    .WithLocation(7, 9),
                new DiagnosticResult(ParallelizeLoopAnalyzer.Descriptor)
                    .WithArguments("Parallel.ForEach")
                    .WithLocation(12, 9),
                new DiagnosticResult(ParallelizeLoopAnalyzer.Descriptor)
                    .WithArguments("Parallel.ForAsync")
                    .WithLocation(16, 9),
                new DiagnosticResult(ParallelizeLoopAnalyzer.Descriptor)
                    .WithArguments("Parallel.ForEachAsync")
                    .WithLocation(21, 9),
            },
            TestCode = """
            using System.Threading.Tasks;

            public static class TestClass
            {
                public static async Task TestMethod()
                {
                    for (var i = 0; i < 10; i++)
                    {
                    }

                    int[] ts = [2, 3, 4, 5, 6];
                    foreach (var t in ts)
                    {
                    }

                    for (var i = 0; i < 10; i++)
                    {
                        await Task.Delay(100);
                    }

                    foreach (var t in ts)
                    {
                        await Task.Delay(100);
                    }
                }
            }
            """,
            FixedCode = """
            using System.Threading.Tasks;

            public static class TestClass
            {
                public static async Task TestMethod()
                {
                    Parallel.For(0, 10, i =>
                    {
                    });

                    int[] ts = [2, 3, 4, 5, 6];
                    Parallel.ForEach(ts, t =>
                    {
                    });

                    await Parallel.ForAsync(0, 10, async i =>
                    {
                        await Task.Delay(100);
                    });

                    await Parallel.ForEachAsync(ts, async t =>
                    {
                        await Task.Delay(100);
                    });
                }
            }
            """,
        };
        await fixTest.RunAsync(TestContext.Current.CancellationToken);
    }
}
