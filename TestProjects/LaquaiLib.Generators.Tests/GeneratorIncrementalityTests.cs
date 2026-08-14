namespace LaquaiLib.Generators.Tests;

public static class GeneratorIncrementalityTests
{
    private const string UnrelatedSource = "class TotallyUnrelated { void M() { } }";

    private const string ProxySource = """
        [FullAccessProxy(typeof(global::System.IO.MemoryStream))]
        public partial class MsProxy { }
        """;

    private const string ProxySourceEdited = """
        [FullAccessProxy(typeof(global::System.Text.StringBuilder))]
        public partial class MsProxy { }
        """;

    private static SyntaxTree Parse(string source, string path)
        => CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview), path: path);

    private static CSharpCompilation WithUnrelatedFile(CSharpCompilation compilation)
        => compilation.AddSyntaxTrees(Parse(UnrelatedSource, "Unrelated.cs"));

    public class FullAccessProxy
    {
        // THE MONEY TEST: fails today because FullAccessProxyGenerator combines with context.CompilationProvider,
        // whose identity changes on every edit, keeping SourceOutput permanently Modified instead of Cached.
        [Fact]
        public void UnrelatedEditKeepsSourceOutputCached()
        {
            var (driver, compilation, _) = GeneratorTestHost.RunTracked(new FullAccessProxyGenerator(), ProxySource);
            var result = GeneratorTestHost.RunAgain(driver, WithUnrelatedFile(compilation));
            GeneratorTestHost.AssertStepsCached(result, GeneratorTestHost.SourceOutputStepName);
        }

        // Regression guard: re-running with the exact same Compilation instance must stay cached.
        [Fact]
        public void NoOpRerunKeepsSourceOutputCached()
        {
            var (driver, compilation, _) = GeneratorTestHost.RunTracked(new FullAccessProxyGenerator(), ProxySource);
            var result = GeneratorTestHost.RunAgain(driver, compilation);
            GeneratorTestHost.AssertStepsCached(result, GeneratorTestHost.SourceOutputStepName);
        }

        // Catches reference-based equality in the model: identical text, freshly parsed trees.
        [Fact]
        public void EquivalentButFreshlyParsedTreesKeepSourceOutputCached()
        {
            var (driver, _, _) = GeneratorTestHost.RunTracked(new FullAccessProxyGenerator(), ProxySource);
            var freshCompilation = GeneratorTestHost.CreateCompilation(ProxySource);
            var result = GeneratorTestHost.RunAgain(driver, freshCompilation);
            GeneratorTestHost.AssertStepsCached(result, GeneratorTestHost.SourceOutputStepName);
        }

        // Guards against a generator that caches everything and never regenerates: a real edit must rerun.
        [Fact]
        public void RelevantEditRerunsSourceOutput()
        {
            var (driver, _, _) = GeneratorTestHost.RunTracked(new FullAccessProxyGenerator(), ProxySource);
            var editedCompilation = GeneratorTestHost.CreateCompilation(ProxySourceEdited);
            var result = GeneratorTestHost.RunAgain(driver, editedCompilation);
            GeneratorTestHost.AssertStepsRan(result, GeneratorTestHost.SourceOutputStepName);
        }

        [Fact]
        public void ModelHoldsNoRoslynObjects()
        {
            var (_, _, result) = GeneratorTestHost.RunTracked(new FullAccessProxyGenerator(), ProxySource);
            ModelPurityAssertions.AssertNoRoslynObjectsInModel(result);
        }

        [Fact]
        public void InitializeHasNoCapturingLambdas()
            => CapturingLambdaAssertions.AssertNoCapturingLambdas<FullAccessProxyGenerator>();
    }
}
