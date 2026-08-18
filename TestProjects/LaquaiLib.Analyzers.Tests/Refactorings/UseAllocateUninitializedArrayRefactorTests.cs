namespace LaquaiLib.Analyzers.Tests.Refactorings;

public class UseAllocateUninitializedArrayRefactorTests
{
    private static Task VerifyRefactoring(string source, string fixedSource)
        => new CSharpCodeRefactoringTest<UseAllocateUninitializedArrayRefactor, DefaultVerifier>
        {
            TestCode = source,
            FixedCode = fixedSource,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            // Without this the formatter reflows with Environment.NewLine, making the expected sources platform-dependent
            TestState = { AnalyzerConfigFiles = { ("/.editorconfig", "root = true\n\n[*.cs]\nend_of_line = lf\n") } },
        }.RunAsync();

    private static Task VerifyNoRefactoring(string source)
        => new CSharpCodeRefactoringTest<UseAllocateUninitializedArrayRefactor, DefaultVerifier>
        {
            TestCode = source,
            FixedCode = source.Replace("[||]", "").Replace("[|", "").Replace("|]", ""),
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        }.RunAsync();

    #region new T[length] -> GC.AllocateUninitializedArray<T>(length)
    [Fact]
    public Task ArrayCreationSelected()
        => VerifyRefactoring(
            """
            using System;
            class C
            {
                int[] M() => [|new int[512]|];
            }
            """,
            """
            using System;
            class C
            {
                int[] M() => GC.AllocateUninitializedArray<int>(512);
            }
            """
        );

    [Fact]
    public Task CaretOnNewKeyword()
        => VerifyRefactoring(
            """
            using System;
            class C
            {
                int[] M() => [||]new int[512];
            }
            """,
            """
            using System;
            class C
            {
                int[] M() => GC.AllocateUninitializedArray<int>(512);
            }
            """
        );

    // The caret sits on the size literal, which the outward walk has to climb out of
    [Fact]
    public Task CaretInsideSizeExpression()
        => VerifyRefactoring(
            """
            using System;
            class C
            {
                int[] M() => new int[5[||]12];
            }
            """,
            """
            using System;
            class C
            {
                int[] M() => GC.AllocateUninitializedArray<int>(512);
            }
            """
        );

    [Fact]
    public Task QualifiesGCWhereSystemIsNotInScope()
        => VerifyRefactoring(
            """
            class C
            {
                int[] M() => [|new int[512]|];
            }
            """,
            """
            class C
            {
                int[] M() => System.GC.AllocateUninitializedArray<int>(512);
            }
            """
        );

    // Below the LAQ0006 threshold, where the analyzer deliberately stays quiet
    [Fact]
    public Task LengthBelowZeroingThreshold()
        => VerifyRefactoring(
            """
            using System;
            class C
            {
                byte[] M() => [|new byte[4]|];
            }
            """,
            """
            using System;
            class C
            {
                byte[] M() => GC.AllocateUninitializedArray<byte>(4);
            }
            """
        );

    // Non-constant length, which the analyzer cannot compare against the threshold
    [Fact]
    public Task NonConstantLength()
        => VerifyRefactoring(
            """
            using System;
            class C
            {
                byte[] M(int n) => [|new byte[n * 2]|];
            }
            """,
            """
            using System;
            class C
            {
                byte[] M(int n) => GC.AllocateUninitializedArray<byte>(n * 2);
            }
            """
        );

    [Fact]
    public Task UnmanagedConstrainedTypeParameter()
        => VerifyRefactoring(
            """
            using System;
            class C
            {
                T[] M<T>() where T : unmanaged => [|new T[512]|];
            }
            """,
            """
            using System;
            class C
            {
                T[] M<T>() where T : unmanaged => GC.AllocateUninitializedArray<T>(512);
            }
            """
        );

    // Unconstrained, so the substituted type could turn out to contain references; GC.AllocateUninitializedArray<T> has no unmanaged constraint of its own and just degrades to zeroing in that case
    [Fact]
    public Task UnconstrainedTypeParameter()
        => VerifyRefactoring(
            """
            using System;
            class C
            {
                T[] M<T>() => [|new T[512]|];
            }
            """,
            """
            using System;
            class C
            {
                T[] M<T>() => GC.AllocateUninitializedArray<T>(512);
            }
            """
        );

    [Fact]
    public Task StructConstrainedTypeParameter()
        => VerifyRefactoring(
            """
            using System;
            class C
            {
                T[] M<T>() where T : struct => [|new T[512]|];
            }
            """,
            """
            using System;
            class C
            {
                T[] M<T>() where T : struct => GC.AllocateUninitializedArray<T>(512);
            }
            """
        );

    [Fact]
    public Task ClassConstrainedTypeParameter()
        => VerifyRefactoring(
            """
            using System;
            class C
            {
                T[] M<T>() where T : class => [|new T[512]|];
            }
            """,
            """
            using System;
            class C
            {
                T[] M<T>() where T : class => GC.AllocateUninitializedArray<T>(512);
            }
            """
        );

    [Fact]
    public Task CustomUnmanagedStruct()
        => VerifyRefactoring(
            """
            using System;
            struct S { public int A; }
            class C
            {
                S[] M() => [|new S[512]|];
            }
            """,
            """
            using System;
            struct S { public int A; }
            class C
            {
                S[] M() => GC.AllocateUninitializedArray<S>(512);
            }
            """
        );
    #endregion

    #region GC.AllocateUninitializedArray<T>(length) -> new T[length]
    [Fact]
    public Task InvocationSelected()
        => VerifyRefactoring(
            """
            using System;
            class C
            {
                int[] M() => [|GC.AllocateUninitializedArray<int>(512)|];
            }
            """,
            """
            using System;
            class C
            {
                int[] M() => new int[512];
            }
            """
        );

    [Fact]
    public Task CaretOnMethodName()
        => VerifyRefactoring(
            """
            using System;
            class C
            {
                int[] M() => GC.Allocate[||]UninitializedArray<int>(512);
            }
            """,
            """
            using System;
            class C
            {
                int[] M() => new int[512];
            }
            """
        );

    [Fact]
    public Task ExplicitlyUnpinned()
        => VerifyRefactoring(
            """
            using System;
            class C
            {
                int[] M() => [|GC.AllocateUninitializedArray<int>(512, false)|];
            }
            """,
            """
            using System;
            class C
            {
                int[] M() => new int[512];
            }
            """
        );

    [Fact]
    public Task NamedArgumentsOutOfOrder()
        => VerifyRefactoring(
            """
            using System;
            class C
            {
                int[] M() => [|GC.AllocateUninitializedArray<int>(pinned: false, length: 512)|];
            }
            """,
            """
            using System;
            class C
            {
                int[] M() => new int[512];
            }
            """
        );

    [Fact]
    public Task ManagedElementTypeSwitchesBack()
        => VerifyRefactoring(
            """
            using System;
            class C
            {
                string[] M() => [|GC.AllocateUninitializedArray<string>(512)|];
            }
            """,
            """
            using System;
            class C
            {
                string[] M() => new string[512];
            }
            """
        );

    [Fact]
    public Task ThroughUsingStatic()
        => VerifyRefactoring(
            """
            using static System.GC;
            class C
            {
                int[] M() => [|AllocateUninitializedArray<int>(512)|];
            }
            """,
            """
            using static System.GC;
            class C
            {
                int[] M() => new int[512];
            }
            """
        );
    #endregion

    #region not offered
    [Fact]
    public Task ManagedElementType()
        => VerifyNoRefactoring(
            """
            class C
            {
                string[] M() => [|new string[512]|];
            }
            """
        );

    [Fact]
    public Task StructContainingAReference()
        => VerifyNoRefactoring(
            """
            struct S { public string A; }
            class C
            {
                S[] M() => [|new S[512]|];
            }
            """
        );

    [Fact]
    public Task WithInitializer()
        => VerifyNoRefactoring(
            """
            class C
            {
                int[] M() => [|new int[2] { 1, 2 }|];
            }
            """
        );

    [Fact]
    public Task MultiDimensional()
        => VerifyNoRefactoring(
            """
            class C
            {
                int[,] M() => [|new int[2, 512]|];
            }
            """
        );

    [Fact]
    public Task Jagged()
        => VerifyNoRefactoring(
            """
            class C
            {
                int[][] M() => [|new int[512][]|];
            }
            """
        );

    // Rewriting this to 'new int[512]' would move the allocation out of the pinned object heap
    [Fact]
    public Task PinnedAllocation()
        => VerifyNoRefactoring(
            """
            using System;
            class C
            {
                int[] M() => [|GC.AllocateUninitializedArray<int>(512, true)|];
            }
            """
        );

    [Fact]
    public Task PinnedByNonConstant()
        => VerifyNoRefactoring(
            """
            using System;
            class C
            {
                int[] M(bool pin) => [|GC.AllocateUninitializedArray<int>(512, pin)|];
            }
            """
        );

    [Fact]
    public Task ZeroingAllocateArray()
        => VerifyNoRefactoring(
            """
            using System;
            class C
            {
                int[] M() => [|GC.AllocateArray<int>(512)|];
            }
            """
        );

    [Fact]
    public Task UnrelatedInvocation()
        => VerifyNoRefactoring(
            """
            using System;
            class C
            {
                int[] M() => [|Array.Empty<int>()|];
            }
            """
        );
    #endregion
}
