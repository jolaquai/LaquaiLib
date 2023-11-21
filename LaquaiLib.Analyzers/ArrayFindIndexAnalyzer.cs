using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LaquaiLib.Analyzers;


/// <summary>
/// Represents an analyzer that checks for the usage of a loop construct where a call to <see cref="Array.FindIndex{T}(T[], Predicate{T})"/> can be used instead.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ArrayFindIndexAnalyzer : DiagnosticAnalyzer
{
    public static string DiagnosticId { get; } = Util.GetAnalyzerId(typeof(ArrayFindIndexAnalyzer));
    internal static LocalizableString Title { get; } = "Array.FindIndex Analyzer";
    internal static LocalizableString MessageFormat { get; } = "'{0}'";
    internal static string Category { get; } = Util.GetAnalyzerCategory(typeof(ArrayFindIndexAnalyzer));

    internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterSyntaxNodeAction(AnalyzeForNode, SyntaxKind.ForStatement);
    }

    private static void AnalyzeForNode(SyntaxNodeAnalysisContext context)
    {
        var forStatement = (ForStatementSyntax)context.Node;

        var condition = forStatement.Condition;

        if (condition is BinaryExpressionSyntax binaryExpression
            && binaryExpression.Kind() == SyntaxKind.LessThanExpression
            && binaryExpression.Right is MemberAccessExpressionSyntax memberAccess
            && memberAccess.Name.Identifier.ValueText == "Length"
            && memberAccess.Expression is IdentifierNameSyntax identifierName
            && identifierName.Identifier.ValueText == "array"
            && binaryExpression.Left is IdentifierNameSyntax indexIdentifier
            && context.SemanticModel.GetSymbolInfo(indexIdentifier).Symbol is ILocalSymbol indexSymbol
            && context.SemanticModel.GetSymbolInfo(identifierName).Symbol is ILocalSymbol arraySymbol
            && arraySymbol.Type is INamedTypeSymbol namedTypeSymbol
            && namedTypeSymbol.IsGenericType
            && namedTypeSymbol.ConstructedFrom.SpecialType == SpecialType.System_Array
            && namedTypeSymbol.TypeArguments.Length == 1
            && namedTypeSymbol.TypeArguments[0].Equals(indexSymbol.Type)
        )
        {
            var diagnostic = Diagnostic.Create(Rule, forStatement.GetLocation(), "Use Array.FindIndex instead of for loop");

            context.ReportDiagnostic(diagnostic);
        }
    }
}
