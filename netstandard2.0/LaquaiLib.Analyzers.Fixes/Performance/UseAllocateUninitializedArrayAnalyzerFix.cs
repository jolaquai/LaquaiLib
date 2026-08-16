namespace LaquaiLib.Analyzers.Fixes.Performance;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseAllocateUninitializedArrayAnalyzerFix)), Shared]
public class UseAllocateUninitializedArrayAnalyzerFix() : LaquaiLibTokenFixer("LAQ0006")
{
    public override ImmutableArray<CodeActionInfo> GetCodeActionInfos(CompilationUnitSyntax compilationUnitSyntax, SyntaxToken syntaxToken, Diagnostic diagnostic)
    {
        // Defensive only - the analyzer already gates all of this, but a fixer can be handed a diagnostic computed against a stale document
        if (!syntaxToken.IsKind(SyntaxKind.NewKeyword)
            || syntaxToken.Parent is not ArrayCreationExpressionSyntax { Initializer: null } arrayCreation
            || arrayCreation.Type.RankSpecifiers.Count != 1
            || arrayCreation.Type.RankSpecifiers[0].Sizes.Count != 1)
        {
            return [];
        }

        var size = arrayCreation.Type.RankSpecifiers[0].Sizes[0];
        if (size is OmittedArraySizeExpressionSyntax)
        {
            return [];
        }

        return [new CodeActionInfo("Use GC.AllocateUninitializedArray", editor => ReplaceWithAllocateUninitializedArrayAsync(editor, arrayCreation, arrayCreation.Type.ElementType, size), "UseAllocateUninitializedArray")];
    }

    private static ValueTask ReplaceWithAllocateUninitializedArrayAsync(DocumentEditor editor, ArrayCreationExpressionSyntax arrayCreation, TypeSyntax elementType, ExpressionSyntax size)
    {
        var genericName = SyntaxFactory.GenericName(
            SyntaxFactory.Identifier("AllocateUninitializedArray"),
            SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList(elementType.WithoutTrivia()))
        );
        // Fully qualified so the Simplifier can shorten it to `GC` only where `System` is actually in scope
        var memberAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ParseName("System.GC"), genericName);
        var argumentList = SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(size.WithoutTrivia())));
        var invocation = SyntaxFactory.InvocationExpression(memberAccess, argumentList);

        editor.ReplaceNode(arrayCreation, invocation.WithTriviaFrom(arrayCreation).Formatted);
        return ValueTask.CompletedTask;
    }
}
