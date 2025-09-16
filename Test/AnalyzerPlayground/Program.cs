using System;

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
        //var analyzerTest = new CSharpAnalyzerTest<UnsafeAccessorValidators, DefaultVerifier>
        //{
        //    ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
        //    ExpectedDiagnostics =
        //    {
        //        new DiagnosticResult(UnsafeAccessorValidators.MissingMemberDescriptor)
        //            .WithLocation(11, 26)
        //            .WithArguments(["System.IO.MemoryStream", "field", "byte[]", "_buffer"])
        //    },
        //    TestCode = """
        //    using System.Text.RegularExpressions;
        //    using System.Runtime.CompilerServices;
        //    using System.IO;
        //    using System;

        //    namespace Test;

        //    public class A
        //    {
        //        [UnsafeAccessor(UnsafeAccessorKind.Field)]
        //        private static extern ref byte[] _buffer(MemoryStream _);
        //    }
        //    """,
        //};
        //await analyzerTest.RunAsync(TestContext.Current.CancellationToken);
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
