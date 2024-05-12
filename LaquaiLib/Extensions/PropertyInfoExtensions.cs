using System.Reflection;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="PropertyInfo"/> Type.
/// </summary>
public static class PropertyInfoExtensions
{
    /// <summary>
    /// Retrieves the value of the property represented by this <paramref name="propertyInfo"/> from the given <paramref name="obj"/>ect typed as <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The Type to attempt to convert the retrieved value to.</typeparam>
    /// <param name="propertyInfo">The <see cref="PropertyInfo"/> instance representing the property to retrieve the value of.</param>
    /// <param name="obj">The <see cref="object"/> instance to retrieve the value from. May be <see langword="null"/> if <paramref name="propertyInfo"/> represents a property that is static.</param>
    /// <returns>The value of the property represented by this <paramref name="propertyInfo"/> typed as <typeparamref name="T"/>.</returns>
    public static T GetValue<T>(this PropertyInfo propertyInfo, object? obj)
    {
        ArgumentNullException.ThrowIfNull(propertyInfo);
        var value = propertyInfo.GetValue(obj);
        return (T)value;
    }

    /// <summary>
    /// Attempts to retrieve the value of the property represented by this <paramref name="propertyInfo"/> from the given <paramref name="obj"/>ect typed as <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The Type to attempt to convert the retrieved value to.</typeparam>
    /// <param name="propertyInfo">The <see cref="PropertyInfo"/> instance representing the property to retrieve the value of.</param>
    /// <param name="obj">The <see cref="object"/> instance to retrieve the value from. May be <see langword="null"/> if <paramref name="propertyInfo"/> represents a property that is static.</param>
    /// <returns>The value of the property represented by this <paramref name="propertyInfo"/> typed as <typeparamref name="T"/> if the property exists and its could be cast to <typeparamref name="T"/>, otherwise <c>default</c>.</returns>
    public static T? GetValueOrDefault<T>(this PropertyInfo propertyInfo, object? obj)
    {
        ArgumentNullException.ThrowIfNull(propertyInfo);
        var value = propertyInfo.GetValue(obj);
        if (value?.GetType().CanCastTo(typeof(T)) == true)
        {
            return (T)value;
        }
        return default;
    }
}
