namespace LaquaiLib.Analyzers.Performance__0XXX_;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AvoidCastAfterCloneAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Descriptor { get; } = new(
        id: "LAQ0002",
        title: "Do not cast after clone operations",
        messageFormat: "Do not cast after {0} invocations, use Unsafe.As instead",
        description: "Sane implementations of clone methods will return the same type as the object being cloned. Casting the result incurs an unnecessary type check and can be replaced with Unsafe.As to improve performance.",
        category: AnalyzerCategories.Performance,
        defaultSeverity: DiagnosticSeverity.Warning,
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
        var invocation = (InvocationExpressionSyntax)context.Node;
        if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        var targetName = "";
        if (!IsApplicable(methodSymbol, ref targetName))
        {
            return;
        }

        // Look for parent cast expression
        Location loc = null;
        if (invocation.FirstAncestorOrSelf<CastExpressionSyntax>() is CastExpressionSyntax castExpression)
        {
            // Report diagnostic for using cast after Clone()
            var locStart = castExpression.OpenParenToken.GetLocation().SourceSpan.Start;
            var locEnd = castExpression.CloseParenToken.GetLocation().SourceSpan.End;
            loc = Location.Create(context.Node.SyntaxTree, TextSpan.FromBounds(locStart, locEnd));
        }
        else if (invocation.FirstAncestorOrSelf<BinaryExpressionSyntax>() is BinaryExpressionSyntax binaryExpr && binaryExpr.IsKind(SyntaxKind.AsExpression))
        {
            // Report diagnostic for using as expression after Clone()
            var locStart = binaryExpr.OperatorToken.GetLocation().SourceSpan.Start;
            var locEnd = binaryExpr.Right.GetLocation().SourceSpan.End;
            loc = Location.Create(context.Node.SyntaxTree, TextSpan.FromBounds(locStart, locEnd));
        }
        if (loc is not null && targetName is not null)
        {
            var diagnostic = Diagnostic.Create(Descriptor, loc, targetName);
            context.ReportDiagnostic(diagnostic);
        }
    }

    /// <summary>
    /// Gets whether the method symbol is applicable for this analyzer. Returns <see langword="true"/> only for definitively known clone methods.
    /// Their implementations are assumed to be sane.
    /// </summary>
    private static bool IsApplicable(IMethodSymbol methodSymbol, ref string targetName)
    {
        var symbolName = methodSymbol.Name;
        // Check if this is a call to ICloneable.Clone()
        var isICloneable = methodSymbol.ContainingType.AllInterfaces.Any(static i => i.ToDisplayString() == "System.ICloneable");
        if (symbolName is "Clone" && isICloneable)
        {
            targetName = "System.ICloneable.Clone";
            return true;
        }

        // Also report for Open XML Clone or CloneNode methods (regardless of arguments to the latter since casting either way is unnecessary)
        var isOpenXmlElement = methodSymbol.OriginalDefinition.ContainingType.ToDisplayString() == "DocumentFormat.OpenXml.OpenXmlElement";
        Debug.WriteLine(methodSymbol.OriginalDefinition.ContainingType.ToDisplayString());
        if (symbolName is "Clone" or "CloneNode" && isOpenXmlElement)
        {
            targetName = "DocumentFormat.OpenXml.OpenXmlElement." + symbolName;
            return true;
        }

        return false;
    }
}
