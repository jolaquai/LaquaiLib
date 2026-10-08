namespace LaquaiLib.Analyzers.Fixes.Fixes;

/// <summary>
/// Reorders the attribute lists LAQ1001 reports on so every attribute across them is alphabetized by type name.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AttributeOrderFixer)), Shared]
public sealed class AttributeOrderFixer() : LaquaiLibNodeFixer(["LAQ1001"])
{
    public override ImmutableArray<CodeActionInfo> GetCodeActionInfos(CompilationUnitSyntax compilationUnitSyntax, SyntaxNode syntaxNode, Diagnostic diagnostic)
    {
        // A group of exactly one (necessarily multi-attribute) list resolves to that list itself; a group of several resolves to their common parent
        var declaration = syntaxNode is AttributeListSyntax list ? list.Parent : syntaxNode;
        if (declaration is null)
            return [];

        var groups = AttributeOrderHelper.GetTargetGroups(declaration);
        for (var i = 0; i < groups.Length; i++)
        {
            var group = groups[i];
            if (AttributeOrderHelper.GetSpan(group) != diagnostic.Location.SourceSpan)
                continue;

            return [new CodeActionInfo("Order attributes alphabetically", editor => ApplyAsync(editor, group), "OrderAttributesAlphabetically")];
        }
        return [];
    }

    private static ValueTask ApplyAsync(DocumentEditor editor, ImmutableArray<AttributeListSyntax> group)
    {
        var ordered = AttributeOrderHelper.BuildOrdered(group);

        // Insert the new lists right after the old block, then remove the old block; inserting first keeps the anchor node alive for InsertAfter to resolve against
        editor.InsertAfter(group[group.Length - 1], ordered.Cast<SyntaxNode>());
        for (var i = 0; i < group.Length; i++)
            editor.RemoveNode(group[i], SyntaxRemoveOptions.KeepNoTrivia);

        return ValueTask.CompletedTask;
    }
}
