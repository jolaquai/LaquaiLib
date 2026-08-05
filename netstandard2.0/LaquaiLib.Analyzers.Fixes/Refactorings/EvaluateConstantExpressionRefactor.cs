namespace LaquaiLib.Analyzers.Fixes.Refactorings;

[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(EvaluateConstantExpressionRefactor)), Shared]
public sealed class EvaluateConstantExpressionRefactor : LaquaiLibOperationRefactoring
{
    private static readonly ImmutableHashSet<SyntaxKind> _alreadyLiteralKinds = [
        SyntaxKind.NumericLiteralExpression, SyntaxKind.StringLiteralExpression, SyntaxKind.CharacterLiteralExpression,
        SyntaxKind.TrueLiteralExpression, SyntaxKind.FalseLiteralExpression, SyntaxKind.NullLiteralExpression
    ];

    public override ImmutableArray<CodeActionInfo> GetCodeActionInfos(CompilationUnitSyntax compilationUnitSyntax, IOperation operation, TextSpan span)
    {
        if (operation is not { ConstantValue.HasValue: true } || operation.Syntax is not ExpressionSyntax expression || _alreadyLiteralKinds.Contains(expression.Kind()))
            return [];

        var literal = CreateLiteral(operation.ConstantValue.Value, operation.Type);
        if (literal is null)
            return [];

        return [new CodeActionInfo("Evaluate constant expression", editor => ReplaceAsync(editor, expression, literal), "EvaluateConstantExpression")];
    }

    private static ValueTask ReplaceAsync(DocumentEditor editor, ExpressionSyntax target, ExpressionSyntax replacement)
    {
        editor.ReplaceNode(target, replacement.WithTriviaFrom(target).Formatted);
        return ValueTask.CompletedTask;
    }

    private static ExpressionSyntax CreateLiteral(object value, ITypeSymbol type)
    {
        if (value is null)
        {
            return SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
        }
        _ = 1 + 2;

        switch (value)
        {
            case bool b:
                return SyntaxFactory.LiteralExpression(b ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);
            case string s:
                return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(s));
            case char c:
                return SyntaxFactory.LiteralExpression(SyntaxKind.CharacterLiteralExpression, SyntaxFactory.Literal(c));
            case int i:
                return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(i));
            case uint ui:
                return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(ui));
            case long l:
                return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(l));
            case ulong ul:
                return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(ul));
            case float f:
                return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(f));
            case double d:
                return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(d));
            case decimal m:
                return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(m));
            // No native literal syntax for these - preserve the type with an explicit cast
            case byte or sbyte or short or ushort when type is not null:
                var narrowLiteral = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(Convert.ToInt32(value)));
                return SyntaxFactory.CastExpression(PredefinedTypeSyntax(type.SpecialType), narrowLiteral).Formatted;
            default:
                return null;
        }
    }

    private static PredefinedTypeSyntax PredefinedTypeSyntax(SpecialType specialType) => SyntaxFactory.PredefinedType(SyntaxFactory.Token(specialType switch
    {
        SpecialType.System_Byte => SyntaxKind.ByteKeyword,
        SpecialType.System_SByte => SyntaxKind.SByteKeyword,
        SpecialType.System_Int16 => SyntaxKind.ShortKeyword,
        SpecialType.System_UInt16 => SyntaxKind.UShortKeyword,
        _ => SyntaxKind.IntKeyword,
    }));
}
