using LaquaiLib.Analyzers.Shared;

namespace LaquaiLib.Analyzers.Quality__1XXX_;

/// <summary>
/// Flags attribute lists whose attributes, across every list sharing the same target on a single declaration, are not ordered alphabetically by attribute type name.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AttributeOrderAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Descriptor { get; } = new(
        id: "LAQ1001",
        title: "Order attributes alphabetically",
        messageFormat: "Attributes should be ordered alphabetically by attribute type name",
        description: "Attributes applied to the same declaration and target are easier to scan when ordered alphabetically by their type name, ignoring namespace and the optional 'Attribute' suffix.",
        category: AnalyzerCategories.Quality,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Descriptor];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.AttributeList);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var attributeList = (AttributeListSyntax)context.Node;
        var declaration = attributeList.Parent;
        if (declaration is null)
            return;

        // Every attribute list sharing a parent is analyzed together in one pass; only the first one needs to trigger it
        if (declaration.ChildNodes().OfType<AttributeListSyntax>().FirstOrDefault() != attributeList)
            return;

        var groups = AttributeOrderHelper.GetTargetGroups(declaration);
        for (var i = 0; i < groups.Length; i++)
        {
            var group = groups[i];
            if (AttributeOrderHelper.IsOrdered(group))
                continue;

            var location = Location.Create(attributeList.SyntaxTree, AttributeOrderHelper.GetSpan(group));
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
        }
    }
}
