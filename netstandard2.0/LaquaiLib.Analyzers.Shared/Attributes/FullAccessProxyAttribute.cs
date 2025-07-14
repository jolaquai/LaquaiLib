namespace LaquaiLib.Analyzers.Shared.Attributes;

/// <summary>
/// Marks a type declaration that should have members source-generated that proxy the members of the <typeparamref name="TProxied"/> type.
/// </summary>
/// <typeparam name="TProxied">The type of the type to proxy. Must be a reference type.</typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class FullAccessProxyAttribute<TProxied> : Attribute
    where TProxied : class;