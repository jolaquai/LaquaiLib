using System.CodeDom.Compiler;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsGetterOrSetter(this MethodInfo methodInfo) => IsGetter(methodInfo) || IsSetter(methodInfo);

    /// <summary>
    /// Determines whether a method represented by a <paramref name="methodInfo"/> instance is a property getter.
    /// </summary>
    /// <param name="methodInfo">A <see cref="MethodInfo"/> instance representing the method to check.</param>
    /// <returns>A value indicating whether the method is a property getter.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsGetter(this MethodInfo methodInfo) => methodInfo.Name.StartsWith("get_") && methodInfo.GetParameters().Length == 0;
    /// <summary>
    /// Determines whether a method represented by a <paramref name="methodInfo"/> instance is a property setter.
    /// </summary>
    /// <param name="methodInfo">A <see cref="MethodInfo"/> instance representing the method to check.</param>
    /// <returns>A value indicating whether the method is a property setter.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSetter(this MethodInfo methodInfo) => methodInfo.Name.StartsWith("set_") && methodInfo.GetParameters().Length == 1;

    /// <summary>
    /// Determines whether a method represented by a <paramref name="methodInfo"/> instance is an event subscription adder or remover.
    /// </summary>
    /// <param name="methodInfo">A <see cref="MethodInfo"/> instance representing the method to check.</param>
    /// <returns>A value indicating whether the method is an event subscription adder or remover.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAdderOrRemover(this MethodInfo methodInfo) => IsAdder(methodInfo) || IsRemover(methodInfo);

    /// <summary>
    /// Determines whether a method represented by a <paramref name="methodInfo"/> instance is an event subscription adder.
    /// </summary>
    /// <param name="methodInfo">A <see cref="MethodInfo"/> instance representing the method to check.</param>
    /// <returns>A value indicating whether the method is an event subscription adder.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAdder(this MethodInfo methodInfo) => methodInfo.Name.StartsWith("add_") && methodInfo.GetParameters().Length == 1;
    /// <summary>
    /// Determines whether a method represented by a <paramref name="methodInfo"/> instance is an event subscription remover.
    /// </summary>
    /// <param name="methodInfo">A <see cref="MethodInfo"/> instance representing the method to check.</param>
    /// <returns>A value indicating whether the method is an event subscription remover.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRemover(this MethodInfo methodInfo) => methodInfo.Name.StartsWith("remove_") && methodInfo.GetParameters().Length == 1;

    /// <summary>
    /// Determines whether a method represented by a <paramref name="methodInfo"/> instance is an accessor (includes property getters/setters and event subscription adders/removers).
    /// </summary>
    /// <param name="methodInfo">A <see cref="MethodInfo"/> instance representing the method to check.</param>
    /// <returns>A value indicating whether the method is an accessor.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAccessor(this MethodInfo methodInfo) => methodInfo.IsGetterOrSetter() || methodInfo.IsAdderOrRemover();

    /// <summary>
    /// Determines whether a method represented by a <paramref name="methodInfo"/> instance is marked <see langword="extern"/>.
    /// </summary>
    /// <param name="methodInfo">The <see cref="MethodInfo"/> instance representing the method to check.</param>
    /// <returns><see langword="true"/> if the method is marked <see langword="extern"/>, otherwise <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsExtern(this MethodInfo methodInfo) => methodInfo.GetMethodBody() is null;
    /// <summary>
    /// Determines whether a method represented by a <paramref name="methodInfo"/> instance is marked <see langword="partial"/>.
    /// </summary>
    /// <param name="methodInfo">The <see cref="MethodInfo"/> instance representing the method to check.</param>
    /// <returns><see langword="true"/> if the method is marked <see langword="partial"/>, otherwise <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPartial(this MethodInfo methodInfo)
    {
        var type = methodInfo.DeclaringType;
        return type?.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Count(m => m.Name == methodInfo.Name || m.Name.Contains($"<{methodInfo.Name}>", StringComparison.OrdinalIgnoreCase)) > 1;
    }

    public delegate void BodyGenerator(IndentedTextWriter writer, string accessibility, IReadOnlyList<string> modifiers, string returnType, string methodName, IReadOnlyList<string> genericParameters, IReadOnlyList<(string Type, string Name, object DefaultValue)> parameters);
    /// <summary>
    /// Gets a string representation of the signature of the method represented by the specified <see cref="MethodInfo"/>, optionally applying any transforms as specified by the provided factory methods or generating a body.
    /// </summary>
    /// <param name="method">The <see cref="MethodInfo"/> instance representing the method.</param>
    /// <returns>A string representation of the method's signature.</returns>
    public static string RebuildMethod(this MethodInfo method,
        Func<string, string> accessibilityTransform = null,
        Action<List<string>> modifiersTransform = null,
        Func<string, string> returnTypeTransform = null,
        Action<List<string>> genericParametersTransform = null,
        Func<string, string> nameTransform = null,
        Action<List<(string, string, object)>> parametersTransform = null,
        BodyGenerator bodyGenerator = null
    )
    {
        var accessibility = method.GetAccessibility();
        if (accessibilityTransform is not null)
        {
            accessibility = accessibilityTransform(accessibility);
        }

        // Determine if the generic method requires an unsafe context
        var unsafeRequired = method.GetParameters().Any(p => p.ParameterType.IsPointer);

        var sb = new StringBuilder();

        var modifiers = new List<string>();
#pragma warning disable IDE0058 // Expression value is never used
        if (method.IsStatic)
        {
            modifiers.Add("static");
        }
        if (unsafeRequired)
        {
            modifiers.Add("unsafe");
        }
        if (method.IsAbstract)
        {
            modifiers.Add("abstract");
        }
        if (method.IsVirtual)
        {
            modifiers.Add("virtual");
        }

        if (modifiersTransform is not null)
        {
            modifiersTransform(modifiers);
        }

        sb.Append(accessibility);
        sb.Append(' ');
        sb.AppendJoin(' ', modifiers);

        var returnType = method.ReturnType.GetFriendlyName();
        if (returnTypeTransform is not null)
        {
            returnType = returnTypeTransform(returnType);
        }

        sb.Append(' ');
        sb.Append(returnType);
        sb.Append(' ');

        var methodName = method.Name;
        if (nameTransform is not null)
        {
            methodName = nameTransform(methodName);
        }
        sb.Append(methodName);

        List<string> genericParameters = null;
        if (method.IsGenericMethod)
        {
            genericParameters = method.GetGenericArguments().Select(t => t.Name).ToList();
            if (genericParametersTransform is not null)
            {
                genericParametersTransform(genericParameters);
            }

            sb.Append($"<{string.Join(", ", genericParameters)}>");
        }

        var parameters = method.GetParameters().Select(p => (p.ParameterType.GetFriendlyName(), p.Name, p.DefaultValue)).ToList();
        if (parametersTransform is not null)
        {
            parametersTransform(parameters);
        }

        sb.Append('(');
        var first = true;
        foreach (var (type, name, defaultValue) in parameters)
        {
            if (!first)
            {
                sb.Append(", ");
            }
            first = false;

            sb.Append(type);
            sb.Append(' ');
            sb.Append(name);

            if (defaultValue is not (null or DBNull))
            {
                sb.Append(" = ");
                sb.Append(defaultValue);
            }
        }
        sb.Append(')');

        if (bodyGenerator is not null)
        {
            using var writer = new StringWriter(sb);
            using var itw = new IndentedTextWriter(writer, "    ");
            bodyGenerator(itw, accessibility, modifiers, returnType, methodName, genericParameters, parameters);
            sb.AppendLine();
        }
        else
        {
            sb.Append(';');
        }
#pragma warning restore IDE0058 // Expression value is never used

        return sb.ToString();
    }
}
