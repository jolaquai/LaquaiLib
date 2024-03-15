namespace LaquaiLib.Extensions;

/// <summary>
/// Represents a set of options for <see cref="TypeExtensions.Reflect(Type, ReflectionOptions)"/>.
/// </summary>
public record ReflectionOptions
{
    /// <summary>
    /// The namespace into which the generated type(s) should be placed. If <see langword="null"/> or empty, the code is generated without a namespace declaration.
    /// </summary>
    public string? Namespace { get; init; }
    /// <summary>
    /// Whether to make the generated type inherit from the <see cref="Type"/>.
    /// <para/>If <see langword="false"/>, a private field of the original type is generated and all method calls are initially redirected to that field.
    /// <para/>If <see langword="true"/>, the generated type inherits from <see cref="Type"/> and all method calls are initially redirected to <see langword="base"/>.
    /// <para/>If <see langword="null"/>, only a skeleton of the type is generated, with all methods throwing <see cref="NotImplementedException"/>s.
    /// </summary>
    public bool? Inherit { get; init; }
    /// <summary>
    /// Whether to generate code for all members <see cref="Type"/> inherits from its base types. This defaults to <see langword="true"/>.
    /// </summary>
    public bool Inherited { get; init; } = true;
    /// <summary>
    /// Whether to generate code for all <see cref="Type"/>s that the original type has nested within it.
    /// </summary>
    public bool Deep { get; init; }
    /// <summary>
    /// Whether to ignore members which cannot be redirected to an instance of the original type (or to <see langword="base"/> when <see cref="Inherit"/> is <see langword="true"/>). Defaults to <see langword="true"/>.
    /// <para/>This is ignored when <see cref="Inherit"/> is <see langword="null"/>.
    /// </summary>
    /// <remarks>
    /// Setting this to <see langword="false"/> is helpful when you intend to supply custom implementations for inaccessible members, otherwise this will pollute your generated code with <see cref="NotImplementedException"/> (inside methods which potentially cannot be called from outside the generated type).
    /// </remarks>
    public bool IgnoreInaccessible { get; init; } = true;

    /// <summary>
    /// Returns a cached instance of <see cref="ReflectionOptions"/> with the default behavior.
    /// </summary>
    public static ReflectionOptions Default { get; } = new ReflectionOptions();
}
