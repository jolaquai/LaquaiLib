using Microsoft.CodeAnalysis.Editing;

namespace LaquaiLib.Analyzers.Fixes.Performance;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InlineArrowExpressionMethodsAnalyzerFix)), Shared]
public class InlineArrowExpressionMethodsAnalyzerFix() : LaquaiLibTokenFixer("LAQ0003")
{
    public override FixInfo GetFixInfo(CompilationUnitSyntax compilationUnitSyntax, SyntaxToken syntaxToken, Diagnostic diagnostic)
    {
        var node = syntaxToken.Parent;
        while (node is not (ArrowExpressionClauseSyntax or null))
        {
            node = node.Parent;
        }
        if (node is not ArrowExpressionClauseSyntax arrowExpressionClauseSyntax)
        {
            return FixInfo.Empty;
        }

        var fixInfo = FixInfo.Empty;
        switch (node.Parent)
        {
            case MethodDeclarationSyntax methodDeclarationSyntax:
            {
                fixInfo = new FixInfo("Inline method", async e => RewriteDeclaration(e, methodDeclarationSyntax));
                break;
            }
            case PropertyDeclarationSyntax propertyDeclarationSyntax:
            {
                // If the ArrowExpressionClauseSyntax is directly inside a PropertyDeclarationSyntax, we first need to rewrite the expression into 'get => ...' form
                fixInfo = new FixInfo("Inline property", async e => RewriteDeclaration(e, propertyDeclarationSyntax));
                break;
            }
            case AccessorDeclarationSyntax accessorDeclarationSyntax:
            {
                fixInfo = new FixInfo("Inline accessor", async e => RewriteDeclaration(e, accessorDeclarationSyntax));
                break;
            }
            case IndexerDeclarationSyntax indexerDeclarationSyntax:
            {
                // If the ArrowExpressionClauseSyntax is directly inside a PropertyDeclarationSyntax, we first need to rewrite the expression into 'get => ...' form
                fixInfo = new FixInfo("Inline indexer", async e => RewriteDeclaration(e, indexerDeclarationSyntax));
                break;
            }
            case OperatorDeclarationSyntax operatorDeclarationSyntax:
            {
                fixInfo = new FixInfo("Inline operator", async e => RewriteDeclaration(e, operatorDeclarationSyntax));
                break;
            }
            case ConversionOperatorDeclarationSyntax conversionOperatorDeclarationSyntax:
            {
                fixInfo = new FixInfo("Inline conversion operator", async e => RewriteDeclaration(e, conversionOperatorDeclarationSyntax));
                break;
            }
            case LocalFunctionStatementSyntax localFunctionStatementSyntax:
            {
                fixInfo = new FixInfo("Inline local function", async e => RewriteDeclaration(e, localFunctionStatementSyntax));
                break;
            }
        }

        if (!fixInfo.HasFix)
        {
            return FixInfo.Empty;
        }
        PostFixAction += d => WellKnownPostFixActions.AddUsingsIfNotExist(d, "System.Runtime.CompilerServices");

        return fixInfo;
    }

#pragma warning disable IDE0022 // Use expression body for method
    private static void RewriteDeclaration(DocumentEditor editor, PropertyDeclarationSyntax propertyDeclarationSyntax)
    {
        // Create the getter with the arrow expression
        var getter = SyntaxFactory.AccessorDeclaration(
            SyntaxKind.GetAccessorDeclaration,
            SyntaxFactory.List<AttributeListSyntax>(),
            SyntaxFactory.TokenList(),
            SyntaxFactory.Token(SyntaxKind.GetKeyword),
            SyntaxFactory.ArrowExpressionClause(propertyDeclarationSyntax.ExpressionBody.Expression),
            SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        _ = CreateNewNode(getter, getter.AttributeLists, (node, newLists) => node.WithAttributeLists(newLists), out getter);

        // Create the accessor list with just the getter
        var accessorList = SyntaxFactory.AccessorList(SyntaxFactory.SingletonList(getter));

        // Replace the property with one that has the explicit getter
        var newProperty = propertyDeclarationSyntax
            .WithExpressionBody(null)
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None))
            .WithAccessorList(accessorList)
            .WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation);

        editor.ReplaceNode(propertyDeclarationSyntax, newProperty);
    }

    private static void RewriteDeclaration(DocumentEditor editor, IndexerDeclarationSyntax indexerDeclarationSyntax)
    {
        // Create the getter with the arrow expression
        var getter = SyntaxFactory.AccessorDeclaration(
            SyntaxKind.GetAccessorDeclaration,
            SyntaxFactory.List<AttributeListSyntax>(),
            SyntaxFactory.TokenList(),
            SyntaxFactory.Token(SyntaxKind.GetKeyword),
            SyntaxFactory.ArrowExpressionClause(indexerDeclarationSyntax.ExpressionBody.Expression),
            SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        _ = CreateNewNode(getter, getter.AttributeLists, (node, newLists) => node.WithAttributeLists(newLists), out getter);

        // Create the accessor list with just the getter
        var accessorList = SyntaxFactory.AccessorList(
            SyntaxFactory.SingletonList(getter));

        // Replace the indexer with one that has the explicit getter
        var newIndexer = indexerDeclarationSyntax
            .WithExpressionBody(null)
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None))
            .WithAccessorList(accessorList)
            .WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation);

        editor.ReplaceNode(indexerDeclarationSyntax, newIndexer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void RewriteDeclaration(DocumentEditor editor, MethodDeclarationSyntax methodDeclarationSyntax)
    {
        AddAggressiveInliningAttribute(editor, methodDeclarationSyntax, methodDeclarationSyntax.AttributeLists,
            (node, newAttributeLists) => node.WithAttributeLists(newAttributeLists).WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void RewriteDeclaration(DocumentEditor editor, AccessorDeclarationSyntax accessorDeclarationSyntax)
    {
        AddAggressiveInliningAttribute(editor, accessorDeclarationSyntax, accessorDeclarationSyntax.AttributeLists,
            (node, newAttributeLists) => node.WithAttributeLists(newAttributeLists).WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void RewriteDeclaration(DocumentEditor editor, OperatorDeclarationSyntax operatorDeclarationSyntax)
    {
        AddAggressiveInliningAttribute(editor, operatorDeclarationSyntax, operatorDeclarationSyntax.AttributeLists,
            (node, newAttributeLists) => node.WithAttributeLists(newAttributeLists).WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void RewriteDeclaration(DocumentEditor editor, ConversionOperatorDeclarationSyntax conversionOperatorDeclarationSyntax)
    {
        AddAggressiveInliningAttribute(editor, conversionOperatorDeclarationSyntax, conversionOperatorDeclarationSyntax.AttributeLists,
            (node, newAttributeLists) => node.WithAttributeLists(newAttributeLists).WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void RewriteDeclaration(DocumentEditor editor, LocalFunctionStatementSyntax localFunctionStatementSyntax)
    {
        AddAggressiveInliningAttribute(editor, localFunctionStatementSyntax, localFunctionStatementSyntax.AttributeLists,
            (node, newAttributeLists) => node.WithAttributeLists(newAttributeLists).WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation));
    }

    private static void AddAggressiveInliningAttribute<T>(DocumentEditor editor, T node, SyntaxList<AttributeListSyntax> attributeLists, Func<T, SyntaxList<AttributeListSyntax>, T> withAttributeLists) where T : SyntaxNode
    {
        var flowControl = CreateNewNode(node, attributeLists, withAttributeLists, out var newNode);
        if (!flowControl)
        {
            return;
        }
        editor.ReplaceNode(node, newNode);
    }

    private static bool CreateNewNode<T>(T node, SyntaxList<AttributeListSyntax> attributeLists, Func<T, SyntaxList<AttributeListSyntax>, T> withAttributeLists, out T newNode) where T : SyntaxNode
    {
        // Check if MethodImpl(MethodImplOptions.AggressiveInlining) is already present
        var hasAttribute = attributeLists.Any(attrList => attrList.Attributes.Any(attr => attr.Name.ToString() == "MethodImpl" && attr.ArgumentList?.Arguments.Count == 1 && attr.ArgumentList.Arguments[0].Expression.ToString() == "MethodImplOptions.AggressiveInlining"));

        if (hasAttribute)
        {
            newNode = node;
            return false;
        }

        var methodImplAttribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("System.Runtime.CompilerServices.MethodImpl"), SyntaxFactory.AttributeArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.AttributeArgument(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName("System.Runtime.CompilerServices.MethodImplOptions"), SyntaxFactory.IdentifierName("AggressiveInlining")))))).WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation);

        var newAttributeList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(methodImplAttribute));
        var newAttributeLists = attributeLists.Add(newAttributeList);
        newNode = withAttributeLists(node, newAttributeLists);
        return true;
    }
}
