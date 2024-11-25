using System.Linq.Expressions;
using System.Reflection;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="ConstructorInfo"/> Type.
/// </summary>
public static class ConstructorInfoExtensions
{
    /// <summary>
    /// Attempts to instantiate a new object of the type <paramref name="ctorInfo"/> belongs to, using the given <paramref name="parameters"/>.
    /// </summary>
    /// <param name="ctorInfo">The <see cref="ConstructorInfo"/> that identifies a constructor of the type to instantiate.</param>
    /// <param name="parameters">The parameters to pass to the constructor.</param>
    /// <returns>An instance of the type <paramref name="ctorInfo"/> belongs to, or <see langword="null"/> if the constructor could not be invoked.</returns>
    public static object New(this ConstructorInfo ctorInfo, params ReadOnlySpan<object> parameters)
    {
        try
        {
            return ctorInfo.Invoke(parameters.ToArray());
        }
        catch
        {
            return default;
        }
    }
    /// <summary>
    /// Attempts to instantiate a new object of the type <paramref name="ctorInfo"/> belongs to, using the given <paramref name="parameters"/> and returns it cast to <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to cast the new instance to.</typeparam>
    /// <param name="ctorInfo">The <see cref="ConstructorInfo"/> that identifies a constructor of the type to instantiate.</param>
    /// <param name="parameters">The parameters to pass to the constructor. May be <see langword="null"/> if the constructor has no parameters.</param>
    /// <returns>An instance of <typeparamref name="T"/>, or <see langword="null"/> if the constructor could not be invoked.</returns>
    public static T New<T>(this ConstructorInfo ctorInfo, params ReadOnlySpan<object> parameters)
    {
        try
        {
            return (T)ctorInfo.Invoke(parameters.ToArray());
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Creates a delegate that acts like the instance constructor represented by <paramref name="ctorInfo"/>.
    /// </summary>
    /// <typeparam name="TDelegate">The type of delegate to create. Must be a <see cref="Func{TResult}"/> overload.</typeparam>
    /// <param name="ctorInfo">The <see cref="ConstructorInfo"/> representing an instance constructor of a type.</param>
    /// <returns>The created delegate.</returns>
    /// <exception cref="ArgumentException">Thrown if the type specified for <typeparamref name="TDelegate"/> is not a <see cref="System.Func{TResult}"/> overload.</exception>
    /// <exception cref="ArgumentException">Thrown if the type specified for <typeparamref name="TDelegate"/> does not match the constructor's signature</exception>
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

        if ((ctorParams.Length > 1 && ctorParams.SequenceEqual(tDelegateParams)) || ctorInfo.DeclaringType != tDelegateReturn)
        {
            throw new ArgumentException($"""
                The type specified for {nameof(TDelegate)} does not match the constructor's signature. The return type and all parameters must match exactly:
                    Delegate:    delegate({string.Join(", ", tDelegateParams.Select(static p => p.ParameterType.Namespace + '.' + p.ParameterType.Name))}) => {tDelegateReturn.Namespace + '.' + tDelegateReturn.Name}
                    Constructor: .ctor({string.Join(", ", ctorParams.Select(static p => p.ParameterType.Namespace + '.' + p.ParameterType.Name))}) => {ctorInfo.DeclaringType.Namespace + '.' + ctorInfo.DeclaringType.Name}
                """, nameof(TDelegate));
        }

        var paramExprs = ctorParams.Select(static p => Expression.Parameter(p.ParameterType)).ToArray();
        var newExpr = Expression.New(ctorInfo, paramExprs);
        return Expression.Lambda<TDelegate>(newExpr, paramExprs).Compile();
    }
}
