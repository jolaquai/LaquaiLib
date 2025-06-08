namespace LaquaiLib.Analyzers.Performance__0XXX_;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseAllocateUninitializedArrayAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Descriptor { get; } = new(
        id: "LAQ0005",
        title: "Use GC.AllocateUnitializedArray if all indices are definitely assigned",
        messageFormat: "If all indices in this array are definitely assigned, use GC.AllocateUnitializedArray to improve performance",
        description: "Arrays need not be zeroed before being used if all slots are being assigned after creation.",
        category: AnalyzerCategories.Performance,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Descriptor];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ArrayCreationExpression);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var arrayCreationExpressionSyntax = Unsafe.As<ArrayCreationExpressionSyntax>(context.Node);
        var semanticModel = context.SemanticModel;
        var typeInfo = semanticModel.GetTypeInfo(arrayCreationExpressionSyntax.Type.ElementType);

        // GC.AllocateUninitializedArray is only applicable for value types, since a IsReferenceOrContainsReferences check is done inside the method
        if (typeInfo.Type.IsValueType)
        {
            // Report the diagnostic
            var diagnostic = Diagnostic.Create(Descriptor, arrayCreationExpressionSyntax.NewKeyword.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}
