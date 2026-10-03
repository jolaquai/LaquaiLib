namespace LaquaiLib.Analyzers.Fixes.Fixes;

/// <summary>
/// Wraps the interpolated string LAQ0009 reports on in <c>string.Create(null, stackalloc char[N], ...)</c>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InterpolatedStringCreateFixer)), Shared]
public sealed class InterpolatedStringCreateFixer() : LaquaiLibNodeFixer(["LAQ0009"])
{
    public override FixAllProvider GetFixAllProvider() => null;

    public override ImmutableArray<CodeActionInfo> GetCodeActionInfos(CompilationUnitSyntax compilationUnitSyntax, SyntaxNode syntaxNode, Diagnostic diagnostic)
    {
        if (syntaxNode is not InterpolatedStringExpressionSyntax interpolated
            || !diagnostic.Properties.TryGetValue(StringCreateHelper.BufferLengthKey, out var lengthText)
            || !int.TryParse(lengthText, out var length))
            return [];

        return [new CodeActionInfo($"Change to 'string.Create' over a {length}-char stack buffer", editor =>
        {
            editor.ReplaceNode(interpolated, StringCreateHelper.Build(interpolated, length).Formatted);
            return ValueTask.CompletedTask;
        }, "ChangeToStringCreate")];
    }
}
