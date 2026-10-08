using LaquaiLib.Analyzers.Quality__1XXX_;

namespace LaquaiLib.Analyzers.Tests.Quality;

public class AttributeOrderAnalyzerTests
{
    private static Task VerifyAnalyzer(string source)
        => new CSharpAnalyzerTest<AttributeOrderAnalyzer, DefaultVerifier>
        {
            TestCode = source,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            // Without this the formatter reflows with Environment.NewLine, making the expected sources platform-dependent
            TestState = { AnalyzerConfigFiles = { ("/.editorconfig", "root = true\n\n[*.cs]\nend_of_line = crlf\n") } },
        }.RunAsync();

    private static Task VerifyNoDiagnostic(string source) => VerifyAnalyzer(source);

    private const string Attributes =
        """
        using System.Diagnostics.CodeAnalysis;
        using System.Runtime.CompilerServices;

        """;

    #region offered
    [Fact]
    public Task ReversedSeparateLists()
        => VerifyAnalyzer(
            Attributes + """
            class C
            {
                {|LAQ1001:[MethodImpl(MethodImplOptions.NoInlining)][DoesNotReturn]|}
                static void M() => throw null;
            }
            """
        );

    [Fact]
    public Task ReversedSingleList()
        => VerifyAnalyzer(
            Attributes + """
            class C
            {
                {|LAQ1001:[MethodImpl(MethodImplOptions.NoInlining), DoesNotReturn]|}
                static void M() => throw null;
            }
            """
        );

    [Fact]
    public Task InterleavingListsCannotBeReorderedAsBlocks()
        => VerifyAnalyzer(
            Attributes + """
            class C
            {
                {|LAQ1001:[MethodImpl(MethodImplOptions.NoInlining), DoesNotReturn][ExcludeFromCodeCoverage]|}
                static void M() => throw null;
            }
            """
        );

    [Fact]
    public Task NamespaceIsIgnoredWhenComparing()
        => VerifyAnalyzer(
            Attributes + """
            class C
            {
                {|LAQ1001:[System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.NoInlining)][DoesNotReturn]|}
                static void M() => throw null;
            }
            """
        );

    [Fact]
    public Task ParameterAttributesAreChecked()
        => VerifyAnalyzer(
            Attributes + """
            class C
            {
                static void M({|LAQ1001:[DisallowNull, AllowNull]|} object x) { }
            }
            """
        );

    [Fact]
    public Task TypeParameterAttributesAreChecked()
        => VerifyAnalyzer(
            """
            using System;

            [AttributeUsage(AttributeTargets.GenericParameter)] class BAttribute : Attribute { }
            [AttributeUsage(AttributeTargets.GenericParameter)] class AAttribute : Attribute { }

            class C<{|LAQ1001:[B, A]|} T> { }
            """
        );

    [Fact]
    public Task AssemblyAttributesAreChecked()
        => VerifyAnalyzer(
            """
            {|LAQ1001:[assembly: System.CLSCompliant(false)]
            [assembly: System.Reflection.AssemblyCompany("A")]|}
            """
        );
    #endregion

    #region not offered
    [Fact]
    public Task AlreadyOrderedSeparateListsNeverReports()
        => VerifyNoDiagnostic(
            Attributes + """
            class C
            {
                [DoesNotReturn][MethodImpl(MethodImplOptions.NoInlining)]
                static void M() => throw null;
            }
            """
        );

    [Fact]
    public Task AlreadyOrderedSingleListNeverReports()
        => VerifyNoDiagnostic(
            Attributes + """
            class C
            {
                [DoesNotReturn, MethodImpl(MethodImplOptions.NoInlining)]
                static void M() => throw null;
            }
            """
        );

    [Fact]
    public Task SingleAttributeNeverReports()
        => VerifyNoDiagnostic(
            Attributes + """
            class C
            {
                [DoesNotReturn]
                static void M() => throw null;
            }
            """
        );

    // Attribute lists that don't share a target are never merged or reordered against each other, even when they'd otherwise be out of order
    [Fact]
    public Task DifferentTargetsAreNotCrossOrdered()
        => VerifyNoDiagnostic(
            """
            using System.Diagnostics.CodeAnalysis;

            class C
            {
                [ExcludeFromCodeCoverage]
                [return: NotNull]
                static object M() => new object();
            }
            """
        );

    // Written with and without the "Attribute" suffix, "Foo" and "FooAttribute" normalize to the same key; a naive
    // unstripped comparison would consider this pair out of order ("Foo" < "FooAttribute"), so this pins that it isn't
    [Fact]
    public Task ExplicitAttributeSuffixNormalizesToTheSameKeyAsWithoutIt()
        => VerifyNoDiagnostic(
            """
            using System;

            [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
            class FooAttribute : Attribute { }

            class C
            {
                [FooAttribute][Foo]
                static void M() { }
            }
            """
        );
    #endregion
}
