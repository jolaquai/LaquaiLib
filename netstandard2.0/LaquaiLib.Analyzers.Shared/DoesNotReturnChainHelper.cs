namespace LaquaiLib.Analyzers.Shared;

/// <summary>
/// Shared logic for detecting whether a method or property/indexer accessor unconditionally starts by calling another one marked <c>[DoesNotReturn]</c>, directly or through a chain of such calls.
/// Used by both <c>MissingDoesNotReturnAnalyzer</c> and <c>MissingDoesNotReturnFixer</c> so the two stay in lockstep.
/// </summary>
public static class DoesNotReturnChainHelper
{
    /// <summary>
    /// Gets whether <paramref name="symbol"/> itself carries <c>System.Diagnostics.CodeAnalysis.DoesNotReturnAttribute</c>.
    /// </summary>
    public static bool IsDoesNotReturn(ISymbol symbol)
    {
        var attributes = symbol.GetAttributes();
        for (var i = 0; i < attributes.Length; i++)
            if (IsDoesNotReturnAttribute(attributes[i].AttributeClass))
                return true;
        return false;
    }

    private static bool IsDoesNotReturnAttribute(INamedTypeSymbol attributeClass)
        => attributeClass is { Name: "DoesNotReturnAttribute", ContainingNamespace: { Name: "CodeAnalysis", ContainingNamespace: { Name: "Diagnostics", ContainingNamespace: { Name: "System", ContainingNamespace.IsGlobalNamespace: true } } } };

    /// <summary>
    /// Resolves the member symbol a syntax node participates in this analysis as: a method or accessor declaration's own <see cref="IMethodSymbol"/>, or - for an expression-bodied property/indexer with no explicit accessor list - the <see cref="IPropertySymbol.GetMethod"/> it implies.
    /// </summary>
    public static IMethodSymbol GetMethodSymbol(SyntaxNode node, SemanticModel semanticModel, CancellationToken cancellationToken)
        => semanticModel.GetDeclaredSymbol(node, cancellationToken) switch
        {
            IMethodSymbol method => method,
            IPropertySymbol { GetMethod: { } getMethod } => getMethod,
            _ => null
        };

    /// <summary>
    /// Splits <paramref name="node"/>'s body into its block and expression-bodied forms. Returns <see langword="false"/> if it has neither: an abstract/extern/auto-implemented member, or a property/indexer with an explicit accessor list (each of its accessors is analyzed on its own instead).
    /// </summary>
    public static bool TryGetBody(SyntaxNode node, out BlockSyntax block, out ArrowExpressionClauseSyntax expressionBody)
    {
        (block, expressionBody) = node switch
        {
            MethodDeclarationSyntax method => (method.Body, method.ExpressionBody),
            AccessorDeclarationSyntax accessor => (accessor.Body, accessor.ExpressionBody),
            PropertyDeclarationSyntax { AccessorList: null } property => (null, property.ExpressionBody),
            IndexerDeclarationSyntax { AccessorList: null } indexer => (null, indexer.ExpressionBody),
            _ => (null, null)
        };
        return block is not null || expressionBody is not null;
    }

    /// <summary>
    /// Gets the expression a block or expression body unconditionally evaluates first: an expression body's expression, or a block's first statement if it is a bare expression statement or a <see langword="return"/> with a value.
    /// Anything else - including an empty block, or a first statement whose execution isn't unconditional (an <see langword="if"/>, a loop, ...) - yields <see langword="null"/>.
    /// </summary>
    public static ExpressionSyntax GetLeadExpression(BlockSyntax block, ArrowExpressionClauseSyntax expressionBody)
    {
        if (expressionBody is not null)
            return Unwrap(expressionBody.Expression);
        if (block is null || block.Statements.Count == 0)
            return null;

        return block.Statements[0] switch
        {
            ExpressionStatementSyntax expressionStatement => Unwrap(expressionStatement.Expression),
            ReturnStatementSyntax { Expression: { } returnExpression } => Unwrap(returnExpression),
            _ => null
        };
    }

    private static ExpressionSyntax Unwrap(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
            expression = parenthesized.Expression;
        return expression;
    }

    /// <summary>
    /// Resolves what <paramref name="expression"/> - <see cref="GetLeadExpression"/>'s result - unconditionally calls: the invoked method for a plain call, a target property/indexer's setter for a simple assignment to it, or its getter for a bare read of it.
    /// Anything else (a literal, a conditional access, a field, a local, a compound assignment, ...) yields <see langword="null"/>, since none of those on their own ever fail to return.
    /// </summary>
    public static IMethodSymbol ResolveCallTarget(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        switch (expression)
        {
            case InvocationExpressionSyntax invocation:
                return semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol as IMethodSymbol;
            case AssignmentExpressionSyntax assignment when assignment.IsKind(SyntaxKind.SimpleAssignmentExpression):
                return (semanticModel.GetSymbolInfo(assignment.Left, cancellationToken).Symbol as IPropertySymbol)?.SetMethod;
            case MemberAccessExpressionSyntax or IdentifierNameSyntax or ElementAccessExpressionSyntax:
                return (semanticModel.GetSymbolInfo(expression, cancellationToken).Symbol as IPropertySymbol)?.GetMethod;
            default:
                return null;
        }
    }

    /// <summary>
    /// Gets whether <paramref name="method"/> never returns: it is itself marked <c>[DoesNotReturn]</c>, or its body unconditionally starts by calling something that, recursively, never returns.
    /// Only source methods can be followed past the first hop; anything without declaring syntax (metadata, or a member with no body to inspect) stops the chain unless it is directly marked. Async methods and iterators are never followed into, since neither runs its body synchronously on the caller's stack. A cycle stops the walk without reporting a false positive.
    /// </summary>
    public static bool NeverReturns(IMethodSymbol method, Compilation compilation, CancellationToken cancellationToken)
        => method is not null && NeverReturnsCore(method, compilation, new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default), cancellationToken);

    private static bool NeverReturnsCore(IMethodSymbol method, Compilation compilation, HashSet<IMethodSymbol> visited, CancellationToken cancellationToken)
    {
        method = method.OriginalDefinition;
        if (!visited.Add(method))
            return false;

        if (IsDoesNotReturn(method))
            return true;
        if (method.IsAsync)
            return false;

        var references = method.DeclaringSyntaxReferences;
        for (var i = 0; i < references.Length; i++)
        {
            var syntax = references[i].GetSyntax(cancellationToken);
            if (!TryGetBody(syntax, out var block, out var expressionBody) || IsIterator(block))
                continue;

            var lead = GetLeadExpression(block, expressionBody);
            if (lead is null)
                continue;

            var semanticModel = compilation.GetSemanticModel(syntax.SyntaxTree);
            var next = ResolveCallTarget(lead, semanticModel, cancellationToken);
            if (next is not null && NeverReturnsCore(next, compilation, visited, cancellationToken))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Gets whether <paramref name="block"/> contains a <see langword="yield"/> statement of its own - i.e. whether the member it belongs to is an iterator, which never runs any of its body synchronously when called; only <see cref="System.Collections.IEnumerator.MoveNext"/> does. Nested local functions and lambdas are skipped, since a <see langword="yield"/> inside one belongs to that function, not the member being checked.
    /// </summary>
    public static bool IsIterator(BlockSyntax block)
    {
        if (block is null)
            return false;
        foreach (var descendant in block.DescendantNodes(static n => n is not (AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax)))
            if (descendant is YieldStatementSyntax)
                return true;
        return false;
    }
}
