namespace LaquaiLib.Analyzers.Fixes.Refactorings;

/// <summary>
/// Changes an explicit reference cast or <see langword="as"/> expression to an <see cref="System.Runtime.CompilerServices.Unsafe.As{T}(object)"/> call, skipping the runtime type check.
/// LAQ0002 only reports casts immediately wrapping a known-sane clone call; this is offered for any explicit reference conversion, since whether the checked cast is worth keeping is the caller's to know.
/// </summary>
[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(ChangeToUnsafeAsRefactor)), Shared]
public sealed class ChangeToUnsafeAsRefactor : LaquaiLibOperationRefactoring
{
    public override ImmutableArray<CodeActionInfo> GetCodeActionInfos(CompilationUnitSyntax compilationUnitSyntax, IOperation operation, TextSpan span)
    {
        if (operation is not IConversionOperation convOp)
            return [];

        var conv = convOp.GetConversion();
        // conv.IsImplicit covers all upcasts (class hierarchy AND interface) - IsBaseOf only walked BaseType chain
        if (conv.IsImplicit)
            return [];

        var operand = convOp.Operand;
        var operandType = operand?.Type;
        var targetType = convOp.Type;

        // Built-in reference conversions always relate two reference types, but so can a user-defined operator
        // between two unrelated classes - either way, Unsafe.As<T>(object) is the right (simpler) overload for it.
        // Everything else that isn't a reference conversion needs to actually reinterpret bits via the ref-based
        // overload, which is only equivalent to the original conversion when a user-defined operator is what we'd
        // be skipping - built-in numeric/enum/unboxing/nullable conversions transform the value rather than just
        // reinterpreting its representation, so offering this refactor for them would silently corrupt data.
        var useSingleTypeOverload = conv.IsReference || (operandType?.IsReferenceType == true && targetType?.IsReferenceType == true);
        if (!useSingleTypeOverload && !conv.IsUserDefined)
            return [];

        var operandIsLValue = operand is not null && IsLValueOperand(operand);
        var syntaxNode = operation.Syntax;

        // A non-lvalue operand needs a temporary hoisted somewhere. An expression-bodied member has no statement to
        // hoist before, but its arrow can be rewritten into a block to make room; failing that (field initializers,
        // lambda expression bodies, etc.) there's nowhere to put it without changing how often it's evaluated, so
        // don't offer a fix we can't actually perform - otherwise the action's AddUsings postfix would still run and
        // leave a stray, unused using behind
        if (!useSingleTypeOverload && !operandIsLValue && FindHoistAnchor(syntaxNode).Kind == HoistAnchorKind.None)
            return [];

        if (syntaxNode is CastExpressionSyntax)
            return [new CodeActionInfo("Change to 'Unsafe.As'", editor => ReplaceWithUnsafeAsAsync(editor, syntaxNode, useSingleTypeOverload, operandType, operandIsLValue), "ChangeToUnsafeAsCall_CastExpressionSyntax", WellKnownPostFixActions.AddUsings("System.Runtime.CompilerServices"))];
        if (syntaxNode is BinaryExpressionSyntax binaryExpr && binaryExpr.IsKind(SyntaxKind.AsExpression))
            return [new CodeActionInfo("Change to 'Unsafe.As'", editor => ReplaceWithUnsafeAsAsync(editor, syntaxNode, useSingleTypeOverload, operandType, operandIsLValue), "ChangeToUnsafeAsCall_AsExpression", WellKnownPostFixActions.AddUsings("System.Runtime.CompilerServices"))];

        return [];
    }
    // Explicitly disallow refactor-all for this since it's dangerous and irresponsible to blindly change all casts to Unsafe.As
    public override RefactorAllProvider GetRefactorAllProvider() => null;

    // Whether 'operand' can be passed as a 'ref' argument as-is (a "moveable variable" in spec terms), so we can skip hoisting a temporary for it
    private static bool IsLValueOperand(IOperation operand) => operand switch
    {
        ILocalReferenceOperation => true,
        IParameterReferenceOperation => true,
        IArrayElementReferenceOperation => true,
        IInstanceReferenceOperation => true,
        IFieldReferenceOperation { Field.IsReadOnly: false } => true,
        IPropertyReferenceOperation { Property.RefKind: not RefKind.None } => true,
        _ => false,
    };

    private enum HoistAnchorKind { None, Statement, ArrowBody }

    // Finds where a hoisted temporary can go without crossing into an enclosing lexical scope - crossing one (e.g.
    // hoisting above a local function or lambda instead of inside it) would change how often the temporary's
    // initializer runs relative to the original per-invocation semantics of that inner scope. A block-bodied
    // lambda/local function/etc. already has its own statement inside that scope, found before any boundary is
    // crossed; an expression-bodied one hits its ArrowExpressionClauseSyntax first instead; anything else that
    // reaches a lambda/anonymous method boundary without finding either has nowhere safe to hoist to
    private static (HoistAnchorKind Kind, SyntaxNode Node) FindHoistAnchor(SyntaxNode expression)
    {
        for (var node = expression.Parent; node is not null; node = node.Parent)
        {
            switch (node)
            {
                case ArrowExpressionClauseSyntax arrow:
                    return (HoistAnchorKind.ArrowBody, arrow);
                case AnonymousFunctionExpressionSyntax:
                    return (HoistAnchorKind.None, null);
                case StatementSyntax statement:
                    return (HoistAnchorKind.Statement, statement);
            }
        }
        return (HoistAnchorKind.None, null);
    }

    // Whether the arrow body is used as a statement rather than a return value - true for void/set-like members and
    // for 'async void'/'async Task' methods, where 'M() => E;' desugars to '{ E; }', not '{ return E; }'
    private static bool IsVoidArrowContext(SyntaxNode arrowOwner) => arrowOwner switch
    {
        MethodDeclarationSyntax m => IsVoidOrFireAndForgetAsync(m.Modifiers, m.ReturnType),
        LocalFunctionStatementSyntax lf => IsVoidOrFireAndForgetAsync(lf.Modifiers, lf.ReturnType),
        AccessorDeclarationSyntax accessor => !accessor.IsKind(SyntaxKind.GetAccessorDeclaration),
        ConstructorDeclarationSyntax or DestructorDeclarationSyntax => true,
        _ => false,
    };
    private static bool IsVoidOrFireAndForgetAsync(SyntaxTokenList modifiers, TypeSyntax returnType)
    {
        if (returnType is PredefinedTypeSyntax { Keyword: var keyword } && keyword.IsKind(SyntaxKind.VoidKeyword))
            return true;
        if (!modifiers.Any(SyntaxKind.AsyncKeyword))
            return false;
        return returnType is IdentifierNameSyntax { Identifier.ValueText: "Task" }
            or QualifiedNameSyntax { Right: IdentifierNameSyntax { Identifier.ValueText: "Task" } };
    }

    // Rewrites the arrow-bodied member owning 'expression' into an equivalent block body so 'temporaryDeclaration' has
    // somewhere to live; 'newInvocationExpression' replaces 'expression' wherever it sits within the arrow's expression
    private static bool TryRewriteArrowBody(DocumentEditor documentEditor, ArrowExpressionClauseSyntax arrowClause, SyntaxNode expression, ExpressionSyntax newInvocationExpression, StatementSyntax temporaryDeclaration)
    {
        var finalExpression = arrowClause.Expression == expression
            ? newInvocationExpression
            : arrowClause.Expression.ReplaceNode(expression, newInvocationExpression);

        StatementSyntax trailingStatement = IsVoidArrowContext(arrowClause.Parent)
            ? SyntaxFactory.ExpressionStatement(finalExpression)
            : SyntaxFactory.ReturnStatement(finalExpression);
        var block = SyntaxFactory.Block(temporaryDeclaration, trailingStatement);

        // Every ArrowExpressionClauseSyntax owner allowed by the C# grammar is covered here
        SyntaxNode newOwner = arrowClause.Parent switch
        {
            MethodDeclarationSyntax m => m.WithExpressionBody(null).WithSemicolonToken(default).WithBody(block),
            LocalFunctionStatementSyntax lf => lf.WithExpressionBody(null).WithSemicolonToken(default).WithBody(block),
            OperatorDeclarationSyntax op => op.WithExpressionBody(null).WithSemicolonToken(default).WithBody(block),
            ConversionOperatorDeclarationSyntax co => co.WithExpressionBody(null).WithSemicolonToken(default).WithBody(block),
            ConstructorDeclarationSyntax ctor => ctor.WithExpressionBody(null).WithSemicolonToken(default).WithBody(block),
            DestructorDeclarationSyntax dtor => dtor.WithExpressionBody(null).WithSemicolonToken(default).WithBody(block),
            AccessorDeclarationSyntax acc => acc.WithExpressionBody(null).WithSemicolonToken(default).WithBody(block),
            PropertyDeclarationSyntax p => p.WithExpressionBody(null).WithSemicolonToken(default)
                .WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.SingletonList(SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, block)))),
            IndexerDeclarationSyntax ix => ix.WithExpressionBody(null).WithSemicolonToken(default)
                .WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.SingletonList(SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, block)))),
            _ => null,
        };
        if (newOwner is null)
            return false;

        documentEditor.ReplaceNode(arrowClause.Parent, newOwner.WithAdditionalAnnotations(Formatter.Annotation));
        return true;
    }

    private static ValueTask ReplaceWithUnsafeAsAsync(DocumentEditor documentEditor, SyntaxNode expression, bool useSingleTypeOverload, ITypeSymbol operandType, bool operandIsLValue)
    {
        if (useSingleTypeOverload)
        {
            ExpressionSyntax replaceTarget = null;
            TypeSyntax targetType = null;
            if (expression is CastExpressionSyntax castExpression)
            {
                replaceTarget = castExpression.Expression;
                targetType = castExpression.Type;
            }
            else if (expression is BinaryExpressionSyntax binaryExpr && binaryExpr.IsKind(SyntaxKind.AsExpression)
                && binaryExpr.OperatorToken.IsKind(SyntaxKind.AsKeyword) && binaryExpr.Right is TypeSyntax typeSyntax)
            {
                replaceTarget = binaryExpr.Left;
                targetType = typeSyntax;
            }

            if (replaceTarget is not null && targetType is not null)
            {
                var genericNameSyntax = SyntaxFactory.GenericName(SyntaxFactory.Identifier("As"), SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList(targetType)));
                var unsafeType = SyntaxFactory.ParseName("Unsafe").WithAdditionalAnnotations(Simplifier.Annotation);
                var memberAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, unsafeType, genericNameSyntax);
                if (replaceTarget is ParenthesizedExpressionSyntax peSyntax)
                    replaceTarget = peSyntax.Expression;
                var argumentList = SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(replaceTarget)));
                var newExpression = SyntaxFactory.InvocationExpression(memberAccess, argumentList).WithAdditionalAnnotations(Formatter.Annotation);

                documentEditor.ReplaceNode(expression, newExpression.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation));
            }
        }
        else if (operandType is not null)
        {
            ExpressionSyntax replaceTarget = null;
            TypeSyntax targetType = null;
            if (expression is CastExpressionSyntax castExpression)
            {
                replaceTarget = castExpression.Expression;
                targetType = castExpression.Type;
            }
            else if (expression is BinaryExpressionSyntax binaryExpr && binaryExpr.IsKind(SyntaxKind.AsExpression)
                && binaryExpr.OperatorToken.IsKind(SyntaxKind.AsKeyword) && binaryExpr.Right is TypeSyntax typeSyntax)
            {
                replaceTarget = binaryExpr.Left;
                targetType = typeSyntax;
            }

            if (replaceTarget is not null && targetType is not null)
            {
                if (replaceTarget is ParenthesizedExpressionSyntax peSyntax)
                    replaceTarget = peSyntax.Expression;

                var sourceType = SyntaxFactory.ParseTypeName(operandType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)).WithAdditionalAnnotations(Simplifier.Annotation);
                var genericNameSyntax = SyntaxFactory.GenericName(SyntaxFactory.Identifier("As"), SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList([sourceType, targetType])));
                var unsafeType = SyntaxFactory.ParseName("Unsafe").WithAdditionalAnnotations(Simplifier.Annotation);
                var memberAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, unsafeType, genericNameSyntax);

                if (operandIsLValue)
                {
                    var argument = SyntaxFactory.Argument(replaceTarget).WithRefKindKeyword(SyntaxFactory.Token(SyntaxKind.RefKeyword));
                    var argumentList = SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(argument));
                    var newExpression = SyntaxFactory.InvocationExpression(memberAccess, argumentList).WithAdditionalAnnotations(Formatter.Annotation);

                    documentEditor.ReplaceNode(expression, newExpression.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation));
                }
                else
                {
                    // A non-lvalue operand (e.g. a call result) can't be passed by 'ref' directly, so it needs a temporary to take the address of
                    var identifier = SyntaxFactory.Identifier("asTarget").WithAdditionalAnnotations(RenameAnnotation.Create());
                    var declarator = SyntaxFactory.VariableDeclarator(identifier).WithInitializer(SyntaxFactory.EqualsValueClause(replaceTarget.WithoutTrivia()));
                    var declaration = SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var"), SyntaxFactory.SingletonSeparatedList(declarator));
                    var temporaryDeclaration = SyntaxFactory.LocalDeclarationStatement(declaration).WithAdditionalAnnotations(Formatter.Annotation);

                    var refArgument = SyntaxFactory.IdentifierName(identifier.WithoutTrivia());
                    var argument = SyntaxFactory.Argument(refArgument).WithRefKindKeyword(SyntaxFactory.Token(SyntaxKind.RefKeyword));
                    var argumentList = SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(argument));
                    var newExpression = SyntaxFactory.InvocationExpression(memberAccess, argumentList).WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation);

                    var (anchorKind, anchorNode) = FindHoistAnchor(expression);
                    if (anchorKind == HoistAnchorKind.Statement)
                    {
                        documentEditor.InsertBefore(anchorNode, temporaryDeclaration);
                        documentEditor.ReplaceNode(expression, newExpression);
                    }
                    else if (anchorKind == HoistAnchorKind.ArrowBody)
                    {
                        TryRewriteArrowBody(documentEditor, (ArrowExpressionClauseSyntax)anchorNode, expression, newExpression, temporaryDeclaration);
                    }
                }
            }
        }

        return ValueTask.CompletedTask;
    }
}
