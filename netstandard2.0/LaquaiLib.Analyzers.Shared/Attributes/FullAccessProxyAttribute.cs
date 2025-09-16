namespace LaquaiLib.Analyzers.Shared.Attributes;

/// <summary>
/// Marks a type declaration that should have members source-generated that proxy the members of the <typeparamref name="TProxied"/> type.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class FullAccessProxyAttribute : Attribute
{
    /// <summary>
    /// Initializes a new <see cref="FullAccessProxyAttribute"/> using the specified type to proxy.
    /// </summary>
    /// <param name="proxied">The type of the type to proxy. Must be a reference type.</param>
    public FullAccessProxyAttribute(Type proxied)
    {
        ValidateType(proxied);

        Proxied = proxied;
    }

    private static void ValidateType(Type proxied)
    {
        if (proxied is null)
        {
            throw new ArgumentNullException(nameof(proxied));
        }
        if (proxied.IsValueType)
        {
            throw new InvalidOperationException("The type to proxy must be a reference type.");
        }
    }

    /// <summary>
    /// Initializes a new <see cref="FullAccessProxyAttribute"/> using the fully-qualified name of the type to proxy.
    /// </summary>
    /// <param name="fullyQualifiedTypeName">The fully-qualified name of the type to proxy, which must be appropriate to pass to <see cref="Type.GetType(string)"/> (as in, to successfully resolve a <see cref="Type"/>'s name to itself, its <see cref="Type.AssemblyQualifiedName"/> must be specified if it is not defined in <c>mscorlib</c>). Must resolve to a reference type.</param>
    public FullAccessProxyAttribute(string fullyQualifiedTypeName)
    {
        var proxied = Type.GetType(fullyQualifiedTypeName, true);
        ValidateType(proxied);

        Proxied = proxied;
    }

    /// <summary>
    /// Gets the <see cref="Type"/> instance representing the type being proxied.
    /// </summary>
    public Type Proxied { get; }
}