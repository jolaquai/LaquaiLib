namespace LaquaiLib.Analyzers.Tests.Refactorings;

public class ChangeToUnsafeAsRefactorTests
{
    private static Task VerifyRefactoring(string source, string fixedSource)
        => new CSharpCodeRefactoringTest<ChangeToUnsafeAsRefactor, DefaultVerifier>
        {
            TestCode = source,
            FixedCode = fixedSource,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            // Without this the formatter reflows with Environment.NewLine, making the expected sources platform-dependent
            TestState = { AnalyzerConfigFiles = { ("/.editorconfig", "root = true\n\n[*.cs]\nend_of_line = crlf\n") } },
        }.RunAsync();

    private static Task VerifyNoRefactoring(string source)
        => new CSharpCodeRefactoringTest<ChangeToUnsafeAsRefactor, DefaultVerifier>
        {
            TestCode = source,
            FixedCode = source.Replace("[|", "").Replace("|]", ""),
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
                void M(Derived d) { var b = [|(Base)d|]; }
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
                void M(MyClass m) { var i = [|(IMyInterface)m|]; }
            }
            """
        );

    [Fact]
    public Task NoRefactoringForUnconstrainedGenericUnboxCast()
        => VerifyNoRefactoring(
            """
            using System.Runtime.CompilerServices;
            class C
            {
                static T GetUninitializedObject<T>() => [|(T)RuntimeHelpers.GetUninitializedObject(typeof(T))|];
            }
            """
        );

    [Fact]
    public Task NoRefactoringForValueTypeCast()
        => VerifyNoRefactoring(
            """
            class C
            {
                void M(double d) { var i = [|(int)d|]; }
            }
            """
        );

    [Fact]
    public Task NoRefactoringForEnumToUnderlyingCast()
        => VerifyNoRefactoring(
            """
            class C
            {
                enum E { A, B }
                void M(E e) { var i = [|(int)e|]; }
            }
            """
        );

    [Fact]
    public Task NoRefactoringForUnboxingCast()
        => VerifyNoRefactoring(
            """
            class C
            {
                void M(object o) { var i = [|(int)o|]; }
            }
            """
        );

    [Fact]
    public Task NoRefactoringForNullableUnwrapCast()
        => VerifyNoRefactoring(
            """
            class C
            {
                void M(int? n) { var i = [|(int)n|]; }
            }
            """
        );

    [Fact]
    public Task NoRefactoringForNonLValueOperandInLambdaExpressionBody()
        => VerifyNoRefactoring(
            """
            using System;
            struct S1
            {
                public static explicit operator S2(S1 s) => default;
            }
            struct S2 { }
            class C
            {
                static S1 GetS1() => default;
                void M()
                {
                    Func<S2> f = () => [|(S2)GetS1()|];
                }
            }
            """
        );

    [Fact]
    public Task UserDefinedStructConversionFromLocalTakesRefDirectly()
        => VerifyRefactoring(
            """
            using System.Runtime.CompilerServices;
            struct S1
            {
                public static explicit operator S2(S1 s) => default;
            }
            struct S2 { }
            class C
            {
                void M()
                {
                    var s1 = new S1();
                    var s2 = [|(S2)s1|];
                }
            }
            """,
            """
            using System.Runtime.CompilerServices;
            struct S1
            {
                public static explicit operator S2(S1 s) => default;
            }
            struct S2 { }
            class C
            {
                void M()
                {
                    var s1 = new S1();
                    var s2 = Unsafe.As<S1, S2>(ref s1);
                }
            }
            """
        );

    [Fact]
    public Task UserDefinedStructConversionFromParameterTakesRefDirectly()
        => VerifyRefactoring(
            """
            using System.Runtime.CompilerServices;
            struct S1
            {
                public static explicit operator S2(S1 s) => default;
            }
            struct S2 { }
            class C
            {
                void M(S1 s1) { var s2 = [|(S2)s1|]; }
            }
            """,
            """
            using System.Runtime.CompilerServices;
            struct S1
            {
                public static explicit operator S2(S1 s) => default;
            }
            struct S2 { }
            class C
            {
                void M(S1 s1) { var s2 = Unsafe.As<S1, S2>(ref s1); }
            }
            """
        );

    [Fact]
    public Task UserDefinedStructConversionFromFieldTakesRefDirectly()
        => VerifyRefactoring(
            """
            using System.Runtime.CompilerServices;
            struct S1
            {
                public static explicit operator S2(S1 s) => default;
            }
            struct S2 { }
            class C
            {
                S1 field;
                void M() { var s2 = [|(S2)field|]; }
            }
            """,
            """
            using System.Runtime.CompilerServices;
            struct S1
            {
                public static explicit operator S2(S1 s) => default;
            }
            struct S2 { }
            class C
            {
                S1 field;
                void M() { var s2 = Unsafe.As<S1, S2>(ref field); }
            }
            """
        );

    [Fact]
    public Task UserDefinedStructConversionFromArrayElementTakesRefDirectly()
        => VerifyRefactoring(
            """
            using System.Runtime.CompilerServices;
            struct S1
            {
                public static explicit operator S2(S1 s) => default;
            }
            struct S2 { }
            class C
            {
                void M(S1[] arr) { var s2 = [|(S2)arr[0]|]; }
            }
            """,
            """
            using System.Runtime.CompilerServices;
            struct S1
            {
                public static explicit operator S2(S1 s) => default;
            }
            struct S2 { }
            class C
            {
                void M(S1[] arr) { var s2 = Unsafe.As<S1, S2>(ref arr[0]); }
            }
            """
        );

    [Fact]
    public Task UserDefinedStructConversionFromReadonlyFieldHoistsTemporary()
        => VerifyRefactoring(
            """
            using System.Runtime.CompilerServices;
            struct S1
            {
                public static explicit operator S2(S1 s) => default;
            }
            struct S2 { }
            class C
            {
                readonly S1 field;
                void M()
                {
                    var s2 = [|(S2)field|];
                }
            }
            """,
            """
            using System.Runtime.CompilerServices;
            struct S1
            {
                public static explicit operator S2(S1 s) => default;
            }
            struct S2 { }
            class C
            {
                readonly S1 field;
                void M()
                {
                    var asTarget = field;
                    var s2 = Unsafe.As<S1, S2>(ref asTarget);
                }
            }
            """
        );

    [Fact]
    public Task UserDefinedStructConversionFromMethodCallHoistsTemporary()
        => VerifyRefactoring(
            """
            using System.Runtime.CompilerServices;
            struct S1
            {
                public static explicit operator S2(S1 s) => default;
            }
            struct S2 { }
            class C
            {
                static S1 GetS1() => default;
                void M()
                {
                    var s2 = [|(S2)GetS1()|];
                }
            }
            """,
            """
            using System.Runtime.CompilerServices;
            struct S1
            {
                public static explicit operator S2(S1 s) => default;
            }
            struct S2 { }
            class C
            {
                static S1 GetS1() => default;
                void M()
                {
                    var asTarget = GetS1();
                    var s2 = Unsafe.As<S1, S2>(ref asTarget);
                }
            }
            """
        );

    [Fact]
    public Task ExpressionBodiedMethodRewritesToBlockToHostTemporary()
        => VerifyRefactoring(
            """
            using System.Runtime.CompilerServices;
            struct S1
            {
                public static explicit operator S2(S1 s) => default;
            }
            struct S2 { }
            class C
            {
                static S1 GetS1() => default;
                S2 M() => [|(S2)GetS1()|];
            }
            """,
            """
            using System.Runtime.CompilerServices;
            struct S1
            {
                public static explicit operator S2(S1 s) => default;
            }
            struct S2 { }
            class C
            {
                static S1 GetS1() => default;
                S2 M()
                {
                    var asTarget = GetS1();
                    return Unsafe.As<S1, S2>(ref asTarget);
                }
            }
            """
        );

    [Fact]
    public Task ExpressionBodiedPropertyRewritesToBlockGetterToHostTemporary()
        => VerifyRefactoring(
            """
            using System.Runtime.CompilerServices;
            struct S1
            {
                public static explicit operator S2(S1 s) => default;
            }
            struct S2 { }
            class C
            {
                static S1 GetS1() => default;
                S2 P => [|(S2)GetS1()|];
            }
            """,
            """
            using System.Runtime.CompilerServices;
            struct S1
            {
                public static explicit operator S2(S1 s) => default;
            }
            struct S2 { }
            class C
            {
                static S1 GetS1() => default;
                S2 P
                {
                    get
                    {
                        var asTarget = GetS1();
                        return Unsafe.As<S1, S2>(ref asTarget);
                    }
                }
            }
            """
        );

    [Fact]
    public Task ExpressionBodiedSetAccessorRewritesToBlockStatementToHostTemporary()
        => VerifyRefactoring(
            """
            using System.Runtime.CompilerServices;
            struct S1
            {
                public static explicit operator S2(S1 s) => default;
            }
            struct S2 { }
            class C
            {
                static S1 GetS1() => default;
                S2 field;
                S2 P { set => field = [|(S2)GetS1()|]; }
            }
            """,
            """
            using System.Runtime.CompilerServices;
            struct S1
            {
                public static explicit operator S2(S1 s) => default;
            }
            struct S2 { }
            class C
            {
                static S1 GetS1() => default;
                S2 field;
                S2 P {
                    set
                    {
                        var asTarget = GetS1();
                        field = Unsafe.As<S1, S2>(ref asTarget);
                    }
                }
            }
            """
        );

    [Fact]
    public Task ExpressionBodiedLocalFunctionRewritesToBlockToHostTemporary()
        => VerifyRefactoring(
            """
            using System.Runtime.CompilerServices;
            struct S1
            {
                public static explicit operator S2(S1 s) => default;
            }
            struct S2 { }
            class C
            {
                static S1 GetS1() => default;
                void M()
                {
                    S2 Local() => [|(S2)GetS1()|];
                    Local();
                }
            }
            """,
            """
            using System.Runtime.CompilerServices;
            struct S1
            {
                public static explicit operator S2(S1 s) => default;
            }
            struct S2 { }
            class C
            {
                static S1 GetS1() => default;
                void M()
                {
                    S2 Local() { var asTarget = GetS1(); return Unsafe.As<S1, S2>(ref asTarget); }

                    Local();
                }
            }
            """
        );

    [Fact]
    public Task ExpressionBodiedAsyncVoidMethodRewritesToStatementBlock()
        => VerifyRefactoring(
            """
            using System.Runtime.CompilerServices;
            struct S1
            {
                public static explicit operator S2(S1 s) => default;
            }
            struct S2 { }
            class C
            {
                static S1 GetS1() => default;
                static void Use(S2 s) { }
                async void M() => Use([|(S2)GetS1()|]);
            }
            """,
            """
            using System.Runtime.CompilerServices;
            struct S1
            {
                public static explicit operator S2(S1 s) => default;
            }
            struct S2 { }
            class C
            {
                static S1 GetS1() => default;
                static void Use(S2 s) { }
                async void M()
                {
                    var asTarget = GetS1();
                    Use(Unsafe.As<S1, S2>(ref asTarget));
                }
            }
            """
        );

    [Fact]
    public Task UserDefinedConversionBetweenUnrelatedReferenceTypesUsesSingleTypeOverload()
        => VerifyRefactoring(
            """
            using System.Runtime.CompilerServices;
            class A
            {
                public static explicit operator A(B b) => new A();
            }
            class B { }
            class C
            {
                void M(B b) { var a = [|(A)b|]; }
            }
            """,
            """
            using System.Runtime.CompilerServices;
            class A
            {
                public static explicit operator A(B b) => new A();
            }
            class B { }
            class C
            {
                void M(B b) { var a = Unsafe.As<A>(b); }
            }
            """
        );
}
