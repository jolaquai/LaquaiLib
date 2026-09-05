using System.Text;

namespace LaquaiLib.Analyzers.Shared;

/// <summary>
/// The rewrite LAQ0007 offers for the concatenation it was reported on.
/// </summary>
public enum ConcatRewrite
{
    /// <summary>No rewrite is worth offering; nothing is reported.</summary>
    None,
    /// <summary>A single string literal holding the merged text of every part.</summary>
    MergedLiteral,
    /// <summary>A <c>string.Concat</c> call over the parts.</summary>
    StringConcat,
    /// <summary>An interpolated string over the parts.</summary>
    InterpolatedString
}

/// <summary>
/// One operand of a flattened string concatenation.
/// </summary>
public readonly struct ConcatPart
{
    internal ConcatPart(string text, ExpressionSyntax expression, InterpolationSyntax interpolation, bool isString, bool isSpan, bool verbatimText)
    {
        Text = text;
        Expression = expression;
        Interpolation = interpolation;
        IsString = isString;
        IsSpan = isSpan;
        VerbatimText = verbatimText;
    }

    /// <summary>
    /// The literal text this part contributes, or <see langword="null"/> if it contributes the value of <see cref="Expression"/>.
    /// </summary>
    public string Text { get; }
    /// <summary>
    /// The expression this part was written as, or <see langword="null"/> for a text run of an interpolated string.
    /// </summary>
    public ExpressionSyntax Expression { get; }
    /// <summary>
    /// The interpolation hole this part came from, if it came from one.
    /// </summary>
    public InterpolationSyntax Interpolation { get; }
    /// <summary>
    /// Whether this part is typed <see cref="string"/>.
    /// </summary>
    public bool IsString { get; }
    /// <summary>
    /// Whether this part is typed <see cref="ReadOnlySpan{T}"/> of <see cref="char"/>.
    /// </summary>
    public bool IsSpan { get; }
    /// <summary>
    /// Whether <see cref="Text"/> came from verbatim or raw source.
    /// </summary>
    public bool VerbatimText { get; }

    /// <summary>
    /// Whether this part carries an alignment or format specifier.
    /// </summary>
    public bool HasClauses => Interpolation is { } interpolation && (interpolation.AlignmentClause is not null || interpolation.FormatClause is not null);
}

/// <summary>
/// The <c>string.Concat</c> overloads a compilation offers.
/// </summary>
public readonly struct ConcatOverloads
{
    private readonly int _spanArities;
    private readonly bool _spanOfString;

    private ConcatOverloads(int spanArities, bool spanOfString)
    {
        _spanArities = spanArities;
        _spanOfString = spanOfString;
    }

    /// <summary>
    /// Gets whether <c>string.Concat</c> has an overload taking exactly <paramref name="arity"/> <see cref="ReadOnlySpan{T}"/>-of-<see cref="char"/> parameters.
    /// </summary>
    public bool HasSpanConcat(int arity) => (uint)arity < 32 && (_spanArities & (1 << arity)) != 0;
    /// <summary>
    /// Gets whether <c>string.Concat(params ReadOnlySpan&lt;string&gt;)</c> exists.
    /// </summary>
    public bool HasSpanOfStringConcat => _spanOfString;

    /// <summary>
    /// Resolves the overload set of <paramref name="compilation"/>. Call once per compilation, not once per node.
    /// </summary>
    public static ConcatOverloads Create(Compilation compilation)
    {
        var spanDefinition = compilation.GetTypeByMetadataName("System.ReadOnlySpan`1");
        if (spanDefinition is null)
            return default;

        var spanOfChar = spanDefinition.Construct(compilation.GetSpecialType(SpecialType.System_Char));
        var spanOfString = spanDefinition.Construct(compilation.GetSpecialType(SpecialType.System_String));

        var arities = 0;
        var ofString = false;
        var members = compilation.GetSpecialType(SpecialType.System_String).GetMembers("Concat");
        for (var i = 0; i < members.Length; i++)
        {
            if (members[i] is not IMethodSymbol { IsStatic: true } method)
                continue;

            var parameters = method.Parameters;
            if (parameters.Length == 1)
            {
                ofString |= SymbolEqualityComparer.Default.Equals(parameters[0].Type, spanOfString);
                continue;
            }
            if (parameters.Length is < 2 or > 4)
                continue;

            var allSpans = true;
            for (var j = 0; j < parameters.Length; j++)
                if (!SymbolEqualityComparer.Default.Equals(parameters[j].Type, spanOfChar))
                {
                    allSpans = false;
                    break;
                }
            if (allSpans)
                arities |= 1 << parameters.Length;
        }

        return new ConcatOverloads(arities, ofString);
    }
}

/// <summary>
/// Shared logic for deciding which form a string concatenation is best written in and for rewriting it into that form.
/// Used by both <c>StringConcatenationAnalyzer</c> and <c>StringConcatenationFixer</c> so the two stay in lockstep.
/// </summary>
public static class StringConcatenationHelper
{
    /// <summary>
    /// Gets whether <paramref name="node"/> evaluates to a <see cref="string"/>.
    /// </summary>
    public static bool IsStringConcatenation(ExpressionSyntax node, SemanticModel semanticModel, CancellationToken cancellationToken)
        => IsString(semanticModel.GetTypeInfo(node, cancellationToken).Type);

    /// <summary>
    /// Gets whether <paramref name="node"/> is a <c>+</c> chain or interpolated string that an enclosing node already accounts for, and so must not be reported on its own.
    /// </summary>
    public static bool IsNestedInConcatenation(ExpressionSyntax node, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        SyntaxNode current = node;
        while (current.Parent is ParenthesizedExpressionSyntax parenthesized)
            current = parenthesized;

        return current.Parent switch
        {
            BinaryExpressionSyntax { RawKind: (int)SyntaxKind.AddExpression } add => IsString(semanticModel.GetTypeInfo(add, cancellationToken).Type),
            InterpolationSyntax { AlignmentClause: null, FormatClause: null } => true,
            _ => false
        };
    }

    /// <summary>
    /// Decides which rewrite <paramref name="node"/> deserves and produces the parts that rewrite is built from.
    /// </summary>
    /// <param name="node">The <c>+</c> chain or interpolated string to classify.</param>
    /// <param name="semanticModel">The <see cref="SemanticModel"/> for <paramref name="node"/>.</param>
    /// <param name="overloads">The <c>string.Concat</c> overloads available to the compilation.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <param name="parts">The flattened operands of the concatenation, in evaluation order.</param>
    public static ConcatRewrite Classify(ExpressionSyntax node, SemanticModel semanticModel, ConcatOverloads overloads, CancellationToken cancellationToken, out ImmutableArray<ConcatPart> parts)
    {
        var builder = ImmutableArray.CreateBuilder<ConcatPart>();
        var flattened = false;
        var ok = node is InterpolatedStringExpressionSyntax root
            ? CanFlattenInterpolated(root) && FlattenInterpolated(root, semanticModel, cancellationToken, builder, ref flattened)
            : Flatten(node, semanticModel, cancellationToken, builder, ref flattened);
        if (!ok)
        {
            parts = [];
            return ConcatRewrite.None;
        }
        parts = builder.ToImmutable();

        if (parts.Length < 2)
            return ConcatRewrite.None;

        var allText = true;
        var concatable = true;
        var anySpan = false;
        for (var i = 0; i < parts.Length; i++)
        {
            var part = parts[i];
            allText &= part.Text is not null;
            concatable &= !part.HasClauses && (part.IsString || part.IsSpan);
            anySpan |= part.IsSpan;
        }

        if (allText)
            return ConcatRewrite.MergedLiteral;
        if (semanticModel.GetConstantValue(node, cancellationToken).HasValue)
            return ConcatRewrite.None;

        var isAdd = node.IsKind(SyntaxKind.AddExpression);
        if (concatable)
        {
            if (anySpan)
                return overloads.HasSpanConcat(parts.Length) ? ConcatRewrite.StringConcat
                    : isAdd ? ConcatRewrite.InterpolatedString : ConcatRewrite.None;

            return isAdd || (parts.Length >= 5 && overloads.HasSpanOfStringConcat) ? ConcatRewrite.StringConcat : ConcatRewrite.None;
        }

        return isAdd || flattened ? ConcatRewrite.InterpolatedString : ConcatRewrite.None;
    }

    /// <summary>
    /// Builds the replacement for <paramref name="node"/> in the form <paramref name="rewrite"/> names.
    /// </summary>
    public static ExpressionSyntax Build(ExpressionSyntax node, ConcatRewrite rewrite, ImmutableArray<ConcatPart> parts) => rewrite switch
    {
        ConcatRewrite.MergedLiteral => BuildMergedLiteral(parts).WithTriviaFrom(node),
        ConcatRewrite.StringConcat => BuildStringConcat(parts).WithTriviaFrom(node),
        ConcatRewrite.InterpolatedString => BuildInterpolatedString(parts).WithTriviaFrom(node),
        _ => node
    };

    /// <summary>
    /// Gets the title the code action offering <paramref name="rewrite"/> is registered under.
    /// </summary>
    public static string GetTitle(ConcatRewrite rewrite) => rewrite switch
    {
        ConcatRewrite.MergedLiteral => "Merge into a single string literal",
        ConcatRewrite.StringConcat => "Use string.Concat",
        ConcatRewrite.InterpolatedString => "Use an interpolated string",
        _ => null
    };

    /// <summary>
    /// Gets the noun phrase naming <paramref name="rewrite"/> in the diagnostic message.
    /// </summary>
    public static string GetReplacementName(ConcatRewrite rewrite) => rewrite switch
    {
        ConcatRewrite.MergedLiteral => "a single string literal",
        ConcatRewrite.StringConcat => "string.Concat",
        ConcatRewrite.InterpolatedString => "an interpolated string",
        _ => null
    };

    #region flattening
    private static bool Flatten(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, ImmutableArray<ConcatPart>.Builder parts, ref bool flattened)
    {
        var inner = Unparenthesize(expression);
        switch (inner)
        {
            case BinaryExpressionSyntax { RawKind: (int)SyntaxKind.AddExpression } add when IsString(semanticModel.GetTypeInfo(add, cancellationToken).Type):
                return Flatten(add.Left, semanticModel, cancellationToken, parts, ref flattened)
                    && Flatten(add.Right, semanticModel, cancellationToken, parts, ref flattened);
            case LiteralExpressionSyntax { RawKind: (int)SyntaxKind.StringLiteralExpression } literal:
                parts.Add(new ConcatPart(literal.Token.ValueText, literal, null, true, false, IsVerbatimLiteral(literal.Token.Text)));
                return true;
            case InterpolatedStringExpressionSyntax interpolated when CanFlattenInterpolated(interpolated):
                flattened = true;
                return FlattenInterpolated(interpolated, semanticModel, cancellationToken, parts, ref flattened);
        }

        var type = semanticModel.GetTypeInfo(inner, cancellationToken).Type;
        parts.Add(new ConcatPart(null, inner, null, IsString(type), IsSpanOfChar(type), false));
        return true;
    }

    private static bool FlattenInterpolated(InterpolatedStringExpressionSyntax interpolated, SemanticModel semanticModel, CancellationToken cancellationToken, ImmutableArray<ConcatPart>.Builder parts, ref bool flattened)
    {
        var verbatimText = !interpolated.StringStartToken.IsKind(SyntaxKind.InterpolatedStringStartToken);
        var contents = interpolated.Contents;

        for (var i = 0; i < contents.Count; i++)
        {
            switch (contents[i])
            {
                case InterpolatedStringTextSyntax text:
                    parts.Add(new ConcatPart(UnescapeBraces(text.TextToken.ValueText), null, null, true, false, verbatimText));
                    break;
                case InterpolationSyntax { AlignmentClause: null, FormatClause: null } interpolation:
                    var before = parts.Count;
                    if (!Flatten(interpolation.Expression, semanticModel, cancellationToken, parts, ref flattened))
                        return false;
                    flattened |= parts.Count - before > 1;
                    break;
                case InterpolationSyntax interpolation:
                    var type = semanticModel.GetTypeInfo(interpolation.Expression, cancellationToken).Type;
                    parts.Add(new ConcatPart(null, interpolation.Expression, interpolation, IsString(type), IsSpanOfChar(type), false));
                    break;
                default:
                    return false;
            }
        }

        return true;
    }

    private static bool CanFlattenInterpolated(InterpolatedStringExpressionSyntax interpolated)
    {
        if (!IsRawInterpolated(interpolated))
            return true;

        var contents = interpolated.Contents;
        for (var i = 0; i < contents.Count; i++)
            if (contents[i] is InterpolatedStringTextSyntax)
                return false;
        return true;
    }

    private static ExpressionSyntax Unparenthesize(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
            expression = parenthesized.Expression;
        return expression;
    }

    private static bool IsString(ITypeSymbol type) => type?.SpecialType == SpecialType.System_String;
    private static bool IsSpanOfChar(ITypeSymbol type)
        => type is INamedTypeSymbol { Name: "ReadOnlySpan", Arity: 1, ContainingNamespace: { Name: "System", ContainingNamespace.IsGlobalNamespace: true } } named
        && named.TypeArguments[0].SpecialType == SpecialType.System_Char;
    private static bool IsVerbatimLiteral(string text) => text.Length > 0 && text[0] == '@';
    private static bool IsRawInterpolated(InterpolatedStringExpressionSyntax interpolated)
        => interpolated.StringStartToken.IsKind(SyntaxKind.InterpolatedSingleLineRawStringStartToken)
        || interpolated.StringStartToken.IsKind(SyntaxKind.InterpolatedMultiLineRawStringStartToken);
    #endregion

    #region building
    private static ExpressionSyntax BuildMergedLiteral(ImmutableArray<ConcatPart> parts)
    {
        var anyVerbatim = false;
        var builder = new StringBuilder();
        for (var i = 0; i < parts.Length; i++)
        {
            builder.Append(parts[i].Text);
            anyVerbatim |= parts[i].VerbatimText;
        }

        var value = builder.ToString();
        return anyVerbatim && !HasBadVerbatimChar(value) ? VerbatimLiteral(value) : RegularLiteral(value);
    }

    private static ExpressionSyntax BuildStringConcat(ImmutableArray<ConcatPart> parts)
    {
        var arguments = new ArgumentSyntax[parts.Length];
        for (var i = 0; i < parts.Length; i++)
        {
            var part = parts[i];
            arguments[i] = SyntaxFactory.Argument(part.Expression is null ? RegularLiteral(part.Text) : part.Expression.WithoutTrivia());
        }

        return SyntaxFactory.InvocationExpression(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                SyntaxFactory.IdentifierName("Concat")),
            SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(arguments)));
    }

    private static ExpressionSyntax BuildInterpolatedString(ImmutableArray<ConcatPart> parts)
    {
        var verbatim = ShouldUseVerbatim(parts);
        var contents = new List<InterpolatedStringContentSyntax>(parts.Length);
        for (var i = 0; i < parts.Length; i++)
        {
            var part = parts[i];
            if (part.Text is not null)
                AppendText(part.Text, contents, verbatim);
            else if (part.HasClauses)
                contents.Add(part.Interpolation.WithoutTrivia());
            else
                contents.Add(SyntaxFactory.Interpolation(Parenthesize(part.Expression.WithoutTrivia())));
        }

        return SyntaxFactory.InterpolatedStringExpression(
            SyntaxFactory.Token(verbatim ? SyntaxKind.InterpolatedVerbatimStringStartToken : SyntaxKind.InterpolatedStringStartToken),
            SyntaxFactory.List(contents),
            SyntaxFactory.Token(SyntaxKind.InterpolatedStringEndToken));
    }

    private static bool ShouldUseVerbatim(ImmutableArray<ConcatPart> parts)
    {
        var anyVerbatim = false;
        for (var i = 0; i < parts.Length; i++)
        {
            var part = parts[i];
            if (part.Text is null)
                continue;
            if (part.VerbatimText)
                anyVerbatim = true;
            else if (HasBadVerbatimChar(part.Text))
                return false;
        }
        return anyVerbatim;
    }

    private static bool HasBadVerbatimChar(string value)
    {
        for (var i = 0; i < value.Length; i++)
            if (value[i] is '\0' or '\a' or '\b' or '\f' or '\v' or '\r' or '\n' or '\t')
                return true;
        return false;
    }

    private static void AppendText(string value, List<InterpolatedStringContentSyntax> contents, bool verbatim)
    {
        if (value.Length == 0)
            return;

        var braced = EscapeBraces(value);
        var encoded = verbatim ? braced.Replace("\"", "\"\"") : SymbolDisplay.FormatLiteral(braced, false);

        var last = contents.Count - 1;
        if (last >= 0 && contents[last] is InterpolatedStringTextSyntax previous)
        {
            contents[last] = InterpolatedText(previous.TextToken.Text + encoded, previous.TextToken.ValueText + braced);
            return;
        }
        contents.Add(InterpolatedText(encoded, braced));
    }

    private static InterpolatedStringTextSyntax InterpolatedText(string text, string value)
        => SyntaxFactory.InterpolatedStringText(SyntaxFactory.Token(default, SyntaxKind.InterpolatedStringTextToken, text, value, default));

    private static string EscapeBraces(string value) => value.IndexOfAny(['{', '}']) >= 0 ? value.Replace("{", "{{").Replace("}", "}}") : value;
    private static string UnescapeBraces(string value) => value.IndexOfAny(['{', '}']) >= 0 ? value.Replace("{{", "{").Replace("}}", "}") : value;

    private static LiteralExpressionSyntax RegularLiteral(string value) => Literal(value, "\"" + SymbolDisplay.FormatLiteral(value, false) + "\"");
    private static LiteralExpressionSyntax VerbatimLiteral(string value) => Literal(value, "@\"" + value.Replace("\"", "\"\"") + "\"");
    private static LiteralExpressionSyntax Literal(string value, string text)
        => SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(default, text, value, default));

    private static ExpressionSyntax Parenthesize(ExpressionSyntax expression) => NeedsParens(expression) ? SyntaxFactory.ParenthesizedExpression(expression) : expression;

    private static bool NeedsParens(ExpressionSyntax expression) => expression switch
    {
        ConditionalExpressionSyntax or AssignmentExpressionSyntax or SwitchExpressionSyntax => true,
        LambdaExpressionSyntax or AnonymousMethodExpressionSyntax or QueryExpressionSyntax or IsPatternExpressionSyntax => true,
        BinaryExpressionSyntax { RawKind: (int)SyntaxKind.AsExpression or (int)SyntaxKind.IsExpression or (int)SyntaxKind.CoalesceExpression } => true,
        _ => false
    };
    #endregion
}
