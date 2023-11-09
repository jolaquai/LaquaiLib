using System.Reflection;

namespace LaquaiLib.Attributes;

/// <summary>
/// Indicates to consumers that the attributed member should not be reflected (into).
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class DoNotReflectAttribute : Attribute
{
    /// <summary>
    /// Throws a <see cref="ReflectionException"/> if the given <paramref name="member"/> is marked with <see cref="DoNotReflectAttribute"/>.
    /// </summary>
    /// <param name="member">The <see cref="MemberInfo"/> to check.</param>
    public static void ThrowIfMarked(MemberInfo member)
    {
        ArgumentNullException.ThrowIfNull(member);
        if (member.GetCustomAttribute<DoNotReflectAttribute>() is not null)
        {
            if (member is Type type)
            {
                throw new ReflectionException(type);
            }
            throw new ReflectionException(member.DeclaringType!, member);
        }
    }
    /// <summary>
    /// Throws a <see cref="ReflectionException"/> if the given <paramref name="assembly"/> is marked with <see cref="DoNotReflectAttribute"/>.
    /// </summary>
    /// <param name="assembly">The <see cref="Assembly"/> to check.</param>
    public static void ThrowIfMarked(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        if (assembly.GetCustomAttribute<DoNotReflectAttribute>() is not null)
        {
            throw new ReflectionException();
        }
    }
}

/// <summary>
/// The <see cref="Exception"/> that is thrown when a member marked with <see cref="DoNotReflectAttribute"/> is attempted to be reflected (into).
/// </summary>
/// <remarks>
/// Consuming code bases implementing the Reflection-DoNotReflectAttribute system should also opt to throwing this exception when appropriate.
/// </remarks>
public sealed class ReflectionException : Exception
{
    /// <summary>
    /// The <see cref="Type"/> that was attempted to be reflected (into).
    /// </summary>
    public Type? Type { get; }
    /// <summary>
    /// The <see cref="MemberInfo"/> representing the member that was attempted to be reflected (into).
    /// May be reference-equal to <see cref="Type"/> if the member is a type.
    /// </summary>
    public MemberInfo? Member { get; }

    /// <summary>
    /// Throws a generic <see cref="ReflectionException"/>. Used when an entire <see cref="Assembly"/> is marked with <see cref="DoNotReflectAttribute"/>.
    /// </summary>
    internal ReflectionException() : base("The assembly is marked with DoNotReflectAttribute and must not be reflected (into).")
    {
    }
    /// <summary>
    /// Instantiates a new <see cref="ReflectionException"/> with the given message.
    /// </summary>
    /// <param name="message">The message of the exception.</param>
    /// <param name="type">The <see cref="Type"/> that was attempted to be reflected (into).</param>
    /// <param name="member">The <see cref="MemberInfo"/> representing the member that was attempted to be reflected (into).</param>
    public ReflectionException(string message, Type type, MemberInfo? member = null) : base(message)
    {
        ArgumentNullException.ThrowIfNull(type);

        Type = type;
        Member = member ?? type;
    }
    /// <summary>
    /// Instantiates a new <see cref="ReflectionException"/> with the default message, the given <see cref="Type"/> and <see cref="MemberInfo"/>.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> that was attempted to be reflected (into).</param>
    /// <param name="member">The <see cref="MemberInfo"/> representing the member that was attempted to be reflected (into).</param>
    public ReflectionException(Type type, MemberInfo? member = null) : this($"The assembly {member?.Name ?? type?.Name ?? ""} of type {type?.Name ?? ""} is marked with {nameof(DoNotReflectAttribute)} and must not be reflected (into).", type, member)
    {
    }
    /// <summary>
    /// Instantiates a new <see cref="ReflectionException"/> with the given message and an <paramref name="innerException"/> that was the cause of this exception.
    /// </summary>
    /// <param name="message">The message of the exception.</param>
    /// <param name="innerException">The <see cref="Exception"/> that was the cause of this exception.</param>
    /// <param name="type">The <see cref="Type"/> that was attempted to be reflected (into).</param>
    /// <param name="member">The <see cref="MemberInfo"/> representing the member that was attempted to be reflected (into).</param>
    public ReflectionException(string message, Exception innerException, Type type, MemberInfo? member = null) : base(message, innerException)
    {
        ArgumentNullException.ThrowIfNull(type);

        Type = type;
        Member = member ?? type;
    }
}
