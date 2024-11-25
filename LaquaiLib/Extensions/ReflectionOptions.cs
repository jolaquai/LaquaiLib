namespace LaquaiLib.Extensions;

/// <summary>
/// Represents a set of options for <see cref="TypeExtensions.Reflect(Type, ReflectionOptions)"/>.
/// </summary>
public readonly struct ReflectionOptions()
{
    /// <summary>
    /// The namespace into which the generated type(s) should be placed. If <see langword="null"/> or empty, the code is generated without a namespace declaration.
    /// </summary>
    public string Namespace { get; init; }
    /// <summary>
    /// How interaction with the reflected type is to be outlined in the generated code. Defaults to <see cref="InheritanceBehavior.FieldDelegation"/>.
    /// </summary>
    public InheritanceBehavior Inherit { get; init; } = InheritanceBehavior.FieldDelegation;
    /// <summary>
    /// Whether to generate code for all members <see cref="Type"/> inherits from its base types. This defaults to <see langword="true"/>.
    /// </summary>
    public bool IncludeHierarchy { get; init; } = true;
    /// <summary>
    /// Whether to generate code for all <see cref="Type"/>s that the original type has nested within it.
    /// </summary>
    public bool Deep { get; init; }
    /// <summary>
    /// Whether to ignore members which cannot be redirected to an instance of the original type (or to <see langword="base"/> when <see cref="Inherit"/> is <see langword="true"/>). Defaults to <see langword="true"/>.
    /// <para/>This is ignored when <see cref="Inherit"/> is <see cref="InheritanceBehavior.None"/>.
    /// </summary>
    /// <remarks>
    /// Setting this to <see langword="false"/> is helpful when you intend to supply custom implementations for inaccessible members, otherwise this will pollute your generated code with <see cref="NotImplementedException"/> (inside methods which potentially cannot be called from outside the generated type).
    /// </remarks>
    public bool IgnoreInaccessible { get; init; } = true;

    /// <summary>
    /// Returns a cached instance of <see cref="ReflectionOptions"/> with the default behavior.
    /// </summary>
    public static ReflectionOptions Default { get; }

    /// <summary>
    /// Specifies the behavior of inheritance for the generated type.
    /// </summary>
    public enum InheritanceBehavior
    {
        /// <summary>
        /// Specifies that the generated code should include a private field of the original type to which all method calls are initially redirected.
        /// </summary>
        FieldDelegation,
        /// <summary>
        /// Specifies that the generated code should inherit from the original type and redirect all method calls to <see langword="base"/>.
        /// </summary>
        Inherit,
        /// <summary>
        /// Specifies that only a skeleton of the type should be generated, with all methods throwing <see cref="NotImplementedException"/>s.
        /// </summary>
        None
    }
}
