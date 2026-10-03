using System.Globalization;
using System.Text;

namespace LaquaiLib.Analyzers.Shared;

/// <summary>
/// What <see cref="StringCreateHelper.GetPlan"/> found worth offering for an interpolated string.
/// </summary>
public readonly struct StringCreatePlan
{
    internal StringCreatePlan(int stackLength, int exactLength)
    {
        StackLength = stackLength;
        ExactLength = exactLength;
    }

    /// <summary>
    /// The <see langword="stackalloc"/> length to wrap the interpolation with, or 0 if wrapping it is illegal, unsafe or pointless.
    /// </summary>
    public int StackLength { get; }
    /// <summary>
    /// The exact length of the final string if it is known with certainty and <c>string.Create&lt;TState&gt;</c> may be used, or 0.
    /// </summary>
    public int ExactLength { get; }
    /// <summary>
    /// Whether neither rewrite is offered.
    /// </summary>
    public bool IsEmpty => StackLength == 0 && ExactLength == 0;
}

public enum ExactSegmentKind
{
    /// <summary>Literal text, including constant holes and the padding of their alignment.</summary>
    Text,
    /// <summary>A <see cref="char"/> hole.</summary>
    Char,
    /// <summary>A <see cref="Guid"/> hole.</summary>
    Guid
}

/// <summary>
/// One run of the output of an interpolated string whose final length is known exactly.
/// </summary>
public readonly struct ExactSegment
{
    internal ExactSegment(ExactSegmentKind kind, string text, ExpressionSyntax expression, string format, int contentLength, int padLeft, int padRight)
    {
        Kind = kind;
        Text = text;
        Expression = expression;
        Format = format;
        ContentLength = contentLength;
        PadLeft = padLeft;
        PadRight = padRight;
    }

    public ExactSegmentKind Kind { get; }
    /// <summary>The literal text of a <see cref="ExactSegmentKind.Text"/> segment.</summary>
    public string Text { get; }
    /// <summary>The hole expression of any other segment.</summary>
    public ExpressionSyntax Expression { get; }
    /// <summary>The format specifier of a <see cref="ExactSegmentKind.Guid"/> segment, or <see langword="null"/>.</summary>
    public string Format { get; }
    public int ContentLength { get; }
    public int PadLeft { get; }
    public int PadRight { get; }
    public int Length => PadLeft + ContentLength + PadRight;
}

/// <summary>
/// Shared logic for rewriting an interpolated string into a <c>string.Create</c> call, used by LAQ0009, its fixers and <c>UseStringCreateRefactor</c>.
/// The stackalloc rewrite hands the interpolation to the same <c>DefaultInterpolatedStringHandler</c> it already compiles to, only with its growth buffer coming off the stack.
/// A <see langword="null"/> provider is what the handler already formats with, and an undersized buffer still falls back to the pool rather than truncating.
/// </summary>
public static class StringCreateHelper
{
    public const string BufferLengthKey = "BufferLength";
    public const string ExactLengthKey = "ExactLength";

    private const string MethodName = "Create";
    private const string HandlerMetadataName = "System.Runtime.CompilerServices.DefaultInterpolatedStringHandler";
    private const string HandlerAttributeMetadataName = "System.Runtime.CompilerServices.InterpolatedStringHandlerAttribute";
    private const string ExpressionMetadataName = "System.Linq.Expressions.Expression";
    private const string FormattableStringMetadataName = "System.FormattableString";
    /// <summary>The 2KB ceiling for a stack buffer, in <see cref="char"/>s.</summary>
    private const int MaximumBufferLength = 1024;
    private const int MinimumBufferLength = 32;
    /// <summary>Roslyn only lowers to <c>string.Concat(string, string)</c> while the operands fit its four-argument overload.</summary>
    private const int MaximumConcatOperands = 4;

    /// <summary>
    /// Gets <c>string.Create(IFormatProvider, Span&lt;char&gt;, ref DefaultInterpolatedStringHandler)</c>, or <see langword="null"/> if the target framework predates it.
    /// </summary>
    public static IMethodSymbol GetStringCreate(Compilation compilation)
    {
        if (compilation.GetTypeByMetadataName(HandlerMetadataName) is not { } handler
            || compilation.GetTypeByMetadataName("System.IFormatProvider") is not { } formatProvider)
            return null;

        foreach (var member in compilation.GetSpecialType(SpecialType.System_String).GetMembers(MethodName))
            if (member is IMethodSymbol { IsStatic: true, Arity: 0, DeclaredAccessibility: Accessibility.Public, Parameters.Length: 3 } method
                && SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, formatProvider)
                && SymbolEqualityComparer.Default.Equals(method.Parameters[2].Type, handler))
                return method;
        return null;
    }

    /// <summary>
    /// Decides which <c>string.Create</c> rewrites <paramref name="interpolatedString"/> deserves.
    /// </summary>
    public static StringCreatePlan GetPlan(InterpolatedStringExpressionSyntax interpolatedString, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        // A constant interpolation - no holes, or a const/attribute/pattern context - builds nothing at runtime to move off the pool
        if (semanticModel.GetConstantValue(interpolatedString, cancellationToken).HasValue)
            return default;

        // Converted to a handler, FormattableString or IFormattable, the interpolation is not producing a string here at all
        if (semanticModel.GetTypeInfo(interpolatedString, cancellationToken).ConvertedType is not { } convertedType
            || IsInterpolationTarget(convertedType, semanticModel.Compilation))
            return default;

        if (LowersToConcat(interpolatedString, semanticModel, cancellationToken))
            return default;

        var exactLength = 0;
        if (TryGetExactSegments(interpolatedString, semanticModel, cancellationToken, out _, out var length) && !IsInExpressionTree(interpolatedString, semanticModel, cancellationToken))
            exactLength = length;

        // CS4007: both the handler and the Span<char> are ref structs, so neither survives an await the bare interpolation compiles fine across
        var stackLength = 0;
        if (!ContainsAwait(interpolatedString) && !IsUnsafeSite(interpolatedString, semanticModel, cancellationToken))
            stackLength = EstimateBufferLength(interpolatedString, semanticModel, cancellationToken);

        return new StringCreatePlan(stackLength, exactLength);
    }

    /// <summary>
    /// Builds <c>string.Create(null, stackalloc char[length], $"...")</c> around <paramref name="interpolatedString"/>, carrying its trivia. The result is not formatted.
    /// </summary>
    public static InvocationExpressionSyntax Build(InterpolatedStringExpressionSyntax interpolatedString, int length)
    {
        var buffer = SyntaxFactory.StackAllocArrayCreationExpression(
            // The factory's `stackalloc` carries no trivia of its own, so it would run straight into the element type
            SyntaxFactory.Token(SyntaxKind.StackAllocKeyword).WithTrailingTrivia(SyntaxFactory.Space),
            SyntaxFactory.ArrayType(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.CharKeyword)),
                SyntaxFactory.SingletonList(SyntaxFactory.ArrayRankSpecifier(SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                    SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(length))
                )))
            ),
            null
        );
        var memberAccess = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)),
            SyntaxFactory.IdentifierName(MethodName)
        );
        var argumentList = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)),
            SyntaxFactory.Argument(buffer),
            SyntaxFactory.Argument(interpolatedString.WithoutTrivia())
        ]));
        return SyntaxFactory.InvocationExpression(memberAccess, argumentList).WithTriviaFrom(interpolatedString);
    }

    /// <summary>
    /// Splits <paramref name="interpolatedString"/> into runs whose output and length are known with certainty, or returns <see langword="false"/> if any part of it is not.
    /// Constant holes are folded into the surrounding text. The only non-constant holes that qualify are <see cref="char"/>s and <see cref="Guid"/>s with a fixed format.
    /// </summary>
    public static bool TryGetExactSegments(InterpolatedStringExpressionSyntax interpolatedString, SemanticModel semanticModel, CancellationToken cancellationToken, out ImmutableArray<ExactSegment> segments, out int length)
    {
        segments = default;
        length = 0;
        // Raw literals have delimiter-dependent brace handling and indentation trimming; not worth proving exact
        if (interpolatedString.StringStartToken.IsKind(SyntaxKind.InterpolatedSingleLineRawStringStartToken)
            || interpolatedString.StringStartToken.IsKind(SyntaxKind.InterpolatedMultiLineRawStringStartToken))
            return false;

        var builder = ImmutableArray.CreateBuilder<ExactSegment>();
        var pending = new StringBuilder();
        foreach (var content in interpolatedString.Contents)
        {
            if (content is InterpolatedStringTextSyntax text)
            {
                // ValueText keeps the doubled braces of the source
                pending.Append(text.TextToken.ValueText.Replace("{{", "{").Replace("}}", "}"));
                continue;
            }

            var interpolation = (InterpolationSyntax)content;
            if (!TryGetExactHole(interpolation, semanticModel, cancellationToken, out var kind, out var holeText, out var contentLength, out var width, out var format))
                return false;

            var padding = Math.Max(0, Math.Abs(width) - contentLength);
            var padLeft = width > 0 ? padding : 0;
            var padRight = width < 0 ? padding : 0;
            if (kind == ExactSegmentKind.Text)
            {
                pending.Append(' ', padLeft).Append(holeText).Append(' ', padRight);
                continue;
            }

            Flush();
            builder.Add(new ExactSegment(kind, null, interpolation.Expression, format, contentLength, padLeft, padRight));
        }
        Flush();

        segments = builder.ToImmutable();
        for (var i = 0; i < segments.Length; i++)
            length += segments[i].Length;
        return true;

        void Flush()
        {
            if (pending.Length == 0)
                return;
            var value = pending.ToString();
            builder.Add(new ExactSegment(ExactSegmentKind.Text, value, null, null, value.Length, 0, 0));
            pending.Clear();
        }
    }

    private static bool IsInterpolationTarget(ITypeSymbol convertedType, Compilation compilation)
    {
        if (SymbolEqualityComparer.Default.Equals(convertedType, compilation.GetTypeByMetadataName(FormattableStringMetadataName))
            || SymbolEqualityComparer.Default.Equals(convertedType, compilation.GetTypeByMetadataName("System.IFormattable")))
            return true;

        var handlerAttribute = compilation.GetTypeByMetadataName(HandlerAttributeMetadataName);
        foreach (var attribute in convertedType.GetAttributes())
            if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, handlerAttribute))
                return true;
        return false;
    }

    private static bool ContainsAwait(InterpolatedStringExpressionSyntax interpolatedString)
    {
        foreach (var descendant in interpolatedString.DescendantNodes())
            if (descendant is AwaitExpressionSyntax)
                return true;
        return false;
    }

    /// <summary>
    /// Gets whether a <see langword="stackalloc"/> at this position would fail to compile or would allocate more than once per frame.
    /// </summary>
    private static bool IsUnsafeSite(InterpolatedStringExpressionSyntax interpolatedString, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        // A lambda body gets its own frame per invocation, so an enclosing loop or handler block stops mattering once one is crossed
        var crossedFunction = false;
        for (var current = interpolatedString.Parent; current is not null; current = current.Parent)
            switch (current)
            {
                // CS0255
                case CatchClauseSyntax or FinallyClauseSyntax when !crossedFunction:
                // CA2014: the localloc lands inside the loop body, growing the frame every iteration instead of being reclaimed at the bottom
                case ForStatementSyntax or CommonForEachStatementSyntax or WhileStatementSyntax or DoStatementSyntax when !crossedFunction:
                    return true;
                case AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax:
                    crossedFunction = true;
                    break;
                case MemberDeclarationSyntax:
                    // CS8640/CS8952: neither the handler nor the Span<char> can be lifted into an expression tree
                    return IsInExpressionTree(interpolatedString, semanticModel, cancellationToken);
            }
        return IsInExpressionTree(interpolatedString, semanticModel, cancellationToken);
    }

    private static bool IsInExpressionTree(InterpolatedStringExpressionSyntax interpolatedString, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        var expression = semanticModel.Compilation.GetTypeByMetadataName(ExpressionMetadataName);
        if (expression is null)
            return false;

        for (var current = interpolatedString.Parent; current is not null; current = current.Parent)
        {
            if (current is MemberDeclarationSyntax)
                return false;
            if (current is AnonymousFunctionExpressionSyntax && InheritsFrom(semanticModel.GetTypeInfo(current, cancellationToken).ConvertedType, expression))
                return true;
        }
        return false;
    }

    private static bool InheritsFrom(ITypeSymbol type, INamedTypeSymbol baseType)
    {
        for (var current = type; current is not null; current = current.BaseType)
            if (SymbolEqualityComparer.Default.Equals(current, baseType))
                return true;
        return false;
    }

    /// <summary>
    /// Gets whether the compiler already lowers this interpolation to a <c>string.Concat</c> call, which no handler-based rewrite can improve on.
    /// </summary>
    private static bool LowersToConcat(InterpolatedStringExpressionSyntax interpolatedString, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        var operands = 0;
        var lastWasText = false;
        foreach (var content in interpolatedString.Contents)
        {
            if (content is InterpolatedStringTextSyntax)
            {
                // Adjacent literal runs are folded at compile time and reach Concat as a single operand
                if (!lastWasText)
                    operands++;
                lastWasText = true;
                continue;
            }

            var interpolation = (InterpolationSyntax)content;
            // Alignment or a format specifier is formatting only the handler can do
            if (interpolation.AlignmentClause is not null || interpolation.FormatClause is not null)
                return false;
            if (semanticModel.GetTypeInfo(interpolation.Expression, cancellationToken).Type?.SpecialType != SpecialType.System_String)
                return false;
            operands++;
            lastWasText = false;
        }
        return operands <= MaximumConcatOperands;
    }

    /// <summary>
    /// Sizes the buffer to the exact final length if that is known with certainty (see <see cref="TryGetExactSegments"/>), else to the literal text plus a per-hole worst case, rounded up.
    /// Either is clamped to <see cref="MaximumBufferLength"/>. Overshooting wastes stack, undershooting lands back on the pool the rewrite exists to avoid, and neither changes the result.
    /// </summary>
    private static int EstimateBufferLength(InterpolatedStringExpressionSyntax interpolatedString, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        if (TryGetExactSegments(interpolatedString, semanticModel, cancellationToken, out _, out var exactLength))
            return Math.Min(exactLength, MaximumBufferLength);

        var length = 0;
        foreach (var content in interpolatedString.Contents)
        {
            if (content is InterpolatedStringTextSyntax text)
            {
                length += text.TextToken.ValueText.Length;
                continue;
            }

            var interpolation = (InterpolationSyntax)content;
            var hole = EstimateHoleLength(interpolation, semanticModel, cancellationToken);
            if (interpolation.AlignmentClause is { Value: var alignment }
                && semanticModel.GetConstantValue(alignment, cancellationToken).Value is int width)
                hole = Math.Max(hole, Math.Abs(width));
            length += hole;
        }

        length = (length + 15) & ~15;
        return Math.Min(Math.Max(length, MinimumBufferLength), MaximumBufferLength);
    }

    /// <summary>
    /// Resolves a hole whose output does not depend on its value or the culture: a constant (see <see cref="TryGetConstantText"/>), a <see cref="char"/>, or a <see cref="Guid"/> with a fixed format; with an optional constant alignment.
    /// </summary>
    private static bool TryGetExactHole(InterpolationSyntax interpolation, SemanticModel semanticModel, CancellationToken cancellationToken, out ExactSegmentKind kind, out string text, out int contentLength, out int width, out string format)
    {
        kind = ExactSegmentKind.Text;
        text = null;
        contentLength = 0;
        width = 0;
        format = interpolation.FormatClause?.FormatStringToken.ValueText;

        var constant = semanticModel.GetConstantValue(interpolation.Expression, cancellationToken);
        var type = semanticModel.GetTypeInfo(interpolation.Expression, cancellationToken).Type;
        if (constant.HasValue)
        {
            if (type is null or { TypeKind: TypeKind.Enum } || !TryGetConstantText(constant.Value, format, out text))
                return false;
            contentLength = text.Length;
        }
        else if (type?.SpecialType == SpecialType.System_Char && format is null)
        {
            kind = ExactSegmentKind.Char;
            contentLength = 1;
        }
        else if (type?.ToDisplayString() == "System.Guid" && TryGetGuidLength(format, out contentLength))
            kind = ExactSegmentKind.Guid;
        else
            return false;

        if (interpolation.AlignmentClause is { Value: var alignment })
        {
            if (semanticModel.GetConstantValue(alignment, cancellationToken).Value is not int constantWidth)
                return false;
            width = constantWidth;
        }
        return true;
    }

    /// <summary>
    /// Renders a compile-time constant the way the handler would where that cannot vary by culture: strings, chars, booleans, <see langword="null"/> and non-negative integers (optionally with a <c>D</c> or <c>X</c> format).
    /// </summary>
    private static bool TryGetConstantText(object value, string format, out string text)
    {
        text = null;
        switch (value)
        {
            case null:
                text = "";
                return format is null;
            case string s:
                text = s;
                return format is null;
            case char c:
                text = c.ToString();
                return format is null;
            case bool b:
                text = b ? "True" : "False";
                return format is null;
            case sbyte or byte or short or ushort or int or uint or long or ulong:
                if (value is sbyte or short or int or long && Convert.ToInt64(value) < 0)
                    return false;
                if (format is not null && !IsIntegerFormat(format))
                    return false;
                text = ((IFormattable)value).ToString(format, CultureInfo.InvariantCulture);
                return true;
            default:
                return false;
        }
    }

    // Only D and X (one letter plus up to two precision digits) are digit-for-digit identical across cultures for non-negative values
    private static bool IsIntegerFormat(string format)
    {
        if (format.Length is < 1 or > 3 || format[0] is not ('D' or 'd' or 'X' or 'x'))
            return false;
        for (var i = 1; i < format.Length; i++)
            if (format[i] is < '0' or > '9')
                return false;
        return true;
    }

    private static bool TryGetGuidLength(string format, out int length)
    {
        switch (format)
        {
            case null or "D" or "d": length = 36; return true;
            case "N" or "n": length = 32; return true;
            case "B" or "b" or "P" or "p": length = 38; return true;
            case "X" or "x": length = 68; return true;
            default: length = 0; return false;
        }
    }

    private static int EstimateHoleLength(InterpolationSyntax interpolation, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        // A format specifier can expand a value far past its default rendering ("D" on a DateTime, "N" on a double)
        if (interpolation.FormatClause is not null)
            return 32;

        var type = semanticModel.GetTypeInfo(interpolation.Expression, cancellationToken).Type;
        switch (type?.SpecialType)
        {
            case SpecialType.System_Char: return 1;
            case SpecialType.System_Boolean: return 5;
            case SpecialType.System_SByte or SpecialType.System_Byte: return 4;
            case SpecialType.System_Int16 or SpecialType.System_UInt16: return 6;
            case SpecialType.System_Int32 or SpecialType.System_UInt32: return 11;
            case SpecialType.System_Int64 or SpecialType.System_UInt64: return 20;
            case SpecialType.System_Single: return 16;
            case SpecialType.System_Double: return 24;
            case SpecialType.System_Decimal: return 30;
            case SpecialType.System_DateTime: return 33;
        }
        return type?.ToDisplayString() switch
        {
            "System.Guid" => 36,
            "System.TimeSpan" => 26,
            "System.DateTimeOffset" => 40,
            _ => 16
        };
    }
}
