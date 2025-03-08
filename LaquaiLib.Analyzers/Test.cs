using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LaquaiLib.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PreferForOverForEach : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _descriptor = new(
        id: "LAQ0001",
        title: "Use a for loop when it is available instead of a foreach loop for improved performance",
        messageFormat: "Use a for loop when it is available instead of a foreach loop for improved performance",
        category: AnalyzerCategories.Performance,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [_descriptor];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ForEachStatement);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var foreachStatement = (ForEachStatementSyntax)context.Node;
        var expression = foreachStatement.Expression;

        // Skip if the expression is not a valid collection for a for loop
        if (!CanUseForLoop(expression, context.SemanticModel))
        {
            return;
        }

        // Report the diagnostic
        var diagnostic = Diagnostic.Create(_descriptor, foreachStatement.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }

    private bool CanUseForLoop(ExpressionSyntax expression, SemanticModel semanticModel)
    {
        var typeInfo = semanticModel.GetTypeInfo(expression);
        if (typeInfo.Type == null)
        {
            return false;
        }

        // Check if the type is an array
        if (typeInfo.Type.TypeKind == TypeKind.Array)
        {
            return true;
        }

        // Check if the type is a List<T>
        if (typeInfo.Type is INamedTypeSymbol namedType &&
            namedType.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IReadOnlyList_T)
        {
            return true;
        }

        // Check if the type implements IList or IReadOnlyList
        if (ImplementsListInterface(typeInfo.Type))
        {
            return true;
        }

        return false;
    }

    private bool ImplementsListInterface(ITypeSymbol typeSymbol)
    {
        // Check for IList<T>, IReadOnlyList<T>, or IList implementation
        foreach (var interfaceType in typeSymbol.AllInterfaces)
        {
            if (interfaceType.OriginalDefinition.SpecialType is SpecialType.System_Collections_Generic_IList_T or
                SpecialType.System_Collections_Generic_IReadOnlyList_T)
            {
                return true;
            }
        }

        return false;
    }
}
