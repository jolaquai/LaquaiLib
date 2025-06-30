using LaquaiLib.Analyzers.Shared.Attributes;

namespace LaquaiLib.Analyzers.CorrectUsage__9XXX_;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class FullAccessProxyAttributeUsageAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor InvalidAttributePlacementDescriptor { get; } = new(
        id: "LAQ9001",
        title: $"{nameof(FullAccessProxyAttribute<>)} must be applied on a partial type",
        messageFormat: $"{nameof(FullAccessProxyAttribute<>)} must be applied on a partial type",
        category: AnalyzerCategories.Validity,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [
        InvalidAttributePlacementDescriptor,
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.Attribute);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var attributeSyntax = (AttributeSyntax)context.Node;

        // Check if this is a FullAccessProxyAttribute
        if (!IsFullAccessProxyAttribute(attributeSyntax, context.SemanticModel))
        {
            return;
        }

        // Get the type declaration that this attribute is applied to
        if (attributeSyntax.Parent?.Parent is not TypeDeclarationSyntax typeDecl)
        {
            return;
        }

        // LAQ9001: Check if the type is partial
        if (!typeDecl.Modifiers.Any(static m => m.IsKind(SyntaxKind.PartialKeyword)))
        {
            var diagnostic = Diagnostic.Create(
                InvalidAttributePlacementDescriptor,
                attributeSyntax.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsFullAccessProxyAttribute(AttributeSyntax attributeSyntax, SemanticModel semanticModel)
    {
        if (semanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeConstructor)
        {
            return false;
        }

        var attributeType = attributeConstructor.ContainingType;
        var baseTypeName = attributeType.OriginalDefinition.MetadataName;

        return baseTypeName == typeof(FullAccessProxyAttribute<>).Name;
    }
}
