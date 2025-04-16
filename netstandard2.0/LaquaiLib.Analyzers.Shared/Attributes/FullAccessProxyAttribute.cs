using System.Reflection;

namespace LaquaiLib.Analyzers.Shared.Attributes;

/// <summary>
/// Marks a type declaration that should have members source-generated that proxy the members of the <typeparamref name="TProxied"/> type.
/// </summary>
/// <typeparam name="TProxied">The type of the type to proxy.</typeparam>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class FullAccessProxyAttribute<TProxied> : Attribute
{
    /// <summary>
    /// Specifies a <see cref="System.Reflection.BindingFlags"/> value that indicate which members to generate proxies for.
    /// Defaults to all members, regardless of accessibility.
    /// </summary>
    public BindingFlags BindingFlags { get; set; } = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
    /// <summary>
    /// Specifies whether to generate proxies for members declared at the level higher than <typeparamref name="TProxied"/> in the type hierarchy.
    /// This property is ignored when the attribute is placed on a <see langword="struct"/> declaration.
    /// </summary>
    public bool IncludeHierarchy { get; set; } = true;
}
