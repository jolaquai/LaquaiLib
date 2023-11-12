using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;

using LaquaiLib.Attributes;
using LaquaiLib.Util;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Type"/> Type.
/// </summary>
public static class TypeExtensions
{
    #region Type derivation / implementation stuff
    /// <summary>
    /// Returns a collection of all types that implement the supplied interface.
    /// </summary>
    /// <param name="type">The interface type to get the implementing types for.</param>
    /// <returns>A collection of all types that implement the supplied interface.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="type"/> is not an interface type.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="type"/>'s assembly cannot be resolved.</exception>
    public static IEnumerable<Type> GetInterfaceImplementingTypes(this Type type)
    {
        if (!type.IsInterface)
        {
            throw new ArgumentException("Supplied type must be an interface.", nameof(type));
        }

        if (Assembly.GetAssembly(type) is Assembly assemblyOfInput)
        {
            return assemblyOfInput.GetTypes().Where(t => t.GetInterfaces().Contains(type));
        }
        else
        {
            throw new ArgumentException("Supplied type must be part of an assembly.", nameof(type));
        }
    }

    /// <summary>
    /// Returns a collection of all types that inherit from the supplied type.
    /// </summary>
    /// <param name="type">The type to get the inheriting types for.</param>
    /// <returns>A collection of all types that inherit from the supplied type.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="type"/>'s assembly cannot be resolved.</exception>
    public static IEnumerable<Type> GetInheritingTypes(this Type type)
    {
        if (Assembly.GetAssembly(type) is Assembly assemblyOfInput)
        {
            return assemblyOfInput.GetTypes().Where(t => t.IsSubclassOf(type));
        }
        else
        {
            throw new ArgumentException("Supplied type must be part of an assembly.", nameof(type));
        }
    }

    /// <summary>
    /// Returns a collection of all types that inherit from the supplied type and are not abstract.
    /// </summary>
    /// <param name="type">The type to get the non-abstract inheriting types for.</param>
    /// <returns>A collection of all types that inherit from the supplied type and are not abstract.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="type"/>'s assembly cannot be resolved.</exception>
    public static IEnumerable<Type> GetNonAbstractInheritingTypes(this Type type)
    {
        if (Assembly.GetAssembly(type) is Assembly assemblyOfInput)
        {
            return assemblyOfInput.GetTypes().Where(t => t.IsSubclassOf(type) && !t.IsAbstract);
        }
        else
        {
            throw new ArgumentException("Supplied type must be part of an assembly.", nameof(type));
        }
    }

    /// <summary>
    /// Returns a collection of all types that inherit from the supplied type and contain public constructors.
    /// </summary>
    /// <param name="type">The type to get the constructable inheriting types for.</param>
    /// <returns>A collection of all types that inherit from the supplied type and contain public constructors.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="type"/>'s assembly cannot be resolved.</exception>
    public static IEnumerable<Type> GetConstructableInheritingTypes(this Type type)
    {
        if (Assembly.GetAssembly(type) is Assembly assemblyOfInput)
        {
            return assemblyOfInput.GetTypes().Where(t => t.IsSubclassOf(type) && t.GetConstructors().Any(c => c.IsPublic));
        }
        else
        {
            throw new ArgumentException("Supplied type must be part of an assembly.", nameof(type));
        }
    }
    #endregion

    /// <summary>
    /// Attempts to instantiate a new object of the supplied <paramref name="type"/> using the given <paramref name="parameters"/>.
    /// </summary>
    /// <param name="type">A <see cref="Type"/> instance representing the type to instantiate.</param>
    /// <param name="parameters">The parameters to pass to the constructor. May be <see langword="null"/> to target the parameterless constructor.</param>
    /// <returns>An instance of the supplied <paramref name="type"/>, or <see langword="null"/> if a constructor matching the given <paramref name="parameters"/> could not be found or that constructor could not be invoked.</returns>
    public static object? New(this Type type, params object?[]? parameters)
    {
        try
        {
            if (parameters is null)
            {
                return Activator.CreateInstance(type);
            }
            var types = parameters.Select(obj => obj?.GetType()).ToArray();
            return type.GetConstructor(types).New(parameters);
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Returns the default value for the supplied type.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to get the default value for.</param>
    /// <returns>The default value for the supplied type.</returns>
    public static object? GetDefault(this Type type) => type.IsValueType ? Activator.CreateInstance(type) : (type == typeof(string) ? string.Empty : null);

    /// <summary>
    /// Compiles a <see cref="Dictionary{TKey, TValue}"/> of all instance fields and properties of the supplied type from the given object, optionally calling all parameterless methods that do not return void.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> the <see cref="FieldInfo"/>, <see cref="PropertyInfo"/> and <see cref="MethodInfo"/> instances are to be reflected from.</param>
    /// <param name="obj">The object to use to collect the values from.</param>
    /// <param name="callMethods">Whether to call all parameterless methods that do not return void instead of adding all method names to the output dictionary. This is a dangerous operation and should only be used if the methods are known to be safe and not have side effects.</param>
    /// <returns>The <see cref="Dictionary{TKey, TValue}"/> as described.</returns>
    public static Dictionary<string, object?> GetInstanceValues(this Type type, object obj, bool callMethods = false)
    {
        var dict = new Dictionary<string, object?>();
        var members = type.GetMembers();

        foreach (var memberInfo in members.Where(member => member.MemberType is MemberTypes.Field
            or MemberTypes.Property
            or MemberTypes.Method))
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    var fieldInfo = (FieldInfo)memberInfo;
                    dict.Add(fieldInfo.Name, fieldInfo.GetValue(obj));
                    break;
                case MemberTypes.Property:
                    var propertyInfo = (PropertyInfo)memberInfo;
                    dict.Add(propertyInfo.Name, propertyInfo.GetValue(obj));
                    break;
                case MemberTypes.Method:
                    var methodInfo = (MethodInfo)memberInfo;
                    if (callMethods)
                    {
                        if (methodInfo.ReturnType != typeof(void))
                        {
                            dict.Add(methodInfo.Name, methodInfo.Invoke(obj, null));
                        }
                    }
                    else
                    {
                        dict.Add($"{methodInfo.Name}({string.Join(", ", methodInfo.GetParameters().Select(paramInfo => $"{paramInfo.ParameterType.FullName} {paramInfo.Name}"))}", null);
                    }
                    break;
            }
        }

        return dict;
    }

    /// <summary>
    /// Compiles a <see cref="Dictionary{TKey, TValue}"/> of all static fields and properties of the supplied type, optionally calling all parameterless methods that do not return void.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> the <see cref="FieldInfo"/>, <see cref="PropertyInfo"/> and <see cref="MethodInfo"/> instances are to be reflected from.</param>
    /// <param name="callMethods">Whether to call all parameterless methods that do not return void. This is a dangerous operation and should only be used if the methods are known to be safe and not have side effects.</param>
    /// <returns>The <see cref="Dictionary{TKey, TValue}"/> as described.</returns>
    public static Dictionary<string, object?> GetStaticValues(this Type type, bool callMethods = false)
    {
        var dict = new Dictionary<string, object?>();
        var members = type.GetMembers(BindingFlags.Public | BindingFlags.Static);

        foreach (var memberInfo in members.Where(member => member.MemberType is MemberTypes.Field
            or MemberTypes.Property
            or MemberTypes.Method))
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    var fieldInfo = (FieldInfo)memberInfo;
                    dict.Add(fieldInfo.Name, fieldInfo.GetValue(null));
                    break;
                case MemberTypes.Property:
                    var propertyInfo = (PropertyInfo)memberInfo;
                    dict.Add(propertyInfo.Name, propertyInfo.GetValue(null));
                    break;
                case MemberTypes.Method:
                    var methodInfo = (MethodInfo)memberInfo;
                    if (!methodInfo.IsAccessor())
                    {
                        if (callMethods)
                        {
                            if (methodInfo.ReturnType != typeof(void))
                            {
                                dict.Add(methodInfo.Name, methodInfo.Invoke(null, null));
                            }
                        }
                        else
                        {
                            dict.Add($"{methodInfo.Name}({string.Join(", ", methodInfo.GetParameters().Select(paramInfo => $"{paramInfo.ParameterType.FullName} {paramInfo.Name}"))})", null);
                        }
                    }
                    break;
            }
        }

        return dict;
    }

    /// <summary>
    /// Determines whether an instance of this <see cref="Type"/> can be cast to the given <paramref name="other"/> <see cref="Type"/>.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> of the instance to be cast.</param>
    /// <param name="other">The <see cref="Type"/> to cast to.</param>
    /// <returns><see langword="true"/> if an instance of <paramref name="type"/> can be cast to <paramref name="other"/>, otherwise <see langword="false"/>.</returns>
    public static bool CanCastTo(this Type type, Type other)
    {
        try
        {
            _ = Convert.ChangeType(Activator.CreateInstance(type), other);
            return true;
        }
        catch (Exception ex)
            when (ex is InvalidCastException
                or FormatException
                or OverflowException
                or ArgumentNullException
                or ArgumentException
                or TargetInvocationException)
        {
            return false;
        }
        catch (Exception ex)
            when (ex is MissingMethodException)
        {
            try
            {
                _ = Convert.ChangeType(type.GetDefault(), other);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
    /// <summary>
    /// Determines whether an instance of the given <paramref name="type"/> can be cast to this <see cref="Type"/>.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to cast to.</param>
    /// <param name="other">The <see cref="Type"/> of the instance to be cast.</param>
    /// <returns><see langword="true"/> if an instance of <paramref name="other"/> can be cast to <paramref name="type"/>, otherwise <see langword="false"/>.</returns>
    public static bool CanCastFrom(this Type type, Type other) => CanCastTo(other, type);

    private static ImmutableDictionary<TypeCode, TypeCode[]> _narrowingConversions = new Dictionary<TypeCode, TypeCode[]>()
    {
        { TypeCode.Byte, new TypeCode[] { TypeCode.SByte } },
        { TypeCode.SByte, new TypeCode[] { TypeCode.Byte, TypeCode.UInt16, TypeCode.UInt32, TypeCode.UInt64 } },
        { TypeCode.Int16, new TypeCode[] { TypeCode.Byte, TypeCode.SByte, TypeCode.UInt16 } },
        { TypeCode.UInt16, new TypeCode[] { TypeCode.Byte, TypeCode.SByte, TypeCode.Int16 } },
        { TypeCode.Int32, new TypeCode[] { TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16, TypeCode.UInt32 } },
        { TypeCode.UInt32, new TypeCode[] { TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32 } },
        { TypeCode.Int64, new TypeCode[] { TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.UInt64 } },
        { TypeCode.UInt64, new TypeCode[] { TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64 } },
        { TypeCode.Decimal, new TypeCode[] { TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.UInt64, TypeCode.Int64 } },
        { TypeCode.Single, new TypeCode[] { TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.UInt64, TypeCode.Int64 } },
        { TypeCode.Double, new TypeCode[] { TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.UInt64, TypeCode.Int64 } }
    }.ToImmutableDictionary();

    private static ImmutableDictionary<TypeCode, TypeCode[]> _consistentWideningConversions = new Dictionary<TypeCode, TypeCode[]>()
    {
        { TypeCode.Byte, new TypeCode[] { TypeCode.UInt16, TypeCode.Int16, TypeCode.UInt32, TypeCode.Int32, TypeCode.UInt64, TypeCode.Int64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal } },
        { TypeCode.SByte, new TypeCode[] { TypeCode.Int16, TypeCode.Int32, TypeCode.Int64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal } },
        { TypeCode.Int16, new TypeCode[] { TypeCode.Int32, TypeCode.Int64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal } },
        { TypeCode.UInt16, new TypeCode[] { TypeCode.UInt32, TypeCode.Int32, TypeCode.UInt64, TypeCode.Int64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal } },
        { TypeCode.Char, new TypeCode[] { TypeCode.UInt16, TypeCode.UInt32, TypeCode.Int32, TypeCode.UInt64, TypeCode.Int64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal } },
        { TypeCode.Int32, new TypeCode[] { TypeCode.Int64, TypeCode.Double, TypeCode.Decimal } },
        { TypeCode.UInt32, new TypeCode[] { TypeCode.Int64, TypeCode.UInt64, TypeCode.Double, TypeCode.Decimal } },
        { TypeCode.Int64, new TypeCode[] { TypeCode.Decimal } },
        { TypeCode.UInt64, new TypeCode[] { TypeCode.Decimal } },
        { TypeCode.Single, new TypeCode[] { TypeCode.Double } }
    }.ToImmutableDictionary();

    private static ImmutableDictionary<TypeCode, TypeCode[]> _lossyWideningConversions = new Dictionary<TypeCode, TypeCode[]>()
    {
        { TypeCode.Int32, new TypeCode[] { TypeCode.Single } },
        { TypeCode.UInt32, new TypeCode[] { TypeCode.Single } },
        { TypeCode.Int64, new TypeCode[] { TypeCode.Single, TypeCode.Double } },
        { TypeCode.UInt64, new TypeCode[] { TypeCode.Single, TypeCode.Double } },
        { TypeCode.Decimal, new TypeCode[] { TypeCode.Single, TypeCode.Double } }
    }.ToImmutableDictionary();

    // "sane" because this method throws if the types are not numeric primitive types
    private static (TypeCode First, TypeCode Second) GetSaneTypeCodes(Type first, Type second)
    {
        var ret = (Type.GetTypeCode(first), Type.GetTypeCode(second));

        ThrowHelper.ThrowOnFirstOffender<ArgumentNullException, TypeCode>(
            item => new[] { "Type must be a numeric primitive type." },
            item => item is TypeCode.Empty or TypeCode.Object or TypeCode.DBNull or TypeCode.Boolean or TypeCode.DateTime or TypeCode.String,
            ret.Item1,
            ret.Item2
        );

        return ret;
    }

    /// <summary>
    /// Determines if there exists a narrowing conversion from this <see cref="Type"/> to <paramref name="other"/>.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to check.</param>
    /// <param name="other">The <see cref="Type"/> to check against.</param>
    /// <returns><see langword="true"/> if there exists a narrowing conversion from this <see cref="Type"/> to <paramref name="other"/>, otherwise <see langword="false"/>.</returns>
    public static bool HasNarrowingConversion(this Type type, Type other)
    {
        var (first, second) = GetSaneTypeCodes(type, other);
        if (first == second)
        {
            return false;
        }

        if (_narrowingConversions.TryGetValue(first, out var narrowingConversions))
        {
            return narrowingConversions.Contains(second);
        }
        // No narrowing conversions exist for the first type
        return false;
    }
    /// <summary>
    /// Determines if there exists a consistent widening conversion (that is, a conversion that is guaranteed to not lose any information) from this <see cref="Type"/> to <paramref name="other"/>.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to check.</param>
    /// <param name="other">The <see cref="Type"/> to check against.</param>
    /// <returns><see langword="true"/> if there exists a consistent widening conversion from this <see cref="Type"/> to <paramref name="other"/>, otherwise <see langword="false"/>.</returns>
    public static bool HasConsistentWideningConversion(this Type type, Type other)
    {
        var (first, second) = GetSaneTypeCodes(type, other);
        if (first == second)
        {
            return false;
        }

        if (_consistentWideningConversions.TryGetValue(first, out var consistentWideningConversions))
        {
            return Array.IndexOf(consistentWideningConversions, second) != -1;
        }
        // No consistent widening conversions exist for the first type
        return false;
    }
    /// <summary>
    /// Determines if there exists a lossy widening conversion (that is, a conversion that may lose information) from this <see cref="Type"/> to <paramref name="other"/>.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to check.</param>
    /// <param name="other">The <see cref="Type"/> to check against.</param>
    /// <returns><see langword="true"/> if there exists a lossy widening conversion from this <see cref="Type"/> to <paramref name="other"/>, otherwise <see langword="false"/>.</returns>
    public static bool HasLossyWideningConversion(this Type type, Type other)
    {
        var (first, second) = GetSaneTypeCodes(type, other);
        if (first == second)
        {
            return false;
        }

        if (_lossyWideningConversions.TryGetValue(first, out var lossyWideningConversions))
        {
            return Array.IndexOf(lossyWideningConversions, second) != -1;
        }
        // No lossy widening conversions exist for the first type
        return false;
    }
    /// <summary>
    /// Determines if there exists a widening conversion from this <see cref="Type"/> to <paramref name="other"/>.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to check.</param>
    /// <param name="other">The <see cref="Type"/> to check against.</param>
    /// <returns><see langword="true"/> if there exists a widening conversion from this <see cref="Type"/> to <paramref name="other"/>, otherwise <see langword="false"/>.</returns>
    public static bool HasWideningConversion(this Type type, Type other) => type.HasConsistentWideningConversion(other) || type.HasLossyWideningConversion(other);

    /// <summary>
    /// Reflects the entirety of this <see cref="Type"/> and generates .NET 8.0 code that can be used to replicate it.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to reflect.</param>
    /// <param name="namespace">The namespace to place the generated type(s) into. If <see langword="null"/> or empty, the namespace of the <paramref name="type"/> is used. If that also resolves to <see langword="null"/> or empty, the code is generated without a namespace declaration.</param>
    /// <param name="inherited">Whether to generate code for all members <paramref name="type"/> inherits from its base types.</param>
    /// <param name="deep">Whether to generate code for all <see cref="Type"/>s that are referenced by this <paramref name="type"/> in any way.</param>
    /// <returns>A <see cref="string"/> containing the generated code.</returns>
    /// <remarks>
    /// This method is not guaranteed to generate compilable code. It is intended to be used as a starting point for replicating existing types you may not have access to.
    /// <para/>This method respects the <see cref="DoNotReflectAttribute"/>. If <paramref name="type"/> itself, or <paramref name="deep"/> is <see langword="true"/> and any member (including <see cref="Type"/>s) referenced by this <paramref name="type"/>, is marked with this attribute, a <see cref="ReflectionException"/> is thrown.
    /// </remarks>
    public static string Reflect(this Type type, string? @namespace = null, bool inherited = true, bool deep = false)
    {
        var bindingFlags =
            BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.Instance
            | BindingFlags.Static;
        if (!inherited)
        {
            bindingFlags |= BindingFlags.DeclaredOnly;
        }

        ArgumentNullException.ThrowIfNull(type);
        DoNotReflectAttribute.ThrowIfMarked(type.Assembly);

        @namespace ??= type.Namespace;

        using (var sw = new StringWriter())
        {
            using (var itw = new IndentedTextWriter(sw, "    "))
            {
                if (!string.IsNullOrWhiteSpace(@namespace))
                {
                    itw.WriteLine($"namespace {@namespace};");
                }

                itw.WriteLine($"public {(type.IsInterface ? "interface" : "class")} {type.Name}");

                if (type.IsGenericType)
                {
                    itw.Write("<");
                    itw.Write(string.Join(", ", type.GetGenericArguments().Select(t => t.Name)));
                    itw.Write(">");
                }

                itw.WriteLine();
                itw.WriteLine("{");

                foreach (var field in type.GetFields(bindingFlags))
                {
                    DoNotReflectAttribute.ThrowIfMarked(field);
                    itw.WriteLine($"    {field.GetAccessibility()} {field.FieldType.GetFriendlyName()} {field.Name};");
                }

                foreach (var property in type.GetProperties(bindingFlags))
                {
                    DoNotReflectAttribute.ThrowIfMarked(property);
                    itw.WriteLine($"    {property.GetAccessibility()} {property.PropertyType.GetFriendlyName()} {property.Name} {{ {property.GetMethod?.GetAccessibility()}get; {property.SetMethod?.GetAccessibility()}set; }}");
                }

                foreach (var method in type.GetMethods(bindingFlags).ArrayWhere(m => !m.IsSpecialName))
                {
                    DoNotReflectAttribute.ThrowIfMarked(method);
                    itw.WriteLine($"    {method.GetAccessibility()} {method.ReturnType.GetFriendlyName()} {method.Name}({string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType.GetFriendlyName()} {p.Name}"))});");
                }

                if (deep)
                {
                    foreach (var nestedType in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        DoNotReflectAttribute.ThrowIfMarked(nestedType);

                        itw.WriteLine();
                        itw.WriteLine(nestedType.Reflect(@namespace, deep));
                    }
                }

                itw.WriteLine("}");
            }

            return sw.ToString();
        }
    }

    private static string GetLeastAccessibleModifier(IEnumerable<string> modifiers)
    {
        var modifiersEnumerated = modifiers.ToArray();
        if (modifiersEnumerated.ArrayContains("private")) // same type only
        {
            return "private";
        }
        else if (modifiersEnumerated.ArrayContains("private protected")) // same type and derived types in same assembly
        {
            return "private protected";
        }
        else if (modifiersEnumerated.ArrayContains("protected")) // same type and derived types
        {
            return "protected";
        }
        else if (modifiersEnumerated.ArrayContains("internal")) // same assembly only
        {
            return "internal";
        }
        else if (modifiersEnumerated.ArrayContains("protected internal")) // same type and derived types OR same assembly
        {
            return "protected internal";
        }
        else // public
        {
            return "public";
        }
    }
    private static string GetAccessibility(MethodBase methodBase)
    {
        return methodBase switch
        {
            { IsPublic: true } => "public",
            { IsFamily: true } => "protected",
            { IsAssembly: true } => "internal",
            { IsPrivate: true } => "private",
            { IsFamilyAndAssembly: true } => "private protected",
            { IsFamilyOrAssembly: true } => "protected internal",
            _ => "private"
        };
    }
    private static string GetAccessibility(FieldInfo fieldInfo)
    {
        return fieldInfo switch
        {
            { IsPublic: true } => "public",
            { IsFamily: true } => "protected",
            { IsAssembly: true } => "internal",
            { IsPrivate: true } => "private",
            { IsFamilyAndAssembly: true } => "private protected",
            { IsFamilyOrAssembly: true } => "protected internal",
            _ => "private"
        };
    }
    private static string GetAccessibility(Type type)
    {
        return type switch
        {
            { IsPublic: true } => "public",
            { IsNestedPublic: true } => "public",
            { IsNestedFamily: true } => "protected",
            { IsNestedAssembly: true } => "internal",
            { IsNestedPrivate: true } => "private",
            { IsNestedFamANDAssem: true } => "private protected",
            { IsNestedFamORAssem: true } => "protected internal",
            _ => "private"
        };
    }
    private static string GetAccessibility(this MemberInfo member)
    {
        ArgumentNullException.ThrowIfNull(member);
        if (member is PropertyInfo propertyInfo)
        {
            if (propertyInfo.CanRead && propertyInfo.GetGetMethod(true) is MethodBase getMethod)
            {
                return GetAccessibility(getMethod);
            }
            else if (propertyInfo.CanWrite && propertyInfo.GetSetMethod(true) is MethodBase setMethod)
            {
                return GetAccessibility(setMethod);
            }
            return "private";
        }
        else if (member is FieldInfo fieldInfo)
        {
            return GetAccessibility(fieldInfo);
        }
        else if (member is MethodBase methodBase)
        {
            return GetAccessibility(methodBase);
        }
        else if (member is EventInfo eventInfo)
        {
            var accessors = new List<string>();
            if (eventInfo.GetAddMethod(true) is MethodBase addMethod)
            {
                accessors.Add(GetAccessibility(addMethod));
            }
            if (eventInfo.GetRemoveMethod(true) is MethodBase removeMethod)
            {
                accessors.Add(GetAccessibility(removeMethod));
            }
            if (eventInfo.GetRaiseMethod(true) is MethodBase raiseMethod)
            {
                accessors.Add(GetAccessibility(raiseMethod));
            }
            return GetLeastAccessibleModifier(accessors);
        }
        else if (member is Type type)
        {
            return GetAccessibility(type);
        }
        return "private";
    }
    private static string GetFriendlyName(this Type type)
    {
        if (type.IsGenericType)
        {
            var name = type.Name[..type.Name.IndexOf('`')];
            var args = string.Join(", ", type.GetGenericArguments().Select(t => t.GetFriendlyName()));

            return $"{name}<{args}>";
        }

        return type.Name;
    }
}
