namespace LaquaiLib.Analyzers.Shared;

public static class SymbolExtensions
{
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
                        if (arrayTypeSymbol.Rank == 1)
                        {
                            return rtType.MakeArrayType();
                        }
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
                        return rtType.MakePointerType();
                    }
                }
                var name = TypeExtensions.Unkeyword(typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    .Replace("global::", "")
                    .Replace("?", ""));
                var type = Type.GetType(name);

                if (typeSymbol.IsRefLikeType)
                {
                    return type.MakeByRefType();
                }

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
                {
                    return null;
                }

                // Add ref/in/out modifiers
                if (parameterSymbol.RefKind is RefKind.Ref or RefKind.Out)
                {
                    return prelim.MakeByRefType();
                }

                return prelim;
            }
        }
    }
}
