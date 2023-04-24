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
}
