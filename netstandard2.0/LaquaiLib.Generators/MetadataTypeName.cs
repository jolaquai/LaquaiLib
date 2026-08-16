using System.Text;

namespace LaquaiLib.Generators;

/// <summary>
/// Renders an <see cref="ITypeSymbol"/> as a CLR reflection type name suitable for
/// <c>System.Runtime.CompilerServices.UnsafeAccessorTypeAttribute(string)</c> (<see cref="Type.GetType(string)"/> syntax).
/// </summary>
internal static class MetadataTypeName
{
    private static readonly char[] _charsToEscape = ['\\', ',', '[', ']', '+', '*', '&'];

    public static string TryBuild(ITypeSymbol type, IAssemblySymbol compilationAssembly)
    {
        if (type is null)
        {
            return null;
        }

        var sb = new StringBuilder();
        if (!TryAppend(sb, type, compilationAssembly))
        {
            return null;
        }
        return sb.ToString();
    }

    private static bool TryAppend(StringBuilder sb, ITypeSymbol type, IAssemblySymbol compilationAssembly)
    {
        switch (type)
        {
            case IDynamicTypeSymbol:
                sb.Append("System.Object");
                return true;

            case IPointerTypeSymbol pointer:
                if (!TryAppend(sb, pointer.PointedAtType, compilationAssembly))
                {
                    return false;
                }
                sb.Append('*');
                return true;

            case IArrayTypeSymbol array:
                if (array.Rank == 1 && !array.IsSZArray)
                {
                    return false;
                }
                // Naive recursion (no reversal): C# int[,][] has Rank=2, ElementType=int[]; recursing yields
                // "System.Int32[][,]", which is exactly the reflection-name ordering for that jagged/multidim mix.
                if (!TryAppend(sb, array.ElementType, compilationAssembly))
                {
                    return false;
                }
                if (array.Rank == 1)
                {
                    sb.Append("[]");
                }
                else
                {
                    sb.Append('[');
                    for (var i = 1; i < array.Rank; i++)
                    {
                        sb.Append(',');
                    }
                    sb.Append(']');
                }
                return true;

            case ITypeParameterSymbol typeParameter:
                if (typeParameter.TypeParameterKind == TypeParameterKind.Type)
                {
                    sb.Append('!').Append(typeParameter.Ordinal);
                    return true;
                }
                if (typeParameter.TypeParameterKind == TypeParameterKind.Method)
                {
                    sb.Append("!!").Append(typeParameter.Ordinal);
                    return true;
                }
                return false;

            case IFunctionPointerTypeSymbol:
                return false;

            case INamedTypeSymbol named:
                return TryAppendNamed(sb, named, compilationAssembly);

            default:
                if (type.TypeKind == TypeKind.Error)
                {
                    return false;
                }
                return false;
        }
    }

    private static bool TryAppendNamed(StringBuilder sb, INamedTypeSymbol named, IAssemblySymbol compilationAssembly)
    {
        if (named.TypeKind == TypeKind.Error)
        {
            return false;
        }

        var nameStart = sb.Length;

        if (!named.ContainingNamespace.IsGlobalNamespace)
        {
            AppendEscapedNamespace(sb, named.ContainingNamespace);
            sb.Append('.');
        }

        // Collect containing types outermost-first for both name segments and type-argument ordering.
        var containers = new List<INamedTypeSymbol>();
        for (var c = named.ContainingType; c is not null; c = c.ContainingType)
        {
            containers.Add(c);
        }
        for (var i = containers.Count - 1; i >= 0; i--)
        {
            AppendEscaped(sb, containers[i].MetadataName);
            sb.Append('+');
        }
        AppendEscaped(sb, named.MetadataName);

        // Gather type arguments outermost-first (containers first, then this type's own), each built independently.
        var argSb = new StringBuilder();
        var hasArgs = false;
        for (var i = containers.Count - 1; i >= 0; i--)
        {
            var c = containers[i];
            for (var a = 0; a < c.TypeArguments.Length; a++)
            {
                if (hasArgs)
                {
                    argSb.Append(',');
                }
                if (!TryAppendArgument(argSb, c.TypeArguments[a], compilationAssembly))
                {
                    return false;
                }
                hasArgs = true;
            }
        }
        for (var a = 0; a < named.TypeArguments.Length; a++)
        {
            if (hasArgs)
            {
                argSb.Append(',');
            }
            if (!TryAppendArgument(argSb, named.TypeArguments[a], compilationAssembly))
            {
                return false;
            }
            hasArgs = true;
        }

        if (hasArgs)
        {
            sb.Append('[').Append(argSb).Append(']');
        }

        if (named.SpecialType == SpecialType.None
            && !SymbolEqualityComparer.Default.Equals(named.ContainingAssembly, compilationAssembly))
        {
            sb.Append(", ").Append(named.ContainingAssembly.Identity.Name);
        }

        return true;
    }

    private static bool TryAppendArgument(StringBuilder sb, ITypeSymbol argument, IAssemblySymbol compilationAssembly)
    {
        sb.Append('[');
        if (!TryAppend(sb, argument, compilationAssembly))
        {
            return false;
        }
        sb.Append(']');
        return true;
    }

    private static void AppendEscapedNamespace(StringBuilder sb, INamespaceSymbol ns)
    {
        var parts = new List<string>();
        for (var n = ns; n is not null && !n.IsGlobalNamespace; n = n.ContainingNamespace)
        {
            parts.Add(n.Name);
        }
        for (var i = parts.Count - 1; i >= 0; i--)
        {
            AppendEscaped(sb, parts[i]);
            if (i > 0)
            {
                sb.Append('.');
            }
        }
    }

    private static void AppendEscaped(StringBuilder sb, string identifier)
    {
        if (identifier.IndexOfAny(_charsToEscape) < 0)
        {
            sb.Append(identifier);
            return;
        }
        for (var i = 0; i < identifier.Length; i++)
        {
            var ch = identifier[i];
            if (Array.IndexOf(_charsToEscape, ch) >= 0)
            {
                sb.Append('\\');
            }
            sb.Append(ch);
        }
    }
}
