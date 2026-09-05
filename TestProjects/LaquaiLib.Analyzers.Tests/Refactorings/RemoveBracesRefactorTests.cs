namespace LaquaiLib.Analyzers.Tests.Refactorings;

public class RemoveBracesRefactorTests
{
    private static Task VerifyRefactoring(string source, string fixedSource)
        => new CSharpCodeRefactoringTest<RemoveBracesRefactor, DefaultVerifier>
        {
            TestCode = source,
            FixedCode = fixedSource,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            // Without this the formatter reflows with Environment.NewLine, making the expected sources platform-dependent
            TestState = { AnalyzerConfigFiles = { ("/.editorconfig", "root = true\n\n[*.cs]\nend_of_line = crlf\n") } },
        }.RunAsync();

    private static Task VerifyNoRefactoring(string source)
        => new CSharpCodeRefactoringTest<RemoveBracesRefactor, DefaultVerifier>
        {
            TestCode = source,
            FixedCode = source.Replace("[||]", "").Replace("[|", "").Replace("|]", ""),
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        }.RunAsync();

    #region offered
    [Fact]
    public Task IfStatementCaretOnStatement()
        => VerifyRefactoring(
            """
            class C
            {
                void M(bool a)
                {
                    if (a)
                    {
                        [||]N();
                    }
                    N();
                }
                static void N() { }
            }
            """,
            """
            class C
            {
                void M(bool a)
                {
                    if (a)
                        N();
                    N();
                }
                static void N() { }
            }
            """
        );

    [Fact]
    public Task IfStatementCaretOnKeyword()
        => VerifyRefactoring(
            """
            class C
            {
                void M(bool a)
                {
                    i[||]f (a)
                    {
                        N();
                    }
                }
                static void N() { }
            }
            """,
            """
            class C
            {
                void M(bool a)
                {
                    if (a)
                        N();
                }
                static void N() { }
            }
            """
        );

    [Fact]
    public Task IfStatementCaretOnClosingBrace()
        => VerifyRefactoring(
            """
            class C
            {
                void M(bool a)
                {
                    if (a)
                    {
                        N();
                    [||]}
                }
                static void N() { }
            }
            """,
            """
            class C
            {
                void M(bool a)
                {
                    if (a)
                        N();
                }
                static void N() { }
            }
            """
        );

    [Fact]
    public Task ElseClause()
        => VerifyRefactoring(
            """
            class C
            {
                void M(bool a)
                {
                    if (a)
                        N();
                    else
                    {
                        [||]N();
                    }
                }
                static void N() { }
            }
            """,
            """
            class C
            {
                void M(bool a)
                {
                    if (a)
                        N();
                    else
                        N();
                }
                static void N() { }
            }
            """
        );

    // An else wrapping a lone if is an else-if chain once the braces are gone
    [Fact]
    public Task ElseClauseWrappingIfBecomesElseIf()
        => VerifyRefactoring(
            """
            class C
            {
                void M(bool a, bool b)
                {
                    if (a)
                        N();
                    else
                    {
                        [||]if (b)
                            N();
                    }
                }
                static void N() { }
            }
            """,
            """
            class C
            {
                void M(bool a, bool b)
                {
                    if (a)
                        N();
                    else if (b)
                        N();
                }
                static void N() { }
            }
            """
        );

    [Fact]
    public Task ForEachStatement()
        => VerifyRefactoring(
            """
            class C
            {
                void M(int[] xs)
                {
                    foreach (var x in xs)
                    {
                        [||]N();
                    }
                }
                static void N() { }
            }
            """,
            """
            class C
            {
                void M(int[] xs)
                {
                    foreach (var x in xs)
                        N();
                }
                static void N() { }
            }
            """
        );

    [Fact]
    public Task UsingStatement()
        => VerifyRefactoring(
            """
            class C
            {
                void M(System.IDisposable d)
                {
                    using (d)
                    {
                        [||]N();
                    }
                }
                static void N() { }
            }
            """,
            """
            class C
            {
                void M(System.IDisposable d)
                {
                    using (d)
                        N();
                }
                static void N() { }
            }
            """
        );

    [Fact]
    public Task LockStatement()
        => VerifyRefactoring(
            """
            class C
            {
                private readonly object _gate = new object();
                void M()
                {
                    lock (_gate)
                    {
                        [||]N();
                    }
                }
                static void N() { }
            }
            """,
            """
            class C
            {
                private readonly object _gate = new object();
                void M()
                {
                    lock (_gate)
                        N();
                }
                static void N() { }
            }
            """
        );

    [Fact]
    public Task SingleLineBlockKeepsTheFollowingLine()
        => VerifyRefactoring(
            """
            class C
            {
                void M(bool a)
                {
                    while (a) { [||]N(); }
                    N();
                }
                static void N() { }
            }
            """,
            """
            class C
            {
                void M(bool a)
                {
                    while (a) N();
                    N();
                }
                static void N() { }
            }
            """
        );

    [Fact]
    public Task CommentAboveStatementIsPreserved()
        => VerifyRefactoring(
            """
            class C
            {
                void M(bool a)
                {
                    if (a)
                    {
                        // note
                        [||]N();
                    }
                }
                static void N() { }
            }
            """,
            """
            class C
            {
                void M(bool a)
                {
                    if (a)
                        // note
                        N();
                }
                static void N() { }
            }
            """
        );

    // The sibling block declares the same name, so the fixed source only compiles if the out variable stays scoped to the embedded statement
    [Fact]
    public Task CommentAboveClosingBraceStaysAfterStatement()
        => VerifyRefactoring(
            """
            class C
            {
                void M(bool a)
                {
                    if (a)
                    {
                        [||]N();
                        // note
                    }
                    N();
                }
                static void N() { }
            }
            """,
            """
            class C
            {
                void M(bool a)
                {
                    if (a)
                        N();
                        // note
                    N();
                }
                static void N() { }
            }
            """
        );

    [Fact]
    public Task CommentOnClosingBraceMovesWhereTheBraceWas()
        => VerifyRefactoring(
            """
            class C
            {
                void M(bool a)
                {
                    if (a)
                    {
                        [||]N();
                    } // note
                    N();
                }
                static void N() { }
            }
            """,
            """
            class C
            {
                void M(bool a)
                {
                    if (a)
                        N();
                    // note
                    N();
                }
                static void N() { }
            }
            """
        );

    [Fact]
    public Task CommentOnOpeningBraceMovesWhereTheBraceWas()
        => VerifyRefactoring(
            """
            class C
            {
                void M(bool a)
                {
                    if (a)
                    { // note
                        [||]N();
                    }
                }
                static void N() { }
            }
            """,
            """
            class C
            {
                void M(bool a)
                {
                    if (a)
                        // note
                        N();
                }
                static void N() { }
            }
            """
        );

    [Fact]
    public Task OutVariableStaysScopedToTheStatement()
        => VerifyRefactoring(
            """
            class C
            {
                void M(bool a)
                {
                    if (a)
                    {
                        [||]N(out var v);
                    }
                    if (a)
                    {
                        N(out var v);
                    }
                }
                static void N(out int v) => v = 0;
            }
            """,
            """
            class C
            {
                void M(bool a)
                {
                    if (a)
                        N(out var v);
                    if (a)
                    {
                        N(out var v);
                    }
                }
                static void N(out int v) => v = 0;
            }
            """
        );

    // The while clause terminates a do statement, so nothing can reattach to the if
    [Fact]
    public Task DanglingIfUnderDoStatement()
        => VerifyRefactoring(
            """
            class C
            {
                void M(bool a, bool b, ref int x)
                {
                    if (a)
                    {
                        [||]do
                            if (b)
                                x = 1;
                        while (b);
                    }
                    else
                    {
                        x = 2;
                    }
                }
            }
            """,
            """
            class C
            {
                void M(bool a, bool b, ref int x)
                {
                    if (a)
                        do
                            if (b)
                                x = 1;
                        while (b);
                    else
                    {
                        x = 2;
                    }
                }
            }
            """
        );

    // Same dangling if, but with no else anywhere for it to capture
    [Fact]
    public Task DanglingIfWithNothingToCapture()
        => VerifyRefactoring(
            """
            class C
            {
                void M(bool a, bool b, ref int x)
                {
                    if (a)
                    {
                        [||]for (var i = 0; i < 1; i++)
                            if (b)
                                x = 1;
                    }
                    x = 2;
                }
            }
            """,
            """
            class C
            {
                void M(bool a, bool b, ref int x)
                {
                    if (a)
                        for (var i = 0; i < 1; i++)
                            if (b)
                                x = 1;
                    x = 2;
                }
            }
            """
        );
    #endregion

    #region not offered
    [Fact]
    public Task MethodBody()
        => VerifyNoRefactoring(
            """
            class C
            {
                void M()
                {
                    [||]N();
                }
                static void N() { }
            }
            """
        );

    [Fact]
    public Task FreeStandingBlock()
        => VerifyNoRefactoring(
            """
            class C
            {
                void M()
                {
                    {
                        [||]N();
                    }
                }
                static void N() { }
            }
            """
        );

    [Fact]
    public Task TryBlock()
        => VerifyNoRefactoring(
            """
            class C
            {
                void M()
                {
                    try
                    {
                        [||]N();
                    }
                    catch { }
                }
                static void N() { }
            }
            """
        );

    [Fact]
    public Task SwitchSection()
        => VerifyNoRefactoring(
            """
            class C
            {
                void M(int i)
                {
                    switch (i)
                    {
                        case 0:
                        {
                            [||]N();
                            break;
                        }
                    }
                }
                static void N() { }
            }
            """
        );

    [Fact]
    public Task EmptyBlock()
        => VerifyNoRefactoring(
            """
            class C
            {
                void M(bool a)
                {
                    if (a)
                    {
                    [||]}
                }
            }
            """
        );

    [Fact]
    public Task MultipleStatements()
        => VerifyNoRefactoring(
            """
            class C
            {
                void M(bool a)
                {
                    if (a)
                    {
                        [||]N();
                        N();
                    }
                }
                static void N() { }
            }
            """
        );

    [Fact]
    public Task LocalDeclaration()
        => VerifyNoRefactoring(
            """
            class C
            {
                void M(bool a)
                {
                    if (a)
                    {
                        [||]var x = 1;
                    }
                }
            }
            """
        );

    [Fact]
    public Task LocalFunction()
        => VerifyNoRefactoring(
            """
            class C
            {
                void M(bool a)
                {
                    if (a)
                    {
                        [||]void L() { }
                        L();
                    }
                }
            }
            """
        );

    [Fact]
    public Task LabeledStatement()
        => VerifyNoRefactoring(
            """
            class C
            {
                void M(bool a)
                {
                    if (a)
                    {
                        [||]l: N();
                        goto l;
                    }
                }
                static void N() { }
            }
            """
        );

    [Fact]
    public Task ConditionalDirectiveAroundStatement()
        => VerifyNoRefactoring(
            """
            class C
            {
                void M(bool a)
                {
                    if (a)
                    {
            #if !DEBUG
                        [||]N();
            #endif
                    }
                }
                static void N() { }
            }
            """
        );

    // Unwrapping would let the else bind to the inner if, silently changing what the code does
    [Fact]
    public Task DanglingIfWouldCaptureElse()
        => VerifyNoRefactoring(
            """
            class C
            {
                void M(bool a, bool b, ref int x)
                {
                    i[||]f (a)
                    {
                        if (b)
                            x = 1;
                    }
                    else
                    {
                        x = 2;
                    }
                }
            }
            """
        );

    [Fact]
    public Task DanglingIfUnderLoopWouldCaptureElse()
        => VerifyNoRefactoring(
            """
            class C
            {
                void M(bool a, bool b, ref int x)
                {
                    i[||]f (a)
                    {
                        for (var i = 0; i < 1; i++)
                            if (b)
                                x = 1;
                    }
                    else
                    {
                        x = 2;
                    }
                }
            }
            """
        );

    // The else belongs to the inner if once unwrapped, which is what the braces were saying
    [Fact]
    public Task ThenBranchWrappingIfElse()
        => VerifyNoRefactoring(
            """
            class C
            {
                void M(bool a, bool b, ref int x)
                {
                    i[||]f (a)
                    {
                        if (b)
                            x = 1;
                        else
                            x = 2;
                    }
                }
            }
            """
        );
    #endregion

    #region repeated application
    // Collapsing the innermost blocks first must not open the door to collapsing the outer one afterwards.
    // Each step below feeds the previous step's output back in; the last two assert the outer braces survive.
    [Fact]
    public Task NestedIfElseStep1InnerThenBranch()
        => VerifyRefactoring(
            """
            class C
            {
                void M(bool a, bool b, ref int x)
                {
                    if (a)
                    {
                        if (b)
                        {
                            [||]x = 1;
                        }
                        else
                        {
                            x = 2;
                        }
                    }
                    else
                    {
                        x = 3;
                    }
                }
            }
            """,
            """
            class C
            {
                void M(bool a, bool b, ref int x)
                {
                    if (a)
                    {
                        if (b)
                            x = 1;
                        else
                        {
                            x = 2;
                        }
                    }
                    else
                    {
                        x = 3;
                    }
                }
            }
            """
        );

    [Fact]
    public Task NestedIfElseStep2InnerElseBranch()
        => VerifyRefactoring(
            """
            class C
            {
                void M(bool a, bool b, ref int x)
                {
                    if (a)
                    {
                        if (b)
                            x = 1;
                        else
                        {
                            [||]x = 2;
                        }
                    }
                    else
                    {
                        x = 3;
                    }
                }
            }
            """,
            """
            class C
            {
                void M(bool a, bool b, ref int x)
                {
                    if (a)
                    {
                        if (b)
                            x = 1;
                        else
                            x = 2;
                    }
                    else
                    {
                        x = 3;
                    }
                }
            }
            """
        );

    [Fact]
    public Task NestedIfElseStep3OuterElseBranch()
        => VerifyRefactoring(
            """
            class C
            {
                void M(bool a, bool b, ref int x)
                {
                    if (a)
                    {
                        if (b)
                            x = 1;
                        else
                            x = 2;
                    }
                    else
                    {
                        [||]x = 3;
                    }
                }
            }
            """,
            """
            class C
            {
                void M(bool a, bool b, ref int x)
                {
                    if (a)
                    {
                        if (b)
                            x = 1;
                        else
                            x = 2;
                    }
                    else
                        x = 3;
                }
            }
            """
        );

    [Fact]
    public Task NestedIfElseStep4OuterBracesSurviveCaretOnKeyword()
        => VerifyNoRefactoring(
            """
            class C
            {
                void M(bool a, bool b, ref int x)
                {
                    i[||]f (a)
                    {
                        if (b)
                            x = 1;
                        else
                            x = 2;
                    }
                    else
                        x = 3;
                }
            }
            """
        );

    [Fact]
    public Task NestedIfElseStep4OuterBracesSurviveCaretOnBrace()
        => VerifyNoRefactoring(
            """
            class C
            {
                void M(bool a, bool b, ref int x)
                {
                    if (a)
                    {
                        if (b)
                            x = 1;
                        else
                            x = 2;
                    [||]}
                    else
                        x = 3;
                }
            }
            """
        );

    [Fact]
    public Task DanglingIfOuterBracesSurviveAfterElseCollapse()
        => VerifyNoRefactoring(
            """
            class C
            {
                void M(bool a, bool b, ref int x)
                {
                    i[||]f (a)
                    {
                        if (b)
                            x = 1;
                    }
                    else
                        x = 2;
                }
            }
            """
        );
    #endregion
}
