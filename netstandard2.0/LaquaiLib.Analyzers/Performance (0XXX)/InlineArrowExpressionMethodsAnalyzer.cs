using System.Collections.Immutable;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LaquaiLib.Analyzers.Performance;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class InlineArrowExpressionMethodsAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Descriptor { get; } = new(
        id: "LAQ0004",
        title: "Inline small methods",
        messageFormat: "Apply [MethodImpl(MethodImplOptions.AggressiveInlining)] to small methods to improve performance.",
        description: "Methods that are very small (for example, ones declared using an arrow expression) should be aggressively inlined to improve performance.",
        category: AnalyzerCategories.Performance,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Descriptor];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ArrowExpressionClause);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var node = Unsafe.As<ArrowExpressionClauseSyntax>(context.Node);

        switch (node.Parent)
        {
            case MethodDeclarationSyntax methodDeclarationSyntax:
            {
                AnalyzeDeclaration(context, methodDeclarationSyntax, methodDeclarationSyntax.Identifier.GetLocation());
                break;
            }
            case PropertyDeclarationSyntax propertyDeclarationSyntax:
            {
                var propertySymbol = context.SemanticModel.GetDeclaredSymbol(propertyDeclarationSyntax);
                if (propertySymbol?.GetMethod != null)
                {
                    AnalyzeSymbol(context, propertySymbol.GetMethod, propertyDeclarationSyntax.Identifier.GetLocation());
                }
                break;
            }
            case AccessorDeclarationSyntax accessorDeclarationSyntax:
            {
                AnalyzeDeclaration(context, accessorDeclarationSyntax, accessorDeclarationSyntax.Keyword.GetLocation());
                break;
            }
            case IndexerDeclarationSyntax indexerDeclarationSyntax:
            {
                var indexerSymbol = context.SemanticModel.GetDeclaredSymbol(indexerDeclarationSyntax);
                if (indexerSymbol?.GetMethod != null)
                {
                    AnalyzeSymbol(context, indexerSymbol.GetMethod, indexerDeclarationSyntax.ThisKeyword.GetLocation());
                }
                break;
            }
            case OperatorDeclarationSyntax operatorDeclarationSyntax:
            {
                AnalyzeDeclaration(context, operatorDeclarationSyntax, operatorDeclarationSyntax.OperatorToken.GetLocation());
                break;
            }
            case ConversionOperatorDeclarationSyntax conversionOperatorDeclarationSyntax:
            {
                AnalyzeDeclaration(context, conversionOperatorDeclarationSyntax, conversionOperatorDeclarationSyntax.Type.GetLocation());
                break;
            }
            case LocalFunctionStatementSyntax localFunctionStatementSyntax:
            {
                AnalyzeDeclaration(context, localFunctionStatementSyntax, localFunctionStatementSyntax.Identifier.GetLocation());
                break;
            }
            default:
            {
                return;
            }
        }
    }

    private static void AnalyzeDeclaration(SyntaxNodeAnalysisContext context, SyntaxNode declaration, Location location)
    {
        var symbol = context.SemanticModel.GetDeclaredSymbol(declaration);
        if (symbol != null)
        {
            AnalyzeSymbol(context, symbol, location);
        }
    }

    private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context, ISymbol symbol, Location location)
    {
        if (!HasAggressiveInliningAttribute(symbol))
        {
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
        }
    }

    private const int _aggressiveInlining = (int)MethodImplOptions.AggressiveInlining;
    private static bool HasAggressiveInliningAttribute(ISymbol symbol)
    {
        // Check for [MethodImpl(MethodImplOptions.AggressiveInlining)]
        foreach (var attribute in symbol.GetAttributes())
        {
            if (IsMethodImplAttribute(attribute.AttributeClass))
            {
                if (attribute.ConstructorArguments.Length > 0)
                {
                    var constructorArg = attribute.ConstructorArguments[0];

                    // Check if the argument is an enum value
                    if (constructorArg.Kind == TypedConstantKind.Enum)
                    {
                        var enumType = constructorArg.Type as INamedTypeSymbol;
                        if (enumType?.Name == "MethodImplOptions")
                        {
                            var intValue = (int)constructorArg.Value;
                            // MethodImplOptions.AggressiveInlining is 256
                            if ((intValue & _aggressiveInlining) != 0)
                            {
                                return true;
                            }
                        }
                    }
                    // Or it might be directly an integer
                    else if (constructorArg.Value is int intValue && (intValue & _aggressiveInlining) != 0)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private static bool IsMethodImplAttribute(ITypeSymbol typeSymbol)
    {
        var containingNs = typeSymbol.ContainingNamespace.ToString();
        return typeSymbol.Name == "MethodImplAttribute" && (containingNs == "System.Runtime.CompilerServices" || containingNs.EndsWith(".System.Runtime.CompilerServices"));
    }
}
