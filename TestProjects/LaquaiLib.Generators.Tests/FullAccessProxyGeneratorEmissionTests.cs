namespace LaquaiLib.Generators.Tests;

public class FullAccessProxyGeneratorEmissionTests
{
    [Fact]
    public void TypeofAndStringFormProduceEquivalentOutput()
    {
        var result = GeneratorTestHost.RunGenerator(
            """
            [FullAccessProxy(typeof(System.Text.StringBuilder))]
            public partial class ProxyByType;
            """,
            """
            [FullAccessProxy("System.Text.StringBuilder")]
            public partial class ProxyByString;
            """
        );

        GeneratorTestHost.AssertNoCompilationErrors(result);

        var byType = GeneratorTestHost.GetGeneratedSource(result, "ProxyByType").Replace("ProxyByType", "X");
        var byString = GeneratorTestHost.GetGeneratedSource(result, "ProxyByString").Replace("ProxyByString", "X");

        Assert.Equal(byType, byString);
    }

    [Theory]
    [MemberData(nameof(CompilableTargets))]
    public void GeneratedCodeCompiles(string source)
    {
        var result = GeneratorTestHost.RunGenerator(source);
        GeneratorTestHost.AssertNoCompilationErrors(result);
        Assert.NotEmpty(result.GeneratedSources);
    }

    public static IEnumerable<object[]> CompilableTargets()
    {
        yield return
        [
            """
            public class PocoTarget
            {
                public int Number;
                public string Name { get; set; }
                public int Add(int a, int b) => a + b;
            }

            [FullAccessProxy(typeof(PocoTarget))]
            public partial class PocoProxy;
            """
        ];
        yield return
        [
            """
            public class PrivateMembersTarget
            {
                private int _secret = 42;
                private string GetSecret() => "secret";
                public int Public() => 1;
            }

            [FullAccessProxy(typeof(PrivateMembersTarget))]
            public partial class PrivateMembersProxy;
            """
        ];
        yield return
        [
            """
            public class BaseTarget
            {
                public virtual int Method() => 1;
            }
            public class DerivedTarget : BaseTarget
            {
                public int OwnMethod() => 2;
            }

            [FullAccessProxy(typeof(DerivedTarget))]
            public partial class DerivedProxy;
            """
        ];
        yield return
        [
            """
            public interface ITargetIface
            {
                int Value { get; }
                void Do();
            }
            public class IfaceTarget : ITargetIface
            {
                public int Value => 1;
                public void Do() { }
            }

            [FullAccessProxy(typeof(IfaceTarget))]
            public partial class IfaceProxy;
            """
        ];
        yield return
        [
            """
            [FullAccessProxy(typeof(System.IO.MemoryStream))]
            public partial class MemoryStreamProxy;
            """
        ];
    }

    [Fact]
    public void TwoLevelNestingIsReproduced()
    {
        var result = GeneratorTestHost.RunGenerator(
            """
            public static partial class OuterA
            {
                public static partial class OuterB
                {
                    [FullAccessProxy(typeof(System.IO.MemoryStream))]
                    public partial class Proxy;
                }
            }
            """
        );

        GeneratorTestHost.AssertNoCompilationErrors(result);
        var source = GeneratorTestHost.GetGeneratedSource(result, "Proxy");
        Assert.Contains("partial class OuterA", source);
        Assert.Contains("partial class OuterB", source);
        Assert.Contains("partial class Proxy", source);
    }

    [Fact]
    public void InternalContainerAccessibilityIsPreserved()
    {
        var result = GeneratorTestHost.RunGenerator(
            """
            internal static partial class InternalOuter
            {
                [FullAccessProxy(typeof(System.IO.MemoryStream))]
                public partial class Proxy;
            }
            """
        );

        GeneratorTestHost.AssertNoCompilationErrors(result);
        var source = GeneratorTestHost.GetGeneratedSource(result, "Proxy");
        Assert.Contains("internal static partial class InternalOuter", source);
    }

    [Fact]
    public void GenericContainerReproducesTypeParametersButNotConstraints()
    {
        var result = GeneratorTestHost.RunGenerator(
            """
            public static partial class GenericOuter<T> where T : class
            {
                [FullAccessProxy(typeof(System.IO.MemoryStream))]
                public partial class Proxy;
            }
            """
        );

        GeneratorTestHost.AssertNoCompilationErrors(result);
        var source = GeneratorTestHost.GetGeneratedSource(result, "Proxy");
        Assert.Contains("GenericOuter<T>", source);
        Assert.DoesNotContain("where T", source);
    }

    [Fact]
    public void HintNamesAreUniqueAcrossNamespaces()
    {
        var result = GeneratorTestHost.RunGenerator(
            """
            namespace NsA
            {
                [FullAccessProxy(typeof(System.IO.MemoryStream))]
                public partial class Proxy;
            }
            """,
            """
            namespace NsB
            {
                [FullAccessProxy(typeof(System.IO.MemoryStream))]
                public partial class Proxy;
            }
            """
        );

        GeneratorTestHost.AssertNoCompilationErrors(result);
        Assert.Equal(2, result.GeneratedSources.Length);

        var hintNames = result.GeneratedSources.Select(static gs => gs.HintName).ToArray();
        Assert.Equal(hintNames.Length, hintNames.Distinct().Count());
    }

    [Fact]
    public void HintNamesAreUniqueAcrossNestingLevels()
    {
        var result = GeneratorTestHost.RunGenerator(
            """
            [FullAccessProxy(typeof(System.IO.MemoryStream))]
            public partial class Proxy;

            public static partial class Outer
            {
                [FullAccessProxy(typeof(System.IO.MemoryStream))]
                public partial class Proxy;
            }
            """
        );

        GeneratorTestHost.AssertNoCompilationErrors(result);
        Assert.Equal(2, result.GeneratedSources.Length);

        var hintNames = result.GeneratedSources.Select(static gs => gs.HintName).ToArray();
        Assert.Equal(hintNames.Length, hintNames.Distinct().Count());
    }

    [Fact]
    public void NoEmptyRegionsAreEmitted()
    {
        var result = GeneratorTestHost.RunGenerator(
            """
            [FullAccessProxy(typeof(System.IO.MemoryStream))]
            public partial class Proxy;
            """
        );

        GeneratorTestHost.AssertNoCompilationErrors(result);
        var source = GeneratorTestHost.GetGeneratedSource(result, "Proxy");

        var emptyRegion = new Regex(@"#region ([^\r\n]+)\r?\n\s*#endregion \1", RegexOptions.Multiline);
        var match = emptyRegion.Match(source);
        Assert.False(match.Success, $"Found an empty #region/#endregion pair: {(match.Success ? match.Value : "")}{Environment.NewLine}{source}");
    }

    [Fact]
    public void AllSystemReferencesAreGlobalQualified()
    {
        var result = GeneratorTestHost.RunGenerator(
            """
            [FullAccessProxy(typeof(System.IO.MemoryStream))]
            public partial class Proxy;
            """
        );

        GeneratorTestHost.AssertNoCompilationErrors(result);
        var source = GeneratorTestHost.GetGeneratedSource(result, "Proxy");
        GeneratorTestHost.AssertAllSystemReferencesGlobalQualified(source);
    }

    [Fact]
    public void PropertyAccessorMethodsAreNotForwardedAsOrdinaryMethods()
    {
        var result = GeneratorTestHost.RunGenerator(
            """
            [FullAccessProxy(typeof(System.IO.MemoryStream))]
            public partial class Proxy;
            """
        );

        GeneratorTestHost.AssertNoCompilationErrors(result);
        var source = GeneratorTestHost.GetGeneratedSource(result, "Proxy");

        // Strip the nested Accessors class body (where get_X/set_X ARE expected as UnsafeAccessor stubs) before checking.
        var accessorsStart = source.IndexOf("private static class Accessors", StringComparison.Ordinal);
        var afterAccessors = source.IndexOf("#endregion Unsafe accessors utility", accessorsStart, StringComparison.Ordinal);
        var withoutAccessors = source.Remove(accessorsStart, afterAccessors - accessorsStart);

        var accessorMethodForwarder = new Regex(@"public\s+(?:static\s+)?\S.*\b(get|set|add|remove)_\w+\s*\(");
        var match = accessorMethodForwarder.Match(withoutAccessors);
        Assert.False(match.Success, $"Found a property/event accessor method forwarded as an ordinary method: {(match.Success ? match.Value : "")}");
    }

    [Fact]
    public void ObjectMembersAreNotProxied()
    {
        var result = GeneratorTestHost.RunGenerator(
            """
            public class PlainTarget
            {
                public int Value;
            }

            [FullAccessProxy(typeof(PlainTarget))]
            public partial class PlainProxy;
            """
        );

        GeneratorTestHost.AssertNoCompilationErrors(result);
        var source = GeneratorTestHost.GetGeneratedSource(result, "PlainProxy");

        Assert.DoesNotContain("ToString()", source);
        Assert.DoesNotContain("Equals(", source);
        Assert.DoesNotContain("GetHashCode()", source);
        Assert.DoesNotContain("Finalize", source);
    }

    [Fact]
    public void InheritedMembersAreProxiedWithDeclaringTypeAsAccessorTarget()
    {
        var result = GeneratorTestHost.RunGenerator(
            """
            public class BaseTarget
            {
                public virtual int Method() => 1;
            }
            public class DerivedTarget : BaseTarget
            {
                public int OwnMethod() => 2;
            }

            [FullAccessProxy(typeof(DerivedTarget))]
            public partial class DerivedProxy;
            """
        );

        GeneratorTestHost.AssertNoCompilationErrors(result);
        var source = GeneratorTestHost.GetGeneratedSource(result, "DerivedProxy");

        // The inherited "Method" is declared on BaseTarget - [UnsafeAccessor] doesn't walk the hierarchy,
        // so its stub's target parameter must be BaseTarget, not DerivedTarget.
        Assert.Contains("extern int Method(global::BaseTarget target)", source);
        Assert.Contains("extern int OwnMethod(global::DerivedTarget target)", source);
    }

    [Fact]
    public void StaticAndInstanceMembersGetCorrectUnsafeAccessorKind()
    {
        var result = GeneratorTestHost.RunGenerator(
            """
            public class StaticMembersTarget
            {
                public static int StaticField;
                public static int StaticMethod() => 1;
                public int InstanceMethod() => 2;
            }

            [FullAccessProxy(typeof(StaticMembersTarget))]
            public partial class StaticMembersProxy;
            """
        );

        GeneratorTestHost.AssertNoCompilationErrors(result);
        var source = GeneratorTestHost.GetGeneratedSource(result, "StaticMembersProxy");

        Assert.Contains("UnsafeAccessorKind.StaticField)]" + Environment.NewLine, source.Replace("\r\n", "\n").Replace("\n", Environment.NewLine));
        Assert.Matches(new Regex(@"UnsafeAccessorKind\.StaticMethod\)\]\s*\r?\n\s*public static extern int StaticMethod"), source);
        Assert.Matches(new Regex(@"UnsafeAccessorKind\.Method\)\]\s*\r?\n\s*public static extern int InstanceMethod"), source);
    }

    [Fact]
    public void InitOnlyPropertyIsEmittedGetOnly()
    {
        var result = GeneratorTestHost.RunGenerator(
            """
            public class InitOnlyTarget
            {
                public int Value { get; init; }
            }

            [FullAccessProxy(typeof(InitOnlyTarget))]
            public partial class InitOnlyProxy;
            """
        );

        GeneratorTestHost.AssertNoCompilationErrors(result);
        var source = GeneratorTestHost.GetGeneratedSource(result, "InitOnlyProxy");

        Assert.Contains("public int Value", source);
        Assert.Matches(new Regex(@"public int Value\s*\r?\n\s*\{\s*\r?\n(?:.*\r?\n)*?\s*get =>[^\r\n]*\r?\n\s*\}"), source);
        Assert.DoesNotContain("set_Value", source);
    }

    [Fact]
    public void ForwardableInterfaceAppearsInBaseListWithExplicitImplementation()
    {
        var result = GeneratorTestHost.RunGenerator(
            """
            public interface IForwardable
            {
                int Value { get; }
                void Do();
            }
            public class ForwardableTarget : IForwardable
            {
                public int Value => 1;
                public void Do() { }
            }

            [FullAccessProxy(typeof(ForwardableTarget))]
            public partial class ForwardableProxy;
            """
        );

        GeneratorTestHost.AssertNoCompilationErrors(result);
        var source = GeneratorTestHost.GetGeneratedSource(result, "ForwardableProxy");

        Assert.Contains(": global::IForwardable", source);
        Assert.Contains("global::IForwardable.Value", source);
        Assert.Contains("global::IForwardable.Do()", source);
    }

    [Fact]
    public void NonForwardableInterfaceIsExcludedFromBaseList()
    {
        // An init-only setter can't be forwarded post-construction, so the interface can't be fully forwarded
        // and must be excluded from the proxy's base list. Manual backing field to sidestep the auto-property
        // backing-field emission defect (see report) which is unrelated to what this test is checking.
        var result = GeneratorTestHost.RunGenerator(
            """
            public interface IWithInitOnly
            {
                int Value { get; init; }
            }
            public class WithInitOnlyTarget : IWithInitOnly
            {
                private readonly int _value;
                public int Value { get => _value; init => _value = value; }
            }

            [FullAccessProxy(typeof(WithInitOnlyTarget))]
            public partial class NonForwardableInterfaceProxy;
            """
        );

        GeneratorTestHost.AssertNoCompilationErrors(result);
        var source = GeneratorTestHost.GetGeneratedSource(result, "NonForwardableInterfaceProxy");

        Assert.DoesNotContain("IWithInitOnly", source);
    }

    [Fact]
    public void GeneratedFileCarriesNullableDirective()
    {
        var result = GeneratorTestHost.RunGenerator(
            """
            [FullAccessProxy(typeof(System.IO.MemoryStream))]
            public partial class Proxy;
            """
        );

        GeneratorTestHost.AssertNoCompilationErrors(result);
        var source = GeneratorTestHost.GetGeneratedSource(result, "Proxy");
        Assert.Contains("#nullable enable annotations", source);
    }

    [Fact]
    public void GenericMethodWithTypeParameterReturnTypeIsProxied()
    {
        // The generator's Accessibility guard treats a type parameter's DeclaredAccessibility as non-Public,
        // so a method returning its own generic parameter is silently skipped instead of proxied.
        var result = GeneratorTestHost.RunGenerator(
            """
            public class IdentityTarget
            {
                public T Identity<T>(T value) => value;
            }

            [FullAccessProxy(typeof(IdentityTarget))]
            public partial class IdentityProxy;
            """
        );

        GeneratorTestHost.AssertNoCompilationErrors(result);
        var source = GeneratorTestHost.GetGeneratedSource(result, "IdentityProxy");
        // Identity is itself generic (its own T, not the class's), so the forwarder legally must re-declare
        // <T> (else T is unresolvable, CS0246), hence the arity-qualified needle here.
        Assert.Contains("Identity<T>(", source);
    }

    [Fact]
    public void AutoImplementedPropertyTargetCompiles()
    {
        // GetProxyableMembers doesn't filter out compiler-generated members, so an auto-property's synthesized
        // "<Name>k__BackingField" gets emitted as a proxyable field verbatim, producing invalid syntax
        // (the angle brackets aren't valid in an identifier position).
        var result = GeneratorTestHost.RunGenerator(
            """
            public class AutoPropertyTarget
            {
                public string Name { get; set; }
            }

            [FullAccessProxy(typeof(AutoPropertyTarget))]
            public partial class AutoPropertyProxy;
            """
        );

        GeneratorTestHost.AssertNoCompilationErrors(result);
    }
}
