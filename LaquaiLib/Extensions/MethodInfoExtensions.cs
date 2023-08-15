using System.Reflection;

using LaquaiLib.Extensions;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="MethodInfo"/> Type.
/// </summary>
public static class MethodInfoExtensions
{
    /// <summary>
    /// Determines whether a method represented by a <paramref name="methodInfo"/> instance is a property getter or setter.
    /// </summary>
    /// <param name="methodInfo">A <see cref="MethodInfo"/> instance representing the method to check.</param>
    /// <returns>A value indicating whether the method is a property getter or setter.</returns>
    public static bool IsGetterOrSetter(this MethodInfo methodInfo) => IsGetter(methodInfo) || IsSetter(methodInfo);

    /// <summary>
    /// Determines whether a method represented by a <paramref name="methodInfo"/> instance is a property getter.
    /// </summary>
    /// <param name="methodInfo">A <see cref="MethodInfo"/> instance representing the method to check.</param>
    /// <returns>A value indicating whether the method is a property getter.</returns>
    public static bool IsGetter(this MethodInfo methodInfo) => methodInfo.Name.StartsWith("get_") && methodInfo.GetParameters().Length == 0;
    /// <summary>
    /// Determines whether a method represented by a <paramref name="methodInfo"/> instance is a property setter.
    /// </summary>
    /// <param name="methodInfo">A <see cref="MethodInfo"/> instance representing the method to check.</param>
    /// <returns>A value indicating whether the method is a property setter.</returns>
    public static bool IsSetter(this MethodInfo methodInfo) => methodInfo.Name.StartsWith("set_") && methodInfo.GetParameters().Length == 1;

    /// <summary>
    /// Determines whether a method represented by a <paramref name="methodInfo"/> instance is an event subscription adder or remover.
    /// </summary>
    /// <param name="methodInfo">A <see cref="MethodInfo"/> instance representing the method to check.</param>
    /// <returns>A value indicating whether the method is an event subscription adder or remover.</returns>
    public static bool IsAdderOrRemover(this MethodInfo methodInfo) => IsAdder(methodInfo) || IsRemover(methodInfo);

    /// <summary>
    /// Determines whether a method represented by a <paramref name="methodInfo"/> instance is an event subscription adder.
    /// </summary>
    /// <param name="methodInfo">A <see cref="MethodInfo"/> instance representing the method to check.</param>
    /// <returns>A value indicating whether the method is an event subscription adder.</returns>
    public static bool IsAdder(this MethodInfo methodInfo) => methodInfo.Name.StartsWith("add_") && methodInfo.GetParameters().Length == 1;
    /// <summary>
    /// Determines whether a method represented by a <paramref name="methodInfo"/> instance is an event subscription remover.
    /// </summary>
    /// <param name="methodInfo">A <see cref="MethodInfo"/> instance representing the method to check.</param>
    /// <returns>A value indicating whether the method is an event subscription remover.</returns>
    public static bool IsRemover(this MethodInfo methodInfo) => methodInfo.Name.StartsWith("remove_") && methodInfo.GetParameters().Length == 1;

    /// <summary>
    /// Determines whether a method represented by a <paramref name="methodInfo"/> instance is an accessor (includes property getters/setters and event subscription adders/removers).
    /// </summary>
    /// <param name="methodInfo">A <see cref="MethodInfo"/> instance representing the method to check.</param>
    /// <returns>A value indicating whether the method is an accessor.</returns>
    public static bool IsAccessor(this MethodInfo methodInfo) => methodInfo.IsGetterOrSetter() || methodInfo.IsAdderOrRemover();
}
