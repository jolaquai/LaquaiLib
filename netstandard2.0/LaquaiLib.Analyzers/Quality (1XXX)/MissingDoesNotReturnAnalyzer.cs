using LaquaiLib.Analyzers.Shared;

namespace LaquaiLib.Analyzers.Quality__1XXX_;

/// <summary>
/// Flags a method or property/indexer accessor whose body unconditionally starts by calling another method or accessor marked <c>[DoesNotReturn]</c> - directly, or through a chain of such calls - while it itself is not marked that way.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MissingDoesNotReturnAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Descriptor { get; } = new(
        id: "LAQ1002",
        title: "Add [DoesNotReturn] to members that never return",
        messageFormat: "'{0}' should be marked [DoesNotReturn]: it unconditionally starts by calling '{1}', which never returns",
        description: "When the first statement or expression a method or property/indexer accessor unconditionally executes calls another one marked [DoesNotReturn] - directly, or through a chain of such calls - it never returns to its caller either, and should carry the attribute itself so callers and flow analysis see that.",
        category: AnalyzerCategories.Quality,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Descriptor];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterSyntaxNodeAction(AnalyzeNode,
            SyntaxKind.MethodDeclaration,
            SyntaxKind.GetAccessorDeclaration,
            SyntaxKind.SetAccessorDeclaration,
            SyntaxKind.InitAccessorDeclaration,
            SyntaxKind.PropertyDeclaration,
            SyntaxKind.IndexerDeclaration);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var node = context.Node;

        // A property/indexer with an explicit accessor list is handled entirely through its own GetAccessorDeclaration nodes
        if (node is PropertyDeclarationSyntax { AccessorList: not null } or IndexerDeclarationSyntax { AccessorList: not null })
            return;

        if (!DoesNotReturnChainHelper.TryGetBody(node, out var block, out var expressionBody))
            return;

        var semanticModel = context.SemanticModel;
        var cancellationToken = context.CancellationToken;
        var methodSymbol = DoesNotReturnChainHelper.GetMethodSymbol(node, semanticModel, cancellationToken);
        if (methodSymbol is null
            || methodSymbol.IsAsync
            || DoesNotReturnChainHelper.IsIterator(block)
            || DoesNotReturnChainHelper.IsDoesNotReturn(methodSymbol))
            return;

        var lead = DoesNotReturnChainHelper.GetLeadExpression(block, expressionBody);
        if (lead is null)
            return;

        var target = DoesNotReturnChainHelper.ResolveCallTarget(lead, semanticModel, cancellationToken);
        if (!DoesNotReturnChainHelper.NeverReturns(target, context.Compilation, cancellationToken))
            return;

        var location = Location.Create(node.SyntaxTree, GetReportSpan(node));
        context.ReportDiagnostic(Diagnostic.Create(Descriptor, location, DisplayName(methodSymbol), DisplayName(target)));
    }

    private static TextSpan GetReportSpan(SyntaxNode node) => node switch
    {
        MethodDeclarationSyntax method => method.Identifier.Span,
        AccessorDeclarationSyntax accessor => accessor.Keyword.Span,
        PropertyDeclarationSyntax property => property.Identifier.Span,
        IndexerDeclarationSyntax indexer => indexer.ThisKeyword.Span,
        _ => node.Span
    };

    private static string DisplayName(IMethodSymbol method) => method.AssociatedSymbol is IPropertySymbol property
        ? $"{property.Name}.{(SymbolEqualityComparer.Default.Equals(property.GetMethod, method) ? "get" : "set")}"
        : method.Name;
}
