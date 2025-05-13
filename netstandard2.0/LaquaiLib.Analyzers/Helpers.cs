using System.Reflection;

namespace LaquaiLib.Analyzers;

internal static class Helpers
{
    private static PropertyInfo _underlyingTypeSymbolProperty = typeof(ITypeSymbol).GetProperty("UnderlyingTypeSymbol", BindingFlags.Instance | BindingFlags.NonPublic);
    /// <summary>
    /// Gets the unmanaged size of an <see cref="ITypeSymbol"/>.
    /// </summary>
    /// <param name="typeSymbol">The type symbol to get the size of.</param>w
    /// <returns>The size of the type in bytes or <c>0</c> if the size could not be determined or the type does not have a fixed size.</returns>
    public static int SizeOf(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol.TypeKind is not TypeKind.Struct)
        {
            return 0;
        }

        try
        {
            var type = Type.GetType(typeSymbol.OriginalDefinition.ToDisplayString());
            type ??= Type.GetType(typeSymbol.OriginalDefinition.ContainingNamespace.ToDisplayString() + '.' + typeSymbol.Name);
            return (int)typeof(Unsafe).GetMethod(nameof(Unsafe.SizeOf)).MakeGenericMethod(type).Invoke(null, null);
        }
        catch
        {
            return 0;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReportAll(this ref SyntaxNodeAnalysisContext context, params IEnumerable<Diagnostic> diagnostics)
    {
        foreach (var diag in diagnostics)
        {
            context.ReportDiagnostic(diag);
        }
    }

    public static async Task<bool> HasNonSpanUsageAsync(ISymbol arraySymbol, SemanticModel semanticModel)
    {
        // Get all references to the array symbol
        var references = await FindAllReferences(arraySymbol, semanticModel).ConfigureAwait(false);

        for (var i = 0; i < references.Length; i++)
        {
            // Check if this reference involves a conversion to non-Span type
            if (IsConvertedToNonSpan(references[i], semanticModel))
            {
                return true;
            }
        }

        return false;
    }
    private static async Task<SyntaxNode[]> FindAllReferences(ISymbol symbol, SemanticModel semanticModel)
    {
        var compilation = semanticModel.Compilation;
        var syntaxTree = semanticModel.SyntaxTree;

        // Find all identifier nodes that reference this symbol
        var root = await syntaxTree.GetRootAsync().ConfigureAwait(false);
        var identifiers = root.DescendantNodes().OfType<IdentifierNameSyntax>()
            .Where(id =>
            {
                var symbolInfo = semanticModel.GetSymbolInfo(id);
                return SymbolEqualityComparer.Default.Equals(symbolInfo.Symbol, symbol);
            });

        // Get parent nodes that represent assignments or conversions
        return identifiers.Select(id => id.Parent).ToArray();
    }
    private static bool IsConvertedToNonSpan(SyntaxNode node, SemanticModel semanticModel)
    {
        // Check various syntax patterns where conversion might occur

        // 1. Direct assignment: Span<int> span = array;
        if (node.Parent is VariableDeclaratorSyntax declarator)
        {
            if (declarator.Parent?.Parent is VariableDeclarationSyntax declaration)
            {
                var varDeclTypeInfo = semanticModel.GetTypeInfo(declaration.Type);
                return !IsSpanType(varDeclTypeInfo.Type);
            }
        }

        // 2. Method argument: SomeMethod(array);
        if (node.Parent is ArgumentSyntax argument)
        {
            if (argument.Parent?.Parent is InvocationExpressionSyntax invocation)
            {
                if (semanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol)
                {
                    var parameterIndex = invocation.ArgumentList.Arguments.IndexOf(argument);
                    if (parameterIndex >= 0 && parameterIndex < methodSymbol.Parameters.Length)
                    {
                        var parameterType = methodSymbol.Parameters[parameterIndex].Type;
                        return !IsSpanType(parameterType);
                    }
                }
            }
        }

        // 3. Return statement: return array;
        if (node.Parent is ReturnStatementSyntax)
        {
            var containingMethod = node.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (containingMethod != null)
            {
                var methodSymbol = semanticModel.GetDeclaredSymbol(containingMethod);
                return !IsSpanType(methodSymbol?.ReturnType);
            }
        }

        // 4. Field/property assignment: this.field = array;
        if (node.Parent is AssignmentExpressionSyntax assignment)
        {
            var leftTypeInfo = semanticModel.GetTypeInfo(assignment.Left);
            return !IsSpanType(leftTypeInfo.Type);
        }

        // 5. Implicit conversion in expressions
        var implicitConversionTypeInfo = semanticModel.GetTypeInfo(node);
        if (implicitConversionTypeInfo.ConvertedType != null && !SymbolEqualityComparer.Default.Equals(implicitConversionTypeInfo.Type, implicitConversionTypeInfo.ConvertedType))
        {
            return !IsSpanType(implicitConversionTypeInfo.ConvertedType);
        }

        return false;
    }
    private static bool IsSpanType(ITypeSymbol type)
    {
        if (type == null) return false;

        // Check if it's Span<T> or ReadOnlySpan<T>
        return type.OriginalDefinition?.ToString() is "System.Span<T>" or "System.ReadOnlySpan<T>";
    }
    public static ISymbol GetArraySymbolWithTypeCheck(ArrayCreationExpressionSyntax arrayCreation, SemanticModel semanticModel)
    {
        var symbol = GetArraySymbol(arrayCreation, semanticModel);

        // Ensure the symbol type is actually an array type
        if (symbol != null)
        {
            var symbolType = GetSymbolType(symbol);
            if (symbolType == null || symbolType.TypeKind != TypeKind.Array)
            {
                return null;
            }
        }

        return symbol;
    }
    private static ISymbol GetArraySymbol(ArrayCreationExpressionSyntax arrayCreation, SemanticModel semanticModel)
    {
        var parent = arrayCreation.Parent;

        // Check various parent contexts to find the symbol
        switch (parent)
        {
            // Case 1: Variable declaration - int[] arr = new int[5];
            case VariableDeclaratorSyntax declarator:
                return semanticModel.GetDeclaredSymbol(declarator);

            // Case 2: Assignment expression - arr = new int[5];
            case AssignmentExpressionSyntax assignment when assignment.Right == arrayCreation:
                return semanticModel.GetSymbolInfo(assignment.Left).Symbol;

            // Case 3: Property initializer - public int[] Prop { get; set; } = new int[5];
            case PropertyDeclarationSyntax property:
                return semanticModel.GetDeclaredSymbol(property);

            // Case 4: Field initializer - private int[] field = new int[5];
            case FieldDeclarationSyntax field:
                // Find the specific declarator in the field declaration
                var fieldDeclarator = field.Declaration.Variables
                    .FirstOrDefault(v => v.Initializer?.Value == arrayCreation);
                return fieldDeclarator != null ? semanticModel.GetDeclaredSymbol(fieldDeclarator) : null;

            // Case 5: Equals value clause in object initializer
            case EqualsValueClauseSyntax equalsValue when equalsValue.Parent is PropertyPatternClauseSyntax:
                // Handle object initializer scenarios
                if (equalsValue.Parent?.Parent is InitializerExpressionSyntax initializer)
                {
                    // Try to find the property being initialized
                    var propName = equalsValue.Parent.GetFirstToken().Text;
                    var targetType = semanticModel.GetTypeInfo(initializer.Parent).Type;
                    return targetType?.GetMembers(propName).FirstOrDefault();
                }
                return null;

            // Case 6: Lambda expression body - () => new int[5];
            case LambdaExpressionSyntax lambda:
                // The array doesn't have a symbol in this context
                return null;

            // Case 7: Method/constructor argument - SomeMethod(new int[5]);
            case ArgumentSyntax:
                // No symbol for direct method arguments
                return null;

            // Case 8: Return statement - return new int[5];
            case ReturnStatementSyntax:
                // No symbol for return expressions
                return null;

            // Case 9: Collection initializer element
            case InitializerExpressionSyntax:
                // No symbol for collection elements
                return null;

            // Default case - no symbol found
            default:
                return null;
        }
    }
    private static ITypeSymbol GetSymbolType(ISymbol symbol)
    {
        return symbol switch
        {
            ILocalSymbol local => local.Type,
            IFieldSymbol field => field.Type,
            IPropertySymbol property => property.Type,
            IParameterSymbol parameter => parameter.Type,
            _ => null
        };
    }
}
