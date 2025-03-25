using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace LaquaiLib.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AvoidCastAfterCloneAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Descriptor { get; } = new(
        id: "LAQ0002",
        title: "Do not cast after ICloneable.Clone() invocations",
        messageFormat: "Do not cast after ICloneable.Clone invocations, use Unsafe.As instead",
        description: "Sane ICloneable.Clone() implementations will return the same type as the object being cloned. Casting the result of Clone() is unnecessary and can be replaced with Unsafe.As to improve performance.",
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

        // Check if this is a call to ICloneable.Clone()
        if (methodSymbol.Name != "Clone" || methodSymbol.ContainingType.AllInterfaces.All(i => i.ToDisplayString() != "System.ICloneable"))
        {
            return;
        }

        // Look for parent cast expression
        if (invocation.Parent is CastExpressionSyntax castExpression)
        {
            // Report diagnostic for using cast after Clone()
            var locStart = castExpression.OpenParenToken.GetLocation().SourceSpan.Start;
            var locEnd = castExpression.CloseParenToken.GetLocation().SourceSpan.End;
            var loc = Location.Create(context.Node.SyntaxTree, TextSpan.FromBounds(locStart, locEnd));

            var diagnostic = Diagnostic.Create(Descriptor, loc);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
