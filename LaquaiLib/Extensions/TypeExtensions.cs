using System.CodeDom.Compiler;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;

using LaquaiLib.Util;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Type"/> Type.
/// </summary>
public static partial class TypeExtensions
{
    /// <summary>
    /// Attempts to instantiate a new object of the supplied <paramref name="type"/> using the given <paramref name="parameters"/>.
    /// </summary>
    /// <param name="type">A <see cref="Type"/> instance representing the type to instantiate.</param>
    /// <param name="parameters">The parameters to pass to the constructor. May be <see langword="null"/> to target the parameterless constructor.</param>
    /// <returns>An instance of the supplied <paramref name="type"/>, or <see langword="null"/> if a constructor matching the given <paramref name="parameters"/> could not be found or that constructor could not be invoked.</returns>
    public static object New(this Type type, params object[] parameters)
    {
        try
        {
            if (parameters.Length == 0)
            {
                return Activator.CreateInstance(type);
            }
            var types = parameters.ToArray(static obj => obj?.GetType());
            return type.GetConstructor(types).New(parameters);
        }
        catch
        {
            return default;
        }
    }
    /// <summary>
    /// Returns a (potentially boxed) instance of the default value for the supplied type.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to get the default value for.</param>
    /// <returns>The default value for the supplied type.</returns>
    public static object GetDefault(this Type type) => type.IsValueType ? Activator.CreateInstance(type) : (type == typeof(string) ? string.Empty : null);

    /// <summary>
    /// Compiles a <see cref="Dictionary{TKey, TValue}"/> of all instance fields and properties of the supplied type from the given object, optionally calling all parameterless methods that do not return void.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> the <see cref="FieldInfo"/>, <see cref="PropertyInfo"/> and <see cref="MethodInfo"/> instances are to be reflected from.</param>
    /// <param name="obj">The object to use to collect the values from.</param>
    /// <param name="callMethods">Whether to call all parameterless methods that do not return void instead of adding all method names to the output dictionary. This is a dangerous operation and should only be used if the methods are known to be safe and not have side effects.</param>
    /// <returns>The <see cref="Dictionary{TKey, TValue}"/> as described.</returns>
    public static Dictionary<string, object> GetInstanceValues(this Type type, object obj, bool callMethods = false)
    {
        var dict = new Dictionary<string, object>();
        var members = type.GetMembers();

        foreach (var memberInfo in members.Where(static member => member.MemberType is MemberTypes.Field
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
                        dict.Add($"{methodInfo.Name}({string.Join(", ", methodInfo.GetParameters().Select(static paramInfo => $"{paramInfo.ParameterType.GetFriendlyName()} {paramInfo.Name}"))}", null);
                    }
                    break;
                case MemberTypes.Constructor:
                    break;
                case MemberTypes.Event:
                    break;
                case MemberTypes.TypeInfo:
                    break;
                case MemberTypes.Custom:
                    break;
                case MemberTypes.NestedType:
                    break;
                case MemberTypes.All:
                    break;
                default:
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
    public static Dictionary<string, object> GetStaticValues(this Type type, bool callMethods = false)
    {
        var dict = new Dictionary<string, object>();
        var members = type.GetMembers(BindingFlags.Public | BindingFlags.Static);

        foreach (var memberInfo in members.Where(static member => member.MemberType is MemberTypes.Field
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
                            dict.Add($"{methodInfo.Name}({string.Join(", ", methodInfo.GetParameters().Select(static paramInfo => $"{paramInfo.ParameterType.GetFriendlyName()} {paramInfo.Name}"))})", null);
                        }
                    }
                    break;
                case MemberTypes.Constructor:
                    break;
                case MemberTypes.Event:
                    break;
                case MemberTypes.TypeInfo:
                    break;
                case MemberTypes.Custom:
                    break;
                case MemberTypes.NestedType:
                    break;
                case MemberTypes.All:
                    break;
                default:
                    break;
            }
        }

        return dict;
    }

    #region Mappings
    private static readonly FrozenDictionary<TypeCode, TypeCode[]> _narrowingConversions = new Dictionary<TypeCode, TypeCode[]>()
    {
        [TypeCode.Byte] = [TypeCode.SByte],
        [TypeCode.SByte] = [TypeCode.Byte, TypeCode.UInt16, TypeCode.UInt32, TypeCode.UInt64],
        [TypeCode.Int16] = [TypeCode.Byte, TypeCode.SByte, TypeCode.UInt16],
        [TypeCode.UInt16] = [TypeCode.Byte, TypeCode.SByte, TypeCode.Int16],
        [TypeCode.Int32] = [TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16, TypeCode.UInt32],
        [TypeCode.UInt32] = [TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32],
        [TypeCode.Int64] = [TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.UInt64],
        [TypeCode.UInt64] = [TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64],
        [TypeCode.Decimal] = [TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.UInt64, TypeCode.Int64],
        [TypeCode.Single] = [TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.UInt64, TypeCode.Int64],
        [TypeCode.Double] = [TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.UInt64, TypeCode.Int64]
    }.ToFrozenDictionary();

    private static readonly FrozenDictionary<TypeCode, TypeCode[]> _consistentWideningConversions = new Dictionary<TypeCode, TypeCode[]>()
    {
        [TypeCode.Byte] = [TypeCode.UInt16, TypeCode.Int16, TypeCode.UInt32, TypeCode.Int32, TypeCode.UInt64, TypeCode.Int64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal],
        [TypeCode.SByte] = [TypeCode.Int16, TypeCode.Int32, TypeCode.Int64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal],
        [TypeCode.Int16] = [TypeCode.Int32, TypeCode.Int64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal],
        [TypeCode.UInt16] = [TypeCode.UInt32, TypeCode.Int32, TypeCode.UInt64, TypeCode.Int64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal],
        [TypeCode.Char] = [TypeCode.UInt16, TypeCode.UInt32, TypeCode.Int32, TypeCode.UInt64, TypeCode.Int64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal],
        [TypeCode.Int32] = [TypeCode.Int64, TypeCode.Double, TypeCode.Decimal],
        [TypeCode.UInt32] = [TypeCode.Int64, TypeCode.UInt64, TypeCode.Double, TypeCode.Decimal],
        [TypeCode.Int64] = [TypeCode.Decimal],
        [TypeCode.UInt64] = [TypeCode.Decimal],
        [TypeCode.Single] = [TypeCode.Double]
    }.ToFrozenDictionary();

    private static readonly FrozenDictionary<TypeCode, TypeCode[]> _lossyWideningConversions = new Dictionary<TypeCode, TypeCode[]>()
    {
        [TypeCode.Int32] = [TypeCode.Single],
        [TypeCode.UInt32] = [TypeCode.Single],
        [TypeCode.Int64] = [TypeCode.Single, TypeCode.Double],
        [TypeCode.UInt64] = [TypeCode.Single, TypeCode.Double],
        [TypeCode.Decimal] = [TypeCode.Single, TypeCode.Double]
    }.ToFrozenDictionary();

    private static readonly FrozenDictionary<string, string> _typeKeywordMap = new Dictionary<string, string>()
    {
        { "System.Boolean", "bool" },
        { "System.Char", "char" },
        { "System.SByte", "sbyte" },
        { "System.Byte", "byte" },
        { "System.Int16", "short" },
        { "System.UInt16", "ushort" },
        { "System.Int32", "int" },
        { "System.UInt32", "uint" },
        { "System.IntPtr", "nint" },
        { "System.UIntPtr", "nuint" },
        { "System.Int64", "long" },
        { "System.UInt64", "ulong" },
        { "System.Single", "float" },
        { "System.Double", "double" },
        { "System.Decimal", "decimal" },
        { "System.String", "string" },
        { "System.Object", "object" },
        { "System.Void", "void" },
        { "Boolean", "bool" },
        { "Char", "char" },
        { "SByte", "sbyte" },
        { "Byte", "byte" },
        { "Int16", "short" },
        { "UInt16", "ushort" },
        { "Int32", "int" },
        { "UInt32", "uint" },
        { "IntPtr", "nint" },
        { "UIntPtr", "nuint" },
        { "Int64", "long" },
        { "UInt64", "ulong" },
        { "Single", "float" },
        { "Double", "double" },
        { "Decimal", "decimal" },
        { "String", "string" },
        { "Object", "object" },
        { "Void", "void" },
    }.ToFrozenDictionary();
    #endregion

    // "sane" because this method throws if the types are not numeric primitive types
    private static (TypeCode First, TypeCode Second) GetSaneTypeCodes(Type first, Type second)
    {
        var ret = (Type.GetTypeCode(first), Type.GetTypeCode(second));

        if (ret.Item1 is not (TypeCode.Empty or TypeCode.Object or TypeCode.DBNull or TypeCode.Boolean or TypeCode.DateTime or TypeCode.String))
        {
            throw new ArgumentException("Type must be a numeric primitive type.", nameof(first));
        }
        if (ret.Item2 is not (TypeCode.Empty or TypeCode.Object or TypeCode.DBNull or TypeCode.Boolean or TypeCode.DateTime or TypeCode.String))
        {
            throw new ArgumentException("Type must be a numeric primitive type.", nameof(second));
        }

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

#warning TODO: Support not just classes, make this work for structs, interfaces etc. as well
    /// <summary>
    /// Reflects the entirety of this <see cref="Type"/> and generates C# code that can be used to replicate it.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to reflect.</param>
    /// <param name="options.Inheriting">Whether to make the generated type(s) inherit from the <paramref name="type"/>. If <see langword="false"/>, a private static field of type <paramref name="type"/> is generated and all method calls are redirected to that field. If <see langword="true"/>, the generated type(s) inherit from <paramref name="type"/> and all method calls are redirected to <see langword="base"/>. If <see langword="null"/>, only a skeleton of the type is generated, with all methods throwing <see cref="NotImplementedException"/>s.</param>
    /// <returns>A <see cref="string"/> containing the generated code.</returns>
    /// <remarks>
    /// It is almost guaranteed that this method will not generate compilable code. It is intended to be used as a starting point for replicating existing types you may not have access to.
    /// </remarks>
    public static string Reflect(this Type type, ReflectionOptions options = default)
    {
        var bindingFlags =
            BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.Instance
            | BindingFlags.Static;
        if (!options.IncludeHierarchy)
        {
            bindingFlags |= BindingFlags.DeclaredOnly;
        }

        ArgumentNullException.ThrowIfNull(type);
        var friendlyTypeName = type.GetFriendlyName();

        if (type.IsSealed && options.Inherit is ReflectionOptions.InheritanceBehavior.Inherit)
        {
            throw new TypeAccessException($"Cannot generate code for a type that inherits from sealed type '{friendlyTypeName}'.");
        }

        using var sw = new StringWriter();
        using var itw = new IndentedTextWriter(sw, "    ");

        if (!string.IsNullOrWhiteSpace(options.Namespace))
        {
            itw.WriteLine($"namespace {options.Namespace};");
        }

        #region Type Declaration
        itw.WriteLine($"""
                /// <summary>
                /// [Reflected from {friendlyTypeName} (Assembly '{type.Assembly.GetName().Name}')]
                /// </summary>
                """);
        itw.Write($"public class {type.Name}");

        if (type.IsGenericType)
        {
            itw.Write("<");
            itw.Write(string.Join(", ", type.GetGenericArguments().Select(t => t.FullName)));
            itw.Write(">");
        }
        else
        {
            itw.WriteLine();
        }

        if (options.Inherit is ReflectionOptions.InheritanceBehavior.Inherit)
        {
            itw.WriteLine($" : {friendlyTypeName}");
        }
        #endregion

        itw.WriteLine("{");

        itw.Indent++;

        #region options.Inheriting is ReflectionOptions.InheritanceBehavior.FieldDelegation
        if (options.Inherit is ReflectionOptions.InheritanceBehavior.FieldDelegation)
        {
            itw.WriteLine($"private readonly {friendlyTypeName} _base = new {friendlyTypeName}();");
        }
        #endregion

        #region Fields
        var fields = type.GetFields(bindingFlags);
        for (var i = 0; i < fields.Length; i++)
        {
            var field = fields[i];
            var accessibility = field.GetAccessibility();
            if (options.IgnoreInaccessible && IsInaccessibleAsReflectedType(accessibility, options.Inherit))
            {
                continue;
            }

            // If the field is anything above or equal to protected, it's prooooobably gonna have a documentation comment
            // Since fields cannot be made virtual though, the interitance relationship doesn't matter
            if (options.AddXmlDocCrefs && accessibility.Equals("public", StringComparison.OrdinalIgnoreCase))
            {
                itw.WriteLine($"""/// <inheritdoc cref="{friendlyTypeName}.{field.Name}" />""");
            }

            itw.Write(accessibility);
            itw.Write(' ');
            if (field.IsStatic)
            {
                itw.Write("static ");
            }
            itw.Write(field.FieldType.GetFriendlyName());
            itw.Write(' ');
            itw.Write(field.Name);
            itw.WriteLine(';');
        }
        #endregion

        itw.WriteLine();

        #region Properties
        var props = type.GetProperties(bindingFlags);
        for (var i = 0; i < props.Length; i++)
        {
            var property = props[i];
            var accessibility = property.GetAccessibility();
            if (options.IgnoreInaccessible && IsInaccessibleAsReflectedType(accessibility, options.Inherit))
            {
                continue;
            }

            // If the property is anything above or equal to protected, it's prooooobably gonna have a documentation comment
            if (options.AddXmlDocCrefs && (accessibility.Equals("public", StringComparison.OrdinalIgnoreCase) || (AccessibilityIsAtLeastFamily(accessibility) && !type.IsSealed)))
            {
                itw.WriteLine($"""/// <inheritdoc cref="{friendlyTypeName}.{property.Name}" />""");
            }

            itw.Write(accessibility);
            itw.Write(' ');
            if (property.GetMethod?.IsStatic is true || property.SetMethod?.IsStatic is true)
            {
                itw.Write("static ");
            }
            itw.Write(property.PropertyType.GetFriendlyName());
            itw.Write(' ');
            itw.WriteLine(property.Name);
            itw.WriteLine("{");

            itw.Indent++;
            if (property.GetMethod is not null)
            {
                var getAccessibility = property.GetMethod.GetAccessibility();
                if (!options.IgnoreInaccessible || !IsInaccessibleAsReflectedType(getAccessibility, options.Inherit))
                {
                    itw.Write(getAccessibility);
                    itw.Write(" get => ");
                    switch (options.Inherit)
                    {
                        case ReflectionOptions.InheritanceBehavior.Inherit when !IsInaccessibleAsReflectedType(getAccessibility, options.Inherit):
                            itw.WriteLine($"base.{property.Name};");
                            break;
                        case ReflectionOptions.InheritanceBehavior.FieldDelegation when property.GetMethod.IsPublic:
                            itw.WriteLine($"this._base.{property.Name};");
                            break;
                        case ReflectionOptions.InheritanceBehavior.Inherit:
                        case ReflectionOptions.InheritanceBehavior.FieldDelegation:
                            itw.WriteLine($"""throw new NotImplementedException("Reflected member '{friendlyTypeName}.{property.Name}.get()' is not accessible.");""");
                            break;
                        default:
                            itw.WriteLine($"""throw new NotImplementedException("Reflected member '{friendlyTypeName}.{property.Name}.get()' is not implemented.");""");
                            break;
                    }
                }
            }
            if (property.SetMethod is not null)
            {
                var setAccessibility = property.SetMethod.GetAccessibility();
                if (!options.IgnoreInaccessible || !IsInaccessibleAsReflectedType(setAccessibility, options.Inherit))
                {
                    itw.Write(setAccessibility);
                    itw.Write(" set => ");
                    switch (options.Inherit)
                    {
                        case ReflectionOptions.InheritanceBehavior.Inherit when !IsInaccessibleAsReflectedType(setAccessibility, options.Inherit):
                            itw.WriteLine($"base.{property.Name} = value;");
                            break;
                        case ReflectionOptions.InheritanceBehavior.FieldDelegation when property.SetMethod.IsPublic:
                            itw.WriteLine($"this._base.{property.Name} = value;");
                            break;
                        case ReflectionOptions.InheritanceBehavior.Inherit:
                        case ReflectionOptions.InheritanceBehavior.FieldDelegation:
                            itw.WriteLine($"""throw new NotImplementedException("Reflected member '{friendlyTypeName}.{property.Name}.set()' is not accessible.");""");
                            break;
                        default:
                            itw.WriteLine($"""throw new NotImplementedException("Reflected member '{friendlyTypeName}.{property.Name}.set()' is not implemented.");""");
                            break;
                    }
                }
            }
            itw.Indent--;

            itw.WriteLine("}");
        }
        #endregion

        itw.WriteLine();

        #region Methods
        var methods = type
            .GetMethods(bindingFlags)
            .Where(m => !m.IsSpecialName
                && !UnspeakableMemberNameRegex().IsMatch(m.Name)
                && (!options.IgnoreInaccessible
                    || !IsInaccessibleAsReflectedType(m.GetAccessibility(), options.Inherit)
                )
            )
            .OrderBy(m => m.IsPrivate)          // private
            .ThenBy(m => m.IsFamilyOrAssembly)  // private protected
            .ThenBy(m => m.IsFamily)            // protected
            .ThenBy(m => m.IsAssembly)          // internal
            .ThenBy(m => m.IsFamilyAndAssembly) // protected internal
            .ThenBy(m => m.IsPublic)            // public
            .ThenBy(m => m.Name)                // name
            .ThenBy(m => m.IsGenericMethod)     // generic?
            .ToArray();
        for (var i = 0; i < methods.Length; i++)
        {
            var method = methods[i];
            var accessibility = method.GetAccessibility();
            // Determine if the generic method requires an unsafe context
            var unsafeRequired = method.GetParameters().Any(p => p.ParameterType.IsPointer);

            // If the method is anything above or equal to protected, it's prooooobably gonna have a documentation comment
            if (options.AddXmlDocCrefs && (accessibility.Equals("public", StringComparison.OrdinalIgnoreCase) || (AccessibilityIsAtLeastFamily(accessibility) && !type.IsSealed)))
            {
                itw.Write($"""/// <inheritdoc cref="{friendlyTypeName}.{method.Name}""");

                if (method.IsGenericMethod)
                {
                    var genericTypeParams = method.GetGenericArguments();
                    itw.Write('{');
                    itw.Write(string.Join(", ", genericTypeParams.Select(t => t.Name)));
                    itw.Write('}');
                }

                itw.Write('(');
                itw.Write(string.Join(", ", method.GetParameters().Select(p => p.ParameterType.GetFriendlyName().Replace('<', '{').Replace('>', '}'))));
                itw.Write(')');

                itw.WriteLine("\" />");
            }

            itw.Write(accessibility);
            itw.Write(' ');
            if (method.IsStatic)
            {
                itw.Write("static ");
            }
            if (unsafeRequired)
            {
                itw.Write("unsafe ");
            }
            itw.Write(method.ReturnType.GetFriendlyName());
            itw.Write(' ');
            itw.Write(method.Name);

            if (method.IsGenericMethod)
            {
                itw.Write($"<{string.Join(", ", method.GetGenericArguments().Select(t => t.Name))}>");
            }

            itw.Write('(');
            itw.Write(string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType.GetFriendlyName()} {p.Name}")));
            itw.Write(')');
            switch (options.Inherit)
            {
                case ReflectionOptions.InheritanceBehavior.Inherit when !IsInaccessibleAsReflectedType(accessibility, options.Inherit):
                    itw.WriteLine($" => base.{method.Name}({string.Join(", ", method.GetParameters().Select(p => p.Name))});");
                    break;
                case ReflectionOptions.InheritanceBehavior.FieldDelegation when method.IsPublic:
                    itw.WriteLine($" => this._base.{method.Name}({string.Join(", ", method.GetParameters().Select(p => p.Name))});");
                    break;
                case ReflectionOptions.InheritanceBehavior.Inherit:
                case ReflectionOptions.InheritanceBehavior.FieldDelegation:
                    itw.WriteLine($""" => throw new NotImplementedException("Reflected member '{friendlyTypeName}.{method.Name}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.GetFriendlyName()))})' is not accessible.");""");
                    break;
                default:
                    itw.WriteLine($""" => throw new NotImplementedException("Reflected member '{friendlyTypeName}.{method.Name}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.GetFriendlyName()))})' is not implemented.");""");
                    break;
            }
        }
        #endregion

        if (options.Deep)
        {
            var types = type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);
            for (var i = 0; i < types.Length; i++)
            {
                var nestedType = types[i];
                itw.WriteLine();
                itw.WriteLine(nestedType.Reflect(options));
            }
        }

        itw.Indent--;
        itw.WriteLine("}");

        return sw.ToString();
    }

    internal static bool AccessibilityIsAtLeastFamily(string accessibility) => accessibility.ToUpperInvariant() switch
    {
        "public" => true,
        "protected" => true,
        "internal" => false,
        "private" => false,
        "private protected" => true,
        "protected internal" => true,
        _ => false
    };
    internal static string GetLeastAccessibleModifier(IEnumerable<string> modifiers)
    {
        var modifiersEnumerated = modifiers.ToArray();
        if (modifiersEnumerated.Contains("private protected"))
        {
            return "private protected";
        }
        if (modifiersEnumerated.Contains("protected internal"))
        {
            return "protected internal";
        }
        if (modifiersEnumerated.Contains("private")) // same type only
        {
            return "private";
        }
        if (modifiersEnumerated.Contains("protected"))
        {
            return "protected";
        }
        if (modifiersEnumerated.Contains("internal"))
        {
            return "internal";
        }
        return "public";
    }
    internal static bool IsInaccessibleAsReflectedType(string modifiers, ReflectionOptions.InheritanceBehavior inheritance)
    {
        return modifiers.ToUpperInvariant() switch
        {
            "PRIVATE" => true,
            "PRIVATE PROTECTED" => true, // technically this COULD return true, but that would require the reflected type to be in the same assembly as the reflected type, which is... possible, but stupid
            "PROTECTED" => inheritance is not ReflectionOptions.InheritanceBehavior.Inherit,
            "INTERNAL" => true,
            "PROTECTED INTERNAL" => inheritance is not ReflectionOptions.InheritanceBehavior.Inherit,
            "PUBLIC" => false,
            _ => true
        };
    }
    internal static string GetAccessibility(MethodBase methodBase)
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
    internal static string GetAccessibility(FieldInfo fieldInfo)
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
    internal static string GetAccessibility(Type type)
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
    internal static string GetAccessibility(this MemberInfo member)
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

    /// <summary>
    /// Constructs a more easily readable name for the specified <see cref="Type"/>.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to construct a more easily readable name for.</param>
    /// <param name="includeNamespace">Whether to include the namespace in the name.</param>
    /// <returns>A more easily readable name for the specified <see cref="Type"/>.</returns>
    public static string GetFriendlyName(this Type type, bool includeNamespace = true)
    {
        string operateOn;
        if (includeNamespace)
        {
            operateOn = type.FullName ?? type.Namespace + '.' + type.Name;
        }
        else
        {
            operateOn = type.Name;
        }

        if (type.IsGenericParameter)
        {
            return type.Name;
        }
        else if (type.IsArray && type.GetElementType() is Type elementType)
        {
            return elementType.GetFriendlyName() + "[]";
        }
        if (operateOn.Contains('+', StringComparison.OrdinalIgnoreCase))
        {
            operateOn = type.Namespace + '.' + type.Name;
        }
        if (operateOn.EndsWith('&'))
        {
            return "ref " + AsKeyword(operateOn[..^1]);
        }
        if (operateOn.EndsWith('*'))
        {
            return AsKeyword(operateOn[..^1]) + '*';
        }
        if (operateOn.EndsWith("[]", StringComparison.OrdinalIgnoreCase))
        {
            return AsKeyword(operateOn[..^2]) + "[]";
        }

        if (type.IsGenericType)
        {
            var tickAt = operateOn.IndexOf('`', StringComparison.OrdinalIgnoreCase);
            if (tickAt != -1)
            {
                operateOn = operateOn[..tickAt];
            }
            var args = string.Join(", ", type.GetGenericArguments().Select(t => t.GetFriendlyName()));

            return $"{operateOn}<{args}>";
        }

        return AsKeyword(operateOn);
    }
    /// <summary>
    /// Converts a type name to its C# keyword, if it exists.
    /// </summary>
    /// <param name="type">The type name to convert.</param>
    /// <returns>The type name as a C# keyword, if it exists, otherwise the original type name.</returns>
    public static string AsKeyword(string type) => _typeKeywordMap.TryGetValue(type, out var keyword) ? keyword : type;
    /// <summary>
    /// Converts a <see cref="Type"/> to its C# keyword, if it exists.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to convert.</param>
    /// <returns>The <see cref="Type"/>'s name as a C# keyword, if it exists, otherwise the original <see cref="Type"/>.</returns>
    public static string AsKeyword(this Type type) => AsKeyword(type.FullName);

    /// <summary>
    /// Determines whether the specified <paramref name="type"/> is assignable to a <see cref="Func{TResult}"/> <see langword="delegate"/> overload.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to check.</param>
    /// <param name="returnType">An <see langword="out"/> variable that receives the <see cref="Func{TResult}"/> <see langword="delegate"/> overload that the <paramref name="type"/> is assignable to, if any.</param>
    /// <returns><see langword="true"/> if the <paramref name="type"/> is assignable to a <see cref="Func{TResult}"/> <see langword="delegate"/> overload, otherwise <see langword="false"/>.</returns>
    public static bool IsFunc(this Type type, [NotNullWhen(true)] out Type returnType)
    {
        returnType = null;
        if (!type.IsGenericType)
        {
            return false;
        }

        var gtd = type.GetGenericTypeDefinition();
        if (gtd == typeof(Func<>))
        {
            returnType = type;
        }
        else if (gtd == typeof(Func<,>))
        {
            returnType = type;
        }
        else if (gtd == typeof(Func<,,>))
        {
            returnType = type;
        }
        else if (gtd == typeof(Func<,,,>))
        {
            returnType = type;
        }
        else if (gtd == typeof(Func<,,,,>))
        {
            returnType = type;
        }
        else if (gtd == typeof(Func<,,,,,>))
        {
            returnType = type;
        }
        else if (gtd == typeof(Func<,,,,,,>))
        {
            returnType = type;
        }
        else if (gtd == typeof(Func<,,,,,,,>))
        {
            returnType = type;
        }
        else if (gtd == typeof(Func<,,,,,,,,>))
        {
            returnType = type;
        }
        else if (gtd == typeof(Func<,,,,,,,,,>))
        {
            returnType = type;
        }
        else if (gtd == typeof(Func<,,,,,,,,,,>))
        {
            returnType = type;
        }
        else if (gtd == typeof(Func<,,,,,,,,,,,>))
        {
            returnType = type;
        }
        else if (gtd == typeof(Func<,,,,,,,,,,,,>))
        {
            returnType = type;
        }
        else if (gtd == typeof(Func<,,,,,,,,,,,,,>))
        {
            returnType = type;
        }
        else if (gtd == typeof(Func<,,,,,,,,,,,,,,>))
        {
            returnType = type;
        }
        else if (gtd == typeof(Func<,,,,,,,,,,,,,,,>))
        {
            returnType = type;
        }
        else if (gtd == typeof(Func<,,,,,,,,,,,,,,,>))
        {
            returnType = type;
        }
        else if (gtd == typeof(Func<,,,,,,,,,,,,,,,,>))
        {
            returnType = type;
        }

        if (returnType is not null)
        {
            returnType = returnType.GetGenericArguments()[^1];
            return true;
        }
        return false;
    }
    /// <summary>
    /// Determines whether the specified <paramref name="type"/> is assignable to an <see cref="Action"/> <see langword="delegate"/> overload.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to check.</param>
    /// <param name="takesParameters">Whether the <paramref name="type"/> takes parameters. If <see langword="false"/>, <paramref name="type"/> is not generic.</param>
    /// <returns><see langword="true"/> if the <paramref name="type"/> is assignable to an <see cref="Action"/> <see langword="delegate"/> overload, otherwise <see langword="false"/>.</returns>
    public static bool IsAction(this Type type, out bool takesParameters)
    {
        takesParameters = false;
        if (type == typeof(Action))
        {
            return true;
        }

        takesParameters = true;
        var gtd = type.GetGenericTypeDefinition();
        if (gtd == typeof(Action<>))
        {
            return true;
        }
        else if (gtd == typeof(Action<,>))
        {
            return true;
        }
        else if (gtd == typeof(Action<,,>))
        {
            return true;
        }
        else if (gtd == typeof(Action<,,,>))
        {
            return true;
        }
        else if (gtd == typeof(Action<,,,,>))
        {
            return true;
        }
        else if (gtd == typeof(Action<,,,,,>))
        {
            return true;
        }
        else if (gtd == typeof(Action<,,,,,,>))
        {
            return true;
        }
        else if (gtd == typeof(Action<,,,,,,,>))
        {
            return true;
        }
        else if (gtd == typeof(Action<,,,,,,,,>))
        {
            return true;
        }
        else if (gtd == typeof(Action<,,,,,,,,,>))
        {
            return true;
        }
        else if (gtd == typeof(Action<,,,,,,,,,,>))
        {
            return true;
        }
        else if (gtd == typeof(Action<,,,,,,,,,,,>))
        {
            return true;
        }
        else if (gtd == typeof(Action<,,,,,,,,,,,,>))
        {
            return true;
        }
        else if (gtd == typeof(Action<,,,,,,,,,,,,,>))
        {
            return true;
        }
        else if (gtd == typeof(Action<,,,,,,,,,,,,,,>))
        {
            return true;
        }
        else if (gtd == typeof(Action<,,,,,,,,,,,,,,,>))
        {
            return true;
        }
        return false;
    }

    /*
    System.String <<>m0>b__0_0(System.String)
    Int32 <<>m0>b__0_0(Int32)
    System.String <<>m0>b__0_0(System.String)
    <AppendFormatHelper>g__MoveNext|116_0(string, int[])
    */
    [GeneratedRegex(@"<.*?>\p{Ll}__(\p{L}|\p{Nd}|\||_)+?_\p{Nd}+?(?=\(.*?\))?", RegexOptions.ExplicitCapture)]
    private static partial Regex UnspeakableMemberNameRegex();

    /// <summary>
    /// Finds <see cref="Type"/>s that are assignable to the specified <paramref name="type"/> and constructible (i.e. that are not <see langword="interface"/>s, <see langword="abstract"/> or <see langword="static"/>).
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to find constructible subtypes of.</param>
    /// <param name="assembly">The <see cref="Assembly"/> to search in. If <see langword="null"/>, the assembly of the specified <paramref name="type"/> is used.</param>
    /// <returns>An <see cref="Array"/> of <see cref="Type"/>s that are assignable to the specified <paramref name="type"/> and constructible.</returns>
    public static Type[] FindConstructibleSubtypes(this Type type, Assembly assembly = null)
    {
        assembly ??= type.Assembly;
        // RCS1256 says this is an invalid null check because the parameter is optional with the default set to null, BUT we compound-assigned type's assembly
        // If THAT is null, something's wrong anyway
        ArgumentNullException.ThrowIfNull(assembly);

        return [.. assembly.GetTypes()
            .Where(t => t.IsAssignableTo(type)
                && !t.IsAbstract
                && !t.IsInterface
                && t.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Length > 0 // cannot be static if it has constructors
        )];
    }
}
