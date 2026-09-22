namespace LaquaiLib.Analyzers.Fixes.Fixes;

/// <summary>
/// Adds the <c>[DoesNotReturn]</c> LAQ1002 reports missing: a plain attribute list on a method or explicit accessor, or a <c>[method: DoesNotReturn]</c>-targeted one on an expression-bodied property/indexer, which has no accessor of its own to attribute directly.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MissingDoesNotReturnFixer)), Shared]
public sealed class MissingDoesNotReturnFixer() : LaquaiLibNodeFixer(["LAQ1002"])
{
    public override ImmutableArray<CodeActionInfo> GetCodeActionInfos(CompilationUnitSyntax compilationUnitSyntax, SyntaxNode syntaxNode, Diagnostic diagnostic)
    {
        if (syntaxNode is not (MethodDeclarationSyntax or AccessorDeclarationSyntax or PropertyDeclarationSyntax or IndexerDeclarationSyntax))
            return [];

        return [new CodeActionInfo("Add [DoesNotReturn]", editor => ApplyAsync(editor, syntaxNode), "AddDoesNotReturn", WellKnownPostFixActions.AddUsings("System.Diagnostics.CodeAnalysis"))];
    }

    private static ValueTask ApplyAsync(DocumentEditor editor, SyntaxNode declaration)
    {
        editor.ReplaceNode(declaration, WithDoesNotReturn(declaration));
        return ValueTask.CompletedTask;
    }

    private static SyntaxNode WithDoesNotReturn(SyntaxNode declaration)
    {
        var attribute = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("DoesNotReturn"))));
        return declaration switch
        {
            MethodDeclarationSyntax method => method.WithAttributeLists(method.AttributeLists.Insert(0, attribute)).Formatted,
            AccessorDeclarationSyntax accessor => accessor.WithAttributeLists(accessor.AttributeLists.Insert(0, attribute)).Formatted,
            // [DoesNotReturn] only ever targets a method (CS0657 on 'property'/'method:' target specifiers alike), so an
            // expression-bodied property/indexer has no accessor of its own to put it on; expand the arrow body into an
            // explicit 'get' accessor that carries the attribute instead
            PropertyDeclarationSyntax { ExpressionBody: { } body } property => property
                .WithExpressionBody(null)
                .WithSemicolonToken(default)
                .WithAccessorList(BuildGetAccessorList(attribute, body))
                .Formatted,
            IndexerDeclarationSyntax { ExpressionBody: { } body } indexer => indexer
                .WithExpressionBody(null)
                .WithSemicolonToken(default)
                .WithAccessorList(BuildGetAccessorList(attribute, body))
                .Formatted,
            _ => declaration
        };
    }

    private static AccessorListSyntax BuildGetAccessorList(AttributeListSyntax attribute, ArrowExpressionClauseSyntax body)
        => SyntaxFactory.AccessorList(SyntaxFactory.SingletonList(
            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithAttributeLists(SyntaxFactory.SingletonList(attribute))
                .WithExpressionBody(body)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))));
}
