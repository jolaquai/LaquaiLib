using LaquaiLib.Analyzers.Shared;

namespace LaquaiLib.Analyzers.Performance__0XXX_;

/// <summary>
/// Flags a <c>string.Join</c> call whose separator is provably empty, which is what <c>string.Concat</c> does without paying for the separator.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class JoinWithEmptySeparatorAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Descriptor { get; } = new(
        id: "LAQ0008",
        title: "Do not join with an empty separator",
        messageFormat: "Use string.Concat instead of string.Join with an empty separator",
        category: AnalyzerCategories.Performance,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Descriptor];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var invocation = Unsafe.As<InvocationExpressionSyntax>(context.Node);

        var name = invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.ValueText,
            IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
            _ => null
        };
        if (name != "Join")
            return;

        var semanticModel = context.SemanticModel;
        var cancellationToken = context.CancellationToken;

        if (semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol is not IMethodSymbol { IsStatic: true } method
            || method.ContainingType?.SpecialType != SpecialType.System_String
            || method.Parameters.Length != 2
            || method.Parameters[0].Type.SpecialType != SpecialType.System_String)
            return;

        var separatorIndex = JoinWithEmptySeparatorHelper.GetSeparatorIndex(invocation.ArgumentList.Arguments, method.Parameters[0].Name);
        if (separatorIndex < 0)
            return;

        if (!IsEmptyString(invocation.ArgumentList.Arguments[separatorIndex].Expression, semanticModel, cancellationToken))
            return;

        var properties = ImmutableDictionary<string, string>.Empty.Add(JoinWithEmptySeparatorHelper.SeparatorIndexKey, separatorIndex.ToString());
        var diagnostic = Diagnostic.Create(Descriptor, invocation.GetLocation(), properties);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsEmptyString(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        if (semanticModel.GetConstantValue(expression, cancellationToken) is { HasValue: true, Value: "" })
            return true;

        // string.Empty is a static readonly field, not a constant
        return semanticModel.GetSymbolInfo(expression, cancellationToken).Symbol is IFieldSymbol { Name: "Empty", IsStatic: true } field
            && field.ContainingType?.SpecialType == SpecialType.System_String;
    }
}
