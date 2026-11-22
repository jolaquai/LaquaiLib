using System.Reflection;

namespace LaquaiLib.Extensions;

/// <summary>
/// Specifies the kind of implementation <see cref="EqualityComparer{T}.Default"/> returns and how it will compare instances of the generic type specified for the type parameter of <see cref="EqualityComparer{T}"/>.
/// </summary>
public enum EqualityComparerDefaultOperationMode
{
    /// <summary>
    /// Not a valid enum member.
    /// </summary>
    Invalid,
    /// <summary>
    /// Indicates that the generic type argument's generic type definition is <see cref="Nullable{T}"/>. To find the true operation mode, get <see cref="EqualityComparerExtensions.get_DefaultOperationMode{T}"/> for the underlying type of the nullable type.
    /// </summary>
    Nullable,
    /// <summary>
    /// Indicates that the generic type argument was <see langword="string"/>. The returned implementation of <see cref="EqualityComparer{T}"/> will be a <see cref="StringComparer"/>.
    /// </summary>
    String,
    /// <summary>
    /// Indicates that the generic type argument implements <see cref="IEquatable{T}"/>. The returned implementation of <see cref="EqualityComparer{T}"/> will use the <see cref="IEquatable{T}.Equals(T)"/> method for comparison.
    /// </summary>
    IEquatable,
    /// <summary>
    /// Indicates that the generic type argument is an <see langword="enum"/> type. The returned implementation of <see cref="EqualityComparer{T}"/> will use default <see langword="enum"/> comparison (that is, the underlying integral type of the <see langword="enum"/> is compared numerically).
    /// </summary>
    Enum,
    /// <summary>
    /// Indicates that <see cref="object.Equals(object?)"/> is used for comparison because the generic type argument does not fall into any of the other categories.
    /// Specifically, the generic type argument is a reference type and does not override <see cref="object.Equals(object?)"/>, so the default implementation of <see cref="object.Equals(object?)"/> uses reference equality.
    /// </summary>
    Equals,
    /// <summary>
    /// Indicates that <see cref="object.Equals(object?)"/> is used for comparison because the generic type argument does not fall into any of the other categories.
    /// Specifically, the generic type argument is a reference type and a type in its inheritance chain overrides <see cref="object.Equals(object?)"/> but the generic type argument itself does not, so that inherited override is used for comparison.
    /// </summary>
    EqualsOverrideInherited,
    /// <summary>
    /// Indicates that <see cref="object.Equals(object?)"/> is used for comparison because the generic type argument does not fall into any of the other categories.
    /// Specifically, the generic type argument is a reference type and overrides <see cref="object.Equals(object?)"/>, so that override is used for comparison.
    /// </summary>
    EqualsOverrideDeclared,
    /// <summary>
    /// Indicates that <see cref="ValueType.Equals(object?)"/> is used for comparison because the generic type argument does not fall into any of the other categories.
    /// Specifically, the generic type argument is a value type and does not override <see cref="object.Equals(object?)"/>, so the default implementation of <see cref="ValueType.Equals(object?)"/> is used for comparison.
    /// <para/>For value types that recursively contain no reference type or floating-point fields, a simple byte compare is done. Otherwise, a recursive boxing field-by-field comparison is done.
    /// </summary>
    ValueTypeEquals,
    /// <summary>
    /// Indicates that <see cref="ValueType.Equals(object?)"/> is used for comparison because the generic type argument does not fall into any of the other categories.
    /// Specifically, the generic type argument is a value type and overrides <see cref="object.Equals(object?)"/>, so that override is used for comparison.
    /// </summary>
    ValueTypeEqualsOverride,
}

/// <summary>
/// Provides extensions for <see cref="EqualityComparer{T}"/>.
/// </summary>
public static class EqualityComparerExtensions
{
    extension<T>(EqualityComparer<T>)
    {
        /// <summary>
        /// Gets the type of comparison <see cref="EqualityComparer{T}.Default"/> will use for the type <typeparamref name="T"/>.
        /// </summary>
        public static EqualityComparerDefaultOperationMode DefaultOperationMode
        {
            get
            {
                var type = typeof(T);
                if (type == typeof(string))
                    return EqualityComparerDefaultOperationMode.String;
                else if (type.IsAssignableTo(typeof(IEquatable<T>)))
                    return EqualityComparerDefaultOperationMode.IEquatable;
                else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    return EqualityComparerDefaultOperationMode.Nullable;
                else if (type.IsEnum)
                    return EqualityComparerDefaultOperationMode.Enum;

                var equalsOverride = GetObjectEqualsOverride(type);
                if (type.IsValueType)
                {
                    return equalsOverride == ObjectEqualsOverride.Declared
                        ? EqualityComparerDefaultOperationMode.ValueTypeEqualsOverride
                        : EqualityComparerDefaultOperationMode.ValueTypeEquals;
                }
                return equalsOverride switch
                {
                    ObjectEqualsOverride.Declared => EqualityComparerDefaultOperationMode.EqualsOverrideDeclared,
                    ObjectEqualsOverride.Inherited => EqualityComparerDefaultOperationMode.EqualsOverrideInherited,
                    _ => EqualityComparerDefaultOperationMode.Equals,
                };
            }
        }
    }

    /// <summary>
    /// Describes whether and where a <see cref="Type"/> overrides <see cref="object.Equals(object?)"/>.
    /// </summary>
    internal enum ObjectEqualsOverride
    {
        /// <summary>
        /// The type resolves to <see cref="object.Equals(object?)"/> directly, or only to the <see cref="ValueType"/>/<see cref="Enum"/> plumbing.
        /// </summary>
        None,
        /// <summary>
        /// A base type overrides <see cref="object.Equals(object?)"/>; the queried type itself does not. Implies the queried type is a reference type.
        /// </summary>
        Inherited,
        /// <summary>
        /// The queried type itself declares the override.
        /// </summary>
        Declared,
    }

    /// <summary>
    /// Determines whether <paramref name="type"/> overrides <see cref="object.Equals(object?)"/> and, if so, whether that override is declared on <paramref name="type"/> itself or inherited from a base type.
    /// </summary>
    internal static ObjectEqualsOverride GetObjectEqualsOverride(Type type)
    {
        var m = type.GetMethod(
            nameof(Equals),
            BindingFlags.Instance | BindingFlags.Public,
            null,
            [typeof(object)],
            null);

        if (m is null || !m.IsVirtual)
            return ObjectEqualsOverride.None;
        // a `new` hide passes the IsVirtual check but does not sit in object's Equals slot
        if (m.GetBaseDefinition().DeclaringType != typeof(object))
            return ObjectEqualsOverride.None;

        var d = m.DeclaringType;
        if (d == typeof(object) || d == typeof(ValueType) || d == typeof(Enum))
            return ObjectEqualsOverride.None;

        return d == type ? ObjectEqualsOverride.Declared : ObjectEqualsOverride.Inherited;
    }
}
