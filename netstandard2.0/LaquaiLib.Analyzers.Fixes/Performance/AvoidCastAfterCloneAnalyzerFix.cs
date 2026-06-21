using Microsoft.CodeAnalysis.Editing;

namespace LaquaiLib.Analyzers.Fixes.Performance;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AvoidCastAfterCloneAnalyzerFix)), Shared]
public class AvoidCastAfterCloneAnalyzerFix() : LaquaiLibNodeFixer("LAQ0002")
{
    public override ImmutableArray<CodeActionInfo> GetCodeActionInfos(CompilationUnitSyntax compilationUnitSyntax, SyntaxNode syntaxNode, Diagnostic diagnostic)
    {
        var (flowControl, value) = AddForSyntaxNode(compilationUnitSyntax, syntaxNode);
        if (!flowControl)
            return [value];

        return [];
    }

    internal static (bool flowControl, CodeActionInfo value) AddForSyntaxNode(CompilationUnitSyntax compilationUnitSyntax, SyntaxNode syntaxNode)
    {
        if (syntaxNode is CastExpressionSyntax castExpression)
        {
            return (flowControl: false, value: new CodeActionInfo("Use Unsafe.As", editor => ReplaceWithUnsafeAsAsync(compilationUnitSyntax, editor, syntaxNode), "UseUnsafeAs_CastExpressionSyntax", WellKnownPostFixActions.AddUsings("System.Runtime.CompilerServices")));
        }
        else if (syntaxNode is BinaryExpressionSyntax binaryExpr && binaryExpr.IsKind(SyntaxKind.AsExpression))
        {
            return (flowControl: false, value: new CodeActionInfo("Use Unsafe.As", editor => ReplaceWithUnsafeAsAsync(compilationUnitSyntax, editor, syntaxNode), "UseUnsafeAs_AsExpression", WellKnownPostFixActions.AddUsings("System.Runtime.CompilerServices")));
        }

        return (flowControl: true, value: default);
    }

    internal static ValueTask ReplaceWithUnsafeAsAsync(CompilationUnitSyntax compilationUnitSyntax, DocumentEditor documentEditor, SyntaxNode expression)
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
            var argumentList = SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(replaceTarget)));
            var newExpression = SyntaxFactory.InvocationExpression(memberAccess, argumentList).WithAdditionalAnnotations(Formatter.Annotation);

            documentEditor.ReplaceNode(expression, newExpression.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation));
        }

        return ValueTask.CompletedTask;
    }
}