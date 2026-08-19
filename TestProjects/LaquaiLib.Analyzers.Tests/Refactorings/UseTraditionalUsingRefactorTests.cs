namespace LaquaiLib.Analyzers.Tests.Refactorings;

public class UseTraditionalUsingRefactorTests
{
    private static readonly string SingleKey = $"{typeof(UseTraditionalUsingRefactor).FullName}_ChangeToTraditionalUsing";
    private static readonly string StackedKey = $"{typeof(UseTraditionalUsingRefactor).FullName}_ChangeToStackedTraditionalUsing";

    private static Task VerifySingle(string source, string fixedSource)
        => new CSharpCodeRefactoringTest<UseTraditionalUsingRefactor, DefaultVerifier>
        {
            TestCode = source,
            FixedCode = fixedSource,
            CodeActionEquivalenceKey = SingleKey,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            // Without this the formatter reflows with Environment.NewLine, making the expected sources platform-dependent
            TestState = { AnalyzerConfigFiles = { ("/.editorconfig", "root = true\n\n[*.cs]\nend_of_line = lf\n") } },
        }.RunAsync();

    private static Task VerifyStacked(string source, string fixedSource)
        => new CSharpCodeRefactoringTest<UseTraditionalUsingRefactor, DefaultVerifier>
        {
            TestCode = source,
            FixedCode = fixedSource,
            CodeActionEquivalenceKey = StackedKey,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestState = { AnalyzerConfigFiles = { ("/.editorconfig", "root = true\n\n[*.cs]\nend_of_line = lf\n") } },
        }.RunAsync();

    private static Task VerifyNoRefactoring(string source)
        => new CSharpCodeRefactoringTest<UseTraditionalUsingRefactor, DefaultVerifier>
        {
            TestCode = source,
            FixedCode = source.Replace("[||]", "").Replace("[|", "").Replace("|]", ""),
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        }.RunAsync();

    #region single declaration
    [Fact]
    public Task SingleDeclarationWrapsRestOfBlock()
        => VerifySingle(
            """
            class C
            {
                void M()
                {
                    using v[||]ar d = Get();
                    N();
                    N();
                }
                static System.IDisposable Get() => null;
                static void N() { }
            }
            """,
            """
            class C
            {
                void M()
                {
                    using (var d = Get())
                    {
                        N();
                        N();
                    }
                }
                static System.IDisposable Get() => null;
                static void N() { }
            }
            """
        );

    [Fact]
    public Task SingleDeclarationEmptyBlockWhenNothingFollows()
        => VerifySingle(
            """
            class C
            {
                void M()
                {
                    N();
                    using v[||]ar d = Get();
                }
                static System.IDisposable Get() => null;
                static void N() { }
            }
            """,
            """
            class C
            {
                void M()
                {
                    N();
                    using (var d = Get())
                    {
                    }
                }
                static System.IDisposable Get() => null;
                static void N() { }
            }
            """
        );

    [Fact]
    public Task SingleDeclarationPreservesAwaitKeyword()
        => VerifySingle(
            """
            using System;
            using System.Threading.Tasks;
            class C
            {
                async Task M()
                {
                    await using v[||]ar d = Get();
                    N();
                }
                static IAsyncDisposable Get() => null;
                static void N() { }
            }
            """,
            """
            using System;
            using System.Threading.Tasks;
            class C
            {
                async Task M()
                {
                    await using (var d = Get())
                    {
                        N();
                    }
                }
                static IAsyncDisposable Get() => null;
                static void N() { }
            }
            """
        );

    // The gap statement breaks adjacency, so only the first declaration converts; the second stays in simple form, now one scope deeper
    [Fact]
    public Task SingleDeclarationLeavesNonAdjacentLaterDeclarationUntouched()
        => VerifySingle(
            """
            class C
            {
                void M()
                {
                    using v[||]ar a = Get();
                    N();
                    using var b = Get();
                    N();
                }
                static System.IDisposable Get() => null;
                static void N() { }
            }
            """,
            """
            class C
            {
                void M()
                {
                    using (var a = Get())
                    {
                        N();
                        using var b = Get();
                        N();
                    }
                }
                static System.IDisposable Get() => null;
                static void N() { }
            }
            """
        );

    [Fact]
    public Task CommentAboveDeclarationIsPreserved()
        => VerifySingle(
            """
            class C
            {
                void M()
                {
                    // acquire
                    using v[||]ar d = Get();
                    N();
                }
                static System.IDisposable Get() => null;
                static void N() { }
            }
            """,
            """
            class C
            {
                void M()
                {
                    // acquire
                    using (var d = Get())
                    {
                        N();
                    }
                }
                static System.IDisposable Get() => null;
                static void N() { }
            }
            """
        );
    #endregion

    #region stacked run
    [Fact]
    public Task StackedTwoConsecutiveDeclarations()
        => VerifyStacked(
            """
            class C
            {
                void M()
                {
                    using v[||]ar a = Get();
                    using var b = Get();
                    N();
                }
                static System.IDisposable Get() => null;
                static void N() { }
            }
            """,
            """
            class C
            {
                void M()
                {
                    using (var a = Get())
                    using (var b = Get())
                    {
                        N();
                    }
                }
                static System.IDisposable Get() => null;
                static void N() { }
            }
            """
        );

    // Caret sits on the middle declaration; the run is still found by walking outward in both directions
    [Fact]
    public Task StackedThreeConsecutiveDeclarationsCaretOnMiddleOne()
        => VerifyStacked(
            """
            class C
            {
                void M()
                {
                    using var a = Get();
                    using v[||]ar b = Get();
                    using var c = Get();
                    N();
                }
                static System.IDisposable Get() => null;
                static void N() { }
            }
            """,
            """
            class C
            {
                void M()
                {
                    using (var a = Get())
                    using (var b = Get())
                    using (var c = Get())
                    {
                        N();
                    }
                }
                static System.IDisposable Get() => null;
                static void N() { }
            }
            """
        );

    [Fact]
    public Task StackedPreservesPerDeclarationAwaitKeyword()
        => VerifyStacked(
            """
            using System;
            using System.Threading.Tasks;
            class C
            {
                async Task M()
                {
                    using v[||]ar a = Get();
                    await using var b = GetAsync();
                }
                static IDisposable Get() => null;
                static IAsyncDisposable GetAsync() => null;
            }
            """,
            """
            using System;
            using System.Threading.Tasks;
            class C
            {
                async Task M()
                {
                    using (var a = Get())
                    await using (var b = GetAsync())
                    {
                    }
                }
                static IDisposable Get() => null;
                static IAsyncDisposable GetAsync() => null;
            }
            """
        );
    #endregion

    #region not offered
    [Fact]
    public Task TraditionalUsingStatementNotOffered()
        => VerifyNoRefactoring(
            """
            class C
            {
                void M()
                {
                    using (var d = Get())
                    {
                        [||]N();
                    }
                }
                static System.IDisposable Get() => null;
                static void N() { }
            }
            """
        );

    [Fact]
    public Task PlainLocalDeclarationNotOffered()
        => VerifyNoRefactoring(
            """
            class C
            {
                void M()
                {
                    v[||]ar x = 1;
                }
            }
            """
        );
    #endregion
}
