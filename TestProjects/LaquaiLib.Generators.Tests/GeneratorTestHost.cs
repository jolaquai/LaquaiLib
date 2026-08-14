namespace LaquaiLib.Generators.Tests;

/// <summary>
/// Result of driving <see cref="FullAccessProxyGenerator"/> over a compilation. Keeps generator diagnostics
/// (did the generator complain) separate from final-compilation diagnostics (does the emitted code compile).
/// </summary>
internal readonly struct GeneratorTestResult
{
    public required (string HintName, string Text)[] GeneratedSources { get; init; }
    public required ImmutableArray<Diagnostic> GeneratorDiagnostics { get; init; }
    public required ImmutableArray<Diagnostic> FinalDiagnostics { get; init; }
}

internal static class GeneratorTestHost
{
    private static readonly Lazy<ImmutableArray<MetadataReference>> _references = new(BuildReferences);

    // Mirrors the SDK's default ImplicitUsings set so proxy code that relies on it (e.g. the ctor's bare
    // "ArgumentNullException") compiles the same way it would in any real consuming project. Also brings in
    // the attribute namespace so test sources don't need to spell it out or fully-qualify it every time.
    private const string ImplicitUsingsSource = """
        global using System;
        global using System.Collections.Generic;
        global using System.IO;
        global using System.Linq;
        global using System.Threading;
        global using System.Threading.Tasks;
        global using LaquaiLib.Analyzers.Shared.Attributes;
        """;

    private static ImmutableArray<MetadataReference> BuildReferences()
    {
        var tpa = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        var paths = tpa is not null ? tpa.Split(Path.PathSeparator) : [];

        var builder = ImmutableArray.CreateBuilder<MetadataReference>(paths.Length + 1);
        foreach (var path in paths)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                builder.Add(MetadataReference.CreateFromFile(path));
            }
        }
        builder.Add(MetadataReference.CreateFromFile(typeof(FullAccessProxyAttribute).Assembly.Location));
        return builder.ToImmutable();
    }

    public static CSharpCompilation CreateCompilation(params string[] sources)
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);

        var trees = new SyntaxTree[sources.Length + 1];
        for (var i = 0; i < sources.Length; i++)
        {
            trees[i] = CSharpSyntaxTree.ParseText(sources[i], parseOptions, path: $"Source{i}.cs");
        }
        trees[sources.Length] = CSharpSyntaxTree.ParseText(ImplicitUsingsSource, parseOptions, path: "ImplicitUsings.cs");

        return CSharpCompilation.Create(
            "GeneratorTestAssembly",
            trees,
            _references.Value,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
    }

    public static GeneratorTestResult RunGenerator(params string[] sources)
    {
        var compilation = CreateCompilation(sources);

        // The driver must parse its own generated trees with the same LanguageVersion as the input compilation,
        // otherwise adding them back in throws "Inconsistent language versions".
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        GeneratorDriver driver = CSharpGeneratorDriver.Create([new FullAccessProxyGenerator().AsSourceGenerator()], parseOptions: parseOptions);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var finalCompilation, out var generatorDiagnostics);

        var generatedSources = driver.GetRunResult().Results
            .SelectMany(static r => r.GeneratedSources)
            .Select(static gs => (gs.HintName, Text: gs.SourceText.ToString()))
            .ToArray();

        return new GeneratorTestResult
        {
            GeneratedSources = generatedSources,
            GeneratorDiagnostics = generatorDiagnostics,
            FinalDiagnostics = finalCompilation.GetDiagnostics(),
        };
    }

    public static void AssertNoCompilationErrors(GeneratorTestResult result)
    {
        var errors = result.FinalDiagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length == 0)
        {
            return;
        }

        var sb = new StringBuilder();
        sb.Append("Expected 0 compilation errors, found ").Append(errors.Length).AppendLine(":");
        foreach (var error in errors)
        {
            sb.Append("  ").AppendLine(error.ToString());
        }
        sb.AppendLine();
        sb.AppendLine("Generated sources:");
        foreach (var (hintName, text) in result.GeneratedSources)
        {
            sb.Append("--- ").Append(hintName).AppendLine(" ---");
            sb.AppendLine(text);
        }

        Assert.Fail(sb.ToString());
    }

    public static string GetGeneratedSource(GeneratorTestResult result, string hintNameSubstring)
    {
        var matches = result.GeneratedSources.Where(gs => gs.HintName.Contains(hintNameSubstring, StringComparison.Ordinal)).ToArray();
        if (matches.Length == 0)
        {
            Assert.Fail($"No generated source found with hint name containing '{hintNameSubstring}'. Available: {string.Join(", ", result.GeneratedSources.Select(static gs => gs.HintName))}");
        }
        if (matches.Length > 1)
        {
            Assert.Fail($"Multiple generated sources found with hint name containing '{hintNameSubstring}': {string.Join(", ", matches.Select(static m => m.HintName))}");
        }
        return matches[0].Text;
    }

    // "namespace X;" is exempt: "namespace global::X;" is not legal C#, so the declaration line can never be global::-qualified.
    private static readonly Regex _unqualifiedSystemReference = new(@"(?<!global::)\bSystem\.", RegexOptions.Compiled);

    public static void AssertAllSystemReferencesGlobalQualified(string generatedText)
    {
        var lines = generatedText.Split('\n');
        var offenders = new List<string>();
        foreach (var line in lines)
        {
            var trimmed = line.TrimStart();
            if (trimmed.StartsWith("namespace ", StringComparison.Ordinal))
            {
                continue;
            }
            if (_unqualifiedSystemReference.IsMatch(line))
            {
                offenders.Add(line.Trim());
            }
        }

        if (offenders.Count > 0)
        {
            Assert.Fail($"Found unqualified 'System.' references (missing 'global::'):{Environment.NewLine}{string.Join(Environment.NewLine, offenders)}{Environment.NewLine}Full source:{Environment.NewLine}{generatedText}");
        }
    }
}
