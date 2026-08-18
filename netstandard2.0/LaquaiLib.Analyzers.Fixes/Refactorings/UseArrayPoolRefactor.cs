namespace LaquaiLib.Analyzers.Fixes.Refactorings;

/// <summary>
/// Switches a single-dimensional array creation between the GC-owned <c>new T[length]</c> form and a pooled <c>ArrayPool&lt;T&gt;.Shared.Rent(length)</c> / <c>Return</c> pair, in either direction.
/// Also recognizes the "stack-or-pool" idiom <c>condition ? stackalloc T[length] : new T[length]</c> assigned to a <see cref="System.Span{T}"/> local, pooling just the array branch in place and wrapping the rest of the block in a <see langword="try"/>/<see langword="finally"/> that only returns what was actually rented.
/// Deciding whether an array is safe to pool is escape analysis in the general case, which nothing here attempts to solve. Instead this is only offered where the array is a local declared directly in a block and every later use in that block is one this rewrite can account for -
/// anything that leaves the block (returned, stored in a field, captured by a lambda, passed by <see langword="ref"/>/<see langword="out"/>) is left alone rather than silently mis-rewritten.
/// <see cref="System.Buffers.ArrayPool{T}.Rent(int)"/> does not guarantee the returned array's <c>.Length</c> equals the request, so any <c>.Length</c> read is rebound to the requested length instead of being left to observe the oversized buffer.
/// No "Refactor All" anchors are declared for the same reason: pooling changes ownership semantics at each site, so a blanket sweep is exactly the mistake this refactoring exists to avoid making automatically.
/// </summary>
[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(UseArrayPoolRefactor)), Shared]
public sealed class UseArrayPoolRefactor : LaquaiLibRefactoring
{
    private const string ArrayPoolMetadataName = "System.Buffers.ArrayPool`1";

    public override async ValueTask<ImmutableArray<CodeActionInfo>> GetCodeActionInfosAsync(Document document, CompilationUnitSyntax compilationUnitSyntax, TextSpan span, CancellationToken cancellationToken)
    {
        if (FindTarget(compilationUnitSyntax.FindNode(span, getInnermostNodeForTie: true)) is not { } target)
        {
            return [];
        }

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        // A target framework that predates the type would only get code that doesn't compile
        if (semanticModel.Compilation.GetTypeByMetadataName(ArrayPoolMetadataName) is not { } arrayPool)
        {
            return [];
        }

        return target switch
        {
            ArrayCreationExpressionSyntax arrayCreation => Pool(semanticModel, arrayCreation, arrayPool, cancellationToken),
            InvocationExpressionSyntax invocation => Unpool(semanticModel, invocation, arrayPool, cancellationToken),
            StackAllocArrayCreationExpressionSyntax stackAlloc => PoolConditional(semanticModel, GetConditionalParent(stackAlloc), arrayPool, cancellationToken),
            _ => []
        };
    }

    /// <summary>
    /// Walks out of the size expression or the argument list so the caret only has to sit somewhere inside the creation, but never past the expression it sits in.
    /// </summary>
    private static ExpressionSyntax FindTarget(SyntaxNode node)
    {
        for (var current = node; current is not null; current = current.Parent)
        {
            switch (current)
            {
                case ArrayCreationExpressionSyntax or InvocationExpressionSyntax or StackAllocArrayCreationExpressionSyntax:
                    return (ExpressionSyntax)current;
                case StatementSyntax or MemberDeclarationSyntax or AnonymousFunctionExpressionSyntax:
                    return null;
            }
        }
        return null;
    }

    #region new T[length] -> ArrayPool<T>.Shared.Rent(length)
    private static ImmutableArray<CodeActionInfo> Pool(SemanticModel semanticModel, ArrayCreationExpressionSyntax arrayCreation, INamedTypeSymbol arrayPool, CancellationToken cancellationToken)
    {
        // 'condition ? stackalloc T[n] : new T[n]' is a different shape with a different rewrite - the plain gates below all assume a bare declarator initializer, which this is not
        if (GetConditionalParent(arrayCreation) is { } conditional)
        {
            return PoolConditional(semanticModel, conditional, arrayPool, cancellationToken);
        }

        // ArrayPool<T> only hands out single-dimensional arrays
        var rankSpecifiers = arrayCreation.Type.RankSpecifiers;
        if (rankSpecifiers.Count != 1 || rankSpecifiers[0].Sizes.Count != 1)
        {
            return [];
        }

        var initializer = arrayCreation.Initializer;
        var size = rankSpecifiers[0].Sizes[0];
        // Without an initializer to source a length from, an omitted size leaves nothing to rent
        if (initializer is null && size is OmittedArraySizeExpressionSyntax)
        {
            return [];
        }

        var elementType = arrayCreation.Type.ElementType;
        // Pointer/function pointer types cannot be used as a type argument (CS0306), so ArrayPool<T> has nothing to bind T to
        if (semanticModel.GetTypeInfo(elementType, cancellationToken).Type is not { } elementTypeSymbol
            || elementTypeSymbol.TypeKind is TypeKind.Pointer or TypeKind.FunctionPointer)
        {
            return [];
        }

        // Renting only pays off for a local whose entire remaining lifetime is visible right here
        if (arrayCreation.Parent is not EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax declarator }
            || declarator.Parent is not VariableDeclarationSyntax { Variables.Count: 1, Parent: LocalDeclarationStatementSyntax localDeclaration }
            || localDeclaration.Parent is not BlockSyntax block)
        {
            return [];
        }

        if (semanticModel.GetDeclaredSymbol(declarator, cancellationToken) is not ILocalSymbol local)
        {
            return [];
        }

        var rest = block.Statements.Skip(block.Statements.IndexOf(localDeclaration) + 1).ToImmutableArray();
        if (!IsSafeToPool(semanticModel, local, rest, cancellationToken, out var lengthAccesses))
        {
            return [];
        }

        return [new CodeActionInfo("Change to 'ArrayPool<T>.Shared.Rent'", editor => PoolAsync(editor, block, localDeclaration, size, elementType, initializer, lengthAccesses), "ChangeToArrayPoolRent", WellKnownPostFixActions.AddUsings("System.Buffers"))];
    }

    /// <summary>
    /// Bails on any reference to <paramref name="local"/> whose surrounding shape this rewrite cannot account for: captured by a lambda or local function that could outlive the block,
    /// handed to <see langword="return"/>/<see langword="yield return"/>, stored somewhere other than another local, or passed by <see langword="ref"/>/<see langword="out"/>.
    /// <c>.Length</c> reads are not a bail condition - they are collected in <paramref name="lengthAccesses"/> for the caller to rebind instead, since <see cref="System.Buffers.ArrayPool{T}.Rent(int)"/> only guarantees a length of at least what was asked for.
    /// </summary>
    private static bool IsSafeToPool(SemanticModel semanticModel, ILocalSymbol local, ImmutableArray<StatementSyntax> statements, CancellationToken cancellationToken, out ImmutableArray<MemberAccessExpressionSyntax> lengthAccesses)
    {
        var accesses = ImmutableArray.CreateBuilder<MemberAccessExpressionSyntax>();
        for (var s = 0; s < statements.Length; s++)
        {
            var statement = statements[s];
            foreach (var identifier in statement.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>())
            {
                if (identifier.Identifier.ValueText != local.Name
                    || !SymbolEqualityComparer.Default.Equals(semanticModel.GetSymbolInfo(identifier, cancellationToken).Symbol, local))
                {
                    continue;
                }

                for (var ancestor = identifier.Parent; ancestor is not null && ancestor != statement.Parent; ancestor = ancestor.Parent)
                {
                    if (ancestor is AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax)
                    {
                        lengthAccesses = default;
                        return false;
                    }
                }

                if (identifier.Parent is ReturnStatementSyntax or YieldStatementSyntax)
                {
                    lengthAccesses = default;
                    return false;
                }
                if (identifier.Parent is MemberAccessExpressionSyntax { Name.Identifier.ValueText: "Length" } memberAccess && memberAccess.Expression == identifier)
                {
                    accesses.Add(memberAccess);
                    continue;
                }
                if (identifier.Parent is ArgumentSyntax argument && !argument.RefKindKeyword.IsKind(SyntaxKind.None))
                {
                    lengthAccesses = default;
                    return false;
                }
                // A bare identifier on the left can just as easily be a field or property as a local, so the symbol - not the syntax shape - decides
                if (identifier.Parent is AssignmentExpressionSyntax assignment && assignment.Right == identifier
                    && (assignment.Left is not IdentifierNameSyntax leftName || semanticModel.GetSymbolInfo(leftName, cancellationToken).Symbol is not ILocalSymbol))
                {
                    lengthAccesses = default;
                    return false;
                }
            }
        }
        lengthAccesses = accesses.ToImmutable();
        return true;
    }

    private static ValueTask PoolAsync(DocumentEditor editor, BlockSyntax block, LocalDeclarationStatementSyntax localDeclaration, ExpressionSyntax size, TypeSyntax elementType, InitializerExpressionSyntax initializer, ImmutableArray<MemberAccessExpressionSyntax> lengthAccesses)
    {
        // Captured up front, before anything gets rebuilt - the statement order and count below this index never changes, only the content of individual statements does
        var index = block.Statements.IndexOf(localDeclaration);
        var declaratorIdentifier = localDeclaration.Declaration.Variables[0].Identifier;

        // Rent has no overload that seeds contents, but the element count is already known, so the seed values become individual stores instead
        var rentSize = size;
        IEnumerable<StatementSyntax> leadingStatements = [];
        if (initializer is not null)
        {
            var elements = initializer.Expressions;
            rentSize = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(elements.Count));
            leadingStatements = BuildInitializerAssignments(declaratorIdentifier, elements);
        }

        // '.Length' reads observe Rent's oversized buffer, not the request, so they get rebound to whatever the request actually was
        StatementSyntax lengthCapture = null;
        SyntaxToken? capturedLengthIdentifier = null;
        if (lengthAccesses.Length > 0 && rentSize is not LiteralExpressionSyntax)
        {
            // A non-constant size expression can't just be repeated at every '.Length' site - it may not be pure, or may not even still evaluate to the same value later
            var identifier = SyntaxFactory.Identifier($"{declaratorIdentifier.ValueText}Length").WithAdditionalAnnotations(RenameAnnotation.Create());
            var declaration = SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var"), SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator(identifier).WithInitializer(SyntaxFactory.EqualsValueClause(rentSize.WithoutTrivia()))));
            lengthCapture = SyntaxFactory.LocalDeclarationStatement(declaration);
            capturedLengthIdentifier = identifier;
            // Rent itself must read the captured value too, or the size expression evaluates twice
            rentSize = SyntaxFactory.IdentifierName(identifier);
        }

        // Rebinding has to happen here, while 'lengthAccesses' are still at their original tree position - ReplaceNodes matches by node identity,
        // which does not survive relocating the containing statements into a freshly constructed block below
        var reboundBlock = block;
        if (lengthAccesses.Length > 0)
        {
            var literalToken = rentSize is LiteralExpressionSyntax literal ? literal.Token : default(SyntaxToken?);
            reboundBlock = block.ReplaceNodes(lengthAccesses, (_, _) => capturedLengthIdentifier is { } captured
                ? SyntaxFactory.IdentifierName(captured)
                : SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, literalToken!.Value));
        }

        var statements = reboundBlock.Statements;
        var freshLocalDeclaration = (LocalDeclarationStatementSyntax)statements[index];
        var freshRest = statements.Skip(index + 1);
        var declarator = freshLocalDeclaration.Declaration.Variables[0];

        var newDeclaration = freshLocalDeclaration.ReplaceNode(declarator.Initializer.Value, ArrayPoolInvocation(elementType, "Rent", rentSize.WithoutTrivia()));

        var identifierName = SyntaxFactory.IdentifierName(declarator.Identifier.WithoutTrivia());
        var finallyClause = SyntaxFactory.FinallyClause(SyntaxFactory.Block(SyntaxFactory.ExpressionStatement(ArrayPoolInvocation(elementType, "Return", identifierName))));
        // The stores must run inside the try, or an exception mid-initializer leaks the rented array instead of returning it
        var tryStatement = SyntaxFactory.TryStatement(SyntaxFactory.Block(leadingStatements.Concat(freshRest)), SyntaxFactory.List<CatchClauseSyntax>(), finallyClause);

        IEnumerable<StatementSyntax> afterDeclaration = lengthCapture is not null ? [lengthCapture, newDeclaration] : [newDeclaration];
        var before = statements.Take(index);
        var newBlock = reboundBlock.WithStatements(SyntaxFactory.List(before.Concat(afterDeclaration).Append(tryStatement)));

        editor.ReplaceNode(block, newBlock.Formatted);
        return ValueTask.CompletedTask;
    }

    private static StatementSyntax[] BuildInitializerAssignments(SyntaxToken identifier, SeparatedSyntaxList<ExpressionSyntax> elements)
    {
        var statements = new StatementSyntax[elements.Count];
        for (var i = 0; i < elements.Count; i++)
        {
            var index = SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(i)));
            var target = SyntaxFactory.ElementAccessExpression(SyntaxFactory.IdentifierName(identifier.WithoutTrivia()), SyntaxFactory.BracketedArgumentList(SyntaxFactory.SingletonSeparatedList(index)));
            statements[i] = SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, target, elements[i].WithoutTrivia()));
        }
        return statements;
    }

    private static InvocationExpressionSyntax ArrayPoolInvocation(TypeSyntax elementType, string methodName, ExpressionSyntax argument)
    {
        var arrayPoolType = SyntaxFactory.GenericName(SyntaxFactory.Identifier("ArrayPool"), SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList(elementType.WithoutTrivia())));
        var shared = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, arrayPoolType, SyntaxFactory.IdentifierName("Shared"));
        var method = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, shared, SyntaxFactory.IdentifierName(methodName));
        return SyntaxFactory.InvocationExpression(method, SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(argument))));
    }
    #endregion

    #region condition ? stackalloc T[length] : new T[length] -> stack-or-pool
    /// <summary>
    /// Finds the <see cref="ConditionalExpressionSyntax"/> <paramref name="expression"/> sits in, if any, walking out through parentheses first.
    /// </summary>
    private static ConditionalExpressionSyntax GetConditionalParent(ExpressionSyntax expression)
    {
        SyntaxNode current = expression;
        while (current.Parent is ParenthesizedExpressionSyntax parenthesized)
        {
            current = parenthesized;
        }
        return current.Parent as ConditionalExpressionSyntax;
    }

    private static ExpressionSyntax Unwrap(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
        {
            expression = parenthesized.Expression;
        }
        return expression;
    }

    /// <summary>
    /// Recognizes the "stack-or-pool" idiom - a <see cref="System.Span{T}"/> local seeded from <c>condition ? stackalloc T[n] : new T[n]</c> - and pools the array branch in place.
    /// The stack-allocated branch is left completely untouched: a <see langword="stackalloc"/>'s safe-to-escape scope is tied to the exact block it sits in, so moving it into a
    /// freshly built <see langword="if"/>/<see langword="else"/> one block deeper than the original ternary would trade a compiling program for CS8353. Keeping the same ternary and
    /// only swapping its array-creation branch for <c>(buffer = ArrayPool&lt;T&gt;.Shared.Rent(n)).AsSpan(0, n)</c> keeps every expression at the scope depth it already compiled at.
    /// Unlike the plain array case, <c>.Length</c> on the resulting span never needs rebinding: both branches size it to exactly the requested length via <c>AsSpan(0, length)</c>.
    /// </summary>
    private static ImmutableArray<CodeActionInfo> PoolConditional(SemanticModel semanticModel, ConditionalExpressionSyntax conditional, INamedTypeSymbol arrayPool, CancellationToken cancellationToken)
    {
        if (conditional is null)
        {
            return [];
        }

        var whenTrue = Unwrap(conditional.WhenTrue);
        var whenFalse = Unwrap(conditional.WhenFalse);

        StackAllocArrayCreationExpressionSyntax stackAlloc;
        ArrayCreationExpressionSyntax arrayCreation;
        if (whenTrue is StackAllocArrayCreationExpressionSyntax trueStackAlloc && whenFalse is ArrayCreationExpressionSyntax falseArray)
        {
            (stackAlloc, arrayCreation) = (trueStackAlloc, falseArray);
        }
        else if (whenFalse is StackAllocArrayCreationExpressionSyntax falseStackAlloc && whenTrue is ArrayCreationExpressionSyntax trueArray)
        {
            (stackAlloc, arrayCreation) = (falseStackAlloc, trueArray);
        }
        else
        {
            return [];
        }

        // Rent, like the stack-allocated side, only ever hands back uninitialized storage - neither side has anything to seed from
        if (stackAlloc.Initializer is not null || arrayCreation.Initializer is not null
            || stackAlloc.Type is not ArrayTypeSyntax stackAllocType
            || stackAllocType.RankSpecifiers.Count != 1 || stackAllocType.RankSpecifiers[0].Sizes.Count != 1
            || arrayCreation.Type.RankSpecifiers.Count != 1 || arrayCreation.Type.RankSpecifiers[0].Sizes.Count != 1)
        {
            return [];
        }

        var stackAllocSize = stackAllocType.RankSpecifiers[0].Sizes[0];
        var arraySize = arrayCreation.Type.RankSpecifiers[0].Sizes[0];
        // Two independently-typed-out sizes are only trustworthy as 'the same buffer length' if they are, in fact, written the same way
        if (stackAllocSize is OmittedArraySizeExpressionSyntax || arraySize is OmittedArraySizeExpressionSyntax
            || !SyntaxFactory.AreEquivalent(stackAllocSize, arraySize))
        {
            return [];
        }

        var elementType = semanticModel.GetTypeInfo(arrayCreation.Type.ElementType, cancellationToken).Type;
        var stackAllocElementType = semanticModel.GetTypeInfo(stackAllocType.ElementType, cancellationToken).Type;
        // Pointer/function pointer types cannot be used as a type argument (CS0306), so ArrayPool<T> has nothing to bind T to
        if (elementType is null || elementType.TypeKind is TypeKind.Pointer or TypeKind.FunctionPointer
            || !SymbolEqualityComparer.Default.Equals(elementType, stackAllocElementType))
        {
            return [];
        }

        if (conditional.Parent is not EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax declarator }
            || declarator.Parent is not VariableDeclarationSyntax { Variables.Count: 1, Parent: LocalDeclarationStatementSyntax localDeclaration }
            || localDeclaration.Parent is not BlockSyntax block)
        {
            return [];
        }

        // Only offered for a Span<T> local - that is what both a stackalloc and an array convert to, and it is what the rewrite below preserves
        if (semanticModel.GetDeclaredSymbol(declarator, cancellationToken) is not ILocalSymbol local
            || semanticModel.Compilation.GetTypeByMetadataName("System.Span`1") is not { } spanType
            || local.Type is not INamedTypeSymbol { TypeArguments.Length: 1 } localType
            || !SymbolEqualityComparer.Default.Equals(localType.OriginalDefinition, spanType)
            || !SymbolEqualityComparer.Default.Equals(localType.TypeArguments[0], elementType))
        {
            return [];
        }

        var rest = block.Statements.Skip(block.Statements.IndexOf(localDeclaration) + 1).ToImmutableArray();
        // A Span<T> that spends part of its life as a stackalloc already cannot escape the method (the compiler's ref-safety rules see to that);
        // the same escape gate as the plain-array case is reused anyway, if only to keep behavior conservative and consistent. Its collected '.Length'
        // sites are discarded on purpose - both branches size the span to exactly the request via AsSpan(0, length), so it never needs rebinding here.
        if (!IsSafeToPool(semanticModel, local, rest, cancellationToken, out _))
        {
            return [];
        }

        var elementTypeSyntax = arrayCreation.Type.ElementType;
        return [new CodeActionInfo("Change to stack-or-pool", editor => PoolConditionalAsync(editor, block, localDeclaration, arrayCreation, elementTypeSyntax, arraySize), "ChangeToStackOrPoolRent", WellKnownPostFixActions.AddUsings("System.Buffers", "System"))];
    }

    private static ValueTask PoolConditionalAsync(DocumentEditor editor, BlockSyntax block, LocalDeclarationStatementSyntax localDeclaration, ArrayCreationExpressionSyntax arrayCreation, TypeSyntax elementType, ExpressionSyntax size)
    {
        var spanName = localDeclaration.Declaration.Variables[0].Identifier.ValueText;
        var bufferIdentifier = SyntaxFactory.Identifier($"{spanName}Buffer").WithAdditionalAnnotations(RenameAnnotation.Create());
        var bufferName = SyntaxFactory.IdentifierName(bufferIdentifier);

        var bufferType = SyntaxFactory.ArrayType(elementType.WithoutTrivia(), SyntaxFactory.SingletonList(SyntaxFactory.ArrayRankSpecifier(SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(SyntaxFactory.OmittedArraySizeExpression()))));
        // 'T[] buffer = null;'
        var bufferDeclaration = SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(bufferType,
            SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator(bufferIdentifier).WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression))))));

        // 'new T[size]' -> '(buffer = ArrayPool<T>.Shared.Rent(size)).AsSpan(0, size)' - an assignment's value is the assigned value, so this both
        // records what to return later and produces the span, all within the ternary's own expression - no new block, no ref-safety change
        var rentAssignment = SyntaxFactory.ParenthesizedExpression(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, bufferName, ArrayPoolInvocation(elementType, "Rent", size.WithoutTrivia())));
        var asSpanCall = SyntaxFactory.InvocationExpression(
            SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, rentAssignment, SyntaxFactory.IdentifierName("AsSpan")),
            SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[]
            {
                SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0))),
                SyntaxFactory.Argument(size.WithoutTrivia())
            })));

        var newDeclaration = localDeclaration.ReplaceNode(arrayCreation, asSpanCall);

        // 'finally { if (buffer != null) ArrayPool<T>.Shared.Return(buffer); }' - only the pooled branch has anything to give back
        var returnCall = SyntaxFactory.ExpressionStatement(ArrayPoolInvocation(elementType, "Return", bufferName));
        var notNull = SyntaxFactory.BinaryExpression(SyntaxKind.NotEqualsExpression, bufferName, SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression));
        var finallyClause = SyntaxFactory.FinallyClause(SyntaxFactory.Block(SyntaxFactory.IfStatement(notNull, SyntaxFactory.Block(returnCall))));

        var index = block.Statements.IndexOf(localDeclaration);
        var rest = block.Statements.Skip(index + 1);
        var tryStatement = SyntaxFactory.TryStatement(SyntaxFactory.Block(rest), SyntaxFactory.List<CatchClauseSyntax>(), finallyClause);

        var before = block.Statements.Take(index);
        var newBlock = block.WithStatements(SyntaxFactory.List(before.Append(bufferDeclaration).Append(newDeclaration).Append(tryStatement)));

        editor.ReplaceNode(block, newBlock.Formatted);
        return ValueTask.CompletedTask;
    }
    #endregion

    #region ArrayPool<T>.Shared.Rent(length) -> new T[length]
    private static ImmutableArray<CodeActionInfo> Unpool(SemanticModel semanticModel, InvocationExpressionSyntax invocation, INamedTypeSymbol arrayPool, CancellationToken cancellationToken)
    {
        if (GetRentCall(semanticModel, invocation, arrayPool, cancellationToken) is not (TypeSyntax elementType, ExpressionSyntax size))
        {
            return [];
        }

        if (invocation.Parent is not EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax declarator }
            || declarator.Parent is not VariableDeclarationSyntax { Variables.Count: 1, Parent: LocalDeclarationStatementSyntax localDeclaration }
            || localDeclaration.Parent is not BlockSyntax block)
        {
            return [];
        }

        var statements = block.Statements;
        var index = statements.IndexOf(localDeclaration);
        // Only recognized in the exact shape 'Pool' produces: the try/finally immediately follows the declaration and owns everything left in the block
        if (index < 0 || index + 2 != statements.Count
            || statements[index + 1] is not TryStatementSyntax { Catches.Count: 0, Finally: { } finallyClause } tryStatement)
        {
            return [];
        }

        if (semanticModel.GetDeclaredSymbol(declarator, cancellationToken) is not ILocalSymbol local
            || !IsMatchingReturn(semanticModel, finallyClause, local, arrayPool, cancellationToken))
        {
            return [];
        }

        var (elements, remaining) = size is LiteralExpressionSyntax { Token.Value: int constantSize }
            ? SplitInitializerAssignments(tryStatement.Block.Statements, semanticModel, local, constantSize, cancellationToken)
            : (default, tryStatement.Block.Statements.ToImmutableArray());

        return [new CodeActionInfo($"Change to 'new {elementType}[]'", editor => UnpoolAsync(editor, block, localDeclaration, elementType, size, elements, remaining), "ChangeToZeroInitializingArrayCreation")];
    }

    /// <summary>
    /// Pulls <c>T</c> off <c>ArrayPool&lt;T&gt;</c> in the receiver chain of <paramref name="expression"/>, provided its member name matches <paramref name="methodName"/> off <c>.Shared</c>.
    /// </summary>
    private static TypeSyntax GetArrayPoolElementType(ExpressionSyntax expression, string methodName)
    {
        if (expression is not MemberAccessExpressionSyntax { Expression: MemberAccessExpressionSyntax { Name.Identifier.ValueText: "Shared" } sharedAccess } outer
            || outer.Name.Identifier.ValueText != methodName)
        {
            return null;
        }

        var name = sharedAccess.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name,
            SimpleNameSyntax simpleName => simpleName,
            _ => null
        };
        return name is GenericNameSyntax generic && generic.Identifier.ValueText == "ArrayPool" && generic.TypeArgumentList.Arguments.Count == 1
            ? generic.TypeArgumentList.Arguments[0]
            : null;
    }

    private static (TypeSyntax ElementType, ExpressionSyntax Size)? GetRentCall(SemanticModel semanticModel, InvocationExpressionSyntax invocation, INamedTypeSymbol arrayPool, CancellationToken cancellationToken)
    {
        if (GetArrayPoolElementType(invocation.Expression, "Rent") is not TypeSyntax elementType)
        {
            return null;
        }

        if (semanticModel.GetOperation(invocation, cancellationToken) is not IInvocationOperation operation
            || operation.TargetMethod.Name != "Rent"
            || !SymbolEqualityComparer.Default.Equals(operation.TargetMethod.ContainingType.OriginalDefinition, arrayPool)
            || operation.Instance is not IPropertyReferenceOperation { Property.Name: "Shared" }
            || operation.Arguments.Length != 1
            || (operation.Arguments[0].Syntax as ArgumentSyntax)?.Expression is not ExpressionSyntax size)
        {
            return null;
        }

        return (elementType, size);
    }

    private static bool IsMatchingReturn(SemanticModel semanticModel, FinallyClauseSyntax finallyClause, ILocalSymbol local, INamedTypeSymbol arrayPool, CancellationToken cancellationToken)
    {
        if (finallyClause.Block.Statements.Count != 1
            || finallyClause.Block.Statements[0] is not ExpressionStatementSyntax { Expression: InvocationExpressionSyntax returnInvocation }
            || GetArrayPoolElementType(returnInvocation.Expression, "Return") is null)
        {
            return false;
        }

        if (semanticModel.GetOperation(returnInvocation, cancellationToken) is not IInvocationOperation operation
            || operation.TargetMethod.Name != "Return"
            || !SymbolEqualityComparer.Default.Equals(operation.TargetMethod.ContainingType.OriginalDefinition, arrayPool)
            || operation.Instance is not IPropertyReferenceOperation { Property.Name: "Shared" }
            || operation.Arguments.Length == 0
            || (operation.Arguments[0].Syntax as ArgumentSyntax)?.Expression is not IdentifierNameSyntax arrayArgument)
        {
            return false;
        }

        return SymbolEqualityComparer.Default.Equals(semanticModel.GetSymbolInfo(arrayArgument, cancellationToken).Symbol, local);
    }

    /// <summary>
    /// Recovers the original initializer elements if the try block opens with exactly one <c>arr[i] = value;</c> store per requested slot, in order starting at zero.
    /// Anything short of every slot being written back isn't the shape 'Pool' produces, so it is left as ordinary statements rather than guessed at.
    /// </summary>
    private static (ImmutableArray<ExpressionSyntax> Elements, ImmutableArray<StatementSyntax> Remaining) SplitInitializerAssignments(SyntaxList<StatementSyntax> statements, SemanticModel semanticModel, ILocalSymbol local, int constantSize, CancellationToken cancellationToken)
    {
        var elements = ImmutableArray.CreateBuilder<ExpressionSyntax>();
        var i = 0;
        for (; i < statements.Count && elements.Count < constantSize; i++)
        {
            if (statements[i] is not ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax assignment }
                || assignment.Left is not ElementAccessExpressionSyntax { Expression: IdentifierNameSyntax targetName } elementAccess
                || elementAccess.ArgumentList.Arguments.Count != 1
                || elementAccess.ArgumentList.Arguments[0].Expression is not LiteralExpressionSyntax { Token.Value: int elementIndex }
                || elementIndex != elements.Count
                || targetName.Identifier.ValueText != local.Name
                || !SymbolEqualityComparer.Default.Equals(semanticModel.GetSymbolInfo(targetName, cancellationToken).Symbol, local))
            {
                break;
            }
            elements.Add(assignment.Right);
        }

        if (elements.Count != constantSize)
        {
            return (default, statements.ToImmutableArray());
        }
        return (elements.ToImmutable(), statements.Skip(i).ToImmutableArray());
    }

    private static ValueTask UnpoolAsync(DocumentEditor editor, BlockSyntax block, LocalDeclarationStatementSyntax localDeclaration, TypeSyntax elementType, ExpressionSyntax size, ImmutableArray<ExpressionSyntax> elements, ImmutableArray<StatementSyntax> remaining)
    {
        var declarator = localDeclaration.Declaration.Variables[0];
        // The factory's `new` carries no trivia of its own, so it would run straight into the element type
        var newKeyword = SyntaxFactory.Token(SyntaxKind.NewKeyword).WithTrailingTrivia(SyntaxFactory.Space);

        ArrayCreationExpressionSyntax arrayCreation;
        if (!elements.IsDefault)
        {
            var initializer = SyntaxFactory.InitializerExpression(SyntaxKind.ArrayInitializerExpression, SyntaxFactory.SeparatedList(elements.Select(static e => e.WithoutTrivia())));
            var arrayType = SyntaxFactory.ArrayType(elementType.WithoutTrivia(), SyntaxFactory.SingletonList(SyntaxFactory.ArrayRankSpecifier(SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(SyntaxFactory.OmittedArraySizeExpression()))));
            arrayCreation = SyntaxFactory.ArrayCreationExpression(newKeyword, arrayType, initializer);
        }
        else
        {
            var arrayType = SyntaxFactory.ArrayType(elementType.WithoutTrivia(), SyntaxFactory.SingletonList(SyntaxFactory.ArrayRankSpecifier(SyntaxFactory.SingletonSeparatedList(size.WithoutTrivia()))));
            arrayCreation = SyntaxFactory.ArrayCreationExpression(newKeyword, arrayType, null);
        }

        var newDeclaration = localDeclaration.ReplaceNode(declarator.Initializer.Value, arrayCreation);

        var before = block.Statements.Take(block.Statements.IndexOf(localDeclaration));
        var newBlock = block.WithStatements(SyntaxFactory.List(before.Append(newDeclaration).Concat(remaining)));

        editor.ReplaceNode(block, newBlock.Formatted);
        return ValueTask.CompletedTask;
    }
    #endregion
}
