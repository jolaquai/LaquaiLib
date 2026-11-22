namespace LaquaiLib.Analyzers.Fixes.Fixes;

/// <summary>
/// Renames the <c>string.Join</c> call LAQ0008 reports on to <c>string.Concat</c> and drops its proven-empty separator.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(JoinWithEmptySeparatorFixer)), Shared]
public sealed class JoinWithEmptySeparatorFixer() : LaquaiLibNodeFixer(["LAQ0008"])
{
    public override ImmutableArray<CodeActionInfo> GetCodeActionInfos(CompilationUnitSyntax compilationUnitSyntax, SyntaxNode syntaxNode, Diagnostic diagnostic)
    {
        if (syntaxNode.FirstAncestorOrSelf<InvocationExpressionSyntax>() is not InvocationExpressionSyntax invocation
            || !diagnostic.Properties.TryGetValue(JoinWithEmptySeparatorHelper.SeparatorIndexKey, out var indexText)
            || !int.TryParse(indexText, out var separatorIndex)
            || (uint)separatorIndex >= (uint)invocation.ArgumentList.Arguments.Count)
            return [];

        return [new CodeActionInfo("Use string.Concat", editor =>
        {
            editor.ReplaceNode(invocation, Rewrite(invocation, separatorIndex));
            return ValueTask.CompletedTask;
        }, "UseStringConcat")];
    }

    private static InvocationExpressionSyntax Rewrite(InvocationExpressionSyntax invocation, int separatorIndex)
    {
        var expression = invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.WithName(SyntaxFactory.IdentifierName("Concat").WithTriviaFrom(memberAccess.Name)),
            IdentifierNameSyntax identifier => SyntaxFactory.IdentifierName("Concat").WithTriviaFrom(identifier),
            var other => other
        };

        var original = invocation.ArgumentList.Arguments;
        var arguments = original.RemoveAt(separatorIndex);
        if (separatorIndex == 0 && arguments.Count > 0)
            arguments = arguments.Replace(arguments[0], arguments[0].WithLeadingTrivia(original[0].GetLeadingTrivia()));

        return invocation
            .WithExpression(expression)
            .WithArgumentList(invocation.ArgumentList.WithArguments(arguments));
    }
}
