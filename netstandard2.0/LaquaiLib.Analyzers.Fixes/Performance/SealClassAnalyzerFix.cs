namespace LaquaiLib.Analyzers.Fixes.Performance;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SealClassAnalyzerFix)), Shared]
public class SealClassAnalyzerFix() : LaquaiLibNodeFixer("LAQ0005")
{
    public override FixInfo GetFixInfo(CompilationUnitSyntax compilationUnitSyntax, SyntaxNode syntaxNode, Diagnostic diagnostic)
    {
        var classDeclarationSyntax = Unsafe.As<ClassDeclarationSyntax>(syntaxNode);
        return new FixInfo("Seal class", editor =>
        {
            editor.ReplaceNode(classDeclarationSyntax, classDeclarationSyntax.AddModifiers(SyntaxFactory.Token(SyntaxKind.SealedKeyword)).Formatted);
            return ValueTask.CompletedTask;
        });
    }
}