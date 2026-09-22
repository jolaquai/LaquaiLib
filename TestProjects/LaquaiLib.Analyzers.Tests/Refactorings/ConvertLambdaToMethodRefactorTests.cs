namespace LaquaiLib.Analyzers.Tests.Refactorings;

public class ConvertLambdaToMethodRefactorTests
{
    private static readonly string Key = $"{typeof(ConvertLambdaToMethodRefactor).FullName}_ConvertLambdaToMethod";

    private static Task Verify(string source, string fixedSource)
        => new CSharpCodeRefactoringTest<ConvertLambdaToMethodRefactor, DefaultVerifier>
        {
            TestCode = source,
            FixedCode = fixedSource,
            CodeActionEquivalenceKey = Key,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestState = { AnalyzerConfigFiles = { ("/.editorconfig", "root = true\n\n[*.cs]\nend_of_line = crlf\n") } },
        }.RunAsync();

    private static Task VerifyNoRefactoring(string source)
        => new CSharpCodeRefactoringTest<ConvertLambdaToMethodRefactor, DefaultVerifier>
        {
            TestCode = source,
            FixedCode = source.Replace("[||]", "").Replace("[|", "").Replace("|]", ""),
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        }.RunAsync();

    [Fact]
    public Task ExpressionBodiedLocalNamedFromDeclarator()
        => Verify(
            """
            using System;
            class C
            {
                void M()
                {
                    Func<int, int> square = x =[||]> x * x;
                }
            }
            """,
            """
            using System;
            class C
            {
                void M()
                {
                    Func<int, int> square = Square;
                }

                private static int Square(int x) => x * x;
            }
            """
        );

    [Fact]
    public Task BlockBodiedLocalKeepsBlockAsMethodBody()
        => Verify(
            """
            using System;
            class C
            {
                void M()
                {
                    Func<int, int> square = x =[||]>
                    {
                        return x * x;
                    };
                }
            }
            """,
            """
            using System;
            class C
            {
                void M()
                {
                    Func<int, int> square = Square;
                }

                private static int Square(int x)
                {
                    return x * x;
                }
            }
            """
        );

    [Fact]
    public Task NameDerivedFromParameterWhenPassedAsArgument()
        => Verify(
            """
            using System;
            class C
            {
                void M()
                {
                    Invoke(x =[||]> x * x);
                }
                static void Invoke(Func<int, int> selector) { }
            }
            """,
            """
            using System;
            class C
            {
                void M()
                {
                    Invoke(Selector);
                }

                private static int Selector(int x) => x * x;

                static void Invoke(Func<int, int> selector) { }
            }
            """
        );

    [Fact]
    public Task InstanceMemberAccessProducesInstanceMethod()
        => Verify(
            """
            using System;
            class C
            {
                int _factor = 2;
                void M()
                {
                    Func<int, int> scaled = x =[||]> x * _factor;
                }
            }
            """,
            """
            using System;
            class C
            {
                int _factor = 2;
                void M()
                {
                    Func<int, int> scaled = Scaled;
                }

                private int Scaled(int x) => x * _factor;
            }
            """
        );

    [Fact]
    public Task GeneratedNameAvoidsCollisionWithExistingMember()
        => Verify(
            """
            using System;
            class C
            {
                void Square() { }
                void M()
                {
                    Func<int, int> square = x =[||]> x * x;
                }
            }
            """,
            """
            using System;
            class C
            {
                void Square() { }
                void M()
                {
                    Func<int, int> square = Square2;
                }

                private static int Square2(int x) => x * x;
            }
            """
        );

    #region not offered
    [Fact]
    public Task CapturingOuterLocalIsNotOffered()
        => VerifyNoRefactoring(
            """
            using System;
            class C
            {
                void M()
                {
                    var factor = 2;
                    Func<int, int> scaled = x =[||]> x * factor;
                }
            }
            """
        );

    [Fact]
    public Task CapturingParameterIsNotOffered()
        => VerifyNoRefactoring(
            """
            using System;
            class C
            {
                void M(int factor)
                {
                    Func<int, int> scaled = x =[||]> x * factor;
                }
            }
            """
        );

    [Fact]
    public Task DuplicateDiscardParametersIsNotOffered()
        => VerifyNoRefactoring(
            """
            using System;
            class C
            {
                void M()
                {
                    Func<int, int, int> add = (_, _) =[||]> 0;
                }
            }
            """
        );

    [Fact]
    public Task CaretInLambdaBodyIsNotOffered()
        => VerifyNoRefactoring(
            """
            using System;
            class C
            {
                void M()
                {
                    Func<int, int> square = x => [||]x * x;
                }
            }
            """
        );
    #endregion
}
