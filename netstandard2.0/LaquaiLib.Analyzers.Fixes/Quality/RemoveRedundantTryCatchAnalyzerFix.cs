using Microsoft.CodeAnalysis.Editing;

namespace LaquaiLib.Analyzers.Fixes.Quality;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveRedundantTryCatchAnalyzerFix)), Shared]
public class RemoveRedundantTryCatchAnalyzerFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ["LAQ1001"];

    // Function to get fix info based on diagnostic
    private static (string title, Action<DocumentEditor> fixAction) GetFixInfo(SyntaxNode root, Diagnostic diagnostic)
    {
        var token = root.FindToken(diagnostic.Location.SourceSpan.Start);

        switch (token.Kind())
        {
            case SyntaxKind.TryKeyword when token.Parent is TryStatementSyntax tryStatement:
            {
                return
                (
                    "Remove try statement",
                    editor =>
                    {
                        var replacementBlock = tryStatement.Block.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation);
                        editor.ReplaceNode(tryStatement, replacementBlock);
                    }
                );
            }
            case SyntaxKind.CatchKeyword when token.Parent is CatchClauseSyntax catchClause:
            {
                return
                (
                    "Remove catch clause",
                    editor => editor.RemoveNode(catchClause, SyntaxRemoveOptions.KeepNoTrivia)
                );
            }
            case SyntaxKind.FinallyKeyword when token.Parent is FinallyClauseSyntax finallyClause:
            {
                return
                (
                    "Remove finally clause",
                    editor => editor.RemoveNode(finallyClause, SyntaxRemoveOptions.KeepNoTrivia)
                );
            }
            default:
                return ("Fix redundant try-catch element", _ => { });
        }
    }

    // For single fixes
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var document = context.Document;
        var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        foreach (var diagnostic in context.Diagnostics)
        {
            var (title, fixAction) = GetFixInfo(root, diagnostic);

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: async c =>
                    {
                        var editor = await DocumentEditor.CreateAsync(document, c);
                        fixAction(editor);
                        return editor.GetChangedDocument();
                    },
                    // Use different equivalence keys based on the fix type
                    equivalenceKey: $"RemoveRedundantTryCatch_{title.Replace(" ", "")}"),
                diagnostic);
        }
    }

    // For batch fixes - unified action with a generic title
    public sealed override FixAllProvider GetFixAllProvider() => FixAllProvider.Create(
        async (fixAllContext, document, diagnostics) =>
        {
            if (diagnostics.IsEmpty)
                return document;

            var root = await document.GetSyntaxRootAsync(fixAllContext.CancellationToken).ConfigureAwait(false);
            var editor = await DocumentEditor.CreateAsync(document, fixAllContext.CancellationToken);

            foreach (var diagnostic in diagnostics)
            {
                var (_, fixAction) = GetFixInfo(root, diagnostic);
                fixAction(editor);
            }

            return editor.GetChangedDocument();
        });
}

/// <summary>
/// Provides a base class for all code fix providers in the LaquaiLib library with some shared functionality for implementing single and batch fixes in a more streamlined way.
/// </summary>
public abstract class LaquaiLibCodeFixProvider : CodeFixProvider
{
#warning TODO
}