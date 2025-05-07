namespace LaquaiLib.Analyzers.Fixes.Performance;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AvoidCastAfterCloneAnalyzerFix)), Shared]
public class AvoidCastAfterCloneAnalyzerFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ["LAQ0002"];
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var node = root.FindNode(diagnosticSpan);

        if (node is CastExpressionSyntax castExpression)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Use Unsafe.As",
                    createChangedDocument: c => ReplaceWithUnsafeAsAsync(context.Document, castExpression, c),
                    equivalenceKey: "UseUnsafeAs"),
                diagnostic);
        }
        else if (node is BinaryExpressionSyntax binaryExpr && binaryExpr.IsKind(SyntaxKind.AsExpression))
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Use Unsafe.As",
                    createChangedDocument: c => ReplaceWithUnsafeAsAsync(context.Document, binaryExpr, c),
                    equivalenceKey: "UseUnsafeAs"),
                diagnostic);
        }
    }
    private static async Task<Document> ReplaceWithUnsafeAsAsync(Document document, ExpressionSyntax expression, CancellationToken cancellationToken)
    {
        var root = await document.GetRootAsync(cancellationToken).ConfigureAwait(false);

        var annot = new SyntaxAnnotation();
        root = root.ReplaceNode(expression, expression.WithAdditionalAnnotations(annot));

        expression = root.GetAnnotatedNodes(annot).OfType<ExpressionSyntax>().First();
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
            // Create Unsafe.As<T>(obj) expression
            var genericNameSyntax = SyntaxFactory.GenericName(SyntaxFactory.Identifier("As"), SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList(targetType)));
            var unsafeType = SyntaxFactory.ParseName("System.Runtime.CompilerServices.Unsafe").WithAdditionalAnnotations(Simplifier.Annotation, Simplifier.AddImportsAnnotation);
            var memberAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, unsafeType, genericNameSyntax);
            var argumentList = SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(replaceTarget)));
            var newExpression = SyntaxFactory.InvocationExpression(memberAccess, argumentList).WithAdditionalAnnotations(Formatter.Annotation);

            return document.WithSyntaxRoot(root.ReplaceNode(expression, newExpression));
        }

        return document;
    }
}
