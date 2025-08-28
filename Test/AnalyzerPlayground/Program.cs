using LaquaiLib.Analyzers.Validity__9XXX_;

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

public class TestClass
{
    [Fact]
    public async Task TestAnalyzer()
    {
        var analyzerTest = new CSharpAnalyzerTest<UnsafeAccessorValidators, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
            ExpectedDiagnostics =
            {
                new DiagnosticResult(UnsafeAccessorValidators.ContainingTypeTypeParameterMismatchDescriptor)
                    .WithLocation(8, 32)
                    .WithArguments("<T>", "none")
            },
            TestCode = """
            using System.Runtime.CompilerServices;

            namespace Test;

            public static class AAccessors<T>
            {
                [UnsafeAccessor(UnsafeAccessorKind.Method)]
                public static extern ref U GetValue<U>(A obj);
            }

            public class A
            {
                private T GetValue<T>() => default;
            }
            """,
        };
        await analyzerTest.RunAsync(TestContext.Current.CancellationToken);
    }
    [Fact]
    public async Task TestFix()
    {
        //var fixTest = new CSharpCodeFixTest<SealClassAnalyzer, SealClassAnalyzerFix, DefaultVerifier>
        //{
        //    ExpectedDiagnostics =
        //    {
        //        new DiagnosticResult(SealClassAnalyzer.Descriptor)
        //            .WithLocation(3, 8),
        //        new DiagnosticResult(SealClassAnalyzer.Descriptor)
        //            .WithLocation(4, 8),
        //    },
        //    TestCode = """
        //    public class A;
        //    public class B : A;
        //    public class C : B;
        //    public class D;
        //    """,
        //    FixedCode = """
        //    public class A;
        //    public class B : A;
        //    public sealed class C : B;
        //    public sealed class D;
        //    """,
        //};
        //await fixTest.RunAsync(TestContext.Current.CancellationToken);
    }
}
