using System.Reflection;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="ConstructorInfo"/> Type.
/// </summary>
public static class ConstructorInfoExtensions
{
    /// <summary>
    /// Attempts to instantiate a new object of the type <paramref name="constructorInfo"/> belongs to, using the given <paramref name="parameters"/>.
    /// </summary>
    /// <param name="constructorInfo">The <see cref="ConstructorInfo"/> that identifies a constructor of the type to instantiate.</param>
    /// <param name="parameters">The parameters to pass to the constructor. May be <see langword="null"/> if the constructor has no parameters.</param>
    /// <returns>An instance of the type <paramref name="constructorInfo"/> belongs to, or <see langword="null"/> if the constructor could not be invoked.</returns>
    public static object? New(this ConstructorInfo constructorInfo, params object[] parameters)
    {
        try
        {
            return constructorInfo.Invoke(parameters);
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Attempts to instantiate a new object of the type <paramref name="constructorInfo"/> belongs to, using the given <paramref name="parameters"/> and returns it cast to <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to cast the new instance to.</typeparam>
    /// <param name="constructorInfo">The <see cref="ConstructorInfo"/> that identifies a constructor of the type to instantiate.</param>
    /// <param name="parameters">The parameters to pass to the constructor. May be <see langword="null"/> if the constructor has no parameters. May be <see langword="null"/> if the constructor has no parameters.</param>
    /// <returns>An instance of <typeparamref name="T"/>, or <see langword="null"/> if the constructor could not be invoked.</returns>
    public static T? New<T>(this ConstructorInfo constructorInfo, params object[] parameters)
    {
        try
        {
            return (T?)constructorInfo.Invoke(parameters);
        }
        catch
        {
            return default;
        }
    }
}
