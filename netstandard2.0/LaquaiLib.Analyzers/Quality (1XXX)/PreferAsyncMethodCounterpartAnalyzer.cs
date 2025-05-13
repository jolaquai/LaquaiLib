namespace LaquaiLib.Analyzers.Quality__1XXX_;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PreferAsyncMethodCounterpartAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Descriptor { get; } = new(
        id: "LAQ2001",
        title: "Use async methods in an async context",
        messageFormat: "In async contexts, use async methods instead of their sync counterparts",
        category: AnalyzerCategories.Quality,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Descriptor];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var invocationExpressionSyntax = Unsafe.As<InvocationExpressionSyntax>(context.Node);
        var semanticModel = context.SemanticModel;
        var symbolInfo = semanticModel.GetSymbolInfo(invocationExpressionSyntax, context.CancellationToken);
        if (symbolInfo.Symbol is not IMethodSymbol symbol || symbol.MethodKind != MethodKind.Ordinary || symbol.IsAsync)
        {
            return;
        }

        // Check if the invocation is already awaited
        if (invocationExpressionSyntax.Parent is AwaitExpressionSyntax)
        {
            return;
        }

        // Check if the method even has an async counterpart
        var asyncMethodName = symbol.Name + "Async";
        var asyncMethodSymbol = symbol.ContainingType.GetMembers(asyncMethodName).OfType<IMethodSymbol>().FirstOrDefault();
        if (asyncMethodSymbol is null || asyncMethodSymbol.MethodKind != MethodKind.Ordinary)
        {
            return;
        }

        // Verify the async method returns an awaitable type
        var counterpartReturnType = asyncMethodSymbol.ReturnType;
        if (!IsAwaitableType(counterpartReturnType, semanticModel.Compilation))
        {
            return;
        }

        var memberAccessExpression = invocationExpressionSyntax.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault();
        context.ReportDiagnostic(Diagnostic.Create(Descriptor, memberAccessExpression.Name.GetLocation()));
    }

    private static bool IsAwaitableType(ITypeSymbol type, Compilation compilation)
    {
        if (type is INamedTypeSymbol namedType)
        {
            var taskType = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
            var taskOfTType = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
            var valueTaskType = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask");
            var valueTaskOfTType = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1");

            return SymbolEqualityComparer.Default.Equals(namedType.ConstructedFrom, taskType)
                || SymbolEqualityComparer.Default.Equals(namedType.ConstructedFrom, taskOfTType)
                || SymbolEqualityComparer.Default.Equals(namedType.ConstructedFrom, valueTaskType)
                || SymbolEqualityComparer.Default.Equals(namedType.ConstructedFrom, valueTaskOfTType);
        }
        return false;
    }
}