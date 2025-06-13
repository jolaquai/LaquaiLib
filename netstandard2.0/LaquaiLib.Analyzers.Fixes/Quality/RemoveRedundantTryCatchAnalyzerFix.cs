namespace LaquaiLib.Analyzers.Fixes.Quality;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveRedundantTryCatchAnalyzerFix)), Shared]
public class RemoveRedundantTryCatchAnalyzerFix() : LaquaiLibTokenFixer("LAQ1001")
{
    public override FixInfo GetFixInfo(CompilationUnitSyntax compilationUnitSyntax, SyntaxToken syntaxToken, Diagnostic diagnostic)
    {
        switch (syntaxToken.Kind())
        {
            case SyntaxKind.TryKeyword when syntaxToken.Parent is TryStatementSyntax tryStatement:
            {
                return new FixInfo("Remove try statement", async editor =>
                {
                    editor.InsertAfter(tryStatement, tryStatement.Block.Statements.Select(n => n.WithAdditionalAnnotations(Formatter.Annotation)));
                    editor.RemoveNode(tryStatement);
                });
            }
            case SyntaxKind.CatchKeyword when syntaxToken.Parent is CatchClauseSyntax catchClause:
            {
                return new FixInfo("Remove catch clause", async editor => editor.RemoveNode(catchClause, SyntaxRemoveOptions.KeepNoTrivia));
            }
            case SyntaxKind.FinallyKeyword when syntaxToken.Parent is FinallyClauseSyntax finallyClause:
            {
                return new FixInfo("Remove finally clause", async editor => editor.RemoveNode(finallyClause, SyntaxRemoveOptions.KeepNoTrivia));
            }
        }

        return FixInfo.Empty;
    }
}
