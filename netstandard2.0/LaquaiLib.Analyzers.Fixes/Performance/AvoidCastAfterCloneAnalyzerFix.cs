using Microsoft.CodeAnalysis.Editing;

namespace LaquaiLib.Analyzers.Fixes.Performance;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AvoidCastAfterCloneAnalyzerFix)), Shared]
public class AvoidCastAfterCloneAnalyzerFix() : LaquaiLibNodeFixer("LAQ0002")
{
    public override FixInfo GetFixInfo(CompilationUnitSyntax compilationUnitSyntax, SyntaxNode syntaxNode, Diagnostic diagnostic)
    {
        if (syntaxNode is CastExpressionSyntax castExpression)
        {
            return new FixInfo("Use Unsafe.As", editor => ReplaceWithUnsafeAsAsync(compilationUnitSyntax, editor, syntaxNode), "UseUnsafeAs_CastExpressionSyntax");
        }
        else if (syntaxNode is BinaryExpressionSyntax binaryExpr && binaryExpr.IsKind(SyntaxKind.AsExpression))
        {
            return new FixInfo("Use Unsafe.As", editor => ReplaceWithUnsafeAsAsync(compilationUnitSyntax, editor, syntaxNode), "UseUnsafeAs_AsExpression");
        }

        return FixInfo.Empty;
    }
    private ValueTask ReplaceWithUnsafeAsAsync(CompilationUnitSyntax compilationUnitSyntax, DocumentEditor documentEditor, SyntaxNode expression)
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
            var unsafeType = SyntaxFactory.ParseName("System.Runtime.CompilerServices.Unsafe").WithAdditionalAnnotations(Simplifier.Annotation);
            var memberAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, unsafeType, genericNameSyntax);
            var argumentList = SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(replaceTarget)));
            var newExpression = SyntaxFactory.InvocationExpression(memberAccess, argumentList).WithAdditionalAnnotations(Formatter.Annotation);

            documentEditor.ReplaceNode(expression, newExpression.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation));
        }

        PostFixAction += async d =>
        {
            var root = await d.Root.ConfigureAwait(false);
            var newRoot = root.AddUsingsIfNotExists(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Runtime.CompilerServices")));
            return d.WithSyntaxRoot(newRoot);
        };

        return ValueTask.CompletedTask;
    }
}