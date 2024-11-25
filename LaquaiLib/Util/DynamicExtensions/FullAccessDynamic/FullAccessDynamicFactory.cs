using System.Reflection;
using System.Runtime.CompilerServices;

namespace LaquaiLib.Util.DynamicExtensions.FullAccessDynamic;

/// <summary>
/// Provides static factory methods for <see cref="FullAccessDynamic{T}"/> instances.
/// </summary>
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
    public static dynamic Create(Type type)
    {
        return type == typeof(void)
            ? null
            : (dynamic)Activator.CreateInstance(typeof(FullAccessDynamic<>).MakeGenericType(type), bindingAttr: BindingFlags.Instance | BindingFlags.NonPublic, null, [Activator.CreateInstance(type)], null);
    }
    /// <summary>
    /// Creates a new instance of <see cref="FullAccessDynamic{T}"/> that has the specified <paramref name="type"/> and wraps the specified object <paramref name="instance"/>. This may be <see langword="null"/>.
    /// </summary>
    /// <param name="type">The type of the object to wrap.</param>
    /// <param name="instance">The instance to wrap.</param>
    /// <returns>A new instance of <see cref="FullAccessDynamic{T}"/> that has the specified <paramref name="type"/> and wraps the specified object <paramref name="instance"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static dynamic Create(Type type, object instance)
    {
        return type == typeof(void)
            ? null
            : (dynamic)Activator.CreateInstance(typeof(FullAccessDynamic<>).MakeGenericType(type), bindingAttr: BindingFlags.Instance | BindingFlags.NonPublic, null, [instance], null);
    }

    /// <summary>
    /// Creates a new instance of <see cref="FullAccessDynamic{T}"/> that wraps the current object instance.
    /// </summary>
    /// <typeparam name="T">The type of the object to wrap.</typeparam>
    /// <param name="instance">The instance to wrap.</param>
    /// <returns>The created <see cref="FullAccessDynamic{T}"/> instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static dynamic GetFullAccessDynamic<T>(this T instance) => new FullAccessDynamic<T>(instance);
}
