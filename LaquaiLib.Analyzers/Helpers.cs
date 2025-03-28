using System.Reflection;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;

namespace LaquaiLib.Analyzers;

internal static class Helpers
{
    private static PropertyInfo _underlyingTypeSymbolProperty = typeof(ITypeSymbol).GetProperty("UnderlyingTypeSymbol", BindingFlags.Instance | BindingFlags.NonPublic);
    /// <summary>
    /// Gets the unmanaged size of an <see cref="ITypeSymbol"/>.
    /// </summary>
    /// <param name="typeSymbol">The type symbol to get the size of.</param>
    /// <param name="compilation">A <see cref="Compilation"/> instance to use for semantic analysis.</param>
    /// <returns>The size of the type in bytes or <c>0</c> if the size could not be determined or the type does not have a fixed size.</returns>
    public static int SizeOf(this ITypeSymbol typeSymbol, Compilation compilation)
    {
        if (typeSymbol.TypeKind is not TypeKind.Struct) return 0;
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
}
