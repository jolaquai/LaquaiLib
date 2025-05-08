namespace LaquaiLib.Analyzers.Fixes.Quality;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveRedundantTryCatchAnalyzerFix)), Shared]
public class RemoveRedundantTryCatchAnalyzerFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ["LAQ1001"];
    public sealed override FixAllProvider GetFixAllProvider() => FixAllProvider.Create(async (fixAllContext, document, diagnostics) =>
    {
        if (diagnostics.IsEmpty)
        {
            return document;
        }

        // Get the document root
        var root = await document.GetSyntaxRootAsync(fixAllContext.CancellationToken).ConfigureAwait(false);
        if (root == null)
        {
            return document;
        }

        // Group all the fixes we need to make
        var nodesToReplace = new Dictionary<SyntaxNode, SyntaxNode>();
        var nodesToRemove = new List<SyntaxNode>();

        // Process all diagnostics at once
        foreach (var diagnostic in diagnostics)
        {
            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);

            switch (token.Kind())
            {
                case SyntaxKind.TryKeyword when token.Parent is TryStatementSyntax tryStatement:
                {
                    var replacementBlock = SyntaxFactory.Block(tryStatement.Block.Statements).WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation);
                    nodesToReplace[tryStatement] = replacementBlock;
                    break;
                }
                case SyntaxKind.CatchKeyword when token.Parent is CatchClauseSyntax catchClause:
                {
                    nodesToRemove.Add(catchClause);
                    break;
                }
                case SyntaxKind.FinallyKeyword when token.Parent is FinallyClauseSyntax finallyClause:
                {
                    nodesToRemove.Add(finallyClause);
                    break;
                }
            }
        }

        // Apply all changes at once
        var newRoot = root;

        // First replace the try statements
        if (nodesToReplace.Count > 0)
        {
            newRoot = newRoot.ReplaceNodes(nodesToReplace.Keys, (original, _) => nodesToReplace[original]);
        }
        // Then remove the catch and finally clauses
        if (nodesToRemove.Count > 0)
        {
            newRoot = newRoot.RemoveNodes(nodesToRemove, SyntaxRemoveOptions.KeepNoTrivia);
        }

        return document.WithSyntaxRoot(newRoot);
    });

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var document = context.Document;
        var diags = context.Diagnostics;
        for (var i = 0; i < diags.Length; i++)
        {
            var diagnostic = diags[i];

            var fix = await CreateCodeActionForDocumentAsync(document, diagnostic).ConfigureAwait(false);
            context.RegisterCodeFix(fix, diagnostic);
        }
    }

    internal static async Task<CodeAction> CreateCodeActionForDocumentAsync(Document document, Diagnostic diagnostic)
    {
        var root = await document.GetRootAsync().ConfigureAwait(false);
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var token = root.FindToken(diagnosticSpan.Start);

        switch (token.Kind())
        {
            case SyntaxKind.TryKeyword when token.Parent is TryStatementSyntax tryStatementSyntax:
            {
                return CodeAction.Create(
                    title: "Remove try statement",
                    createChangedDocument: c => Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(tryStatementSyntax, tryStatementSyntax.Block.Statements.Select(s => s.WithAdditionalAnnotations(Formatter.Annotation))))),
                    equivalenceKey: "Remove try statement");
            }
            case SyntaxKind.CatchKeyword when token.Parent is CatchClauseSyntax catchClauseSyntax:
            {
                return CodeAction.Create(
                    title: "Remove catch clause",
                    createChangedDocument: c => Task.FromResult(document.WithSyntaxRoot(root.RemoveNode(catchClauseSyntax, SyntaxRemoveOptions.KeepNoTrivia))),
                    equivalenceKey: "Remove catch clause");
            }
            case SyntaxKind.FinallyKeyword when token.Parent is FinallyClauseSyntax finallyClauseSyntax:
            {
                return CodeAction.Create(
                    title: "Remove finally clause",
                    createChangedDocument: c => Task.FromResult(document.WithSyntaxRoot(root.RemoveNode(finallyClauseSyntax, SyntaxRemoveOptions.KeepNoTrivia))),
                    equivalenceKey: "Remove finally clause");
            }
        }

        throw new InvalidOperationException($"Unexpected token type: {token.ValueText}");
    }
}
