using LaquaiLib.Analyzers.Fixes.Performance;

namespace LaquaiLib.Analyzers.Fixes.Refactorings;

[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(ChangeToUnsafeAsRefactor)), Shared]
public sealed class ChangeToUnsafeAsRefactor : LaquaiLibOperationRefactoring
{
    public override ImmutableArray<CodeActionInfo> GetCodeActionInfos(CompilationUnitSyntax compilationUnitSyntax, IOperation operation, TextSpan span)
    {
        if (operation is not IConversionOperation convOp)
            return [];
        var conv = convOp.GetConversion();
        // conv.IsImplicit covers all upcasts (class hierarchy AND interface) - IsBaseOf only walked BaseType chain
        if (!conv.IsReference || conv.IsImplicit)
            return [];

        var syntaxNode = operation.Syntax;
        var (flowControl, value) = AvoidCastAfterCloneAnalyzerFix.AddForSyntaxNode(compilationUnitSyntax, syntaxNode);
        if (!flowControl)
            return [value];

        return [];
    }
    // Explicitly disallow refactor-all for this since it's dangerous and irresponsible to blindly change all casts to Unsafe.As
    public override RefactorAllProvider GetRefactorAllProvider() => null;
}
