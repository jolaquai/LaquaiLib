namespace LaquaiLib.Generators.Tests;

using LaquaiLib.Generators.SourceGeneratedExtensions;

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

    private const string InlineArraySource = """
        [global::System.Runtime.CompilerServices.InlineArray(4)]
        public struct Buf4 { private int _e0; }
        """;

    private const string InlineArraySourceEdited = """
        [global::System.Runtime.CompilerServices.InlineArray(8)]
        public struct Buf4 { private int _e0; }
        """;

    private const string EnumSource = "public enum Color { Red, Green, Blue }";
    private const string EnumSourceEdited = "public enum Color { Red, Green, Blue, Yellow }";

    private static SyntaxTree Parse(string source, string path)
        => CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview), path: path);

    private static CSharpCompilation WithUnrelatedFile(CSharpCompilation compilation)
        => compilation.AddSyntaxTrees(Parse(UnrelatedSource, "Unrelated.cs"));

    public class FullAccessProxy
    {
        private static readonly string[] _steps = [
            GeneratorStepNames.FullAccessProxyModels,
            GeneratorStepNames.FullAccessProxyFiltered,
            GeneratorTestHost.SourceOutputStepName
        ];

        // THE MONEY TEST: an edit that touches nothing the generator cares about must not re-emit anything.
        [Fact]
        public void UnrelatedEditKeepsSourceOutputCached()
        {
            var (driver, compilation, _) = GeneratorTestHost.RunTracked(new FullAccessProxyGenerator(), ProxySource);
            var result = GeneratorTestHost.RunAgain(driver, WithUnrelatedFile(compilation));
            GeneratorTestHost.AssertStepsCached(result, _steps);
        }

        // Regression guard: re-running with the exact same Compilation instance must stay cached.
        [Fact]
        public void NoOpRerunKeepsSourceOutputCached()
        {
            var (driver, compilation, _) = GeneratorTestHost.RunTracked(new FullAccessProxyGenerator(), ProxySource);
            var result = GeneratorTestHost.RunAgain(driver, compilation);
            GeneratorTestHost.AssertStepsCached(result, _steps);
        }

        // Catches reference-based equality in the model: identical text, freshly parsed trees.
        [Fact]
        public void EquivalentButFreshlyParsedTreesKeepSourceOutputCached()
        {
            var (driver, _, _) = GeneratorTestHost.RunTracked(new FullAccessProxyGenerator(), ProxySource);
            var freshCompilation = GeneratorTestHost.CreateCompilation(ProxySource);
            var result = GeneratorTestHost.RunAgain(driver, freshCompilation);
            GeneratorTestHost.AssertStepsCached(result, _steps);
        }

        // Guards against a generator that caches everything and never regenerates: a real edit must rerun.
        [Fact]
        public void RelevantEditRerunsSourceOutput()
        {
            var (driver, _, _) = GeneratorTestHost.RunTracked(new FullAccessProxyGenerator(), ProxySource);
            var editedCompilation = GeneratorTestHost.CreateCompilation(ProxySourceEdited);
            var result = GeneratorTestHost.RunAgain(driver, editedCompilation);
            GeneratorTestHost.AssertStepsRan(result, _steps);
        }

        [Fact]
        public void ModelHoldsNoRoslynObjects()
        {
            var (_, _, result) = GeneratorTestHost.RunTracked(new FullAccessProxyGenerator(), ProxySource);
            ModelPurityAssertions.AssertNoRoslynObjectsInModel(result, _steps[..^1]);
        }

        [Fact]
        public void InitializeHasNoCapturingLambdas()
            => CapturingLambdaAssertions.AssertNoCapturingLambdas<FullAccessProxyGenerator>();
    }

    public class InlineArrayExtensions
    {
        private static readonly string[] _steps = [
            GeneratorStepNames.InlineArrayModels,
            GeneratorStepNames.InlineArrayFiltered,
            GeneratorStepNames.InlineArrayCollected,
            GeneratorTestHost.SourceOutputStepName
        ];

        // THE MONEY TEST: an edit that touches nothing the generator cares about must not re-emit anything.
        [Fact]
        public void UnrelatedEditKeepsSourceOutputCached()
        {
            var (driver, compilation, _) = GeneratorTestHost.RunTracked(new InlineArrayExtensionsGenerator(), InlineArraySource);
            var result = GeneratorTestHost.RunAgain(driver, WithUnrelatedFile(compilation));
            GeneratorTestHost.AssertStepsCached(result, _steps);
        }

        [Fact]
        public void NoOpRerunKeepsSourceOutputCached()
        {
            var (driver, compilation, _) = GeneratorTestHost.RunTracked(new InlineArrayExtensionsGenerator(), InlineArraySource);
            var result = GeneratorTestHost.RunAgain(driver, compilation);
            GeneratorTestHost.AssertStepsCached(result, _steps);
        }

        [Fact]
        public void EquivalentButFreshlyParsedTreesKeepSourceOutputCached()
        {
            var (driver, _, _) = GeneratorTestHost.RunTracked(new InlineArrayExtensionsGenerator(), InlineArraySource);
            var freshCompilation = GeneratorTestHost.CreateCompilation(InlineArraySource);
            var result = GeneratorTestHost.RunAgain(driver, freshCompilation);
            GeneratorTestHost.AssertStepsCached(result, _steps);
        }

        [Fact]
        public void RelevantEditRerunsSourceOutput()
        {
            var (driver, _, _) = GeneratorTestHost.RunTracked(new InlineArrayExtensionsGenerator(), InlineArraySource);
            var editedCompilation = GeneratorTestHost.CreateCompilation(InlineArraySourceEdited);
            var result = GeneratorTestHost.RunAgain(driver, editedCompilation);
            GeneratorTestHost.AssertStepsRan(result, _steps);
        }

        [Fact]
        public void ModelHoldsNoRoslynObjects()
        {
            var (_, _, result) = GeneratorTestHost.RunTracked(new InlineArrayExtensionsGenerator(), InlineArraySource);
            ModelPurityAssertions.AssertNoRoslynObjectsInModel(result, _steps[..^1]);
        }

        [Fact]
        public void InitializeHasNoCapturingLambdas()
            => CapturingLambdaAssertions.AssertNoCapturingLambdas<InlineArrayExtensionsGenerator>();
    }

    public class EnumExpander
    {
        private static readonly string[] _steps = [
            GeneratorStepNames.EnumExpanderModels,
            GeneratorStepNames.EnumExpanderFiltered,
            GeneratorStepNames.EnumExpanderCollected,
            GeneratorTestHost.SourceOutputStepName
        ];

        // THE MONEY TEST: an edit that touches nothing the generator cares about must not re-emit anything.
        [Fact]
        public void UnrelatedEditKeepsSourceOutputCached()
        {
            var (driver, compilation, _) = GeneratorTestHost.RunTracked(new EnumExpanderGenerator(), EnumSource);
            var result = GeneratorTestHost.RunAgain(driver, WithUnrelatedFile(compilation));
            GeneratorTestHost.AssertStepsCached(result, _steps);
        }

        [Fact]
        public void NoOpRerunKeepsSourceOutputCached()
        {
            var (driver, compilation, _) = GeneratorTestHost.RunTracked(new EnumExpanderGenerator(), EnumSource);
            var result = GeneratorTestHost.RunAgain(driver, compilation);
            GeneratorTestHost.AssertStepsCached(result, _steps);
        }

        [Fact]
        public void EquivalentButFreshlyParsedTreesKeepSourceOutputCached()
        {
            var (driver, _, _) = GeneratorTestHost.RunTracked(new EnumExpanderGenerator(), EnumSource);
            var freshCompilation = GeneratorTestHost.CreateCompilation(EnumSource);
            var result = GeneratorTestHost.RunAgain(driver, freshCompilation);
            GeneratorTestHost.AssertStepsCached(result, _steps);
        }

        [Fact]
        public void RelevantEditRerunsSourceOutput()
        {
            var (driver, _, _) = GeneratorTestHost.RunTracked(new EnumExpanderGenerator(), EnumSource);
            var editedCompilation = GeneratorTestHost.CreateCompilation(EnumSourceEdited);
            var result = GeneratorTestHost.RunAgain(driver, editedCompilation);
            GeneratorTestHost.AssertStepsRan(result, _steps);
        }

        [Fact]
        public void ModelHoldsNoRoslynObjects()
        {
            var (_, _, result) = GeneratorTestHost.RunTracked(new EnumExpanderGenerator(), EnumSource);
            ModelPurityAssertions.AssertNoRoslynObjectsInModel(result, _steps[..^1]);
        }

        [Fact]
        public void InitializeHasNoCapturingLambdas()
            => CapturingLambdaAssertions.AssertNoCapturingLambdas<EnumExpanderGenerator>();
    }
}
