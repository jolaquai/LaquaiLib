using System.Collections.Immutable;
using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;

namespace LaquaiLib.Analyzers;

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
                    title: $"Change to Unsafe.As call",
                    createChangedDocument: c => ReplaceWithUnsafeAsAsync(context.Document, castExpression, c),
                    equivalenceKey: "ChangeToUnsafeAs"),
                diagnostic);
        }
    }
    private async Task<Document> ReplaceWithUnsafeAsAsync(Document document, ExpressionSyntax expression, CancellationToken cancellationToken)
    {
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (expression is not CastExpressionSyntax castExpression)
        {
            return document;
        }

        var targetType = castExpression.Type;
        var cloneExpression = castExpression.Expression;

        // Create Unsafe.As<T>(obj) expression
        var unsafeNamespace = SyntaxFactory.ParseName("System.Runtime.CompilerServices.Unsafe").WithAdditionalAnnotations(Simplifier.Annotation);
        var genericName = SyntaxFactory.GenericName(SyntaxFactory.Identifier("As"), SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList(targetType)));
        var memberAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, unsafeNamespace, genericName);
        var argumentList = SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(cloneExpression)));
        var newExpression = SyntaxFactory.InvocationExpression(memberAccess, argumentList).WithAdditionalAnnotations(Formatter.Annotation);

        var newRoot = root.ReplaceNode(castExpression, newExpression);

        return document.WithSyntaxRoot(newRoot);
    }
}
