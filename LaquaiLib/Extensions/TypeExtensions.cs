using System.CodeDom.Compiler;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Reflection;
using System.Text.RegularExpressions;

using LaquaiLib.Util;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Type"/> Type.
/// </summary>
public static partial class TypeExtensions
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
        return !type.IsInterface
            ? throw new ArgumentException("Supplied type must be an interface.", nameof(type))
            : Assembly.GetAssembly(type) is Assembly assemblyOfInput
            ? assemblyOfInput.GetTypes().Where(t => t.GetInterfaces().Contains(type))
            : throw new ArgumentException("Supplied type must be part of an assembly.", nameof(type));
    }

    /// <summary>
    /// Returns a collection of all types that inherit from the supplied type.
    /// </summary>
    /// <param name="type">The type to get the options.Inheriting types for.</param>
    /// <param name="anyDepth">Whether to include all types that inherit from the supplied type, regardless of hierarchy depth.</param>
    /// <returns>A collection of all types that inherit from the supplied type.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="type"/>'s assembly cannot be resolved.</exception>
    public static IEnumerable<Type> GetInheritingTypes(this Type type, bool anyDepth = false)
    {
        if (Assembly.GetAssembly(type) is not Assembly assemblyOfInput)
        {
            throw new ArgumentException("Supplied type must be part of an assembly.", nameof(type));
        }

        var types = new List<Type>();
        if (anyDepth)
        {
            var stack = new Stack<Type>();
            stack.Push(type);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                types.Add(current);
                foreach (var inheritingType in assemblyOfInput.GetTypes().Where(t => t.IsSubclassOf(current)))
                {
                    stack.Push(inheritingType);
                }
            }
        }
        else
        {
            types = assemblyOfInput.GetTypes().Where(t => t.IsSubclassOf(type)).ToList();
        }
        return types.Except([type]).Distinct();
    }

    /// <summary>
    /// Returns a collection of all types that inherit from the supplied type and are not abstract.
    /// </summary>
    /// <param name="type">The type to get the non-abstract options.Inheriting types for.</param>
    /// <returns>A collection of all types that inherit from the supplied type and are not abstract.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="type"/>'s assembly cannot be resolved.</exception>
    public static IEnumerable<Type> GetNonAbstractInheritingTypes(this Type type)
    {
        return Assembly.GetAssembly(type) is Assembly assemblyOfInput
            ? assemblyOfInput.GetTypes().Where(t => t.IsSubclassOf(type) && !t.IsAbstract)
            : throw new ArgumentException("Supplied type must be part of an assembly.", nameof(type));
    }

    /// <summary>
    /// Returns a collection of all types that inherit from the supplied type and contain public constructors.
    /// </summary>
    /// <param name="type">The type to get the constructable options.Inheriting types for.</param>
    /// <returns>A collection of all types that inherit from the supplied type and contain public constructors.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="type"/>'s assembly cannot be resolved.</exception>
    public static IEnumerable<Type> GetConstructableInheritingTypes(this Type type)
    {
        return Assembly.GetAssembly(type) is Assembly assemblyOfInput
            ? assemblyOfInput.GetTypes().Where(t => t.IsSubclassOf(type) && t.GetConstructors().Any(c => c.IsPublic))
            : throw new ArgumentException("Supplied type must be part of an assembly.", nameof(type));
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
    /// Returns a (potentially boxed) instance of the default value for the supplied type.
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
                        dict.Add($"{methodInfo.Name}({string.Join(", ", methodInfo.GetParameters().Select(paramInfo => $"{paramInfo.ParameterType.GetFriendlyName()} {paramInfo.Name}"))}", null);
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
                            dict.Add($"{methodInfo.Name}({string.Join(", ", methodInfo.GetParameters().Select(paramInfo => $"{paramInfo.ParameterType.GetFriendlyName()} {paramInfo.Name}"))})", null);
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
            Convert.ChangeType(Activator.CreateInstance(type), other);
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
                Convert.ChangeType(type.GetDefault(), other);
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
        { "System.Void", "void" }
    }.ToFrozenDictionary();
    #endregion

    // "sane" because this method throws if the types are not numeric primitive types
    private static (TypeCode First, TypeCode Second) GetSaneTypeCodes(Type first, Type second)
    {
        var ret = (Type.GetTypeCode(first), Type.GetTypeCode(second));

        ThrowHelper.ThrowOnFirstOffender<ArgumentNullException, TypeCode>(
            _ => ["Type must be a numeric primitive type."],
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

    // TODO: Exclude all members with weird names that match FuncSignatureRegex or similar
    // (e.g. automatic private backing fields)
    /// <summary>
    /// Reflects the entirety of this <see cref="Type"/> and generates .NET 8.0 code that can be used to replicate it.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to reflect.</param>
    /// <param name="options.Inheriting">Whether to make the generated type(s) inherit from the <paramref name="type"/>. If <see langword="false"/>, a private static field of type <paramref name="type"/> is generated and all method calls are redirected to that field. If <see langword="true"/>, the generated type(s) inherit from <paramref name="type"/> and all method calls are redirected to <see langword="base"/>. If <see langword="null"/>, only a skeleton of the type is generated, with all methods throwing <see cref="NotImplementedException"/>s.</param>
    /// <returns>A <see cref="string"/> containing the generated code.</returns>
    /// <remarks>
    /// This method is not guaranteed to generate compilable code. It is intended to be used as a starting point for replicating existing types you may not have access to.
    /// </remarks>
    public static string Reflect(
        this Type type,
        ReflectionOptions? options = null
    )
    {
        options ??= ReflectionOptions.Default;

        var bindingFlags =
            BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.Instance
            | BindingFlags.Static;
        if (!options.Inherited)
        {
            bindingFlags |= BindingFlags.DeclaredOnly;
        }

        ArgumentNullException.ThrowIfNull(type);
        if (type.IsSealed && options.Inherit is true)
        {
            throw new TypeAccessException($"Cannot generate code for a type that inherits from sealed type '{type.GetFriendlyName()}'.");
        }

        using (var sw = new StringWriter())
        {
            using (var itw = new IndentedTextWriter(sw, "    "))
            {
                if (!string.IsNullOrWhiteSpace(options.Namespace))
                {
                    itw.WriteLine($"namespace {options.Namespace};");
                }

                #region Type Declaration
                itw.WriteLine($"""
                    /// <summary>
                    /// [Reflected from {type.GetFriendlyName()} (Assembly '{type.Assembly.GetName().Name}')]
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

                if (options.Inherit is true)
                {
                    itw.WriteLine($" : {type.GetFriendlyName()}");
                }
                #endregion

                itw.WriteLine("{");

                itw.Indent++;

                #region options.Inheriting is false
                if (options.Inherit is false)
                {
                    itw.WriteLine($"private readonly {type.GetFriendlyName()} _reflected = new {type.GetFriendlyName()}();");
                }
                #endregion

                #region Fields
                foreach (var field in type.GetFields(bindingFlags))
                {
                    var accessibility = field.GetAccessibility();
                    if (options.IgnoreInaccessible && IsInaccessibleAsReflectedType(accessibility, options.Inherit))
                    {
                        continue;
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
                foreach (var property in type.GetProperties(bindingFlags))
                {
                    var accessibility = property.GetAccessibility();
                    if (options.IgnoreInaccessible && IsInaccessibleAsReflectedType(accessibility, options.Inherit))
                    {
                        continue;
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
                                case true
                                    when !IsInaccessibleAsReflectedType(getAccessibility, options.Inherit):
                                    itw.WriteLine($"base.{property.Name};");
                                    break;
                                case false
                                    when property.GetMethod.IsPublic:
                                    itw.WriteLine($"this._reflected.{property.Name};");
                                    break;
                                case true:
                                case false:
                                    itw.WriteLine($"""throw new NotImplementedException("Reflected member '{type.GetFriendlyName()}.{property.Name}.get()' is not accessible.");""");
                                    break;
                                case null:
                                    itw.WriteLine($"""throw new NotImplementedException("Reflected member '{type.GetFriendlyName()}.{property.Name}.get()' is not implemented.");""");
                                    break;
                            }
                        }
                    }
                    if (property.SetMethod is not null)
                    {
                        var setAccessibility = property.SetMethod.GetAccessibility();
                        itw.Write(setAccessibility);
                        if (!options.IgnoreInaccessible || !IsInaccessibleAsReflectedType(setAccessibility, options.Inherit))
                        {
                            itw.Write(" set => ");
                            switch (options.Inherit)
                            {
                                case true
                                    when !IsInaccessibleAsReflectedType(setAccessibility, options.Inherit):
                                    itw.WriteLine($"base.{property.Name} = value;");
                                    break;
                                case false
                                    when property.SetMethod.IsPublic:
                                    itw.WriteLine($"this._reflected.{property.Name} = value;");
                                    break;
                                case true:
                                case false:
                                    itw.WriteLine($"""throw new NotImplementedException("Reflected member '{type.GetFriendlyName()}.{property.Name}.set()' is not accessible.");""");
                                    break;
                                case null:
                                    itw.WriteLine($"""throw new NotImplementedException("Reflected member '{type.GetFriendlyName()}.{property.Name}.set()' is not implemented.");""");
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
                        && !FuncSignatureRegex().IsMatch(m.Name)
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
                foreach (var method in methods)
                {
                    var accessibility = method.GetAccessibility();
                    // Determine if the generic method requires an unsafe context
                    var unsafeRequired = method.GetParameters().Any(p => p.ParameterType.IsPointer);

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
                        case true
                            when !IsInaccessibleAsReflectedType(accessibility, options.Inherit):
                            itw.WriteLine($" => base.{method.Name}({string.Join(", ", method.GetParameters().Select(p => p.Name))});");
                            break;
                        case false
                            when method.IsPublic:
                            itw.WriteLine($" => this._reflected.{method.Name}({string.Join(", ", method.GetParameters().Select(p => p.Name))});");
                            break;
                        case true:
                        case false:
                            itw.WriteLine($""" => throw new NotImplementedException("Reflected member '{type.GetFriendlyName()}.{method.Name}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.GetFriendlyName()))})' is not accessible.");""");
                            break;
                        case null:
                            itw.WriteLine($""" => throw new NotImplementedException("Reflected member '{type.GetFriendlyName()}.{method.Name}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.GetFriendlyName()))})' is not implemented.");""");
                            break;
                    }
                }
                #endregion

                if (options.Deep)
                {
                    foreach (var nestedType in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        itw.WriteLine();
                        itw.WriteLine(nestedType.Reflect(options));
                    }
                }

                itw.Indent--;
                itw.WriteLine("}");
            }

            return sw.ToString();
        }
    }

    private static string GetLeastAccessibleModifier(IEnumerable<string> modifiers)
    {
        var modifiersEnumerated = modifiers.ToArray();
        if (modifiersEnumerated.Contains("private")) // same type only
        {
            return "private";
        }
        else
        {
            return modifiersEnumerated.Contains("private protected")
                ? "private protected"
                : modifiersEnumerated.Contains("protected")
                    ? "protected"
                    : modifiersEnumerated.Contains("internal")
                        ? "internal"
                        : modifiersEnumerated.Contains("protected internal") ? "protected internal" : "public";
        }
    }
    private static bool IsInaccessibleAsReflectedType(string modifiers, bool? inheriting)
    {
        return modifiers.ToUpperInvariant() switch
        {
            "PRIVATE" => true,
            "PRIVATE PROTECTED" => true, // technically this COULD return true, but that would require the reflected type to be in the same assembly as the reflected type, which is... possible, but stupid
            "PROTECTED" => inheriting is not true,
            "INTERNAL" => true,
            "PROTECTED INTERNAL" => inheriting is not true,
            "PUBLIC" => false,
            _ => true
        };
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
        var operateOn = type.FullName ?? type.Namespace + '.' + type.Name;
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
    private static string AsKeyword(string type) => _typeKeywordMap.TryGetValue(type, out var keyword) ? keyword : type;

    /*
    System.String <<>m0>b__0_0(System.String)
    Int32 <<>m0>b__0_0(Int32)
    System.String <<>m0>b__0_0(System.String)
    <AppendFormatHelper>g__MoveNext|116_0(string, int[])
    */
    [GeneratedRegex(@"<.*?>\p{Ll}__(\p{L}|\p{Nd}|\||_)+?_\p{Nd}+?(?=\(.*?\))?", RegexOptions.ExplicitCapture)]
    private static partial Regex FuncSignatureRegex();
}
