namespace LaquaiLib.Analyzers.Fixes.Refactorings;

/// <summary>
/// Switches an interpolated string between the bare <c>$"..."</c> form and <c>string.Create(null, stackalloc char[length], $"...")</c>, in either direction.
/// The interpolation itself is never rewritten; it is handed to the same <see cref="System.Runtime.CompilerServices.DefaultInterpolatedStringHandler"/> it already compiles to, only with its growth buffer coming off the stack instead of <see cref="System.Buffers.ArrayPool{T}.Shared"/>.
/// A <see langword="null"/> provider is what the handler already formats with, so the rewrite is observationally identical, and an undersized buffer still falls back to the pool rather than truncating.
/// Offered only where <see langword="stackalloc"/> is both legal and safe at the site: not in a <see langword="catch"/>/<see langword="finally"/> (CS0255), not in an expression tree (CS8640/CS8952), not across an <see langword="await"/> in a hole (CS4007), and not inside a loop, where the <c>localloc</c> grows the frame every iteration instead of being reclaimed (CA2014).
/// Interpolations the compiler already lowers to a <see cref="string.Concat(string, string)"/> call - every hole <see cref="string"/>-typed and unformatted, at most four operands - are left alone, since wrapping one trades an exact-size vectorized concat for handler ceremony.
/// No "Refactor All" anchors are declared: the stack budget is a per-site decision, and sweeping a scope would spend it at sites nobody looked at.
/// </summary>
[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(UseStringCreateRefactor)), Shared]
public sealed class UseStringCreateRefactor : LaquaiLibRefactoring
{
    private const string MethodName = "Create";
    private const string HandlerMetadataName = "System.Runtime.CompilerServices.DefaultInterpolatedStringHandler";
    private const string HandlerAttributeMetadataName = "System.Runtime.CompilerServices.InterpolatedStringHandlerAttribute";
    private const string ExpressionMetadataName = "System.Linq.Expressions.Expression";
    private const string FormattableStringMetadataName = "System.FormattableString";
    /// <summary>
    /// The 2KB ceiling for a stack buffer, in <see cref="char"/>s.
    /// </summary>
    private const int MaximumBufferLength = 1024;
    private const int MinimumBufferLength = 32;
    /// <summary>
    /// Roslyn only lowers to <see cref="string.Concat(string, string)"/> while the operands fit its four-argument overload.
    /// </summary>
    private const int MaximumConcatOperands = 4;

    public override async ValueTask<ImmutableArray<CodeActionInfo>> GetCodeActionInfosAsync(Document document, CompilationUnitSyntax compilationUnitSyntax, TextSpan span, CancellationToken cancellationToken)
    {
        if (FindTarget(compilationUnitSyntax.FindNode(span, getInnermostNodeForTie: true)) is not { } target)
            return [];

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        // A target framework that predates the overload would only get code that doesn't compile
        if (GetStringCreate(semanticModel.Compilation) is not { } create)
            return [];

        if (target is InvocationExpressionSyntax invocation)
            return Unwrap(invocation, semanticModel, create, cancellationToken);

        var interpolatedString = (InterpolatedStringExpressionSyntax)target;
        // The caret may be sitting in an interpolation this refactoring already wrapped, where the only thing left to offer is the way back
        if (interpolatedString.Parent is ArgumentSyntax { Parent: ArgumentListSyntax { Parent: InvocationExpressionSyntax enclosing } }
            && Unwrap(enclosing, semanticModel, create, cancellationToken) is { IsEmpty: false } unwrap)
            return unwrap;

        return Wrap(interpolatedString, semanticModel, cancellationToken);
    }

    /// <summary>
    /// Walks out of a hole or an argument list so the caret only has to sit somewhere inside the interpolation, but never past the expression it sits in.
    /// </summary>
    private static ExpressionSyntax FindTarget(SyntaxNode node)
    {
        for (var current = node; current is not null; current = current.Parent)
            switch (current)
            {
                case InterpolatedStringExpressionSyntax or InvocationExpressionSyntax:
                    return (ExpressionSyntax)current;
                case StatementSyntax or MemberDeclarationSyntax or AnonymousFunctionExpressionSyntax:
                    return null;
            }
        return null;
    }

    private static IMethodSymbol GetStringCreate(Compilation compilation)
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

    #region Interpolated string -> string.Create
    private static ImmutableArray<CodeActionInfo> Wrap(InterpolatedStringExpressionSyntax interpolatedString, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        // A constant interpolation - no holes, or a const/attribute/pattern context - builds nothing at runtime to move off the pool
        if (semanticModel.GetConstantValue(interpolatedString, cancellationToken).HasValue)
            return [];

        // Converted to a handler, FormattableString or IFormattable, the interpolation is not producing a string here at all
        if (semanticModel.GetTypeInfo(interpolatedString, cancellationToken).ConvertedType is not { } convertedType
            || IsInterpolationTarget(convertedType, semanticModel.Compilation))
            return [];

        // CS4007: both the handler and the Span<char> are ref structs, so neither survives an await the bare interpolation compiles fine across
        foreach (var descendant in interpolatedString.DescendantNodes())
            if (descendant is AwaitExpressionSyntax)
                return [];

        if (IsUnsafeSite(interpolatedString, semanticModel, cancellationToken) || LowersToConcat(interpolatedString, semanticModel, cancellationToken))
            return [];

        var length = EstimateBufferLength(interpolatedString, semanticModel, cancellationToken);
        return [new CodeActionInfo($"Change to 'string.Create' over a {length}-char stack buffer", editor => ReplaceWithStringCreateAsync(editor, interpolatedString, length), "ChangeToStringCreate")];
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

    /// <summary>
    /// Gets whether a <see langword="stackalloc"/> at this position would fail to compile or would allocate more than once per frame.
    /// </summary>
    private static bool IsUnsafeSite(InterpolatedStringExpressionSyntax interpolatedString, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        var expression = semanticModel.Compilation.GetTypeByMetadataName(ExpressionMetadataName);
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
                // CS8640/CS8952: neither the handler nor the Span<char> can be lifted into an expression tree
                case AnonymousFunctionExpressionSyntax when InheritsFrom(semanticModel.GetTypeInfo(current, cancellationToken).ConvertedType, expression):
                    return true;
                case AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax:
                    crossedFunction = true;
                    break;
                case MemberDeclarationSyntax:
                    return false;
            }
        return false;
    }

    private static bool InheritsFrom(ITypeSymbol type, INamedTypeSymbol baseType)
    {
        if (baseType is null)
            return false;
        for (var current = type; current is not null; current = current.BaseType)
            if (SymbolEqualityComparer.Default.Equals(current, baseType))
                return true;
        return false;
    }

    /// <summary>
    /// Gets whether the compiler already lowers this interpolation to a <see cref="string.Concat(string, string)"/> call, which no handler-based rewrite can improve on.
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
    /// Sizes the buffer to the literal text plus a per-hole worst case, rounded up and clamped to <see cref="MaximumBufferLength"/>.
    /// Only the payoff rides on this: overshooting wastes stack, undershooting lands back on the pool the rewrite exists to avoid, and neither changes the result.
    /// </summary>
    private static int EstimateBufferLength(InterpolatedStringExpressionSyntax interpolatedString, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
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

        // Round up so the emitted literal reads as a buffer size rather than an oddly precise guess
        length = (length + 15) & ~15;
        return Math.Min(Math.Max(length, MinimumBufferLength), MaximumBufferLength);
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

    private static ValueTask ReplaceWithStringCreateAsync(DocumentEditor editor, InterpolatedStringExpressionSyntax interpolatedString, int length)
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
        var invocation = SyntaxFactory.InvocationExpression(memberAccess, argumentList);

        editor.ReplaceNode(interpolatedString, invocation.WithTriviaFrom(interpolatedString).Formatted);
        return ValueTask.CompletedTask;
    }
    #endregion

    #region string.Create -> interpolated string
    private static ImmutableArray<CodeActionInfo> Unwrap(InvocationExpressionSyntax invocation, SemanticModel semanticModel, IMethodSymbol create, CancellationToken cancellationToken)
    {
        // Only the shape this refactoring emits round-trips: a non-null provider formats with a culture the bare interpolation would silently drop
        var arguments = invocation.ArgumentList.Arguments;
        if (arguments.Count != 3
            || arguments[0] is not { NameColon: null, Expression: LiteralExpressionSyntax { RawKind: (int)SyntaxKind.NullLiteralExpression } }
            || arguments[2] is not { NameColon: null, Expression: InterpolatedStringExpressionSyntax handler })
            return [];

        if (semanticModel.GetOperation(invocation, cancellationToken) is not IInvocationOperation operation
            || !SymbolEqualityComparer.Default.Equals(operation.TargetMethod, create))
            return [];

        return [new CodeActionInfo("Change to interpolated string", editor =>
        {
            editor.ReplaceNode(invocation, handler.WithTriviaFrom(invocation).Formatted);
            return ValueTask.CompletedTask;
        }, "ChangeToInterpolatedString")];
    }
    #endregion
}
