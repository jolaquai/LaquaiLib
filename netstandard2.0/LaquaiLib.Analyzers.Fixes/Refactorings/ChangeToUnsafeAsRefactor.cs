namespace LaquaiLib.Analyzers.Fixes.Refactorings;

/// <summary>
/// Changes an explicit reference cast or <see langword="as"/> expression to an <see cref="System.Runtime.CompilerServices.Unsafe.As{TFrom, TTo}(ref TFrom)"/> call, skipping the runtime type check.
/// LAQ0002 only reports casts immediately wrapping a known-sane clone call; this is offered for any explicit reference conversion, since whether the checked cast is worth keeping is the caller's to know.
/// No "Refactor All" anchor is declared: blindly changing every cast in a scope to Unsafe.As is dangerous and irresponsible to do without looking at each site.
/// </summary>
[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(ChangeToUnsafeAsRefactor)), Shared]
public sealed class ChangeToUnsafeAsRefactor : LaquaiLibOperationRefactoring
{
    public override ImmutableArray<CodeActionInfo> GetCodeActionInfos(CompilationUnitSyntax compilationUnitSyntax, IOperation operation, TextSpan span)
    {
        if (operation is not IConversionOperation convOp)
        {
            return [];
        }
        var conv = convOp.GetConversion();
        // conv.IsImplicit covers all upcasts (class hierarchy AND interface) - IsBaseOf only walked BaseType chain
        if (!conv.IsReference || conv.IsImplicit)
        {
            return [];
        }

        var syntaxNode = operation.Syntax;
        if (syntaxNode is CastExpressionSyntax)
        {
            return [new CodeActionInfo("Change to Unsafe.As call", editor => ReplaceWithUnsafeAsAsync(editor, syntaxNode), "ChangeToUnsafeAsCall_CastExpressionSyntax", WellKnownPostFixActions.AddUsings("System.Runtime.CompilerServices"))];
        }
        if (syntaxNode is BinaryExpressionSyntax binaryExpr && binaryExpr.IsKind(SyntaxKind.AsExpression))
        {
            return [new CodeActionInfo("Change to Unsafe.As call", editor => ReplaceWithUnsafeAsAsync(editor, syntaxNode), "ChangeToUnsafeAsCall_AsExpression", WellKnownPostFixActions.AddUsings("System.Runtime.CompilerServices"))];
        }

        return [];
    }
    // Explicitly disallow refactor-all for this since it's dangerous and irresponsible to blindly change all casts to Unsafe.As
    public override RefactorAllProvider GetRefactorAllProvider() => null;

    private static ValueTask ReplaceWithUnsafeAsAsync(DocumentEditor documentEditor, SyntaxNode expression)
    {
        ExpressionSyntax replaceTarget = null;
        TypeSyntax targetType = null;
        if (expression is CastExpressionSyntax castExpression)
        {
            replaceTarget = castExpression.Expression;
            targetType = castExpression.Type;
        }
        else if (expression is BinaryExpressionSyntax binaryExpr && binaryExpr.IsKind(SyntaxKind.AsExpression)
            && binaryExpr.OperatorToken.IsKind(SyntaxKind.AsKeyword) && binaryExpr.Right is TypeSyntax typeSyntax)
        {
            replaceTarget = binaryExpr.Left;
            targetType = typeSyntax;
        }

        if (replaceTarget is not null && targetType is not null)
        {
            var genericNameSyntax = SyntaxFactory.GenericName(SyntaxFactory.Identifier("As"), SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList(targetType)));
            var unsafeType = SyntaxFactory.ParseName("Unsafe").WithAdditionalAnnotations(Simplifier.Annotation);
            var memberAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, unsafeType, genericNameSyntax);
            if (replaceTarget is ParenthesizedExpressionSyntax peSyntax)
            {
                replaceTarget = peSyntax.Expression;
            }
            var argumentList = SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(replaceTarget)));
            var newExpression = SyntaxFactory.InvocationExpression(memberAccess, argumentList).WithAdditionalAnnotations(Formatter.Annotation);

            documentEditor.ReplaceNode(expression, newExpression.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation));
        }

        return ValueTask.CompletedTask;
    }
}
