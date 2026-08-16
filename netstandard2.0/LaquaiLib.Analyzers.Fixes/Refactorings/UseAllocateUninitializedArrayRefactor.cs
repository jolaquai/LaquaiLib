namespace LaquaiLib.Analyzers.Fixes.Refactorings;

/// <summary>
/// Switches a single-dimensional array creation between the zeroing <c>new T[length]</c> form and <c>GC.AllocateUninitializedArray&lt;T&gt;(length)</c>, in either direction.
/// LAQ0006 only reports the sites where skipping the zeroing is guaranteed to pay off; this is offered wherever the switch compiles at all, since whether the initial contents matter is the caller's to know.
/// No "Refactor All" anchors are declared for the same reason: converting every array creation in a scope would drop the zeroing guarantee at sites nobody looked at.
/// </summary>
[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(UseAllocateUninitializedArrayRefactor)), Shared]
public sealed class UseAllocateUninitializedArrayRefactor : LaquaiLibRefactoring
{
    private const string MethodName = "AllocateUninitializedArray";

    public override async ValueTask<ImmutableArray<CodeActionInfo>> GetCodeActionInfosAsync(Document document, CompilationUnitSyntax compilationUnitSyntax, TextSpan span, CancellationToken cancellationToken)
    {
        if (FindTarget(compilationUnitSyntax.FindNode(span, getInnermostNodeForTie: true)) is not { } target)
        {
            return [];
        }

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        // A target framework that predates the method would only get code that doesn't compile
        if (GetAllocateUninitializedArray(semanticModel.Compilation) is not { } allocate)
        {
            return [];
        }

        return target switch
        {
            ArrayCreationExpressionSyntax arrayCreation => Uninitialize(semanticModel, arrayCreation, allocate.ContainingType, cancellationToken),
            InvocationExpressionSyntax invocation => Zero(semanticModel, invocation, allocate, cancellationToken),
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
                case ArrayCreationExpressionSyntax or InvocationExpressionSyntax:
                    return (ExpressionSyntax)current;
                case StatementSyntax or MemberDeclarationSyntax or AnonymousFunctionExpressionSyntax:
                    return null;
            }
        }
        return null;
    }

    private static IMethodSymbol GetAllocateUninitializedArray(Compilation compilation)
    {
        if (compilation.GetTypeByMetadataName("System.GC") is not { } gc)
        {
            return null;
        }
        foreach (var member in gc.GetMembers(MethodName))
        {
            if (member is IMethodSymbol { IsStatic: true, Arity: 1, DeclaredAccessibility: Accessibility.Public } method
                && method.Parameters.Length > 0
                && method.Parameters[0].Type.SpecialType == SpecialType.System_Int32)
            {
                return method;
            }
        }
        return null;
    }

    private static ImmutableArray<CodeActionInfo> Uninitialize(SemanticModel semanticModel, ArrayCreationExpressionSyntax arrayCreation, INamedTypeSymbol gc, CancellationToken cancellationToken)
    {
        // The method takes a length and hands back a T[], so an initializer, a jagged type or a multi-dimensional one has nothing to rewrite to
        var rankSpecifiers = arrayCreation.Type.RankSpecifiers;
        if (arrayCreation.Initializer is not null || rankSpecifiers.Count != 1 || rankSpecifiers[0].Sizes.Count != 1)
        {
            return [];
        }

        var size = rankSpecifiers[0].Sizes[0];
        if (size is OmittedArraySizeExpressionSyntax)
        {
            return [];
        }

        var elementType = arrayCreation.Type.ElementType;
        // Same gate as LAQ0006: everything IsReferenceOrContainsReferences<T>() accepts is handed straight back to 'new T[length]', and pointers cannot be type arguments in the first place
        if (semanticModel.GetTypeInfo(elementType, cancellationToken).Type is not { IsValueType: true, IsUnmanagedType: true, IsRefLikeType: false } type
            || type.TypeKind is TypeKind.Pointer or TypeKind.FunctionPointer)
        {
            return [];
        }

        var receiver = GetReceiver(semanticModel, arrayCreation.SpanStart, gc);
        return [new CodeActionInfo("Change to 'GC.AllocateUninitializedArray'", editor => ReplaceWithAllocateUninitializedArrayAsync(editor, arrayCreation, elementType, size, receiver), "ChangeToAllocateUninitializedArray")];
    }

    /// <summary>
    /// Spells the receiver as bare <c>GC</c> where that name binds to <see cref="System.GC"/> at <paramref name="position"/> and fully qualifies it everywhere else.
    /// The Simplifier cannot do this for us: it does not reduce a member access sitting in expression position, and a qualified name in that position is not a shape the reducer binds at all.
    /// </summary>
    private static ExpressionSyntax GetReceiver(SemanticModel semanticModel, int position, INamedTypeSymbol gc)
    {
        var bound = semanticModel.GetSpeculativeSymbolInfo(position, SyntaxFactory.IdentifierName("GC"), SpeculativeBindingOption.BindAsExpression).Symbol;
        return SymbolEqualityComparer.Default.Equals(bound, gc)
            ? SyntaxFactory.IdentifierName("GC")
            : SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName("System"), SyntaxFactory.IdentifierName("GC"));
    }

    private static ImmutableArray<CodeActionInfo> Zero(SemanticModel semanticModel, InvocationExpressionSyntax invocation, IMethodSymbol allocate, CancellationToken cancellationToken)
    {
        if (semanticModel.GetOperation(invocation, cancellationToken) is not IInvocationOperation operation
            || !SymbolEqualityComparer.Default.Equals(operation.TargetMethod.OriginalDefinition, allocate))
        {
            return [];
        }

        ExpressionSyntax length = null;
        foreach (var argument in operation.Arguments)
        {
            if (argument.Parameter is not { } parameter)
            {
                continue;
            }
            if (parameter.Name == "length")
            {
                length = (argument.Syntax as ArgumentSyntax)?.Expression;
            }
            // 'new T[length]' cannot allocate into the pinned object heap, so anything but a provably unpinned allocation would silently lose its pinning
            else if (parameter.Name == "pinned" && argument.ArgumentKind is not ArgumentKind.DefaultValue && argument.Value.ConstantValue is not { HasValue: true, Value: false })
            {
                return [];
            }
        }
        if (length is null || GetTypeArgument(invocation.Expression) is not TypeSyntax elementType)
        {
            return [];
        }

        return [new CodeActionInfo($"Change to 'new {elementType}[]'", editor => ReplaceWithArrayCreationAsync(editor, invocation, elementType, length), "ChangeToZeroInitializingArrayCreation")];
    }

    /// <summary>
    /// Pulls <c>T</c> off the invoked name. No parameter mentions it, so it is never inferred and always spelled out in source; taking it from there keeps whatever spelling the caller chose.
    /// </summary>
    private static TypeSyntax GetTypeArgument(ExpressionSyntax invoked)
    {
        var name = invoked switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name,
            SimpleNameSyntax simpleName => simpleName,
            _ => null
        };
        return name is GenericNameSyntax generic && generic.TypeArgumentList.Arguments.Count == 1 ? generic.TypeArgumentList.Arguments[0] : null;
    }

    private static ValueTask ReplaceWithAllocateUninitializedArrayAsync(DocumentEditor editor, ArrayCreationExpressionSyntax arrayCreation, TypeSyntax elementType, ExpressionSyntax size, ExpressionSyntax receiver)
    {
        var genericName = SyntaxFactory.GenericName(
            SyntaxFactory.Identifier(MethodName),
            SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList(elementType.WithoutTrivia()))
        );
        var memberAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, receiver, genericName);
        var argumentList = SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(size.WithoutTrivia())));
        var invocation = SyntaxFactory.InvocationExpression(memberAccess, argumentList);

        editor.ReplaceNode(arrayCreation, invocation.WithTriviaFrom(arrayCreation).Formatted);
        return ValueTask.CompletedTask;
    }

    private static ValueTask ReplaceWithArrayCreationAsync(DocumentEditor editor, InvocationExpressionSyntax invocation, TypeSyntax elementType, ExpressionSyntax length)
    {
        var arrayType = SyntaxFactory.ArrayType(
            elementType.WithoutTrivia(),
            SyntaxFactory.SingletonList(SyntaxFactory.ArrayRankSpecifier(SyntaxFactory.SingletonSeparatedList(length.WithoutTrivia())))
        );
        // The factory's `new` carries no trivia of its own, so it would run straight into the element type
        var arrayCreation = SyntaxFactory.ArrayCreationExpression(SyntaxFactory.Token(SyntaxKind.NewKeyword).WithTrailingTrivia(SyntaxFactory.Space), arrayType, null);

        editor.ReplaceNode(invocation, arrayCreation.WithTriviaFrom(invocation).Formatted);
        return ValueTask.CompletedTask;
    }
}
