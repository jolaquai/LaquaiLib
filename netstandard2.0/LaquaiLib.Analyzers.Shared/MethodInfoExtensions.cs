using System.CodeDom.Compiler;
using System.Reflection;
using System.Text;

using Microsoft.CodeAnalysis;

namespace LaquaiLib.Analyzers.Shared;

/// <summary>
/// Provides extension methods for the <see cref="MethodInfo"/> Type.
/// </summary>
public static class MethodInfoExtensions
{
    extension(IMethodSymbol methodSymbol)
    {
        /// <summary>
        /// Gets a <see langword="string"/> representation of the parameters of the method represented by the specified <see cref="IMethodSymbol"/> (that is, a comma-space-separated list of parameter types and names).
        /// </summary>
        public string ParameterString
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => string.Join(", ", methodSymbol.Parameters.Select(p => p.ToDisplayString(SymbolDisplayFormats.FullyQualifiedParameter)));
        }
        /// <summary>
        /// Gets a comma-space-separated list of parameter names that can be used to call the method represented by the specified <see cref="IMethodSymbol"/>. Ref kinds are preserved.
        /// </summary>
        public string ArgumentString
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => string.Join(", ", methodSymbol.Parameters.Select(static p => p.RefKind switch
            {
                RefKind.Ref or RefKind.RefReadOnlyParameter => "ref " + p.Name,
                RefKind.Out => "out " + p.Name,
                RefKind.In => "in " + p.Name,
                _ => p.Name
            }));
        }
        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> that enumerates the overridden methods (in upwards order of the type hierarchy) of the specified <see cref="IMethodSymbol"/>.
        /// </summary>
        public IEnumerable<IMethodSymbol> OverrideChain
        {
            get
            {
                var target = methodSymbol.OverriddenMethod;
                while (target is not null)
                {
                    yield return target;
                    target = target.OverriddenMethod;
                }
            }
        }

        /// <summary>
        /// Determines whether a method represented by a <see cref="IMethodSymbol"/> instance is a property getter or setter.
        /// </summary>
        /// <param name="methodInfo">A <see cref="MethodInfo"/> instance representing the method to check.</param>
        /// <returns>A value indicating whether the method is a property getter or setter.</returns>
        public bool IsGetterOrSetter
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => methodSymbol.IsGetter || methodSymbol.IsSetter;
        }
        /// <summary>
        /// Determines whether a method represented by a <see cref="IMethodSymbol"/> instance is a property getter.
        /// </summary>
        /// <param name="methodInfo">A <see cref="MethodInfo"/> instance representing the method to check.</param>
        /// <returns>A value indicating whether the method is a property getter.</returns>
        public bool IsGetter
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => methodSymbol.Name.StartsWith("get_") && methodSymbol.Parameters.Length == 0;
        }
        /// <summary>
        /// Determines whether a method represented by a <see cref="IMethodSymbol"/> instance is a property setter.
        /// </summary>
        /// <param name="methodInfo">A <see cref="MethodInfo"/> instance representing the method to check.</param>
        /// <returns>A value indicating whether the method is a property setter.</returns>
        public bool IsSetter
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => methodSymbol.Name.StartsWith("set_") && methodSymbol.Parameters.Length == 1;
        }

        /// <summary>
        /// Determines whether a method represented by a <see cref="IMethodSymbol"/> instance is an event subscription adder or remover.
        /// </summary>
        /// <param name="methodInfo">A <see cref="MethodInfo"/> instance representing the method to check.</param>
        /// <returns>A value indicating whether the method is an event subscription adder or remover.</returns>
        public bool IsAdderOrRemover
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => methodSymbol.IsAdder || methodSymbol.IsRemover;
        }
        /// <summary>
        /// Determines whether a method represented by a <see cref="IMethodSymbol"/> instance is an event subscription adder.
        /// </summary>
        /// <param name="methodInfo">A <see cref="MethodInfo"/> instance representing the method to check.</param>
        /// <returns>A value indicating whether the method is an event subscription adder.</returns>
        public bool IsAdder
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => methodSymbol.Name.StartsWith("add_") && methodSymbol.Parameters.Length == 1;
        }
        /// <summary>
        /// Determines whether a method represented by a <see cref="IMethodSymbol"/> instance is an event subscription remover.
        /// </summary>
        /// <param name="methodInfo">A <see cref="MethodInfo"/> instance representing the method to check.</param>
        /// <returns>A value indicating whether the method is an event subscription remover.</returns>
        public bool IsRemover
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => methodSymbol.Name.StartsWith("remove_") && methodSymbol.Parameters.Length == 1;
        }

        /// <summary>
        /// Determines whether a method represented by a <see cref="IMethodSymbol"/> instance is an accessor (includes property getters/setters and event subscription adders/removers).
        /// </summary>
        /// <param name="methodInfo">A <see cref="MethodInfo"/> instance representing the method to check.</param>
        /// <returns>A value indicating whether the method is an accessor.</returns>
        public bool IsAccessor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => methodSymbol.IsGetterOrSetter || methodSymbol.IsAdderOrRemover;
        }
    }
    extension(ISymbol symbol)
    {
        /// <summary>
        /// Gets whether this <see cref="ISymbol"/> is an interface implementation (whether direct or indirect, explicit or implicit).
        /// </summary>
        public bool IsInterfaceImplementation
        {
            get
            {
                {
                    if (symbol is IMethodSymbol methodSymbol && methodSymbol.ExplicitInterfaceImplementations.Length > 0)
                    {
                        return true;
                    }
                }

                var declaringType = symbol.ContainingType;
                var interfaces = declaringType.AllInterfaces;
                for (var i = 0; i < interfaces.Length; i++)
                {
                    var interfaceMembers = interfaces[i].GetMembers();
                    var interfaceImplementations = interfaceMembers.Select(declaringType.FindImplementationForInterfaceMember).ToArray();

                    // Check direct implementation first
                    var isImpl = interfaceImplementations.Any(m => SymbolEqualityComparer.Default.Equals(m, symbol));
                    // If there's no hit, check up the override chain of the methods
                    if (!isImpl && symbol is IMethodSymbol methodSymbol)
                    {
                        var overrides = methodSymbol.OverrideChain.ToArray();
                        isImpl = overrides.Any(overriddenMethod => interfaceImplementations.Any(m => SymbolEqualityComparer.Default.Equals(m, overriddenMethod)));
                        if (isImpl)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }
    }
}
