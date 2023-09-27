using System.Reflection;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="FieldInfo"/> Type.
/// </summary>
public static class FieldInfoExtensions
{
    /// <summary>
    /// Retrieves the value of the field represented by this <paramref name="fieldInfo"/> from the given <paramref name="obj"/>ect typed as <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The Type to attempt to convert the retrieved value to.</typeparam>
    /// <param name="fieldInfo">The <see cref="FieldInfo"/> instance representing the field to retrieve the value of.</param>
    /// <param name="obj">The <see cref="object"/> instance to retrieve the value from. May be <see langword="null"/> if <paramref name="fieldInfo"/> represents a field that is static.</param>
    /// <returns>The value of the field represented by this <paramref name="fieldInfo"/> typed as <typeparamref name="T"/>.</returns>
    public static T GetValue<T>(this FieldInfo fieldInfo, object? obj)
    {
        ArgumentNullException.ThrowIfNull(fieldInfo);
        var value = fieldInfo.GetValue(obj);
        return (T)value;
    }

    /// <summary>
    /// Attempts to retrieve the value of the field represented by this <paramref name="fieldInfo"/> from the given <paramref name="obj"/>ect typed as <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The Type to attempt to convert the retrieved value to.</typeparam>
    /// <param name="fieldInfo">The <see cref="FieldInfo"/> instance representing the field to retrieve the value of.</param>
    /// <param name="obj">The <see cref="object"/> instance to retrieve the value from. May be <see langword="null"/> if <paramref name="fieldInfo"/> represents a field that is static.</param>
    /// <returns>The value of the field represented by this <paramref name="fieldInfo"/> typed as <typeparamref name="T"/> if the field exists and its could be cast to <typeparamref name="T"/>, otherwise <c>default</c>.</returns>
    public static T? GetValueOrDefault<T>(this FieldInfo fieldInfo, object? obj)
    {
        ArgumentNullException.ThrowIfNull(fieldInfo);
        var value = fieldInfo.GetValue(obj);
        if (value is not null && value.GetType().CanCastTo(typeof(T)))
        {
            return (T)value;
        }
        return default;
    }
}
