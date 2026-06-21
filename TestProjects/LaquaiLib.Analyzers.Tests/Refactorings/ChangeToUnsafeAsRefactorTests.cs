namespace LaquaiLib.Analyzers.Tests.Refactorings;

public class ChangeToUnsafeAsRefactorTests
{
    private static Task VerifyRefactoring(string source, string fixedSource)
        => new CSharpCodeRefactoringTest<ChangeToUnsafeAsRefactor, DefaultVerifier>
        {
            TestCode = source,
            FixedCode = fixedSource,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        }.RunAsync();

    private static Task VerifyNoRefactoring(string source)
        => new CSharpCodeRefactoringTest<ChangeToUnsafeAsRefactor, DefaultVerifier>
        {
            TestCode = source,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        }.RunAsync();

    [Fact]
    public Task ExplicitDowncastObjectToString()
        => VerifyRefactoring(
            """
            using System.Runtime.CompilerServices;
            class C
            {
                void M(object obj) { var s = [|(string)obj|]; }
            }
            """,
            """
            using System.Runtime.CompilerServices;
            class C
            {
                void M(object obj) { var s = Unsafe.As<string>(obj); }
            }
            """
        );

    [Fact]
    public Task ExplicitDowncastThroughClassHierarchy()
        => VerifyRefactoring(
            """
            using System.Runtime.CompilerServices;
            class Base { }
            class Derived : Base { }
            class C
            {
                void M(Base b) { var d = [|(Derived)b|]; }
            }
            """,
            """
            using System.Runtime.CompilerServices;
            class Base { }
            class Derived : Base { }
            class C
            {
                void M(Base b) { var d = Unsafe.As<Derived>(b); }
            }
            """
        );

    [Fact]
    public Task AsExpressionObjectToString()
        => VerifyRefactoring(
            """
            using System.Runtime.CompilerServices;
            class C
            {
                void M(object obj) { var s = [|obj as string|]; }
            }
            """,
            """
            using System.Runtime.CompilerServices;
            class C
            {
                void M(object obj) { var s = Unsafe.As<string>(obj); }
            }
            """
        );

    [Fact]
    public Task AsExpressionThroughClassHierarchy()
        => VerifyRefactoring(
            """
            using System.Runtime.CompilerServices;
            class Base { }
            class Derived : Base { }
            class C
            {
                void M(Base b) { var d = [|b as Derived|]; }
            }
            """,
            """
            using System.Runtime.CompilerServices;
            class Base { }
            class Derived : Base { }
            class C
            {
                void M(Base b) { var d = Unsafe.As<Derived>(b); }
            }
            """
        );

    [Fact]
    public Task AddsUsingWhenNotPresent()
        => VerifyRefactoring(
            """
            class C
            {
                void M(object obj) { var s = [|(string)obj|]; }
            }
            """,
            """
            using System.Runtime.CompilerServices;

            class C
            {
                void M(object obj) { var s = Unsafe.As<string>(obj); }
            }
            """
        );

    [Fact]
    public Task NoRefactoringForClassHierarchyUpcast()
        => VerifyNoRefactoring(
            """
            class Base { }
            class Derived : Base { }
            class C
            {
                void M(Derived d) { var b = (Base)d; }
            }
            """
        );

    [Fact]
    public Task NoRefactoringForInterfaceUpcast()
        => VerifyNoRefactoring(
            """
            interface IMyInterface { }
            class MyClass : IMyInterface { }
            class C
            {
                void M(MyClass m) { var i = (IMyInterface)m; }
            }
            """
        );

    [Fact]
    public Task NoRefactoringForValueTypeCast()
        => VerifyNoRefactoring(
            """
            class C
            {
                void M(double d) { var i = (int)d; }
            }
            """
        );
}
