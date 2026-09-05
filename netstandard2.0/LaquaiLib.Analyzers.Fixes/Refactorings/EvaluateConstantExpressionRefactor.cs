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

    protected override async ValueTask<ImmutableArray<TextSpan>> GetRefactorAllSpansAsync(Document document, CompilationUnitSyntax compilationUnitSyntax, CancellationToken cancellationToken)
    {
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        var builder = ImmutableArray.CreateBuilder<TextSpan>();
        Collect(semanticModel, compilationUnitSyntax, builder, cancellationToken);
        return builder.ToImmutable();
    }

    /// <summary>
    /// Collects the outermost constant expression of each subtree; folding a nested one too would edit a subtree that the outer fold already replaced.
    /// Unlike a single invocation, which the user aims at a specific expression, this only claims expressions that actually compute something - a bare reference to a constant or an enum member reads better than the value it stands for.
    /// </summary>
    private static void Collect(SemanticModel semanticModel, SyntaxNode node, ImmutableArray<TextSpan>.Builder builder, CancellationToken cancellationToken)
    {
        foreach (var child in node.ChildNodes())
        {
            if (child is BinaryExpressionSyntax or PrefixUnaryExpressionSyntax or ParenthesizedExpressionSyntax or ConditionalExpressionSyntax
                // Folding an expression that straddles an #if destroys the directive and the inactive configuration
                && !child.ContainsDirectives
                && semanticModel.GetConstantValue(child, cancellationToken).HasValue
                // The constant of an enum-typed expression is its underlying value, so folding one drops the names
                && semanticModel.GetTypeInfo(child, cancellationToken) is not { ConvertedType.TypeKind: TypeKind.Enum })
            {
                builder.Add(child.Span);
                continue;
            }
            Collect(semanticModel, child, builder, cancellationToken);
        }
    }

    private static ValueTask ReplaceAsync(DocumentEditor editor, ExpressionSyntax target, ExpressionSyntax replacement)
    {
        editor.ReplaceNode(target, replacement.WithTriviaFrom(target).Formatted);
        return ValueTask.CompletedTask;
    }

    private static ExpressionSyntax CreateLiteral(object value, ITypeSymbol type)
    {
        if (value is null)
            return SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
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
            // NaN/Infinity have no literal syntax in C#, so a non-finite constant can't be represented as one
            case float f:
                return !float.IsNaN(f) && !float.IsInfinity(f) ? SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(f)) : null;
            case double d:
                return !double.IsNaN(d) && !double.IsInfinity(d) ? SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(d)) : null;
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
