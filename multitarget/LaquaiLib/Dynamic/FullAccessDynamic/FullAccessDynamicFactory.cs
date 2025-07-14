using System.Linq.Expressions;
using System.Reflection;

namespace LaquaiLib.Dynamic;

/// <summary>
/// Provides static factory methods for <see cref="FullAccessDynamic{T}"/> instances.
/// </summary>
[Obsolete($"{nameof(FullAccessDynamic<>)} has been obsoleted in favor of the source generator using {nameof(LaquaiLib.Analyzers.Shared.Attributes.FullAccessProxyAttribute<>)}.")]
public static class FullAccessDynamicFactory
{
    /// <summary>
    /// Creates a new instance of <see cref="FullAccessDynamic{T}"/> that wraps a new instance of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to wrap.</typeparam>
    /// <returns>A new instance of <see cref="FullAccessDynamic{T}"/> that wraps a new instance of <typeparamref name="T"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static dynamic Create<T>() => new FullAccessDynamic<T>();
    /// <summary>
    /// Creates a new instance of <see cref="FullAccessDynamic{T}"/> that wraps the specified instance of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to wrap.</typeparam>
    /// <param name="instance">The instance to wrap.</param>
    /// <returns>A new instance of <see cref="FullAccessDynamic{T}"/> that wraps the specified instance of <typeparamref name="T"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static dynamic Create<T>(T instance) => new FullAccessDynamic<T>(instance);
    /// <summary>
    /// Creates a new instance of <see cref="FullAccessDynamic{T}"/> that has the specified <paramref name="type"/> and wraps a new instance of that type.
    /// </summary>
    /// <param name="type">The type of the object to wrap.</param>
    /// <returns>A new instance of <see cref="FullAccessDynamic{T}"/> that has the specified <paramref name="type"/> and wraps a new instance of that type.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static dynamic Create(Type type) => type == typeof(void)
            ? null
            : (dynamic)Activator.CreateInstance(typeof(FullAccessDynamic<>).MakeGenericType(type), bindingAttr: BindingFlags.Instance | BindingFlags.NonPublic, null, [Activator.CreateInstance(type)], null);
    /// <summary>
    /// Creates a new instance of <see cref="FullAccessDynamic{T}"/> that has the specified <paramref name="type"/> and wraps the specified object <paramref name="instance"/>. This may be <see langword="null"/>.
    /// </summary>
    /// <param name="type">The type of the object to wrap.</param>
    /// <param name="instance">The instance to wrap.</param>
    /// <returns>A new instance of <see cref="FullAccessDynamic{T}"/> that has the specified <paramref name="type"/> and wraps the specified object <paramref name="instance"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static dynamic Create(Type type, object instance)
    {
        // The generic version of this method is more efficient by orders of magnitude
        // The problem is, given only a Type, we have no choice but to use reflection to create the instance
        // Only smart thing to make this worthwhile is to emit an optimized Func<object, dynamic> for each type

        if (type == typeof(void))
        {
            return null;
        }

        if (!_createCache.TryGetValue(type, out var func))
        {
            var parameter = Expression.Parameter(typeof(object), "instance");
            var convertedParameter = Expression.Convert(parameter, type);
            var ctorInfo = typeof(FullAccessDynamic<>).MakeGenericType(type).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, [type]);
            var newExpression = Expression.New(ctorInfo, convertedParameter);
            func = Expression.Lambda<Func<object, dynamic>>(newExpression, parameter).Compile();
            _createCache[type] = func;
        }
        return func(instance);
    }
    private static readonly ConcurrentDictionary<Type, Func<object, dynamic>> _createCache = [];

    /// <summary>
    /// Creates a new instance of <see cref="FullAccessDynamic{T}"/> that wraps the current object instance.
    /// </summary>
    /// <typeparam name="T">The type of the object to wrap.</typeparam>
    /// <param name="instance">The instance to wrap.</param>
    /// <returns>The created <see cref="FullAccessDynamic{T}"/> instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static dynamic GetFullAccessDynamic<T>(this T instance) => new FullAccessDynamic<T>(instance);
}
