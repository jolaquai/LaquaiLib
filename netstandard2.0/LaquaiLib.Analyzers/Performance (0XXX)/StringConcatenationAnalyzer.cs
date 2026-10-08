using LaquaiLib.Analyzers.Shared;

namespace LaquaiLib.Analyzers.Performance__0XXX_;

/// <summary>
/// Flags a string concatenation written in a form the compiler cannot lower as well as an equivalent one: a <c>+</c> chain, or an interpolated string whose parts <c>string.Concat</c> could take directly.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class StringConcatenationAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Descriptor { get; } = new(
        id: "LAQ0007",
        title: "Use the cheapest form of string concatenation",
        messageFormat: "Use {0} instead of {1}",
        category: AnalyzerCategories.Performance,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Descriptor];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(static compilationStartContext =>
        {
            var overloads = ConcatOverloads.Create(compilationStartContext.Compilation);
            compilationStartContext.RegisterSyntaxNodeAction(nodeContext => AnalyzeNode(nodeContext, overloads), SyntaxKind.AddExpression, SyntaxKind.InterpolatedStringExpression);
        });
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context, ConcatOverloads overloads)
    {
        var node = Unsafe.As<ExpressionSyntax>(context.Node);
        var semanticModel = context.SemanticModel;
        var cancellationToken = context.CancellationToken;

        if (node.IsKind(SyntaxKind.AddExpression) && !StringConcatenationHelper.IsStringConcatenation(node, semanticModel, cancellationToken))
            return;
        if (StringConcatenationHelper.IsNestedInConcatenation(node, semanticModel, cancellationToken))
            return;

        var rewrite = StringConcatenationHelper.Classify(node, semanticModel, overloads, cancellationToken, out _);
        if (rewrite is ConcatRewrite.None)
            return;

        var diagnostic = Diagnostic.Create(Descriptor, node.GetLocation(), StringConcatenationHelper.GetReplacementName(rewrite), node.IsKind(SyntaxKind.AddExpression) ? "string concatenation" : "an interpolated string");
        context.ReportDiagnostic(diagnostic);
    }
}
