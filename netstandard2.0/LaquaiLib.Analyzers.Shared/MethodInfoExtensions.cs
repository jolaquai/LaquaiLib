using System.CodeDom.Compiler;
using System.Reflection;
using System.Text;

namespace LaquaiLib.Analyzers.Shared;

/// <summary>
/// Provides extension methods for the <see cref="MethodInfo"/> Type.
/// </summary>
public static class MethodInfoExtensions
{
    extension(MethodBase methodBase)
    {
        /// <summary>
        /// Determines whether a method represented by a <paramref name="methodInfo"/> instance is a property getter or setter.
        /// </summary>
        /// <param name="methodInfo">A <see cref="MethodInfo"/> instance representing the method to check.</param>
        /// <returns>A value indicating whether the method is a property getter or setter.</returns>
        public bool IsGetterOrSetter
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => methodBase.IsGetter || methodBase.IsSetter;
        }
        /// <summary>
        /// Determines whether a method represented by a <paramref name="methodInfo"/> instance is a property getter.
        /// </summary>
        /// <param name="methodInfo">A <see cref="MethodInfo"/> instance representing the method to check.</param>
        /// <returns>A value indicating whether the method is a property getter.</returns>
        public bool IsGetter
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => methodBase.Name.StartsWith("get_") && methodBase.GetParameters().Length == 0;
        }
        /// <summary>
        /// Determines whether a method represented by a <paramref name="methodInfo"/> instance is a property setter.
        /// </summary>
        /// <param name="methodInfo">A <see cref="MethodInfo"/> instance representing the method to check.</param>
        /// <returns>A value indicating whether the method is a property setter.</returns>
        public bool IsSetter
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => methodBase.Name.StartsWith("set_") && methodBase.GetParameters().Length == 1;
        }

        /// <summary>
        /// Determines whether a method represented by a <paramref name="methodInfo"/> instance is an event subscription adder or remover.
        /// </summary>
        /// <param name="methodInfo">A <see cref="MethodInfo"/> instance representing the method to check.</param>
        /// <returns>A value indicating whether the method is an event subscription adder or remover.</returns>
        public bool IsAdderOrRemover
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => methodBase.IsAdder || methodBase.IsRemover;
        }
        /// <summary>
        /// Determines whether a method represented by a <paramref name="methodInfo"/> instance is an event subscription adder.
        /// </summary>
        /// <param name="methodInfo">A <see cref="MethodInfo"/> instance representing the method to check.</param>
        /// <returns>A value indicating whether the method is an event subscription adder.</returns>
        public bool IsAdder
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => methodBase.Name.StartsWith("add_") && methodBase.GetParameters().Length == 1;
        }
        /// <summary>
        /// Determines whether a method represented by a <paramref name="methodInfo"/> instance is an event subscription remover.
        /// </summary>
        /// <param name="methodInfo">A <see cref="MethodInfo"/> instance representing the method to check.</param>
        /// <returns>A value indicating whether the method is an event subscription remover.</returns>
        public bool IsRemover
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => methodBase.Name.StartsWith("remove_") && methodBase.GetParameters().Length == 1;
        }

        /// <summary>
        /// Determines whether a method represented by a <paramref name="methodInfo"/> instance is an accessor (includes property getters/setters and event subscription adders/removers).
        /// </summary>
        /// <param name="methodInfo">A <see cref="MethodInfo"/> instance representing the method to check.</param>
        /// <returns>A value indicating whether the method is an accessor.</returns>
        public bool IsAccessor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => methodBase.IsGetterOrSetter || methodBase.IsAdderOrRemover;
        }

        /// <summary>
        /// Determines whether a method represented by a <paramref name="methodInfo"/> instance is marked <see langword="extern"/>.
        /// </summary>
        /// <param name="methodInfo">The <see cref="MethodInfo"/> instance representing the method to check.</param>
        /// <returns><see langword="true"/> if the method is marked <see langword="extern"/>, otherwise <see langword="false"/>.</returns>
        public bool IsExtern
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => methodBase.GetMethodBody() is null;
        }
        /// <summary>
        /// Determines whether a method represented by a <paramref name="methodInfo"/> instance is marked <see langword="partial"/>.
        /// </summary>
        /// <param name="methodInfo">The <see cref="MethodInfo"/> instance representing the method to check.</param>
        /// <returns><see langword="true"/> if the method is marked <see langword="partial"/>, otherwise <see langword="false"/>.</returns>
        public bool IsPartial
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var type = methodBase.DeclaringType;
                return type?.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Count(m => m.Name == methodBase.Name || m.Name.Contains($"<{methodBase.Name}>", StringComparison.OrdinalIgnoreCase)) > 1;
            }
        }

        /// <summary>
        /// Gets a string representation of the signature of the method represented by the specified <see cref="MethodInfo"/>.
        /// </summary>
        /// <returns>A string representation of the method's signature.</returns>
        public string Signature
        {
            get
            {
                var accessibility = methodBase.GetAccessibility();

                // Determine if the generic method requires an unsafe context
                var unsafeRequired = methodBase.GetParameters().Any(static p => p.ParameterType.IsPointer);

#pragma warning disable IDE0058 // Expression value is never used
                var sb = new StringBuilder();

                var friendlyTypeName = methodBase.DeclaringType.GetFriendlyName();
                sb.Append("    ");

                var modifiers = new List<string>();
                if (methodBase.IsStatic)
                {
                    modifiers.Add("static");
                }
                if (unsafeRequired)
                {
                    modifiers.Add("unsafe");
                }
                if (methodBase.IsAbstract)
                {
                    modifiers.Add("abstract");
                }
                if (methodBase.IsVirtual)
                {
                    modifiers.Add("virtual");
                }

                sb.Append(accessibility);
                sb.Append(' ');
                sb.Append(string.Join(" ", modifiers));

                string returnType;
                if (methodBase is MethodInfo methodInfo)
                {
                    returnType = methodInfo.ReturnType.GetFriendlyName();
                }
                else if (methodBase is ConstructorInfo constructorInfo)
                {
                    returnType = constructorInfo.DeclaringType.GetFriendlyName();
                }
                else
                {
                    throw new InvalidOperationException("MethodBase is neither MethodInfo nor ConstructorInfo.");
                }

                sb.Append(' ');
                sb.Append(returnType);
                sb.Append(' ');

                var methodName = methodBase.Name;
                sb.Append(methodName);

                List<string> genericParameters = null;
                if (methodBase.IsGenericMethod)
                {
                    genericParameters = [.. methodBase.GetGenericArguments().Select(static t => t.Name)];

                    if (genericParameters.Count > 0)
                    {
                        sb.Append($"<{string.Join(", ", genericParameters)}>");
                    }
                }

                var parameters = methodBase.GetParameters().Select(static p => (p.ParameterType.GetFriendlyName(), p.Name, p.DefaultValue)).ToList();

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

                sb.Append(';');
#pragma warning restore IDE0058 // Expression value is never used

                return sb.ToString();
            }
        }

        /// <summary>
        /// Gets a <see langword="string"/> representation of the parameters of the method represented by the specified <see cref="MethodBase"/> (that is, a comma-space-separated list of parameter types and names).
        /// </summary>
        public string ParameterString
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => string.Join(", ", methodBase.GetParameters().Select(p => $"{p.ParameterType.GetFriendlyName()} {p.Name}"));
        }
        /// <summary>
        /// Gets a comma-space-separated list of parameter names that can be used to call the method represented by the specified <see cref="MethodBase"/>.
        /// </summary>
        public string ArgumentString
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => string.Join(", ", methodBase.GetParameters().Select(p => p.Name));
        }
    }

    /// <summary>
    /// Represents a method that may be called by <see cref="RebuildMethod(MethodInfo, bool, Func{string, string}, Action{List{string}}, Func{string, string}, Action{List{string}}, Func{string, string}, Action{List{ValueTuple{string, string, object}}}, BodyGenerator)"/> to generate the body of a method to be rebuilt.
    /// </summary>
    /// <param name="writer">An <see cref="IndentedTextWriter"/> instance to write the method body to.</param>
    /// <param name="accessibility">The accessibility of the method.</param>
    /// <param name="modifiers">The modifiers of the method.</param>
    /// <param name="returnType">The return type of the method.</param>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="genericParameters">The generic parameters of the method.</param>
    /// <param name="parameters">The parameters of the method.</param>
    public delegate void BodyGenerator(IndentedTextWriter writer, string accessibility, IReadOnlyList<string> modifiers, string returnType, string methodName, IReadOnlyList<string> genericParameters, IReadOnlyList<(string Type, string Name, object DefaultValue)> parameters);
}
