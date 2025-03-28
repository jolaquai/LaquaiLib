using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LaquaiLib.Analyzers;

/// <summary>
/// Recommends usage of <see langword="stackalloc"/>'d buffers over heap-allocating arrays for temporary buffers with a known (and safe) size.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PreferStackallocForTemporaryBuffersAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Descriptor { get; } = new(
        id: "LAQ0003",
        title: "Use stackalloc for temporary buffers",
        messageFormat: "Use stackalloc for temporary buffers",
        category: AnalyzerCategories.Performance,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Descriptor];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ArrayCreationExpression);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var acExpr = (ArrayCreationExpressionSyntax)context.Node;

        if (!AnalyzerApplicable(context, acExpr))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Descriptor, acExpr.GetLocation()));
    }

    private static bool AnalyzerApplicable(SyntaxNodeAnalysisContext context, ArrayCreationExpressionSyntax acExpr)
    {
        var type = acExpr.Type;
        var rankSpecifierExprs = type.RankSpecifiers;
        if (rankSpecifierExprs.Count != 1 || rankSpecifierExprs[0] is not ArrayRankSpecifierSyntax rankSpecifier || rankSpecifier.Rank != 1
            || rankSpecifier.Sizes.Count != 1)
        {
            return false;
        }

        // Have to figure out if the rank is a constant expression or at least somehow statically known at compile time
        var sizeOpt = rankSpecifier.Sizes[0] switch
        {
            LiteralExpressionSyntax literal when literal.Token.Value is int v => new Optional<object>(v),
            IdentifierNameSyntax identifier => context.SemanticModel.GetConstantValue(identifier),
            _ => new Optional<object>()
        };
        if (!sizeOpt.HasValue)
        {
            return false;
        }
        var size = (int)sizeOpt.Value;
        if (size <= 0)
        {
            return false;
        }

        // Is the size safe to stackalloc?
        var typeBeingCreated = type.ElementType;
        var sizeofType = context.SemanticModel.GetTypeInfo(typeBeingCreated).Type?.SizeOf(context.Compilation);
        // Cutoff is 768 since anything above that tends to not even really help when stackalloc'd, but anything below 512 is definitely safe and worth it
        const int cutoff = 768;
        if (sizeofType is null or 0 || (sizeofType == 1 && size > cutoff) || size * (int)sizeofType > cutoff)
        {
            return false;
        }

        // stackalloc assignments into fields or properties are not allowed
        if (acExpr.FirstAncestorOrSelf<MemberDeclarationSyntax>() is FieldDeclarationSyntax or PropertyDeclarationSyntax)
        {
            return false;
        }

        // Check if the array creation is in a method body
        // Update: also apply to non-method allocations since ref structs can happily store stackallocs
        // AnalyzeSymbolDataFlow implicitly supports this
        var methodDeclarationSyntax = acExpr.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        var structDeclarationSyntax = acExpr.FirstAncestorOrSelf<StructDeclarationSyntax>();
        if (!AnalyzeSymbolDataFlow(context, acExpr, methodDeclarationSyntax, structDeclarationSyntax))
        {
            return false;
        }

        return true;
    }

    private static bool AnalyzeSymbolDataFlow(SyntaxNodeAnalysisContext context, ArrayCreationExpressionSyntax acExpr, MethodDeclarationSyntax method, StructDeclarationSyntax structDeclarationSyntax)
    {
        // Check if the buffer is never implicitly converted to Memory<> since that would prevent stackalloc'ing
        var dataFlow = context.SemanticModel.AnalyzeDataFlow(acExpr);
        if (dataFlow?.Succeeded is not true)
        {
            return false;
        }

        // Track the array instance
        var variableDeclaratorSyntax = acExpr.FirstAncestorOrSelf<VariableDeclaratorSyntax>();
        ISymbol arraySymbol = null;
        if (variableDeclaratorSyntax is not null)
        {
            arraySymbol = context.SemanticModel.GetDeclaredSymbol(variableDeclaratorSyntax);
            if (arraySymbol is null)
            {
                // We're not being assigned to a variable, so we may be inside a field or property initializer
                return CheckFlowInFieldOrPropertyAssignment(context, acExpr);
            }
        }

        IEnumerable<SyntaxNode> nodes = null;
        if (method is not null)
        {
            nodes = (method.Body ?? (SyntaxNode)method.ExpressionBody).DescendantNodes();
        }
        else
        {
            return false;
        }

        // Look for implicit conversions throughout the nodes
        foreach (var node in nodes)
        {
            // Skip the array creation itself
            if (node == acExpr)
            {
                continue;
            }

            // Check assignments
            if (node is AssignmentExpressionSyntax assignment && assignment.Right.Contains(acExpr))
            {
                var leftType = context.SemanticModel.GetTypeInfo(assignment.Left).Type;
                if (leftType != null && (leftType.Name == "Memory" || leftType.Name == "ReadOnlyMemory"))
                {
                    return false;
                }
            }

            // Check return statements
            else if (node is ReturnStatementSyntax returnStmt && returnStmt.Expression?.Contains(acExpr) == true)
            {
                var returnType = context.SemanticModel.GetTypeInfo(returnStmt.Expression).ConvertedType;
                if (returnType != null && (returnType.Name == "Memory" || returnType.Name == "ReadOnlyMemory"))
                {
                    return false;
                }
            }

            // Check method invocations
            else if (node is InvocationExpressionSyntax invocation)
            {
                // If this fails, someone is passing the array directly to a method without holding a reference to it
                // While a) that's insane, and b) it's not our problem, we have to bail here because we have no idea what the method does
                // It may return some form of reference to the passed buffer
                if (arraySymbol is null)
                {
                    return false;
                }

                foreach (var arg in invocation.ArgumentList.Arguments)
                {
                    if (arg.Expression.Contains(acExpr)
                        || SymbolEqualityComparer.Default.Equals(context.SemanticModel.GetSymbolInfo(arg.Expression).Symbol, arraySymbol))
                    {
                        if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol paramSymbol)
                        {
                            var argIndex = invocation.ArgumentList.Arguments.IndexOf(arg);
                            if (argIndex < paramSymbol.Parameters.Length)
                            {
                                var paramType = paramSymbol.Parameters[argIndex].Type;
                                if (paramType.Name is "Memory" or "ReadOnlyMemory")
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }

            // Check property assignments
            else if (node is ObjectCreationExpressionSyntax objectCreation)
            {
                foreach (var initializer in objectCreation.Initializer?.Expressions ?? Enumerable.Empty<ExpressionSyntax>())
                {
                    if (initializer is AssignmentExpressionSyntax assn && assn.Right.Contains(acExpr))
                    {
                        var memberSymbol = context.SemanticModel.GetSymbolInfo(assn.Left).Symbol;
                        if (memberSymbol is IPropertySymbol propertySymbol)
                        {
                            if (propertySymbol.Type.Name is "Memory" or "ReadOnlyMemory")
                            {
                                return false;
                            }
                        }
                    }
                }
            }
        }

        return true;
    }

    private static bool CheckFlowInFieldOrPropertyAssignment(SyntaxNodeAnalysisContext context, ArrayCreationExpressionSyntax acExpr)
    {
        // Give up if we're not inside a ref struct, but our assignment is being attempted as if we were, which means the stackalloc isn't allowed here anyway
        var structDeclarationSyntax = acExpr.FirstAncestorOrSelf<StructDeclarationSyntax>();
        if (structDeclarationSyntax is null || !structDeclarationSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.RefKeyword)))
        {
            return false;
        }

        // Don't report the diagnostic if the type of the field or property we're assigning to isn't already of type Span<T>
        ITypeSymbol typeSymbol;
        if (acExpr.FirstAncestorOrSelf<FieldDeclarationSyntax>() is FieldDeclarationSyntax fieldDeclarationSyntax)
        {
            typeSymbol = context.SemanticModel.GetTypeInfo(fieldDeclarationSyntax.Declaration.Type).Type;
        }
        else if (acExpr.FirstAncestorOrSelf<PropertyDeclarationSyntax>() is PropertyDeclarationSyntax propertyDeclarationSyntax)
        {
            typeSymbol = context.SemanticModel.GetTypeInfo(propertyDeclarationSyntax.Type).Type;
        }
        else
        {
            return true;
        }

        if (typeSymbol is null || typeSymbol.Name != "Span")
        {
            return false;
        }

        return true;
    }
}