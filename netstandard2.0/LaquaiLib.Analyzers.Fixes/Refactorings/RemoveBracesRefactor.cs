namespace LaquaiLib.Analyzers.Fixes.Refactorings;

/// <summary>
/// Offers to strip the braces off a block that wraps a single statement, unless doing so would be illegal or change which construct a reader binds the contents to. Comments sitting on the braces move to where their brace was.
/// </summary>
[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(RemoveBracesRefactor)), Shared]
public sealed class RemoveBracesRefactor : LaquaiLibNodeRefactoring
{
    public override ImmutableArray<CodeActionInfo> GetCodeActionInfos(CompilationUnitSyntax compilationUnitSyntax, SyntaxNode syntaxNode, TextSpan span)
    {
        if (FindTargetBlock(syntaxNode, span) is not { } block)
        {
            return [];
        }

        return [new CodeActionInfo("Remove braces", editor =>
        {
            var statement = Unwrap(block);
            // `else { if (x) Y(); }` is an else-if chain once unwrapped, so write it as one instead of leaving the if dangling on the next line
            if (block.Parent is ElseClauseSyntax elseClause && statement is IfStatementSyntax && !HasSignificantTrivia(elseClause.ElseKeyword) && !HasSignificantTrivia(statement.GetLeadingTrivia()))
            {
                editor.ReplaceNode(elseClause, elseClause
                    .WithElseKeyword(elseClause.ElseKeyword.WithTrailingTrivia(SyntaxFactory.Space))
                    .WithStatement(statement.WithoutLeadingTrivia())
                    .WithAdditionalAnnotations(Formatter.Annotation));
                return ValueTask.CompletedTask;
            }
            editor.ReplaceNode(block, statement.WithAdditionalAnnotations(Formatter.Annotation));
            return ValueTask.CompletedTask;
        }, "RemoveBraces")];
    }

    // Document order, so an enclosing block is unwrapped before the block nested in it
    protected override ValueTask<ImmutableArray<TextSpan>> GetRefactorAllSpansAsync(Document document, CompilationUnitSyntax compilationUnitSyntax, CancellationToken cancellationToken)
    {
        var builder = ImmutableArray.CreateBuilder<TextSpan>();
        foreach (var node in compilationUnitSyntax.DescendantNodes())
        {
            if (node is BlockSyntax block && IsCollapsible(block))
            {
                builder.Add(block.Span);
            }
        }
        return new ValueTask<ImmutableArray<TextSpan>>(builder.ToImmutable());
    }

    private static StatementSyntax Unwrap(BlockSyntax block)
    {
        var statement = block.Statements[0];
        var open = block.OpenBraceToken;
        var close = block.CloseBraceToken;
        var previousTrailing = open.GetPreviousToken().TrailingTrivia;
        var atLineStart = previousTrailing.Count > 0 && previousTrailing[previousTrailing.Count - 1].IsKind(SyntaxKind.EndOfLineTrivia);

        return statement
            .WithLeadingTrivia(Splice(atLineStart, open.LeadingTrivia, open.TrailingTrivia, statement.GetLeadingTrivia()))
            .WithTrailingTrivia(Splice(false, statement.GetTrailingTrivia(), close.LeadingTrivia, close.TrailingTrivia));
    }

    /// <summary>
    /// Concatenates <paramref name="parts"/>, dropping the lines the braces used to occupy but keeping whatever else sat on them, so a comment ends up where its brace was.
    /// </summary>
    /// <param name="atLineStart">Whether the first of the <paramref name="parts"/> begins a line rather than continuing the one the previous token is on.</param>
    private static SyntaxTriviaList Splice(bool atLineStart, params SyntaxTriviaList[] parts)
    {
        var result = new List<SyntaxTrivia>();
        // Index the current line starts at, or -1 while that line still carries the code the trivia is attached to
        var lineStart = atLineStart ? 0 : -1;
        var keepLine = false;

        for (var p = 0; p < parts.Length; p++)
        {
            var part = parts[p];
            for (var i = 0; i < part.Count; i++)
            {
                var trivia = part[i];
                if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
                {
                    if (lineStart >= 0 && !keepLine)
                    {
                        result.RemoveRange(lineStart, result.Count - lineStart);
                    }
                    else
                    {
                        while (result.Count > 0 && result[result.Count - 1].IsKind(SyntaxKind.WhitespaceTrivia))
                        {
                            result.RemoveAt(result.Count - 1);
                        }
                        result.Add(trivia);
                    }
                    lineStart = result.Count;
                    keepLine = false;
                    continue;
                }
                // Two runs of whitespace only ever meet where a brace used to be
                if (trivia.IsKind(SyntaxKind.WhitespaceTrivia) && result.Count > 0 && result[result.Count - 1].IsKind(SyntaxKind.WhitespaceTrivia))
                {
                    continue;
                }
                keepLine |= !trivia.IsKind(SyntaxKind.WhitespaceTrivia);
                result.Add(trivia);
            }
        }
        return SyntaxFactory.TriviaList(result);
    }

    /// <summary>
    /// Walks outward from the node at the caret, accepting either the block the caret is in or the block owned by a construct whose header it is in. Never looks past the innermost enclosing block.
    /// </summary>
    private static BlockSyntax FindTargetBlock(SyntaxNode node, TextSpan span)
    {
        for (var current = node; current is not null; current = current.Parent)
        {
            if (current is BlockSyntax block)
            {
                return IsCollapsible(block) ? block : null;
            }
            if (GetEmbeddedStatement(current) is BlockSyntax owned && !owned.Span.Contains(span.Start) && IsCollapsible(owned))
            {
                return owned;
            }
            if (current is MemberDeclarationSyntax or AnonymousFunctionExpressionSyntax)
            {
                return null;
            }
        }
        return null;
    }

    private static bool IsCollapsible(BlockSyntax block)
    {
        // Only a block in an embedded statement position can lose its braces; method, try, switch section and free-standing scoping blocks cannot
        if (block.Parent is not { } parent || GetEmbeddedStatement(parent) != block || block.Statements.Count != 1)
        {
            return false;
        }
        var statement = block.Statements[0];
        // Not legal as an embedded statement
        if (statement is LocalDeclarationStatementSyntax or LocalFunctionStatementSyntax or LabeledStatementSyntax)
        {
            return false;
        }
        // Comments hanging off the braces are relocated, but a conditional directive cannot be: moving it changes what it guards, and the configuration that excludes the statement would be left with no statement at all
        if (HasConditionalDirective(block.OpenBraceToken) || HasConditionalDirective(block.CloseBraceToken))
        {
            return false;
        }
        // An unmatched `if` at the tail would swallow the `else` that follows the construct
        if (EndsWithUnmatchedIf(statement) && IsFollowedByElse(block))
        {
            return false;
        }
        // `if (a) if (b) X(); else Y();` is legal but the else no longer visibly belongs to anything; `else if` chains are idiomatic, so only the then-branch is refused
        if (parent is IfStatementSyntax && statement is IfStatementSyntax { Else: not null })
        {
            return false;
        }
        return true;
    }

    private static StatementSyntax GetEmbeddedStatement(SyntaxNode node) => node switch
    {
        IfStatementSyntax n => n.Statement,
        ElseClauseSyntax n => n.Statement,
        ForStatementSyntax n => n.Statement,
        CommonForEachStatementSyntax n => n.Statement,
        WhileStatementSyntax n => n.Statement,
        DoStatementSyntax n => n.Statement,
        UsingStatementSyntax n => n.Statement,
        LockStatementSyntax n => n.Statement,
        FixedStatementSyntax n => n.Statement,
        _ => null
    };

    /// <summary>
    /// Whether <paramref name="statement"/> ends in an <see langword="if"/> that has no <see langword="else"/> of its own, that is, whether it would bind an <see langword="else"/> written after it.
    /// </summary>
    private static bool EndsWithUnmatchedIf(StatementSyntax statement)
    {
        while (true)
        {
            switch (statement)
            {
                case IfStatementSyntax { Else: null }:
                    return true;
                case IfStatementSyntax { Else.Statement: var elseStatement }:
                    statement = elseStatement;
                    continue;
                // A do statement is excluded; its `while` clause terminates it
                case ForStatementSyntax or CommonForEachStatementSyntax or WhileStatementSyntax or UsingStatementSyntax or LockStatementSyntax or FixedStatementSyntax:
                    statement = GetEmbeddedStatement(statement);
                    continue;
                case LabeledStatementSyntax labeled:
                    statement = labeled.Statement;
                    continue;
                default:
                    return false;
            }
        }
    }
    /// <summary>
    /// Whether an <see langword="else"/> follows the position <paramref name="node"/> occupies, walking out through every construct that ends where <paramref name="node"/> does.
    /// </summary>
    private static bool IsFollowedByElse(SyntaxNode node)
    {
        while (true)
        {
            switch (node.Parent)
            {
                case IfStatementSyntax ifStatement when ifStatement.Statement == node:
                    if (ifStatement.Else is not null)
                    {
                        return true;
                    }
                    node = ifStatement;
                    continue;
                case ElseClauseSyntax elseClause:
                    node = elseClause.Parent;
                    continue;
                case ForStatementSyntax or CommonForEachStatementSyntax or WhileStatementSyntax or UsingStatementSyntax or LockStatementSyntax or FixedStatementSyntax or LabeledStatementSyntax:
                    node = node.Parent;
                    continue;
                default:
                    return false;
            }
        }
    }

    private static bool HasSignificantTrivia(SyntaxToken token) => HasSignificantTrivia(token.LeadingTrivia) || HasSignificantTrivia(token.TrailingTrivia);
    private static bool HasSignificantTrivia(SyntaxTriviaList trivia)
    {
        for (var i = 0; i < trivia.Count; i++)
        {
            if (!trivia[i].IsKind(SyntaxKind.WhitespaceTrivia) && !trivia[i].IsKind(SyntaxKind.EndOfLineTrivia))
            {
                return true;
            }
        }
        return false;
    }

    private static bool HasConditionalDirective(SyntaxToken token) => HasConditionalDirective(token.LeadingTrivia) || HasConditionalDirective(token.TrailingTrivia);
    private static bool HasConditionalDirective(SyntaxTriviaList trivia)
    {
        for (var i = 0; i < trivia.Count; i++)
        {
            if (trivia[i].Kind() is SyntaxKind.IfDirectiveTrivia or SyntaxKind.ElifDirectiveTrivia or SyntaxKind.ElseDirectiveTrivia or SyntaxKind.EndIfDirectiveTrivia or SyntaxKind.DisabledTextTrivia)
            {
                return true;
            }
        }
        return false;
    }
}
