using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace LaquaiLib.Analyzers;

internal static class Helpers
{
    /// <summary>
    /// Gets a lower bound for the unmanaged size of an <see cref="ITypeSymbol"/>.
    /// </summary>
    /// <param name="typeSymbol">The type symbol to get the size of.</param>
    /// <param name="compilation">The <see cref="Compilation"/> the symbol belongs to. Supplies the target pointer width.</param>
    /// <param name="cache">A cache of already computed sizes, or <see langword="null"/> to have the walk allocate its own. Pass one per compilation to keep repeated queries at O(distinct types).</param>
    /// <returns>A lower bound for the size of the type in bytes, or <c>-1</c> if no bound could be established.</returns>
    /// <remarks>
    /// The layout is walked through the symbol graph, so it resolves against <paramref name="compilation"/> rather than
    /// whatever happens to be loaded into the analyzer host. Anything that cannot be resolved is counted as its smallest
    /// possible contribution, which keeps the result a lower bound at the cost of occasionally underestimating.
    /// </remarks>
    public static int SizeOf(this ITypeSymbol typeSymbol, Compilation compilation, ConcurrentDictionary<ITypeSymbol, int> cache = null)
        => SizeOfCore(typeSymbol, PointerSize(compilation), cache, null);

    private static int PointerSize(Compilation compilation) => compilation.Options.Platform switch
    {
        Platform.X64 or Platform.Arm64 or Platform.Itanium => 8,
        _ => 4 // AnyCpu lands here; 4 stays a lower bound whichever width the process ends up with
    };

    private static int SizeOfCore(ITypeSymbol type, int pointerSize, ConcurrentDictionary<ITypeSymbol, int> cache, HashSet<ITypeSymbol> inProgress)
    {
        if (type is null)
        {
            return -1;
        }

        switch (type.SpecialType)
        {
            case SpecialType.System_Boolean:
            case SpecialType.System_SByte:
            case SpecialType.System_Byte:
                return 1;
            case SpecialType.System_Char:
            case SpecialType.System_Int16:
            case SpecialType.System_UInt16:
                return 2;
            case SpecialType.System_Int32:
            case SpecialType.System_UInt32:
            case SpecialType.System_Single:
                return 4;
            case SpecialType.System_Int64:
            case SpecialType.System_UInt64:
            case SpecialType.System_Double:
                return 8;
            case SpecialType.System_Decimal:
                return 16;
            case SpecialType.System_IntPtr:
            case SpecialType.System_UIntPtr:
                return pointerSize;
        }

        return type.TypeKind switch
        {
            TypeKind.Enum => SizeOfCore((type as INamedTypeSymbol)?.EnumUnderlyingType, pointerSize, cache, inProgress),
            TypeKind.Pointer or TypeKind.FunctionPointer => pointerSize,
            TypeKind.Struct => StructSize(type as INamedTypeSymbol, pointerSize, cache, inProgress),
            _ => -1
        };
    }

    private static int StructSize(INamedTypeSymbol type, int pointerSize, ConcurrentDictionary<ITypeSymbol, int> cache, HashSet<ITypeSymbol> inProgress)
    {
        if (type is null)
        {
            return -1;
        }

        if (cache is not null && cache.TryGetValue(type, out var cached))
        {
            return cached;
        }

        // System.Numerics.Vector<T> has no fixed width; the JIT picks it per machine
        if (type.OriginalDefinition is { Name: "Vector", Arity: 1, ContainingNamespace: { Name: "Numerics", ContainingNamespace: { Name: "System", ContainingNamespace.IsGlobalNamespace: true } } })
        {
            return -1;
        }

        // Both are threaded down through the recursion; the cache also keeps nested layouts from being walked once per path through them
        cache ??= new ConcurrentDictionary<ITypeSymbol, int>(SymbolEqualityComparer.Default);
        inProgress ??= new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

        // A struct containing itself is CS0523, but the walk still has to terminate on it
        if (!inProgress.Add(type))
        {
            return -1;
        }

        int size;
        try
        {
            size = type.OriginalDefinition.DeclaringSyntaxReferences.IsEmpty
                ? MetadataStructSize(type, pointerSize, cache, inProgress)
                : SourceStructSize(type, pointerSize, cache, inProgress);
        }
        finally
        {
            inProgress.Remove(type);
        }

        cache[type] = size;
        return size;
    }

    private static int SourceStructSize(INamedTypeSymbol type, int pointerSize, ConcurrentDictionary<ITypeSymbol, int> cache, HashSet<ITypeSymbol> inProgress)
    {
        ReadStructLayout(type, out var isExplicit, out var declaredSize);

        long accumulated = 0;
        var fieldCount = 0;
        var members = type.GetMembers();
        for (var i = 0; i < members.Length; i++)
        {
            if (members[i] is not IFieldSymbol { IsStatic: false, IsConst: false } field)
            {
                continue;
            }

            fieldCount++;
            var fieldSize = FieldSize(field, pointerSize, cache, inProgress);
            if (fieldSize <= 0)
            {
                fieldSize = 1; // no field can occupy less, so substituting it keeps the result a lower bound
            }

            if (isExplicit)
            {
                // Explicit fields may overlap, so the sum proves nothing
                accumulated = Math.Max(accumulated, (long)FieldOffset(field) + fieldSize);
            }
            else
            {
                // Padding and Auto reordering only ever grow a struct past the sum of its fields
                accumulated += fieldSize;
            }
        }

        return Clamp(Math.Max(ApplyInlineArray(type, accumulated, fieldCount), declaredSize));
    }

    private static int MetadataStructSize(INamedTypeSymbol type, int pointerSize, ConcurrentDictionary<ITypeSymbol, int> cache, HashSet<ITypeSymbol> inProgress)
    {
        var definition = type.OriginalDefinition;

        var known = WellKnownStructSize(definition);
        if (known > 0)
        {
            return known;
        }

        // T? is its underlying value plus a flag byte, so the sum stays under whatever the padding makes of it
        if (definition.SpecialType == SpecialType.System_Nullable_T)
        {
            var underlying = type.TypeArguments.Length == 1 ? SizeOfCore(type.TypeArguments[0], pointerSize, cache, inProgress) : -1;
            return underlying > 0 ? Clamp((long)underlying + 1) : -1;
        }

        long largest = 0;
        var fieldCount = 0;
        var members = type.GetMembers();
        for (var i = 0; i < members.Length; i++)
        {
            if (members[i] is not IFieldSymbol { IsStatic: false, IsConst: false } field)
            {
                continue;
            }

            // GenAPI strips concrete private fields out of reference assemblies and fabricates these two in their place, so anything left says nothing about the real layout
            if (field.Name is "_dummy" or "_dummyPrimitive")
            {
                return -1;
            }

            fieldCount++;
            largest = Math.Max(largest, FieldSize(field, pointerSize, cache, inProgress));
        }

        // StructLayout and FieldOffset are pseudo-custom attributes that never reach GetAttributes() on a metadata symbol, so overlapping fields would be summed as if they were sequential; the largest field alone is a lower bound under any LayoutKind
        return fieldCount == 0 ? -1 : Clamp(ApplyInlineArray(type, largest, fieldCount));
    }

    /// <summary>
    /// Scales <paramref name="accumulated"/> by the inline array length of <paramref name="type"/>, if it is one.
    /// </summary>
    private static long ApplyInlineArray(INamedTypeSymbol type, long accumulated, int fieldCount)
    {
        // An inline array is N copies of its one field; a different count means the declaration is broken (CS9169) and there is nothing to scale
        if (fieldCount != 1)
        {
            return accumulated;
        }

        var attributes = type.GetAttributes();
        for (var i = 0; i < attributes.Length; i++)
        {
            if (IsSystemRuntimeAttribute(attributes[i].AttributeClass, "CompilerServices", "InlineArrayAttribute")
                && attributes[i].ConstructorArguments.Length > 0
                && attributes[i].ConstructorArguments[0].Value is int length and > 1)
            {
                return accumulated * length;
            }
        }
        return accumulated;
    }

    /// <summary>
    /// Gets the measured size of a well-known metadata struct, or <c>0</c> if <paramref name="definition"/> is not one.
    /// </summary>
    private static int WellKnownStructSize(INamedTypeSymbol definition)
    {
        var containingNamespace = definition.ContainingNamespace;
        if (definition.Arity == 0 && containingNamespace is { Name: "System", ContainingNamespace.IsGlobalNamespace: true })
        {
            return definition.Name switch
            {
                "Guid" or "DateTimeOffset" => 16,
                "DateTime" or "TimeSpan" or "TimeOnly" or "Range" => 8,
                "DateOnly" or "Index" => 4,
                "Half" => 2,
                _ => 0
            };
        }

        if (definition.Arity == 0 && containingNamespace is { Name: "Numerics", ContainingNamespace: { Name: "System", ContainingNamespace.IsGlobalNamespace: true } })
        {
            return definition.Name switch
            {
                "Matrix4x4" => 64,
                "Matrix3x2" => 24,
                "Vector4" or "Quaternion" or "Plane" or "Complex" => 16,
                "Vector3" => 12,
                "Vector2" => 8,
                _ => 0
            };
        }

        // Unlike Vector<T>, the intrinsic vectors are fixed width by definition
        if (definition.Arity == 1 && containingNamespace is { Name: "Intrinsics", ContainingNamespace: { Name: "Runtime", ContainingNamespace: { Name: "System", ContainingNamespace.IsGlobalNamespace: true } } })
        {
            return definition.Name switch
            {
                "Vector512" => 64,
                "Vector256" => 32,
                "Vector128" => 16,
                "Vector64" => 8,
                _ => 0
            };
        }

        return 0;
    }

    private static int FieldSize(IFieldSymbol field, int pointerSize, ConcurrentDictionary<ITypeSymbol, int> cache, HashSet<ITypeSymbol> inProgress)
    {
        if (field.RefKind != RefKind.None)
        {
            return pointerSize;
        }

        if (field.IsFixedSizeBuffer)
        {
            var elementSize = SizeOfCore((field.Type as IPointerTypeSymbol)?.PointedAtType, pointerSize, cache, inProgress);
            return elementSize > 0 && field.FixedSize > 0 ? (int)Math.Min((long)elementSize * field.FixedSize, int.MaxValue) : -1;
        }

        return field.Type is { IsReferenceType: true } ? pointerSize : SizeOfCore(field.Type, pointerSize, cache, inProgress);
    }

    private static int Clamp(long size) => (int)Math.Min(Math.Max(size, 1), int.MaxValue);

    private static void ReadStructLayout(INamedTypeSymbol type, out bool isExplicit, out int declaredSize)
    {
        isExplicit = false;
        declaredSize = 0;

        var attributes = type.GetAttributes();
        for (var i = 0; i < attributes.Length; i++)
        {
            var attribute = attributes[i];
            if (!IsSystemRuntimeAttribute(attribute.AttributeClass, "InteropServices", "StructLayoutAttribute"))
            {
                continue;
            }

            var ctorArgs = attribute.ConstructorArguments;
            if (ctorArgs.Length > 0)
            {
                // The LayoutKind argument surfaces as its underlying value, and the legacy overload takes a short
                isExplicit = ctorArgs[0].Value switch
                {
                    int asInt => asInt == (int)LayoutKind.Explicit,
                    short asShort => asShort == (short)LayoutKind.Explicit,
                    _ => false
                };
            }

            var namedArgs = attribute.NamedArguments;
            for (var j = 0; j < namedArgs.Length; j++)
            {
                if (namedArgs[j].Key == "Size" && namedArgs[j].Value.Value is int size and > 0)
                {
                    declaredSize = size;
                }
            }

            return;
        }
    }

    private static int FieldOffset(IFieldSymbol field)
    {
        var attributes = field.GetAttributes();
        for (var i = 0; i < attributes.Length; i++)
        {
            if (IsSystemRuntimeAttribute(attributes[i].AttributeClass, "InteropServices", "FieldOffsetAttribute")
                && attributes[i].ConstructorArguments.Length > 0
                && attributes[i].ConstructorArguments[0].Value is int offset and >= 0)
            {
                return offset;
            }
        }
        return 0;
    }

    private static bool IsSystemRuntimeAttribute(INamedTypeSymbol attributeClass, string subNamespace, string name)
        => attributeClass is { ContainingNamespace: { ContainingNamespace: { Name: "Runtime", ContainingNamespace: { Name: "System", ContainingNamespace.IsGlobalNamespace: true } } } }
        && attributeClass.Name == name
        && attributeClass.ContainingNamespace.Name == subNamespace;

    public static int? GetArraySize(this ArrayCreationExpressionSyntax arrayCreation, SemanticModel semanticModel)
    {
        var rankSpecifier = arrayCreation.Type.RankSpecifiers.FirstOrDefault();
        var sizeExpression = rankSpecifier?.Sizes.FirstOrDefault();

        if (sizeExpression == null)
        {
            return null;
        }

        var constantValue = semanticModel.GetConstantValue(sizeExpression);
        return constantValue.HasValue ? AsInt32(constantValue.Value) : null;
    }

    /// <summary>
    /// Reinterprets a boxed integral constant as an <see cref="int"/>, saturating instead of overflowing.
    /// </summary>
    private static int? AsInt32(object value)
    {
        // An array size may be any of int, uint, long or ulong, and anything narrower widens into one of those
        long widened;
        switch (value)
        {
            case int asInt:
                return asInt;
            case long asLong:
                widened = asLong;
                break;
            case uint asUInt:
                widened = asUInt;
                break;
            case ulong asULong:
                return asULong > (ulong)int.MaxValue ? int.MaxValue : (int)asULong;
            case short asShort:
                return asShort;
            case ushort asUShort:
                return asUShort;
            case sbyte asSByte:
                return asSByte;
            case byte asByte:
                return asByte;
            case nint asNInt:
                widened = asNInt;
                break;
            case nuint asNUInt:
                return asNUInt > (nuint)int.MaxValue ? int.MaxValue : (int)asNUInt;
            default:
                return null;
        }
        return (int)Math.Min(Math.Max(widened, int.MinValue), int.MaxValue);
    }
}
