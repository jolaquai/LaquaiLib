using System.Collections.Frozen;

namespace LaquaiLib.Analyzers.Shared;

public static class TypeExtensions
{
    extension(Type type)
    {
        /// <summary>
        /// Constructs a more easily readable name for the specified <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to construct a more easily readable name for.</param>
        /// <param name="includeNamespace">Whether to include the namespace in the name.</param>
        /// <returns>A more easily readable name for the specified <see cref="Type"/>.</returns>
        public string ToDisplayString()
        {
            var operateOn = type.FullName ?? type.Namespace + '.' + type.Name;

            if (type.IsGenericParameter)
                return type.Name;
            else if (type.IsArray && type.GetElementType() is Type elementType)
                return elementType.ToDisplayString() + "[]";
            if (operateOn.Contains(['+'], StringComparison.OrdinalIgnoreCase))
                operateOn = type.Namespace + '.' + type.Name;
            if (operateOn.EndsWith(['&']))
                return "ref " + AsKeyword(operateOn.Substring(0, operateOn.Length - 1));
            if (operateOn.EndsWith(['*']))
                return AsKeyword(operateOn.Substring(0, operateOn.Length - 1)) + '*';
            if (operateOn.EndsWith("[]", StringComparison.OrdinalIgnoreCase))
                return AsKeyword(operateOn.Substring(0, operateOn.Length - 2)) + "[]";

            if (type.IsGenericType)
            {
                var tickAt = operateOn.IndexOf(['`'], StringComparison.OrdinalIgnoreCase);
                if (tickAt != -1)
                    operateOn = operateOn.Substring(0, tickAt);
                var args = string.Join(", ", type.GetGenericArguments().Select(static t => t.ToDisplayString()));

                return $"{operateOn}<{args}>";
            }

            return AsKeyword(operateOn);
        }
        /// <summary>
        /// Converts a <see cref="Type"/> to its C# keyword, if it exists.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to convert.</param>
        /// <returns>The <see cref="Type"/>'s name as a C# keyword, if it exists, otherwise the original <see cref="Type"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string AsKeyword() => AsKeyword(type.FullName);
    }

    /// <summary>
    /// Converts a type name to its C# keyword, if it exists.
    /// </summary>
    /// <param name="type">The type name to convert.</param>
    /// <returns>The type name as a C# keyword, if it exists, otherwise the original type name.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string AsKeyword(string type) => _typeKeywordMap.TryGetValue(type, out var keyword) ? keyword : type;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Unkeyword(string keyword) => _keywordTypeMap.TryGetValue(keyword, out var type) ? type : keyword;

    #region Mappings
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
        { "System.IntPtr", "nint" },
        { "System.UIntPtr", "nuint" },
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
        { "IntPtr", "nint" },
        { "UIntPtr", "nuint" },
        { "Int64", "long" },
        { "UInt64", "ulong" },
        { "Single", "float" },
        { "Double", "double" },
        { "Decimal", "decimal" },
        { "String", "string" },
        { "Object", "object" },
        { "Void", "void" },
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    private static readonly FrozenDictionary<string, string> _keywordTypeMap = new Dictionary<string, string>()
    {
        { "bool", "System.Boolean" },
        { "char", "System.Char" },
        { "sbyte", "System.SByte" },
        { "byte", "System.Byte" },
        { "short", "System.Int16" },
        { "ushort", "System.UInt16" },
        { "int", "System.Int32" },
        { "uint", "System.UInt32" },
        { "nint", "System.IntPtr" },
        { "nuint", "System.UIntPtr" },
        { "long", "System.Int64" },
        { "ulong", "System.UInt64" },
        { "float", "System.Single" },
        { "double", "System.Double" },
        { "decimal", "System.Decimal" },
        { "string", "System.String" },
        { "object", "System.Object" },
        { "void", "System.Void" },
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    #endregion
}
