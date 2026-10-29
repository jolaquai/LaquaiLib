namespace LaquaiLib.Analyzers.Fixes.Fixes;

/// <summary>
/// Rewrites the concatenation LAQ0007 reports on into whichever of a merged literal, a <c>string.Concat</c> call or an interpolated string the shared classification picked for it.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(StringConcatenationFixer)), Shared]
public sealed class StringConcatenationFixer() : LaquaiLibFixer(["LAQ0007"])
{
    public override async ValueTask<ImmutableArray<CodeActionInfo>> GetCodeActionInfosAsync(Document document, CompilationUnitSyntax compilationUnitSyntax, Diagnostic diagnostic, CancellationToken cancellationToken)
    {
        if (compilationUnitSyntax.FindNode(diagnostic.Location.SourceSpan) is not ExpressionSyntax node)
            return [];

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        var overloads = ConcatOverloads.Create(semanticModel.Compilation);

        var rewrite = StringConcatenationHelper.Classify(node, semanticModel, overloads, cancellationToken, out var parts);
        if (rewrite is ConcatRewrite.None)
            return [];

        var replacement = StringConcatenationHelper.Build(node, rewrite, parts);
        return [new CodeActionInfo(StringConcatenationHelper.GetTitle(rewrite), editor =>
        {
            editor.ReplaceNode(node, replacement.Formatted);
            return ValueTask.CompletedTask;
        }, rewrite.ToString())];
    }
}
