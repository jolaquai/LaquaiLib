namespace LaquaiLib.Analyzers.Fixes.Refactorings;

/// <summary>
/// Offered on a lambda's <c>=></c> token, extracts the lambda into a new method on the enclosing type and replaces the lambda expression with a method group reference to it.
/// Only offered when the lambda's own natural signature is fully resolved and it captures nothing from the enclosing scope but (optionally) <see langword="this"/> - anything else captured
/// would need to become closure state that a plain method has nowhere to keep, so those lambdas are left alone rather than mis-converted.
/// </summary>
[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(ConvertLambdaToMethodRefactor)), Shared]
public sealed class ConvertLambdaToMethodRefactor : LaquaiLibRefactoring
{
    public override async ValueTask<ImmutableArray<CodeActionInfo>> GetCodeActionInfosAsync(Document document, CompilationUnitSyntax compilationUnitSyntax, TextSpan span, CancellationToken cancellationToken)
    {
        var token = compilationUnitSyntax.FindToken(span.Start);
        if (!token.IsKind(SyntaxKind.EqualsGreaterThanToken) || token.Parent is not LambdaExpressionSyntax lambda)
            return [];

        if (lambda.FirstAncestorOrSelf<TypeDeclarationSyntax>() is not { } typeDeclaration)
            return [];

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel.GetSymbolInfo(lambda, cancellationToken).Symbol is not IMethodSymbol methodSymbol)
            return [];

        // Duplicate discard parameters are only legal on a lambda, never on an ordinary method
        var parameters = methodSymbol.Parameters;
        var discardCount = 0;
        for (var i = 0; i < parameters.Length; i++)
            if (parameters[i].Name == "_" && ++discardCount > 1)
                return [];

        if (!TryGetRequiredInstance(semanticModel, lambda, out var needsInstance))
            return [];
        if (needsInstance && typeDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
            return [];

        var member = lambda.Ancestors().OfType<MemberDeclarationSyntax>().FirstOrDefault(m => m.Parent == typeDeclaration);
        if (member is null)
            return [];

        var name = GetUniqueMethodName(typeDeclaration, GetBaseName(semanticModel, lambda, cancellationToken));
        return [new CodeActionInfo("Convert to method", editor => ConvertAsync(editor, member, lambda, methodSymbol, needsInstance, name), "ConvertLambdaToMethod")];
    }

    /// <summary>
    /// Determines whether <paramref name="lambda"/> can become a method at all, and if so, whether that method must be an instance method.
    /// Fails (returns <see langword="false"/>) if the lambda captures anything from the enclosing scope other than <see langword="this"/>.
    /// </summary>
    private static bool TryGetRequiredInstance(SemanticModel semanticModel, LambdaExpressionSyntax lambda, out bool needsInstance)
    {
        needsInstance = false;

        var dataFlow = semanticModel.AnalyzeDataFlow(lambda);
        if (dataFlow is not { Succeeded: true })
            return false;

        var captured = dataFlow.CapturedInside;
        for (var i = 0; i < captured.Length; i++)
        {
            if (captured[i] is IParameterSymbol { IsThis: true })
            {
                needsInstance = true;
                continue;
            }
            return false;
        }
        return true;
    }

    private static string GetBaseName(SemanticModel semanticModel, LambdaExpressionSyntax lambda, CancellationToken cancellationToken)
    {
        var baseName = lambda.Parent switch
        {
            EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax declarator } => declarator.Identifier.ValueText,
            AssignmentExpressionSyntax { Left: IdentifierNameSyntax identifier } assignment when assignment.Right == lambda => identifier.Identifier.ValueText,
            AssignmentExpressionSyntax { Left: MemberAccessExpressionSyntax memberAccess } assignment when assignment.Right == lambda => memberAccess.Name.Identifier.ValueText,
            ArgumentSyntax { NameColon.Name.Identifier.ValueText: var named } => named,
            ArgumentSyntax { Parent: ArgumentListSyntax { Parent: InvocationExpressionSyntax invocation } argumentList } argument
                when semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol is IMethodSymbol invoked
                    && argumentList.Arguments.IndexOf(argument) is var index and >= 0 && index < invoked.Parameters.Length
                => invoked.Parameters[index].Name,
            _ => "Lambda"
        };

        baseName = baseName.TrimStart('_');
        if (baseName.Length == 0)
            return "Lambda";
        return char.IsUpper(baseName[0]) ? baseName : char.ToUpperInvariant(baseName[0]) + baseName.Substring(1);
    }

    private static string GetUniqueMethodName(TypeDeclarationSyntax typeDeclaration, string baseName)
    {
        var existing = new HashSet<string>(StringComparer.Ordinal);
        foreach (var member in typeDeclaration.Members)
        {
            switch (member)
            {
                case MethodDeclarationSyntax m:
                    existing.Add(m.Identifier.ValueText);
                    break;
                case PropertyDeclarationSyntax p:
                    existing.Add(p.Identifier.ValueText);
                    break;
                case EventDeclarationSyntax e:
                    existing.Add(e.Identifier.ValueText);
                    break;
                case BaseTypeDeclarationSyntax t:
                    existing.Add(t.Identifier.ValueText);
                    break;
                case FieldDeclarationSyntax f:
                    foreach (var declarator in f.Declaration.Variables)
                        existing.Add(declarator.Identifier.ValueText);
                    break;
                case EventFieldDeclarationSyntax ef:
                    foreach (var declarator in ef.Declaration.Variables)
                        existing.Add(declarator.Identifier.ValueText);
                    break;
            }
        }

        if (!existing.Contains(baseName))
            return baseName;
        var suffix = 2;
        while (existing.Contains(baseName + suffix))
            suffix++;
        return baseName + suffix;
    }

    private static ValueTask ConvertAsync(DocumentEditor editor, MemberDeclarationSyntax member, LambdaExpressionSyntax lambda, IMethodSymbol methodSymbol, bool needsInstance, string name)
    {
        var modifiers = new List<SyntaxToken> { SyntaxFactory.Token(SyntaxKind.PrivateKeyword) };
        if (!needsInstance)
            modifiers.Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword));
        if (methodSymbol.IsAsync)
            modifiers.Add(SyntaxFactory.Token(SyntaxKind.AsyncKeyword));

        var parameterList = SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(methodSymbol.Parameters.Select(p => BuildParameter(p))));
        var returnType = BuildTypeSyntax(methodSymbol.ReturnType);

        var method = SyntaxFactory.MethodDeclaration(returnType, name)
            .WithModifiers(SyntaxFactory.TokenList(modifiers))
            .WithParameterList(parameterList);

        method = lambda.Body switch
        {
            BlockSyntax block => method.WithBody(block),
            ExpressionSyntax expression => method
                .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(expression))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
            _ => method
        };

        editor.InsertAfter(member, method.Formatted);
        editor.ReplaceNode(lambda, SyntaxFactory.IdentifierName(name).WithTriviaFrom(lambda).Formatted);
        return ValueTask.CompletedTask;
    }

    private static ParameterSyntax BuildParameter(IParameterSymbol parameter)
    {
        var syntax = SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameter.Name)).WithType(BuildTypeSyntax(parameter.Type).WithTrailingTrivia(SyntaxFactory.Space));
        var refKeyword = parameter.RefKind switch
        {
            RefKind.Ref => SyntaxKind.RefKeyword,
            RefKind.Out => SyntaxKind.OutKeyword,
            RefKind.In => SyntaxKind.InKeyword,
            _ => SyntaxKind.None
        };
        return refKeyword != SyntaxKind.None ? syntax.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(refKeyword).WithTrailingTrivia(SyntaxFactory.Space))) : syntax;
    }

    private static TypeSyntax BuildTypeSyntax(ITypeSymbol type) => SyntaxFactory.ParseTypeName(type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
}
