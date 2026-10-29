namespace LaquaiLib.Analyzers.Tests.Refactorings;

public class UseArrayPoolRefactorTests
{
    private static Task VerifyRefactoring(string source, string fixedSource)
        => new CSharpCodeRefactoringTest<UseArrayPoolRefactor, DefaultVerifier>
        {
            TestCode = source,
            FixedCode = fixedSource,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            // Without this the formatter reflows with Environment.NewLine, making the expected sources platform-dependent
            TestState = { AnalyzerConfigFiles = { ("/.editorconfig", "root = true\n\n[*.cs]\nend_of_line = crlf\n") } },
        }.RunAsync();

    private static Task VerifyNoRefactoring(string source)
        => new CSharpCodeRefactoringTest<UseArrayPoolRefactor, DefaultVerifier>
        {
            TestCode = source,
            FixedCode = source.Replace("[||]", "").Replace("[|", "").Replace("|]", ""),
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        }.RunAsync();

    #region new T[length] -> ArrayPool<T>.Shared.Rent(length)
    [Fact]
    public Task ArrayCreationSelected()
        => VerifyRefactoring(
            """
            using System.Buffers;
            class C
            {
                void M()
                {
                    var arr = [|new int[512]|];
                    Use(arr);
                }
                void Use(int[] a) { }
            }
            """,
            """
            using System.Buffers;
            class C
            {
                void M()
                {
                    var arr = ArrayPool<int>.Shared.Rent(512);
                    try
                    {
                        Use(arr);
                    }
                    finally
                    {
                        ArrayPool<int>.Shared.Return(arr);
                    }
                }
                void Use(int[] a) { }
            }
            """
        );

    [Fact]
    public Task AddsMissingUsing()
        => VerifyRefactoring(
            """
            class C
            {
                void M()
                {
                    var arr = [|new int[512]|];
                    Use(arr);
                }
                void Use(int[] a) { }
            }
            """,
            """
            using System.Buffers;

            class C
            {
                void M()
                {
                    var arr = ArrayPool<int>.Shared.Rent(512);
                    try
                    {
                        Use(arr);
                    }
                    finally
                    {
                        ArrayPool<int>.Shared.Return(arr);
                    }
                }
                void Use(int[] a) { }
            }
            """
        );

    [Fact]
    public Task ExplicitlyTypedDeclaration()
        => VerifyRefactoring(
            """
            using System.Buffers;
            class C
            {
                void M()
                {
                    int[] arr = [|new int[512]|];
                    Use(arr);
                }
                void Use(int[] a) { }
            }
            """,
            """
            using System.Buffers;
            class C
            {
                void M()
                {
                    int[] arr = ArrayPool<int>.Shared.Rent(512);
                    try
                    {
                        Use(arr);
                    }
                    finally
                    {
                        ArrayPool<int>.Shared.Return(arr);
                    }
                }
                void Use(int[] a) { }
            }
            """
        );

    [Fact]
    public Task WithInitializerBecomesIndividualStores()
        => VerifyRefactoring(
            """
            using System.Buffers;
            class C
            {
                void M()
                {
                    var arr = [|new int[] { 1, 2, 3 }|];
                    Use(arr);
                }
                void Use(int[] a) { }
            }
            """,
            """
            using System.Buffers;
            class C
            {
                void M()
                {
                    var arr = ArrayPool<int>.Shared.Rent(3);
                    try
                    {
                        arr[0] = 1;
                        arr[1] = 2;
                        arr[2] = 3;
                        Use(arr);
                    }
                    finally
                    {
                        ArrayPool<int>.Shared.Return(arr);
                    }
                }
                void Use(int[] a) { }
            }
            """
        );

    // A constant size can just be repeated verbatim at every '.Length' site - no capture needed
    [Fact]
    public Task LengthReadWithConstantSizeIsSubstituted()
        => VerifyRefactoring(
            """
            using System.Buffers;
            class C
            {
                void M()
                {
                    var arr = [|new int[512]|];
                    for (var i = 0; i < arr.Length; i++)
                    {
                        Use(arr[i]);
                    }
                }
                void Use(int a) { }
            }
            """,
            """
            using System.Buffers;
            class C
            {
                void M()
                {
                    var arr = ArrayPool<int>.Shared.Rent(512);
                    try
                    {
                        for (var i = 0; i < 512; i++)
                        {
                            Use(arr[i]);
                        }
                    }
                    finally
                    {
                        ArrayPool<int>.Shared.Return(arr);
                    }
                }
                void Use(int a) { }
            }
            """
        );

    // An initializer's length is also a compile-time constant, so this substitutes just like an explicit constant size
    [Fact]
    public Task LengthReadWithInitializerIsSubstituted()
        => VerifyRefactoring(
            """
            using System.Buffers;
            class C
            {
                void M()
                {
                    var arr = [|new int[] { 1, 2, 3 }|];
                    var len = arr.Length;
                }
            }
            """,
            """
            using System.Buffers;
            class C
            {
                void M()
                {
                    var arr = ArrayPool<int>.Shared.Rent(3);
                    try
                    {
                        arr[0] = 1;
                        arr[1] = 2;
                        arr[2] = 3;
                        var len = 3;
                    }
                    finally
                    {
                        ArrayPool<int>.Shared.Return(arr);
                    }
                }
            }
            """
        );

    // A non-constant size can't be safely re-evaluated at each site, so it's hoisted into a captured local that Rent and every '.Length' read both point at
    [Fact]
    public Task LengthReadWithNonConstantSizeIsCaptured()
        => VerifyRefactoring(
            """
            using System.Buffers;
            class C
            {
                void M(int n)
                {
                    var arr = [|new int[n]|];
                    for (var i = 0; i < arr.Length; i++)
                    {
                        Use(arr[i]);
                    }
                }
                void Use(int a) { }
            }
            """,
            """
            using System.Buffers;
            class C
            {
                void M(int n)
                {
                    var arrLength = n;
                    var arr = ArrayPool<int>.Shared.Rent(arrLength);
                    try
                    {
                        for (var i = 0; i < arrLength; i++)
                        {
                            Use(arr[i]);
                        }
                    }
                    finally
                    {
                        ArrayPool<int>.Shared.Return(arr);
                    }
                }
                void Use(int a) { }
            }
            """
        );

    [Fact]
    public Task NoStatementsAfterDeclaration()
        => VerifyRefactoring(
            """
            using System.Buffers;
            class C
            {
                void M()
                {
                    var arr = [|new int[512]|];
                }
            }
            """,
            """
            using System.Buffers;
            class C
            {
                void M()
                {
                    var arr = ArrayPool<int>.Shared.Rent(512);
                    try
                    {
                    }
                    finally
                    {
                        ArrayPool<int>.Shared.Return(arr);
                    }
                }
            }
            """
        );
    #endregion

    #region ArrayPool<T>.Shared.Rent(length) -> new T[length]
    [Fact]
    public Task InvocationSelected()
        => VerifyRefactoring(
            """
            using System.Buffers;
            class C
            {
                void M()
                {
                    var arr = [|ArrayPool<int>.Shared.Rent(512)|];
                    try
                    {
                        Use(arr);
                    }
                    finally
                    {
                        ArrayPool<int>.Shared.Return(arr);
                    }
                }
                void Use(int[] a) { }
            }
            """,
            """
            using System.Buffers;
            class C
            {
                void M()
                {
                    var arr = new int[512];
                    Use(arr);
                }
                void Use(int[] a) { }
            }
            """
        );

    [Fact]
    public Task RecoversInitializerFromLeadingStores()
        => VerifyRefactoring(
            """
            using System.Buffers;
            class C
            {
                void M()
                {
                    var arr = [|ArrayPool<int>.Shared.Rent(3)|];
                    try
                    {
                        arr[0] = 1;
                        arr[1] = 2;
                        arr[2] = 3;
                        Use(arr);
                    }
                    finally
                    {
                        ArrayPool<int>.Shared.Return(arr);
                    }
                }
                void Use(int[] a) { }
            }
            """,
            """
            using System.Buffers;
            class C
            {
                void M()
                {
                    var arr = new int[] { 1, 2, 3 };
                    Use(arr);
                }
                void Use(int[] a) { }
            }
            """
        );

    // Only 2 of the 3 requested slots were written back, so this isn't the shape 'Pool' would have produced
    [Fact]
    public Task PartialStoresAreNotTreatedAsAnInitializer()
        => VerifyRefactoring(
            """
            using System.Buffers;
            class C
            {
                void M()
                {
                    var arr = [|ArrayPool<int>.Shared.Rent(3)|];
                    try
                    {
                        arr[0] = 1;
                        arr[1] = 2;
                        Use(arr);
                    }
                    finally
                    {
                        ArrayPool<int>.Shared.Return(arr);
                    }
                }
                void Use(int[] a) { }
            }
            """,
            """
            using System.Buffers;
            class C
            {
                void M()
                {
                    var arr = new int[3];
                    arr[0] = 1;
                    arr[1] = 2;
                    Use(arr);
                }
                void Use(int[] a) { }
            }
            """
        );
    #endregion

    #region condition ? stackalloc T[length] : new T[length] -> stack-or-pool
    [Fact]
    public Task StackAllocTrueBranchSelected()
        => VerifyRefactoring(
            """
            using System;
            using System.Buffers;
            class C
            {
                void M(int n)
                {
                    Span<byte> span = n <= 256 ? [|stackalloc byte[n]|] : new byte[n];
                    Use(span);
                }
                void Use(Span<byte> s) { }
            }
            """,
            """
            using System;
            using System.Buffers;
            class C
            {
                void M(int n)
                {
                    byte[] spanBuffer = null;
                    Span<byte> span = n <= 256 ? stackalloc byte[n] : (spanBuffer = ArrayPool<byte>.Shared.Rent(n)).AsSpan(0, n);
                    try
                    {
                        Use(span);
                    }
                    finally
                    {
                        if (spanBuffer != null)
                        {
                            ArrayPool<byte>.Shared.Return(spanBuffer);
                        }
                    }
                }
                void Use(Span<byte> s) { }
            }
            """
        );

    // Caret inside the 'new' branch instead of the stackalloc branch reaches the same rewrite
    [Fact]
    public Task NewFalseBranchSelected()
        => VerifyRefactoring(
            """
            using System;
            using System.Buffers;
            class C
            {
                void M(int n)
                {
                    Span<byte> span = n <= 256 ? stackalloc byte[n] : [|new byte[n]|];
                    Use(span);
                }
                void Use(Span<byte> s) { }
            }
            """,
            """
            using System;
            using System.Buffers;
            class C
            {
                void M(int n)
                {
                    byte[] spanBuffer = null;
                    Span<byte> span = n <= 256 ? stackalloc byte[n] : (spanBuffer = ArrayPool<byte>.Shared.Rent(n)).AsSpan(0, n);
                    try
                    {
                        Use(span);
                    }
                    finally
                    {
                        if (spanBuffer != null)
                        {
                            ArrayPool<byte>.Shared.Return(spanBuffer);
                        }
                    }
                }
                void Use(Span<byte> s) { }
            }
            """
        );

    // The branches can be swapped - 'new' first, stackalloc second - and only the array branch is ever touched, so no branch-order bookkeeping is needed at all
    [Fact]
    public Task BranchesSwapped()
        => VerifyRefactoring(
            """
            using System;
            using System.Buffers;
            class C
            {
                void M(int n)
                {
                    Span<byte> span = n > 256 ? new byte[n] : [|stackalloc byte[n]|];
                    Use(span);
                }
                void Use(Span<byte> s) { }
            }
            """,
            """
            using System;
            using System.Buffers;
            class C
            {
                void M(int n)
                {
                    byte[] spanBuffer = null;
                    Span<byte> span = n > 256 ? (spanBuffer = ArrayPool<byte>.Shared.Rent(n)).AsSpan(0, n) : stackalloc byte[n];
                    try
                    {
                        Use(span);
                    }
                    finally
                    {
                        if (spanBuffer != null)
                        {
                            ArrayPool<byte>.Shared.Return(spanBuffer);
                        }
                    }
                }
                void Use(Span<byte> s) { }
            }
            """
        );

    [Fact]
    public Task VarInferredDeclaration()
        => VerifyRefactoring(
            """
            using System;
            using System.Buffers;
            class C
            {
                void M(int n)
                {
                    var span = n <= 256 ? [|stackalloc byte[n]|] : new byte[n];
                    Use(span);
                }
                void Use(Span<byte> s) { }
            }
            """,
            """
            using System;
            using System.Buffers;
            class C
            {
                void M(int n)
                {
                    byte[] spanBuffer = null;
                    var span = n <= 256 ? stackalloc byte[n] : (spanBuffer = ArrayPool<byte>.Shared.Rent(n)).AsSpan(0, n);
                    try
                    {
                        Use(span);
                    }
                    finally
                    {
                        if (spanBuffer != null)
                        {
                            ArrayPool<byte>.Shared.Return(spanBuffer);
                        }
                    }
                }
                void Use(Span<byte> s) { }
            }
            """
        );

    // '.Length' on the span is always exactly the request on either path, so unlike the plain-array case it is never rebound
    [Fact]
    public Task SpanLengthReadIsLeftAlone()
        => VerifyRefactoring(
            """
            using System;
            using System.Buffers;
            class C
            {
                void M(int n)
                {
                    Span<byte> span = n <= 256 ? [|stackalloc byte[n]|] : new byte[n];
                    var len = span.Length;
                }
            }
            """,
            """
            using System;
            using System.Buffers;
            class C
            {
                void M(int n)
                {
                    byte[] spanBuffer = null;
                    Span<byte> span = n <= 256 ? stackalloc byte[n] : (spanBuffer = ArrayPool<byte>.Shared.Rent(n)).AsSpan(0, n);
                    try
                    {
                        var len = span.Length;
                    }
                    finally
                    {
                        if (spanBuffer != null)
                        {
                            ArrayPool<byte>.Shared.Return(spanBuffer);
                        }
                    }
                }
            }
            """
        );
    #endregion

    #region stack-or-pool not offered
    [Fact]
    public Task StackAllocDifferentSizeExpressions()
        => VerifyNoRefactoring(
            """
            using System;
            class C
            {
                void M(int n, int m)
                {
                    Span<byte> span = n <= 256 ? [|stackalloc byte[n]|] : new byte[m];
                }
            }
            """
        );

    [Fact]
    public Task StackAllocWithInitializer()
        => VerifyNoRefactoring(
            """
            using System;
            class C
            {
                void M()
                {
                    Span<byte> span = true ? [|stackalloc byte[3] { 1, 2, 3 }|] : new byte[3];
                }
            }
            """
        );

    [Fact]
    public Task PlainTernaryWithoutStackAllocIsUnaffected()
        => VerifyNoRefactoring(
            """
            class C
            {
                void M(bool cond, int n)
                {
                    var arr = cond ? [|new int[n]|] : new int[n + 1];
                }
            }
            """
        );
    #endregion

    #region not offered
    [Fact]
    public Task MultiDimensional()
        => VerifyNoRefactoring(
            """
            class C
            {
                void M()
                {
                    var arr = [|new int[2, 512]|];
                }
            }
            """
        );

    [Fact]
    public Task Jagged()
        => VerifyNoRefactoring(
            """
            class C
            {
                void M()
                {
                    var arr = [|new int[512][]|];
                }
            }
            """
        );

    [Fact]
    public Task NotALocalDeclaration()
        => VerifyNoRefactoring(
            """
            class C
            {
                int[] M() => [|new int[512]|];
            }
            """
        );

    [Fact]
    public Task ArrayIsReturned()
        => VerifyNoRefactoring(
            """
            class C
            {
                int[] M()
                {
                    var arr = [|new int[512]|];
                    return arr;
                }
            }
            """
        );

    [Fact]
    public Task ArrayEscapesIntoAField()
        => VerifyNoRefactoring(
            """
            class C
            {
                int[] _field;
                void M()
                {
                    var arr = [|new int[512]|];
                    _field = arr;
                }
            }
            """
        );

    [Fact]
    public Task ArrayCapturedByLambda()
        => VerifyNoRefactoring(
            """
            using System;
            class C
            {
                void M()
                {
                    var arr = [|new int[512]|];
                    Action a = () => Use(arr);
                    a();
                }
                void Use(int[] a) { }
            }
            """
        );

    [Fact]
    public Task PassedByRef()
        => VerifyNoRefactoring(
            """
            class C
            {
                void M()
                {
                    var arr = [|new int[512]|];
                    Use(ref arr);
                }
                void Use(ref int[] a) { }
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

    [Fact]
    public Task CatchClausePresent()
        => VerifyNoRefactoring(
            """
            using System.Buffers;
            class C
            {
                void M()
                {
                    var arr = [|ArrayPool<int>.Shared.Rent(512)|];
                    try
                    {
                        Use(arr);
                    }
                    catch
                    {
                    }
                    finally
                    {
                        ArrayPool<int>.Shared.Return(arr);
                    }
                }
                void Use(int[] a) { }
            }
            """
        );

    [Fact]
    public Task StatementAfterTryBlocksTheRewrite()
        => VerifyNoRefactoring(
            """
            using System.Buffers;
            class C
            {
                void M()
                {
                    var arr = [|ArrayPool<int>.Shared.Rent(512)|];
                    try
                    {
                        Use(arr);
                    }
                    finally
                    {
                        ArrayPool<int>.Shared.Return(arr);
                    }
                    Use(arr);
                }
                void Use(int[] a) { }
            }
            """
        );
    #endregion
}
