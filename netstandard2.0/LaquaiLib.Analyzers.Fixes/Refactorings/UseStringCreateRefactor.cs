namespace LaquaiLib.Analyzers.Fixes.Refactorings;

/// <summary>
/// Turns <c>string.Create(null, stackalloc char[length], $"...")</c> back into the bare <c>$"..."</c> form.
/// The other direction is LAQ0009 and its fixer, which share <see cref="StringCreateHelper"/>.
/// </summary>
[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(UseStringCreateRefactor)), Shared]
public sealed class UseStringCreateRefactor : LaquaiLibRefactoring
{
    public override async ValueTask<ImmutableArray<CodeActionInfo>> GetCodeActionInfosAsync(Document document, CompilationUnitSyntax compilationUnitSyntax, TextSpan span, CancellationToken cancellationToken)
    {
        if (FindTarget(compilationUnitSyntax.FindNode(span, getInnermostNodeForTie: true)) is not { } target)
            return [];

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        // A target framework that predates the overload would only get code that doesn't compile
        if (StringCreateHelper.GetStringCreate(semanticModel.Compilation) is not { } create)
            return [];

        if (target is InvocationExpressionSyntax invocation)
            return Unwrap(invocation, semanticModel, create, cancellationToken);

        // The caret may be sitting in an interpolation that is already wrapped, where the only thing to offer is the way back
        return target.Parent is ArgumentSyntax { Parent: ArgumentListSyntax { Parent: InvocationExpressionSyntax enclosing } }
            ? Unwrap(enclosing, semanticModel, create, cancellationToken)
            : [];
    }

    /// <summary>
    /// Walks out of a hole or an argument list so the caret only has to sit somewhere inside the interpolation, but never past the expression it sits in.
    /// </summary>
    private static ExpressionSyntax FindTarget(SyntaxNode node)
    {
        for (var current = node; current is not null; current = current.Parent)
            switch (current)
            {
                case InterpolatedStringExpressionSyntax or InvocationExpressionSyntax:
                    return (ExpressionSyntax)current;
                case StatementSyntax or MemberDeclarationSyntax or AnonymousFunctionExpressionSyntax:
                    return null;
            }
        return null;
    }

    private static ImmutableArray<CodeActionInfo> Unwrap(InvocationExpressionSyntax invocation, SemanticModel semanticModel, IMethodSymbol create, CancellationToken cancellationToken)
    {
        // Only the shape this refactoring emits round-trips: a non-null provider formats with a culture the bare interpolation would silently drop
        var arguments = invocation.ArgumentList.Arguments;
        if (arguments.Count != 3
            || arguments[0] is not { NameColon: null, Expression: LiteralExpressionSyntax { RawKind: (int)SyntaxKind.NullLiteralExpression } }
            || arguments[2] is not { NameColon: null, Expression: InterpolatedStringExpressionSyntax handler })
            return [];

        if (semanticModel.GetOperation(invocation, cancellationToken) is not IInvocationOperation operation
            || !SymbolEqualityComparer.Default.Equals(operation.TargetMethod, create))
            return [];

        return [new CodeActionInfo("Change to interpolated string", editor =>
        {
            editor.ReplaceNode(invocation, handler.WithTriviaFrom(invocation).Formatted);
            return ValueTask.CompletedTask;
        }, "ChangeToInterpolatedString")];
    }
}
