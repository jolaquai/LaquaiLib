namespace LaquaiLib.Analyzers.Fixes.Performance;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PreferStackallocForTemporaryBuffersAnalyzerFix)), Shared]
public class PreferStackallocForTemporaryBuffersAnalyzerFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ["LAQ0003"];
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var node = root.FindNode(diagnosticSpan);

        var acExprSyntax = (ArrayCreationExpressionSyntax)node;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use 'stackalloc'",
                createChangedDocument: c => ReplaceWithStackallocAsync(context.Document, acExprSyntax, c),
                equivalenceKey: "UseStackalloc"),
            diagnostic);
    }
    private async Task<Document> ReplaceWithStackallocAsync(Document document, ArrayCreationExpressionSyntax arrayCreationExpr, CancellationToken cancellationToken)
    {
        // Debugger.Launch();
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        // 2 is technically the absolute maximum number of changes we'll need to make
        var changes = new Dictionary<SyntaxNode, SyntaxNode>(2);

        // Transform the array creation expression into a stackalloc expression
        // The only real difference between an ACE and a StackAllocACE is the expression type and the keyword, the ArrayTypeSyntax remains unchanged
        var newKeyword = arrayCreationExpr.NewKeyword;
        var stackallocKeyword = SyntaxFactory.Token(SyntaxKind.StackAllocKeyword)
            .WithLeadingTrivia(newKeyword.LeadingTrivia)
            .WithTrailingTrivia(newKeyword.TrailingTrivia);
        var arrayTypeSyntax = arrayCreationExpr.Type;
        var stackAllocArrayCreationExpr = SyntaxFactory.StackAllocArrayCreationExpression(stackallocKeyword, arrayTypeSyntax, arrayCreationExpr.Initializer);

        changes[arrayCreationExpr] = stackAllocArrayCreationExpr;

        // If we're ALSO inside a local declaration, we need to change its type to Span<T> explicitly
        if (arrayCreationExpr.FirstAncestorOrSelf<VariableDeclarationSyntax>() is VariableDeclarationSyntax variableDeclarationSyntax)
        {
            FixVariableDeclarationType(changes, arrayTypeSyntax, variableDeclarationSyntax);
        }
        // else if we're ALSO inside a variable assignment, we need to change the type of the variable to Span<T> explicitly
        else if (arrayCreationExpr.FirstAncestorOrSelf<AssignmentExpressionSyntax>() is AssignmentExpressionSyntax assignmentExpressionSyntax)
        {
            var assignmentTarget = assignmentExpressionSyntax.Left;
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var targetSymbol = semanticModel.GetSymbolInfo(assignmentTarget, cancellationToken).Symbol;

            if (targetSymbol is ILocalSymbol or IFieldSymbol)
            {
                var declaration = targetSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken);
                if (declaration is VariableDeclaratorSyntax variableDeclarator and { Parent: VariableDeclarationSyntax variableDeclaration })
                {
                    var elementType = arrayTypeSyntax.ElementType;
                    var spanType = SyntaxFactory.GenericName(SyntaxFactory.Identifier("Span"), SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList(elementType)));

                    FixVariableDeclarationType(changes, arrayTypeSyntax, variableDeclaration);
                }
            }
        }

        return document.WithSyntaxRoot(root.ReplaceNodes(changes.Keys, (node, _) => changes[node]));
    }

    private static void FixVariableDeclarationType(Dictionary<SyntaxNode, SyntaxNode> changes, ArrayTypeSyntax arrayTypeSyntax, VariableDeclarationSyntax variableDeclarationSyntax)
    {
        var identifierSyntax = variableDeclarationSyntax.ChildNodes().OfType<IdentifierNameSyntax>().Single();

        // If it's not already Span<T>, we need to change it
        // We can't touch the entire declaration, because that would undo the change from new to stackalloc above
        var elementType = arrayTypeSyntax.ElementType;
        var spanType = SyntaxFactory.GenericName(SyntaxFactory.Identifier("Span"), SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList(elementType))).WithLeadingTrivia(identifierSyntax.GetLeadingTrivia()).WithTrailingTrivia(identifierSyntax.GetTrailingTrivia());
        changes[identifierSyntax] = spanType;
    }
}
