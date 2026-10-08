namespace LaquaiLib.Generators.Tests;

public class FullAccessProxyGeneratorDiagnosticsTests
{
    [Fact]
    public void UnresolvableStringTypeReportsFap001()
    {
        var result = GeneratorTestHost.RunGenerator(
            """
            [FullAccessProxy("Nonexistent.Namespace.TotallyMadeUpType")]
            public partial class BadProxy;
            """
        );

        var diagnostic = Assert.Single(result.GeneratorDiagnostics);
        Assert.Equal("FAP001", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("Nonexistent.Namespace.TotallyMadeUpType", diagnostic.GetMessage());
    }

    [Fact]
    public void Fap001IsReportedAtTheAttributeLocation()
    {
        var source =
            """
            [FullAccessProxy("Nonexistent.Namespace.TotallyMadeUpType")]
            public partial class BadProxy;
            """;

        var result = GeneratorTestHost.RunGenerator(source);
        var diagnostic = Assert.Single(result.GeneratorDiagnostics);

        Assert.NotEqual(Location.None, diagnostic.Location);
        var span = diagnostic.Location.GetLineSpan();
        // The attribute application is on the first line of the source.
        Assert.Equal(0, span.StartLinePosition.Line);
    }

    [Fact]
    public void NoProxySourceIsEmittedWhenTypeCannotBeResolved()
    {
        var result = GeneratorTestHost.RunGenerator(
            """
            [FullAccessProxy("Nonexistent.Namespace.TotallyMadeUpType")]
            public partial class BadProxy;
            """
        );

        Assert.Empty(result.GeneratedSources);
    }

    [Fact]
    public void PartialClassWithoutAttributeProducesNoOutput()
    {
        var result = GeneratorTestHost.RunGenerator(
            """
            public partial class NoAttributeTarget;
            """
        );

        Assert.Empty(result.GeneratedSources);
        Assert.Empty(result.GeneratorDiagnostics);
    }

    [Fact]
    public void PublicProxyOverInternalTypeReportsFap002()
    {
        var result = GeneratorTestHost.RunGenerator(
            """
            internal class InternalFap002Target
            {
                public int Method() => 1;
            }

            [FullAccessProxy(typeof(InternalFap002Target))]
            public partial class Fap002Proxy;
            """
        );

        var diagnostic = Assert.Single(result.GeneratorDiagnostics, static d => d.Id == "FAP002");
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        var message = diagnostic.GetMessage();
        Assert.Contains("Fap002Proxy", message);
        Assert.Contains("InternalFap002Target", message);
    }

    [Fact]
    public void InternalProxyOverInternalTypeReportsNoFap002()
    {
        var result = GeneratorTestHost.RunGenerator(
            """
            internal class InternalFap002TargetB
            {
                public int Method() => 1;
            }

            [FullAccessProxy(typeof(InternalFap002TargetB))]
            internal partial class Fap002ProxyB;
            """
        );

        Assert.DoesNotContain(result.GeneratorDiagnostics, static d => d.Id == "FAP002");
    }

    [Fact]
    public void InaccessibleValueTypeResultReportsFap003()
    {
        var result = GeneratorTestHost.RunGenerator(
            """
            public class Fap003ValueTypeHost
            {
                private struct HiddenValue;
                private HiddenValue GetHidden() => default;
            }

            [FullAccessProxy(typeof(Fap003ValueTypeHost), IncludeInaccessible = true)]
            public partial class Fap003ValueTypeProxy;
            """
        );

        // the private nested struct itself also surfaces as a (separately skipped) FAP003, so filter down to the method
        var diagnostic = Assert.Single(result.GeneratorDiagnostics, static d => d.Id == "FAP003" && d.GetMessage().Contains("GetHidden"));
        Assert.Equal(DiagnosticSeverity.Info, diagnostic.Severity);
    }

    [Fact]
    public void ErasedOverloadCollisionReportsFap003AndEmitsOneForwarder()
    {
        var result = GeneratorTestHost.RunGenerator(
            """
            public class Fap003OverloadHost
            {
                private class PrivateA;
                private class PrivateB;
                private void Which(PrivateA a) { }
                private void Which(PrivateB b) { }
            }

            [FullAccessProxy(typeof(Fap003OverloadHost), IncludeInaccessible = true)]
            public partial class Fap003OverloadProxy;
            """
        );

        // the two private nested classes themselves also surface as separately-skipped FAP003s, so filter down to the collision
        var diagnostic = Assert.Single(result.GeneratorDiagnostics, static d => d.Id == "FAP003" && d.GetMessage().Contains("Which"));
        Assert.Contains("erasing its signature makes it indistinguishable", diagnostic.GetMessage());

        var generated = GeneratorTestHost.GetGeneratedSource(result, "Fap003OverloadProxy");
        Assert.Single(Regex.Matches(generated, @"public\s+void Which\("));
    }

    [Fact]
    public void SameOverloadCollisionWithoutIncludeInaccessibleReportsNoFap003()
    {
        var result = GeneratorTestHost.RunGenerator(
            """
            public class Fap003GatedHost
            {
                private class PrivateGatedA;
                private class PrivateGatedB;
                private void Which(PrivateGatedA a) { }
                private void Which(PrivateGatedB b) { }
            }

            [FullAccessProxy(typeof(Fap003GatedHost))]
            public partial class Fap003GatedProxy;
            """
        );

        Assert.DoesNotContain(result.GeneratorDiagnostics, static d => d.Id == "FAP003");
    }

    [Fact]
    public void UnnameableStringResolvedTypeReportsFap004()
    {
        var result = GeneratorTestHost.RunGenerator(
            """
            public class Fap004Host
            {
                private class PrivateNestedFap004;
            }

            [FullAccessProxy("Fap004Host+PrivateNestedFap004")]
            public partial class Fap004Proxy;
            """
        );

        var diagnostic = Assert.Single(result.GeneratorDiagnostics, static d => d.Id == "FAP004");
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Empty(result.GeneratedSources);
    }

    [Fact]
    public void UnnameableStringResolvedTypeWithIncludeInaccessibleProducesNoFap004()
    {
        var result = GeneratorTestHost.RunGenerator(
            """
            public class Fap004HostOk
            {
                private class PrivateNestedFap004Ok;
            }

            [FullAccessProxy("Fap004HostOk+PrivateNestedFap004Ok", IncludeInaccessible = true)]
            public partial class Fap004ProxyOk;
            """
        );

        Assert.DoesNotContain(result.GeneratorDiagnostics, static d => d.Id == "FAP004");
        Assert.NotEmpty(result.GeneratedSources);
    }
}
