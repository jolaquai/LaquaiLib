using LaquaiLib.Analyzers.Fixes;

namespace LaquaiLib.Analyzers.Refactorings__4XXX_;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ParallelizeLoopAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Descriptor { get; } = new(
        id: "LAQ4001",
        title: "Parallelize this loop",
        messageFormat: "Refactor this serial loop into a {0} loop",
        category: AnalyzerCategories.Refactorings,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Descriptor];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterSyntaxNodeAction(AnalyzeForNode, SyntaxKind.ForStatement);
        context.RegisterSyntaxNodeAction(AnalyzeForEachNode, SyntaxKind.ForEachStatement);
    }

    private void AnalyzeForNode(SyntaxNodeAnalysisContext context)
    {
        var forStatementSyntax = Unsafe.As<ForStatementSyntax>(context.Node);

        if (forStatementSyntax.Declaration.Variables.Count != 1)
        {
            return;
        }

        // We restrict ourselves to the very simple case of iterating upwards from an n to a max value
        var loopVar = forStatementSyntax.Declaration.Variables[0];
        if (context.SemanticModel.GetDeclaredSymbol(loopVar, context.CancellationToken) is null)
        {
            return;
        }
        var initializerValueExpression = loopVar.Initializer.Value;

        // Complex loop condition is not supported
        if (forStatementSyntax.Condition is not BinaryExpressionSyntax binaryExpressionSyntax || binaryExpressionSyntax.Kind() is not (SyntaxKind.LessThanExpression or SyntaxKind.LessThanOrEqualExpression))
        {
            return;
        }
        var end = binaryExpressionSyntax.Right;
        var endInclusive = binaryExpressionSyntax.Kind() is SyntaxKind.LessThanOrEqualExpression;
        if (endInclusive)
        {
            end = SyntaxFactory.BinaryExpression(SyntaxKind.AddExpression, end, SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1)));
        }

        // Determine if we need an async context
        if (context.SemanticModel.GetEnclosingSymbol(context.Node.SpanStart, context.CancellationToken) is not IMethodSymbol containingMethod)
        {
            return;
        }
        var doesForAwait = forStatementSyntax.Statement.DescendantNodesAndSelf().OfType<AwaitExpressionSyntax>().Any();
        // No need for ForAsync if there's no await in the loop
        var isAsync = containingMethod.IsAsync && doesForAwait;
        var targetMethod = isAsync ? "ForAsync" : "For";

        // Report the diagnostic
        var diagnostic = Diagnostic.Create(Descriptor, forStatementSyntax.ForKeyword.GetLocation(), $"Parallel.{targetMethod}");
        context.ReportDiagnostic(diagnostic);
    }
    private void AnalyzeForEachNode(SyntaxNodeAnalysisContext context)
    {
        var forEachStatementSyntax = Unsafe.As<ForEachStatementSyntax>(context.Node);

        // Literally any foreach ever can be parallelized
        if (context.SemanticModel.GetEnclosingSymbol(context.Node.SpanStart, context.CancellationToken) is not IMethodSymbol containingMethod)
        {
            return;
        }
        var doesForAwait = forEachStatementSyntax.DescendantNodes().OfType<AwaitExpressionSyntax>().Any();
        // No need for ForAsync if there's no await in the loop
        var isAsync = containingMethod.IsAsync && doesForAwait;
        var targetMethod = isAsync ? "ForEachAsync" : "ForEach";

        var diagnostic = Diagnostic.Create(Descriptor, forEachStatementSyntax.ForEachKeyword.GetLocation(), $"Parallel.{targetMethod}");
        context.ReportDiagnostic(diagnostic);
    }
}