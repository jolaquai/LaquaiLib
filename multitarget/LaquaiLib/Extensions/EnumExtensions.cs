using System.ComponentModel;
using System.Reflection;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides Extension Methods for <see cref="Enum"/> Types.
/// </summary>
public static class EnumExtensions
{
    extension<TEnum>(TEnum any) where TEnum : struct, Enum
    {
        /// <summary>
        /// Returns the <see cref="DescriptionAttribute.Description"/> for the given <see cref="Enum"/> value. If the value is not decorated with a <see cref="DescriptionAttribute"/>, the default <see cref="string"/> representation of the value is returned.
        /// </summary>
        /// <param name="any">The <see cref="Enum"/> value to retrieve the description for.</param>
        /// <returns>The value of the <see cref="DescriptionAttribute.Description"/> for the given <see cref="Enum"/> value or its default <see cref="string"/> representation.</returns>
        public string Description
        {
            get
            {
                var type = any.GetType();
                var name = Enum.GetName(any);
                var fallback = any.ToString();
                return type.GetField(name!) is FieldInfo fieldInfo && fieldInfo.GetCustomAttribute<DescriptionAttribute>() is DescriptionAttribute desc
                    ? desc.Description
                    : fallback;
            }
        }
        /// <summary>
        /// Retrieves all flags that are currently set in the specified <typeparamref name="TEnum"/> value.
        /// Because this implicitly makes <paramref name="any"/> a bitwise-AND combination of the resulting flags, it is not included in the result set.
        /// </summary>
        /// <typeparam name="TEnum">The <see cref="Enum"/> type to retrieve the flags for.</typeparam>
        /// <param name="any">The <typeparamref name="TEnum"/> value to retrieve the flags for.</param>
        /// <returns>The flags that are currently set in the specified <typeparamref name="TEnum"/> value or an empty array if no flags are set.</returns>
        public TEnum[] Flags
        {
            get
            {
                if (typeof(TEnum).GetCustomAttribute<FlagsAttribute>() is null)
                {
                    throw new ArgumentException($"The given Enum type '{typeof(TEnum).FullName}' is not marked with [FlagsAttribute].", nameof(any));
                }
                var flags = Enum.GetValues<TEnum>();
                return flags.Where(f => any.HasFlag(f)).ToArray();
            }
        }
    }

    extension<T>(T source) where T : struct, Enum
    {
        /// <summary>
        /// Determines if an enum value <paramref name="source"/> of <typeparamref name="T"/> has a non-zero value.
        /// </summary>
        /// <typeparam name="T">The type of the enum.</typeparam>
        /// <param name="source">The enum value to test.</param>
        /// <returns>A value indicating whether <paramref name="source"/> has a non-zero value.</returns>
        public bool HasValue() => Convert.ToInt64(source) != 0;
        /// <summary>
        /// Determines if an enum value <paramref name="source"/> of <typeparamref name="T"/> has at least one value that another <paramref name="value"/> also has. May not work correctly with non-<see cref="FlagsAttribute"/> enums.
        /// </summary>
        /// <typeparam name="T">The type of the enum.</typeparam>
        /// <param name="source">The enum value to test.</param>
        /// <param name="value">An enum value that is checked against.</param>
        /// <returns>A value indicating whether <paramref name="source"/> has at least one value that <paramref name="value"/> also has.</returns>
        public bool HasValue(T value)
        {
            var intersect = source.Flags.Intersect(value.Flags).ToArray();
            return intersect.Length switch
            {
                0 => false,
                1 => Convert.ToUInt64(intersect[0]) != 0,
                _ => true,
            };
        }
    }
}
