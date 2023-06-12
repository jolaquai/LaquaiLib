using System.ComponentModel;
using System.Reflection;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides Extension Methods for <see cref="Enum"/> Types.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Returns the <see cref="DescriptionAttribute.Description"/> for the given <see cref="Enum"/> value. If the value is not decorated with a <see cref="DescriptionAttribute"/>, the default <see cref="string"/> representation of the value is returned.
    /// </summary>
    /// <param name="any">The <see cref="Enum"/> value to retrieve the description for.</param>
    /// <returns>The value of the <see cref="DescriptionAttribute.Description"/> for the given <see cref="Enum"/> value or its default <see cref="string"/> representation.</returns>
    public static string GetDescription(this Enum any)
    {
        var type = any.GetType();
        var name = Enum.GetName(type, any);
        var fallback = any.ToString();
        if (type.GetField(name!) is FieldInfo field
            && field.GetCustomAttribute<DescriptionAttribute>() is DescriptionAttribute desc)
        {
            return desc.Description;
        }
        else
        {
            return fallback;
        }
    }
}
