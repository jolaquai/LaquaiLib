namespace LaquaiLib.Analyzers.Shared;

public static class SymbolExtensions
{
    /// <summary>
    /// Resolves a bare reflection type name, trying the unqualified form first (works for the current assembly and
    /// corelib) and falling back to a fully assembly-qualified form built from <paramref name="containingAssembly"/>.
    /// The unqualified form alone misses any BCL type that lives outside corelib in a facade assembly not already
    /// loaded by name into this process (e.g. <see cref="Stack{T}"/> lives in "System.Collections", not corelib).
    /// </summary>
    private static Type ResolveType(string bareName, IAssemblySymbol containingAssembly)
    {
        try
        {
            if (Type.GetType(bareName) is { } unqualified)
                return unqualified;

            if (containingAssembly is not null)
                return Type.GetType($"{bareName}, {containingAssembly.Identity.GetDisplayName()}");
        }
        catch
        {
            // Malformed name for Type.GetType's parser (e.g. a tuple shape) - unresolvable, same as a null result.
        }
        return null;
    }
    extension(ITypeSymbol typeSymbol)
    {
        /// <summary>
        /// Attempts to find the underlying runtime <see cref="Type"/> for the given <see cref="ITypeSymbol"/>.
        /// </summary>
        /// <returns>The final underlying <see cref="Type"/> if found, otherwise <see langword="null"/>. For types in the assembly currently being analyzed (if called from an analyzer context), this will always return <see langword="null"/> since that type does not exist yet.</returns>
        public Type RuntimeType
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                switch (typeSymbol)
                {
                    case IArrayTypeSymbol arrayTypeSymbol:
                    {
                        typeSymbol = arrayTypeSymbol.ElementType;
                        var rtType = typeSymbol.RuntimeType;
                        if (rtType is null)
                            return null;
                        if (arrayTypeSymbol.Rank == 1)
                            return rtType.MakeArrayType();
                        return rtType.MakeArrayType(arrayTypeSymbol.Rank);
                    }
                    case IDynamicTypeSymbol:
                    {
                        return typeof(object);
                    }
                    case IPointerTypeSymbol pointerTypeSymbol:
                    {
                        typeSymbol = pointerTypeSymbol.PointedAtType;
                        var rtType = typeSymbol.RuntimeType;
                        if (rtType is null)
                            return null;
                        return rtType.MakePointerType();
                    }
                    case INamedTypeSymbol { IsGenericType: true } namedTypeSymbol:
                    {
                        // Type.GetType only understands reflection metadata names ('Stack`1'), not C#'s '<T>' syntax,
                        // so the display-string path below never resolves generic types - resolve the open definition
                        // via its metadata name instead, then try to close it over the resolved type arguments.
                        var openType = ResolveType(namedTypeSymbol.ConstructedFrom.ReflectionMetadataName, namedTypeSymbol.ContainingAssembly);
                        if (openType is null)
                            return null;

                        var typeArgs = new Type[namedTypeSymbol.TypeArguments.Length];
                        var allResolved = true;
                        for (var i = 0; i < typeArgs.Length; i++)
                        {
                            typeArgs[i] = namedTypeSymbol.TypeArguments[i].RuntimeType;
                            if (typeArgs[i] is null)
                            {
                                allResolved = false;
                                break;
                            }
                        }
                        // A type argument that fails to resolve is typically a free type parameter of the type
                        // containing the [UnsafeAccessor] method itself (e.g. 'T' in 'Stack<T>' as referenced from
                        // inside 'StackAccessors<T>'), which by definition can never resolve to a concrete runtime
                        // Type. The open generic definition is still enough for callers that only need to check
                        // member existence/shape (they structurally compare against generic parameter positions).
                        if (!allResolved)
                            return openType;
                        try
                        {
                            return openType.MakeGenericType(typeArgs);
                        }
                        catch
                        {
                            return openType;
                        }
                    }
                }
                var name = TypeExtensions.Unkeyword(typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    .Replace("global::", "")
                    .Replace("?", ""));

                // The display string above uses C#'s '<T>' generic syntax, which Type.GetType's assembly-qualified-name
                // parser can throw on for some shapes (e.g. tuples); ResolveType treats any parse failure as unresolvable.
                var type = ResolveType(name, typeSymbol.ContainingAssembly);

                // Type.GetType only understands assembly-qualified/reflection syntax; the display string above
                // uses C#'s '<T>' generic syntax, so this never resolves for generic types (e.g. Span<byte>).
                // Returning null here is correct: callers fall back to other resolution strategies.
                if (type is null)
                    return null;

                if (typeSymbol.IsRefLikeType)
                    return type.MakeByRefType();

                return type;
            }
        }

        public bool IsAssignableTo(ITypeSymbol other)
        {
            if (other is null)
                return false;
            if (SymbolEqualityComparer.Default.Equals(typeSymbol, other))
                return true;
            foreach (var iface in typeSymbol.Interfaces)
                if (iface.IsAssignableTo(other))
                    return true;
            var baseTypeSymbol = other.BaseType;
            while (baseTypeSymbol is not null)
            {
                if (SymbolEqualityComparer.Default.Equals(typeSymbol, baseTypeSymbol))
                    return true;
                baseTypeSymbol = baseTypeSymbol.BaseType;
            }
            return false;
        }
    }
    extension(INamedTypeSymbol namedTypeSymbol)
    {
        /// <summary>
        /// Renders this symbol in reflection's <see cref="Type.FullName"/> shape: namespace, '+' as the nested-type
        /// separator, and metadata arity suffix (e.g. "Stack`1") instead of Roslyn's '&lt;T&gt;' display syntax.
        /// </summary>
        public string ReflectionMetadataName
        {
            get
            {
                var chain = new List<string>();
                for (var current = namedTypeSymbol; current is not null; current = current.ContainingType)
                    chain.Insert(0, current.MetadataName);

                var nestedName = string.Join("+", chain);
                var ns = namedTypeSymbol.ContainingNamespace;
                return ns is null || ns.IsGlobalNamespace ? nestedName : $"{ns.ToDisplayString()}.{nestedName}";
            }
        }
    }
    extension(IParameterSymbol parameterSymbol)
    {
        /// <summary>
        /// Attempts to find the underlying runtime <see cref="Type"/> for the given <see cref="ITypeSymbol"/>.
        /// </summary>
        /// <returns>The final underlying <see cref="Type"/> if found, otherwise <see langword="null"/>. For types in the assembly currently being analyzed (if called from an analyzer context), this will always return <see langword="null"/> since that type does not exist yet.</returns>
        public Type RuntimeType
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var prelim = parameterSymbol.Type.RuntimeType;
                if (prelim is null)
                    return null;

                // Add ref/in/out modifiers
                if (parameterSymbol.RefKind is RefKind.Ref or RefKind.Out)
                    return prelim.MakeByRefType();

                return prelim;
            }
        }
    }
}
