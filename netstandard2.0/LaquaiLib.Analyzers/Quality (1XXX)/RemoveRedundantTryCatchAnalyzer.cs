namespace LaquaiLib.Analyzers.Quality__1XXX_;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RemoveRedundantTryCatchAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor RedundantTry { get; } = new(
        id: "LAQ1001",
        title: "Remove redundant try statement",
        messageFormat: "Remove redundant try statement",
        description: "All present clauses of this try statement have been proven to be redundant.",
        category: AnalyzerCategories.CodeQuality,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );
    public static DiagnosticDescriptor RedundantCatch { get; } = new(
        id: "LAQ1001",
        title: "Remove redundant catch block",
        messageFormat: "Remove redundant catch block",
        description: "This catch clause's only statement is a rethrow of the exception it catches.",
        category: AnalyzerCategories.CodeQuality,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );
    public static DiagnosticDescriptor RedundantFinally { get; } = new(
        id: "LAQ1001",
        title: "Remove redundant finally block",
        messageFormat: "Remove redundant finally block",
        description: "This finally clause is empty or throws an exception (the latter is discouraged).",
        category: AnalyzerCategories.CodeQuality,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [RedundantTry, RedundantCatch, RedundantFinally];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.TryStatement);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var tryStatementSyntax = (TryStatementSyntax)context.Node;
        var tryDiagnostic = Diagnostic.Create(RedundantTry, tryStatementSyntax.TryKeyword.GetLocation());

        Diagnostic finallyDiagnostic = null;
        if (tryStatementSyntax.Finally is FinallyClauseSyntax finallyClauseSyntax
            && finallyClauseSyntax.Block.Statements is var statements
            && statements.All(s => s is ThrowStatementSyntax))
        {
            finallyDiagnostic = Diagnostic.Create(RedundantFinally, finallyClauseSyntax.FinallyKeyword.GetLocation());
        }

        var catches = tryStatementSyntax.Catches;
        if (catches.Count != 0)
        {
            var uselessCatches = catches.Where(c => c.Block.Statements is var statements && statements.Count > 0 && statements.All(ThrowIsRethrow)).ToArray();
            if (uselessCatches.Length == catches.Count) // All catches useless
            {
                if (finallyDiagnostic is not null) // ...and finally is redundant, so the entire try is redundant
                {
                    context.ReportDiagnostic(tryDiagnostic);
                    return;
                }
                else // ...but finally is not redundant, so report all the catches but keep the try-finally
                {
                    for (var i = 0; i < uselessCatches.Length; i++)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(RedundantCatch, uselessCatches[i].CatchKeyword.GetLocation()));
                    }
                }
            }
            else if (uselessCatches.Length > 0) // Only some catches are useless, so report individually
            {
                for (var i = 0; i < uselessCatches.Length; i++)
                {
                    context.ReportDiagnostic(Diagnostic.Create(RedundantCatch, uselessCatches[i].CatchKeyword.GetLocation()));
                }

                if (finallyDiagnostic is not null)
                {
                    context.ReportDiagnostic(finallyDiagnostic);
                }
            }
            else if (finallyDiagnostic is not null) // Only the finally is redundant, so report that
            {
                context.ReportDiagnostic(finallyDiagnostic);
            }
        }
        else if (finallyDiagnostic is not null) // No catches and redundant finally means the entire try statement is redundant
        {
            context.ReportDiagnostic(tryDiagnostic);
        }
    }

    private static bool ThrowIsRethrow(StatementSyntax statementSyntax)
    {
        var enclosingCatch = statementSyntax.FirstAncestorOrSelf<CatchClauseSyntax>();
        if (enclosingCatch is null)
        {
            // panic?
            return false;
        }

        var decl = enclosingCatch.Declaration?.ChildTokens().FirstOrDefault(t => t.IsKind(SyntaxKind.IdentifierToken));
        return statementSyntax is ThrowStatementSyntax tse && (tse.Expression is null || (tse.Expression is IdentifierNameSyntax ins && ins.Identifier.Text == decl?.Text));
    }
}
