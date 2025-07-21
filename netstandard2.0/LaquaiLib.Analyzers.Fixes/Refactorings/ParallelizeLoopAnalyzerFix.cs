using LaquaiLib.Analyzers.Fixes;

namespace LaquaiLib.Analyzers.Fixes.Refactorings;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ParallelizeLoopAnalyzerFix)), Shared]
public class ParallelizeLoopAnalyzerFix() : LaquaiLibNodeFixer("LAQ4001")
{
    public override FixInfo GetFixInfo(CompilationUnitSyntax compilationUnitSyntax, SyntaxNode syntaxToken, Diagnostic diagnostic)
    {
        // Simple differetiation between the loop types and whether we need their async variants
        switch (syntaxToken)
        {
            case ForStatementSyntax forStatementSyntax:
            {
                // We restrict ourselves to the very simple case of iterating upwards from an n to a max value
                var loopVar = forStatementSyntax.Declaration.Variables[0];
                var initializerValueExpression = loopVar.Initializer.Value;

                // Complex loop condition is not supported
                var binaryExpressionSyntax = Unsafe.As<BinaryExpressionSyntax>(forStatementSyntax.Condition);
                var end = binaryExpressionSyntax.Right;
                var endInclusive = binaryExpressionSyntax.Kind() is SyntaxKind.LessThanOrEqualExpression;
                if (endInclusive)
                {
                    end = SyntaxFactory.BinaryExpression(SyntaxKind.AddExpression, end, SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1)));
                }

                // Construct the method call
                // Determine if we need an async context
                var needsAsync = diagnostic.GetMessage().Contains("Async");
                var targetMethod = needsAsync ? "ForAsync" : "For";

                var memberAccessExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName("Parallel"), SyntaxFactory.IdentifierName(targetMethod)
                );
                var invocationExpression = SyntaxFactory.InvocationExpression(memberAccessExpression, SyntaxFactory.ArgumentList([
                    SyntaxFactory.Argument(initializerValueExpression), // loop start inclusive
                    SyntaxFactory.Argument(end), // loop end exclusive
                    SyntaxFactory.Argument(SyntaxFactory.SimpleLambdaExpression(
                        SyntaxFactory.Parameter(loopVar.Identifier),
                        forStatementSyntax.Statement.Formatted
                    )) // loop body
                ]));

                ExpressionSyntax resultExpression = invocationExpression.Formatted;
                if (needsAsync)
                {
                    resultExpression = SyntaxFactory.AwaitExpression(resultExpression).Formatted;
                }

                resultExpression = resultExpression.WithLeadingTrivia(forStatementSyntax.GetLeadingTrivia())
                    .WithTrailingTrivia(forStatementSyntax.GetTrailingTrivia());

                var expressionStatement = SyntaxFactory.ExpressionStatement(resultExpression)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

                // Replace the entire for with the created expression statement
                return new FixInfo($"Convert to Parallel.{targetMethod}", editor =>
                {
                    editor.ReplaceNode(forStatementSyntax, expressionStatement);
                    return ValueTask.CompletedTask;
                });
            }
            case ForEachStatementSyntax forEachStatementSyntax:
            {
                var needsAsync = diagnostic.GetMessage().Contains("Async");
                var targetMethod = needsAsync ? "ForEachAsync" : "ForEach";

                var memberAccessExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName("Parallel"), SyntaxFactory.IdentifierName(targetMethod)
                );

                ExpressionSyntax resultExpression;
                if (needsAsync)
                {
                    resultExpression = SyntaxFactory.AwaitExpression(SyntaxFactory.InvocationExpression(memberAccessExpression, SyntaxFactory.ArgumentList([
                        SyntaxFactory.Argument(forEachStatementSyntax.Expression), // loop target
                        SyntaxFactory.Argument(SyntaxFactory.ParenthesizedLambdaExpression(
                            SyntaxFactory.ParameterList([SyntaxFactory.Parameter(forEachStatementSyntax.Identifier), SyntaxFactory.Parameter(SyntaxFactory.Identifier("ct"))]),
                            forEachStatementSyntax.Statement.Formatted
                        ).WithAsyncKeyword(SyntaxFactory.Token(SyntaxKind.AsyncKeyword))) // loop body
                    ])));
                }
                else
                {
                    resultExpression = SyntaxFactory.InvocationExpression(memberAccessExpression, SyntaxFactory.ArgumentList([
                        SyntaxFactory.Argument(forEachStatementSyntax.Expression), // loop target
                        SyntaxFactory.Argument(SyntaxFactory.SimpleLambdaExpression(
                            SyntaxFactory.Parameter(forEachStatementSyntax.Identifier),
                            forEachStatementSyntax.Statement.Formatted
                        )) // loop body
                    ]));
                }

                resultExpression = resultExpression.WithLeadingTrivia(forEachStatementSyntax.Statement.GetLeadingTrivia())
                    .WithTrailingTrivia(forEachStatementSyntax.Statement.GetTrailingTrivia());

                var expressionStatement = SyntaxFactory.ExpressionStatement(resultExpression)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

                // Replace the entire foreach with the created expression statement
                return new FixInfo($"Convert to Parallel.{targetMethod}", editor =>
                {
                    editor.ReplaceNode(forEachStatementSyntax, expressionStatement);
                    return ValueTask.CompletedTask;
                });
            }
        }

        return FixInfo.Empty;
    }
}
