namespace LaquaiLib.Analyzers.Fixes.Refactorings;

/// <summary>
/// Reverses the built-in "simple using" refactoring: rewrites <c>using var x = ...;</c> back into <c>using (var x = ...) { ... }</c>.
/// The generated block's contents are exactly the statements that followed the declaration in its enclosing block, so the traditional form's explicit scope matches the implicit one it replaces.
/// A second action is offered when the declaration sits in a run of consecutive using declarations: it collapses the whole run into one stacked <c>using (a) using (b) { ... }</c> nest sharing a single trailing block, mirroring how such runs are conventionally written by hand.
/// </summary>
[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(UseTraditionalUsingRefactor)), Shared]
public sealed class UseTraditionalUsingRefactor : LaquaiLibNodeRefactoring
{
    public override ImmutableArray<CodeActionInfo> GetCodeActionInfos(CompilationUnitSyntax compilationUnitSyntax, SyntaxNode syntaxNode, TextSpan span)
    {
        if (FindTarget(syntaxNode) is not { Parent: BlockSyntax block } localDeclaration)
            return [];

        var statements = block.Statements;
        var index = statements.IndexOf(localDeclaration);

        var runStart = index;
        while (runStart > 0 && statements[runStart - 1] is LocalDeclarationStatementSyntax previous && IsUsingDeclaration(previous))
            runStart--;
        var runEnd = index;
        while (runEnd + 1 < statements.Count && statements[runEnd + 1] is LocalDeclarationStatementSyntax next && IsUsingDeclaration(next))
            runEnd++;

        var infos = ImmutableArray.CreateBuilder<CodeActionInfo>();
        infos.Add(new CodeActionInfo("Change to traditional 'using' statement", editor => ConvertAsync(editor, block, index, index), "ChangeToTraditionalUsing"));
        if (runEnd > runStart)
            infos.Add(new CodeActionInfo("Change consecutive 'using' declarations to a stacked traditional 'using' statement", editor => ConvertAsync(editor, block, runStart, runEnd), "ChangeToStackedTraditionalUsing"));
        return infos.ToImmutable();
    }

    /// <summary>
    /// Walks outward from the node at the caret, accepting a using declaration statement, but never looking past the innermost enclosing statement.
    /// </summary>
    private static LocalDeclarationStatementSyntax FindTarget(SyntaxNode node)
    {
        for (var current = node; current is not null; current = current.Parent)
        {
            if (current is LocalDeclarationStatementSyntax local)
                return IsUsingDeclaration(local) ? local : null;
            if (current is StatementSyntax or MemberDeclarationSyntax or AnonymousFunctionExpressionSyntax)
                return null;
        }
        return null;
    }

    private static bool IsUsingDeclaration(LocalDeclarationStatementSyntax localDeclaration)
        => localDeclaration.UsingKeyword.IsKind(SyntaxKind.UsingKeyword) && localDeclaration.Modifiers.Count == 0;

    /// <summary>
    /// Wraps <paramref name="block"/>'s statements from <paramref name="start"/> to <paramref name="end"/> (inclusive), each a using declaration, into a nest of traditional
    /// <see langword="using"/> statements, innermost first, sharing a single trailing block that holds everything that came after <paramref name="end"/>.
    /// </summary>
    private static ValueTask ConvertAsync(DocumentEditor editor, BlockSyntax block, int start, int end)
    {
        var statements = block.Statements;

        StatementSyntax current = SyntaxFactory.Block(statements.Skip(end + 1));
        for (var i = end; i >= start; i--)
        {
            var declaration = (LocalDeclarationStatementSyntax)statements[i];
            var leading = declaration.GetLeadingTrivia();
            // Every nesting level but the outermost becomes another using statement's embedded 'Statement' - the formatter only breaks that onto its own line if trivia already asks it to
            if (i > start)
                leading = leading.Insert(0, SyntaxFactory.ElasticEndOfLine("\n"));
            current = SyntaxFactory.UsingStatement(
                declaration.AttributeLists,
                declaration.AwaitKeyword,
                SyntaxFactory.Token(SyntaxKind.UsingKeyword).WithTrailingTrivia(SyntaxFactory.Space),
                SyntaxFactory.Token(SyntaxKind.OpenParenToken),
                declaration.Declaration.WithoutTrivia(),
                null,
                SyntaxFactory.Token(SyntaxKind.CloseParenToken).WithTrailingTrivia(SyntaxFactory.Space),
                current
            ).WithLeadingTrivia(leading);
        }

        var before = statements.Take(start);
        var newBlock = block.WithStatements(SyntaxFactory.List(before.Append(current)));

        editor.ReplaceNode(block, newBlock.Formatted);
        return ValueTask.CompletedTask;
    }
}
