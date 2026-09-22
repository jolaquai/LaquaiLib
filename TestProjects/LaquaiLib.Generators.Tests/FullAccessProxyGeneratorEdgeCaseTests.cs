namespace LaquaiLib.Generators.Tests;

// These cover generator robustness on unusual inputs. Per instructions: if the generator crashes or emits
// uncompilable code here, that's a FINDING to report, not something to quietly work around in the test.
public class FullAccessProxyGeneratorEdgeCaseTests
{
    [Fact]
    public void GenericMethodTargetCompiles()
    {
        // Return type is deliberately NOT the generic parameter (string, not T) - see FullAccessProxyGeneratorEmissionTests
        // for a case where the return type IS the type parameter, which the generator handles very differently (it
        // silently skips the member instead of failing to compile).
        var result = GeneratorTestHost.RunGenerator(
            """
            public class GenericMethodTarget
            {
                public string Describe<T>(T value) => value.ToString();
            }

            [FullAccessProxy(typeof(GenericMethodTarget))]
            public partial class GenericMethodProxy;
            """
        );

        GeneratorTestHost.AssertNoCompilationErrors(result);
    }

    [Fact]
    public void RefReturningMembersCompile()
    {
        var result = GeneratorTestHost.RunGenerator(
            """
            public class RefReturningTarget
            {
                private int _value;
                public ref int GetRef() => ref _value;
                public ref readonly int GetRefReadonly() => ref _value;
            }

            [FullAccessProxy(typeof(RefReturningTarget))]
            public partial class RefReturningProxy;
            """
        );

        GeneratorTestHost.AssertNoCompilationErrors(result);
    }

    [Fact]
    public void RefOutInParametersCompile()
    {
        var result = GeneratorTestHost.RunGenerator(
            """
            public class RefParamsTarget
            {
                public void ByRef(ref int x) => x++;
                public void ByOut(out int x) => x = 5;
                public void ByIn(in int x) { }
            }

            [FullAccessProxy(typeof(RefParamsTarget))]
            public partial class RefParamsProxy;
            """
        );

        GeneratorTestHost.AssertNoCompilationErrors(result);
    }

    [Fact]
    public void SealedProxiedTypeCompiles()
    {
        var result = GeneratorTestHost.RunGenerator(
            """
            public sealed class SealedTarget
            {
                public int Method() => 1;
            }

            [FullAccessProxy(typeof(SealedTarget))]
            public partial class SealedTargetProxy;
            """
        );

        GeneratorTestHost.AssertNoCompilationErrors(result);
    }

    [Fact]
    public void AbstractProxiedTypeCompiles()
    {
        var result = GeneratorTestHost.RunGenerator(
            """
            public abstract class AbstractTarget
            {
                public int ConcreteMethod() => 1;
                public abstract int AbstractMethod();
            }

            [FullAccessProxy(typeof(AbstractTarget))]
            public partial class AbstractTargetProxy;
            """
        );

        GeneratorTestHost.AssertNoCompilationErrors(result);
    }

    [Fact]
    public void OverloadedMethodsProduceNoDuplicateMembers()
    {
        var result = GeneratorTestHost.RunGenerator(
            """
            public class OverloadedTarget
            {
                public int Method(int a) => a;
                public int Method(int a, int b) => a + b;
            }

            [FullAccessProxy(typeof(OverloadedTarget))]
            public partial class OverloadedProxy;
            """
        );

        GeneratorTestHost.AssertNoCompilationErrors(result);
        var source = GeneratorTestHost.GetGeneratedSource(result, "OverloadedProxy");

        Assert.Contains("Method(int a)", source);
        Assert.Contains("Method(int a, int b)", source);
    }

    [Fact]
    public void ProxyDeclaredInGlobalNamespaceCompiles()
    {
        var result = GeneratorTestHost.RunGenerator(
            """
            [FullAccessProxy(typeof(System.IO.MemoryStream))]
            public partial class GlobalProxy;
            """
        );

        GeneratorTestHost.AssertNoCompilationErrors(result);
        var source = GeneratorTestHost.GetGeneratedSource(result, "GlobalProxy");
        Assert.DoesNotContain("namespace ", source);
    }
}
