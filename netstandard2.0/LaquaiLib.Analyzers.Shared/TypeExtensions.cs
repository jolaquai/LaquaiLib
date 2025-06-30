using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Reflection;

namespace LaquaiLib.Analyzers.Shared;

/// <summary>
/// Provides extension methods for the <see cref="Type"/> Type.
/// </summary>
public static partial class TypeExtensions
{
    private static readonly ConcurrentDictionary<Type, string> _friendlyNameCache = [];

    extension(Type type)
    {
        /// <summary>
        /// Constructs a more easily readable name for the specified <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to construct a more easily readable name for.</param>
        /// <param name="includeNamespace">Whether to include the namespace in the name.</param>
        /// <returns>A more easily readable name for the specified <see cref="Type"/>.</returns>
        public string GetFriendlyName(bool includeNamespace = true)
        {
            if (type is null)
            {
                return "null";
            }
            if (type == typeof(void))
            {
                return "void";
            }
            if (_friendlyNameCache.TryGetValue(type, out var cachedName))
            {
                return cachedName;
            }

            var prefixes = "";
            var suffixes = "";
            string operateOn;
            if (includeNamespace)
            {
                operateOn = type.Namespace + '.' + type.Name;
            }
            else
            {
                operateOn = type.Name;
            }

            if (type.IsGenericParameter)
            {
                return type.Name;
            }
            else if (type.IsArray && type.GetElementType() is Type elementType)
            {
                return elementType.GetFriendlyName() + "[]";
            }
            if (operateOn.Contains("+", StringComparison.OrdinalIgnoreCase))
            {
                operateOn = type.Namespace + '.' + type.Name;
            }
            if (operateOn.EndsWith("&"))
            {
                prefixes += "ref ";
                operateOn = operateOn.Substring(0, operateOn.Length - 1);
            }
            if (operateOn.EndsWith("*"))
            {
                suffixes += "*";
                operateOn = operateOn.Substring(0, operateOn.Length - 1);
            }

            var tickAt = operateOn.IndexOf("`", StringComparison.OrdinalIgnoreCase);
            if (tickAt > -1)
            {
                operateOn = operateOn.Substring(0, tickAt);
                var args = string.Join(", ", type.GetGenericArguments().Select(static t => t.GetFriendlyName()));

                return $"{operateOn}<{args}>";
            }

            return _friendlyNameCache[type] = (prefixes + AsKeyword(operateOn) + suffixes);
        }
        /// <summary>
        /// Converts a <see cref="Type"/> to its C# keyword, if it exists.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to convert.</param>
        /// <returns>The <see cref="Type"/>'s name as a C# keyword, if it exists, otherwise the original <see cref="Type"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string AsKeyword() => AsKeyword(type.FullName);

        /// <summary>
        /// Finds <see cref="Type"/>s that are assignable to the specified <paramref name="type"/> and constructible (i.e. that are not <see langword="interface"/>s, <see langword="abstract"/> or <see langword="static"/>).
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to find constructible subtypes of.</param>
        /// <param name="assembly">The <see cref="Assembly"/> to search in. If <see langword="null"/>, the assembly of the specified <paramref name="type"/> is used.</param>
        /// <returns>An <see cref="Array"/> of <see cref="Type"/>s that are assignable to the specified <paramref name="type"/> and constructible.</returns>
        public Type[] FindConstructibleSubtypes(Assembly assembly = null)
        {
            assembly ??= type.Assembly;
            return [.. assembly.GetTypes()
            .Where(t => type.IsAssignableFrom(t)
                && !t.IsAbstract
                && !t.IsInterface
                && t.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Length > 0 // cannot be static if it has constructors
        )];
        }
    }

    #region Mappings
    private static readonly FrozenDictionary<TypeCode, TypeCode[]> _narrowingConversions = new Dictionary<TypeCode, TypeCode[]>()
    {
        [TypeCode.Byte] = [TypeCode.SByte],
        [TypeCode.SByte] = [TypeCode.Byte, TypeCode.UInt16, TypeCode.UInt32, TypeCode.UInt64],
        [TypeCode.Int16] = [TypeCode.Byte, TypeCode.SByte, TypeCode.UInt16],
        [TypeCode.UInt16] = [TypeCode.Byte, TypeCode.SByte, TypeCode.Int16],
        [TypeCode.Int32] = [TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16, TypeCode.UInt32],
        [TypeCode.UInt32] = [TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32],
        [TypeCode.Int64] = [TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.UInt64],
        [TypeCode.UInt64] = [TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64],
        [TypeCode.Decimal] = [TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.UInt64, TypeCode.Int64],
        [TypeCode.Single] = [TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.UInt64, TypeCode.Int64],
        [TypeCode.Double] = [TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.UInt64, TypeCode.Int64]
    }.ToFrozenDictionary();

    private static readonly FrozenDictionary<TypeCode, TypeCode[]> _consistentWideningConversions = new Dictionary<TypeCode, TypeCode[]>()
    {
        [TypeCode.Byte] = [TypeCode.UInt16, TypeCode.Int16, TypeCode.UInt32, TypeCode.Int32, TypeCode.UInt64, TypeCode.Int64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal],
        [TypeCode.SByte] = [TypeCode.Int16, TypeCode.Int32, TypeCode.Int64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal],
        [TypeCode.Int16] = [TypeCode.Int32, TypeCode.Int64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal],
        [TypeCode.UInt16] = [TypeCode.UInt32, TypeCode.Int32, TypeCode.UInt64, TypeCode.Int64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal],
        [TypeCode.Char] = [TypeCode.UInt16, TypeCode.UInt32, TypeCode.Int32, TypeCode.UInt64, TypeCode.Int64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal],
        [TypeCode.Int32] = [TypeCode.Int64, TypeCode.Double, TypeCode.Decimal],
        [TypeCode.UInt32] = [TypeCode.Int64, TypeCode.UInt64, TypeCode.Double, TypeCode.Decimal],
        [TypeCode.Int64] = [TypeCode.Decimal],
        [TypeCode.UInt64] = [TypeCode.Decimal],
        [TypeCode.Single] = [TypeCode.Double]
    }.ToFrozenDictionary();

    private static readonly FrozenDictionary<TypeCode, TypeCode[]> _lossyWideningConversions = new Dictionary<TypeCode, TypeCode[]>()
    {
        [TypeCode.Int32] = [TypeCode.Single],
        [TypeCode.UInt32] = [TypeCode.Single],
        [TypeCode.Int64] = [TypeCode.Single, TypeCode.Double],
        [TypeCode.UInt64] = [TypeCode.Single, TypeCode.Double],
        [TypeCode.Decimal] = [TypeCode.Single, TypeCode.Double]
    }.ToFrozenDictionary();

    private static readonly FrozenDictionary<string, string> _typeKeywordMap = new Dictionary<string, string>()
    {
        { "System.Boolean", "bool" },
        { "System.Char", "char" },
        { "System.SByte", "sbyte" },
        { "System.Byte", "byte" },
        { "System.Int16", "short" },
        { "System.UInt16", "ushort" },
        { "System.Int32", "int" },
        { "System.UInt32", "uint" },
        { "System.nint", "nint" },
        { "System.Unint", "nuint" },
        { "System.Int64", "long" },
        { "System.UInt64", "ulong" },
        { "System.Single", "float" },
        { "System.Double", "double" },
        { "System.Decimal", "decimal" },
        { "System.String", "string" },
        { "System.Object", "object" },
        { "System.Void", "void" },
        { "Boolean", "bool" },
        { "Char", "char" },
        { "SByte", "sbyte" },
        { "Byte", "byte" },
        { "Int16", "short" },
        { "UInt16", "ushort" },
        { "Int32", "int" },
        { "UInt32", "uint" },
        { "nint", "nint" },
        { "Unint", "nuint" },
        { "Int64", "long" },
        { "UInt64", "ulong" },
        { "Single", "float" },
        { "Double", "double" },
        { "Decimal", "decimal" },
        { "String", "string" },
        { "Object", "object" },
        { "Void", "void" },
    }.ToFrozenDictionary();
    #endregion

    /// <summary>
    /// Converts a type name to its C# keyword, if it exists.
    /// </summary>
    /// <param name="type">The type name to convert.</param>
    /// <returns>The type name as a C# keyword, if it exists, otherwise the original type name.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string AsKeyword(string type) => _typeKeywordMap.TryGetValue(type, out var keyword) ? keyword : type;

    internal static bool AccessibilityIsAtLeastFamily(string accessibility) => accessibility.ToUpperInvariant() switch
    {
        "public" => true,
        "protected" => true,
        "internal" => false,
        "private" => false,
        "private protected" => true,
        "protected internal" => true,
        _ => false
    };
    internal static string GetLeastAccessibleModifier(IEnumerable<string> modifiers)
    {
        var modifiersEnumerated = modifiers.ToArray();
        if (modifiersEnumerated.Contains("private protected"))
        {
            return "private protected";
        }
        if (modifiersEnumerated.Contains("protected internal"))
        {
            return "protected internal";
        }
        if (modifiersEnumerated.Contains("private")) // same type only
        {
            return "private";
        }
        if (modifiersEnumerated.Contains("protected"))
        {
            return "protected";
        }
        if (modifiersEnumerated.Contains("internal"))
        {
            return "internal";
        }
        return "public";
    }
    internal static string GetAccessibility(MethodBase methodBase) => methodBase switch
    {
        { IsPublic: true } => "public",
        { IsFamily: true } => "protected",
        { IsAssembly: true } => "internal",
        { IsPrivate: true } => "private",
        { IsFamilyAndAssembly: true } => "private protected",
        { IsFamilyOrAssembly: true } => "protected internal",
        _ => "private"
    };
    internal static string GetAccessibility(FieldInfo fieldInfo) => fieldInfo switch
    {
        { IsPublic: true } => "public",
        { IsFamily: true } => "protected",
        { IsAssembly: true } => "internal",
        { IsPrivate: true } => "private",
        { IsFamilyAndAssembly: true } => "private protected",
        { IsFamilyOrAssembly: true } => "protected internal",
        _ => "private"
    };
    internal static string GetAccessibility(Type type) => type switch
    {
        { IsPublic: true } => "public",
        { IsNestedPublic: true } => "public",
        { IsNestedFamily: true } => "protected",
        { IsNestedAssembly: true } => "internal",
        { IsNestedPrivate: true } => "private",
        { IsNestedFamANDAssem: true } => "private protected",
        { IsNestedFamORAssem: true } => "protected internal",
        _ => "private"
    };
    internal static string GetAccessibility(this MemberInfo member)
    {
        if (member is PropertyInfo propertyInfo)
        {
            if (propertyInfo.CanRead && propertyInfo.GetGetMethod(true) is MethodBase getMethod)
            {
                return GetAccessibility(getMethod);
            }
            else if (propertyInfo.CanWrite && propertyInfo.GetSetMethod(true) is MethodBase setMethod)
            {
                return GetAccessibility(setMethod);
            }
            return "private";
        }
        else if (member is FieldInfo fieldInfo)
        {
            return GetAccessibility(fieldInfo);
        }
        else if (member is MethodBase methodBase)
        {
            return GetAccessibility(methodBase);
        }
        else if (member is EventInfo eventInfo)
        {
            var accessors = new List<string>();
            if (eventInfo.GetAddMethod(true) is MethodBase addMethod)
            {
                accessors.Add(GetAccessibility(addMethod));
            }
            if (eventInfo.GetRemoveMethod(true) is MethodBase removeMethod)
            {
                accessors.Add(GetAccessibility(removeMethod));
            }
            if (eventInfo.GetRaiseMethod(true) is MethodBase raiseMethod)
            {
                accessors.Add(GetAccessibility(raiseMethod));
            }
            return GetLeastAccessibleModifier(accessors);
        }
        else if (member is Type type)
        {
            return GetAccessibility(type);
        }
        return "private";
    }

    // "sane" because this method throws if the types are not numeric primitive types
    private static (TypeCode First, TypeCode Second) GetSaneTypeCodes(Type type, Type other)
    {
        var ret = (Type.GetTypeCode(type), Type.GetTypeCode(other));

        if (ret.Item1 is not (TypeCode.Empty or TypeCode.Object or TypeCode.DBNull or TypeCode.Boolean or TypeCode.DateTime or TypeCode.String))
        {
            throw new ArgumentException("Type must be a numeric primitive type.", nameof(type));
        }
        if (ret.Item2 is not (TypeCode.Empty or TypeCode.Object or TypeCode.DBNull or TypeCode.Boolean or TypeCode.DateTime or TypeCode.String))
        {
            throw new ArgumentException("Type must be a numeric primitive type.", nameof(other));
        }

        return ret;
    }
}
