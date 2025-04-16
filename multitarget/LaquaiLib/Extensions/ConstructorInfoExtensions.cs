using System.Linq.Expressions;
using System.Reflection;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="ConstructorInfo"/> Type.
/// </summary>
public static class ConstructorInfoExtensions
{
    private static EqualityComparer<ParameterInfo> ParameterInfoTypeComparer => field ??= EqualityComparer<ParameterInfo>.Create(static (x, y) => x.ParameterType == y.ParameterType);

    /// <summary>
    /// Creates a delegate that acts like the instance constructor represented by <paramref name="ctorInfo"/>.
    /// </summary>
    /// <typeparam name="TDelegate">The type of delegate to create. Must be a <see cref="Func{TResult}"/> overload.</typeparam>
    /// <param name="ctorInfo">The <see cref="ConstructorInfo"/> representing an instance constructor of a type.</param>
    /// <returns>The created delegate.</returns>
    /// <exception cref="ArgumentException">Thrown if the type specified for <typeparamref name="TDelegate"/> is not a <see cref="Func{TResult}"/> overload.</exception>
    /// <exception cref="ArgumentException">Thrown if the type specified for <typeparamref name="TDelegate"/> does not match the constructor's signature</exception>
    /// <remarks>
    /// This method is to be used in the same vein as types in <see cref="System.Collections.Frozen"/>. The cost to create the <see langword="delegate"/> from the <see cref="ConstructorInfo"/> is slightly higher, but is amortized when used extensively.
    /// </remarks>
    public static TDelegate CreateDelegate<TDelegate>(this ConstructorInfo ctorInfo)
        where TDelegate : Delegate
    {
        // Prevent mismatch between TDelegate and the constructor's signature
        var ctorParams = ctorInfo.GetParameters();
        var tDelegateParams = typeof(TDelegate).GetMethod("Invoke")!.GetParameters();
        if (!typeof(TDelegate).IsFunc(out var tDelegateReturn))
        {
            throw new ArgumentException($"The type specified for {nameof(TDelegate)} is not a System.Func overload.", nameof(TDelegate));
        }

        if (ctorParams.Length != tDelegateParams.Length || !ctorParams.SequenceEqual(tDelegateParams, ParameterInfoTypeComparer) || ctorInfo.DeclaringType != tDelegateReturn)
        {
            throw new ArgumentException($"""
                The type specified for {nameof(TDelegate)} does not match the constructor's signature. The return type and all parameter types must match exactly:
                    Delegate:    delegate({string.Join(", ", tDelegateParams.Select(static p => p.ParameterType.Namespace + '.' + p.ParameterType.Name))}) => {tDelegateReturn.Namespace + '.' + tDelegateReturn.Name}
                    Constructor: .ctor({string.Join(", ", ctorParams.Select(static p => p.ParameterType.Namespace + '.' + p.ParameterType.Name))}) => {ctorInfo.DeclaringType.Namespace + '.' + ctorInfo.DeclaringType.Name}
                """, nameof(TDelegate));
        }

        var paramExprs = ctorParams.Select(static p => Expression.Parameter(p.ParameterType)).ToArray();
        var newExpr = Expression.New(ctorInfo, paramExprs);
        return Expression.Lambda<TDelegate>(newExpr, paramExprs).Compile();
    }
}
