using LaquaiLib.Analyzers.Shared;

namespace LaquaiLib.Analyzers.Performance__0XXX_;

/// <summary>
/// Analyzes <see langword="class"/> declarations and warns when they can be sealed to improve performance.
/// <para/>In addition to default configuration options, this analyzer supports the following options:
/// <list type="bullet">
/// <item><term>protected_members</term> <description>When set to <c>ignore</c>, the analyzer will continue to report the warning on <see langword="class"/>es that contain <see langword="protected"/> members. By default, sealing such classes would generate <c>CS0628</c>.</description></item>
/// </list>
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SealClassAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Gets the default descriptor for this analyzer. May be reconfigured by analyzer options.
    /// </summary>
    public static DiagnosticDescriptor Descriptor { get; } = new(
        id: "LAQ0005",
        title: "Seal class",
        messageFormat: "Seal this class to improve performance",
        description: "Sealing classes can have measurable performance benefits since it allows certain JIT optimizations.",
        category: AnalyzerCategories.Performance,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    private ImmutableArray<DiagnosticDescriptor> diagnosticsToReport = [Descriptor];
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => diagnosticsToReport;

    public sealed override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
        if (GlobalAnalyzerOptions.CheckAnalyzer(options, Descriptor) is not ImmutableDictionary<string, DiagnosticDescriptor> newDescriptors)
        {
            return;
        }
        var newDescriptor = newDescriptors[Descriptor.Id];
        diagnosticsToReport = [newDescriptors[Descriptor.Id]];

        var classDeclarationSyntax = Unsafe.As<ClassDeclarationSyntax>(context.Node);
        var classTypeSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);
        var compilation = context.Compilation;

        if (classTypeSymbol is null or { IsSealed: true } or { IsAbstract: true } or { IsStatic: true })
        {
            return; // Not a class or already sealed
        }
        var members = classTypeSymbol.GetMembers();
        if (members.Any(m => m is { IsVirtual: true } or { IsSealed: true } or { IsAbstract: true } or { IsOverride: true }))
        {
            return; // Contains members that are sealed, abstract, or overridden
        }
        if (members.Any(m => m.DeclaredAccessibility is Accessibility.Protected) && options.TryGetValue($"dotnet_diagnostic.{newDescriptor.Id}.protected_members", out var ignoreProtectedMembers) && ignoreProtectedMembers?.Equals("ignore", StringComparison.OrdinalIgnoreCase) is false)
        {
            return; // Contains protected members and the option to ignore them is not set
        }

        static IEnumerable<INamedTypeSymbol> GetTypesInNamespace(INamespaceSymbol @namespace)
        {
            foreach (var type in @namespace.GetTypeMembers())
            {
                yield return type;
            }

            foreach (var nestedNamespace in @namespace.GetNamespaceMembers())
            {
                foreach (var type in GetTypesInNamespace(nestedNamespace))
                {
                    yield return type;
                }
            }
        }

        // Purposefully not enumerated!
        var allTypesInCompilation = GetTypesInNamespace(compilation.GlobalNamespace);
        if (allTypesInCompilation.Any(type => SymbolEqualityComparer.Default.Equals(type.BaseType, classTypeSymbol)))
        {
            // Another class has this class as a base type
            return;
        }

        var diagnostic = Diagnostic.Create(newDescriptor, classDeclarationSyntax.Keyword.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }
}