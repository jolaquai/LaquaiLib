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
}
