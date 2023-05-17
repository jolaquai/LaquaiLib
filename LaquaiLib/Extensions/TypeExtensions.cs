using System.Linq;
using System.Reflection;

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
}
