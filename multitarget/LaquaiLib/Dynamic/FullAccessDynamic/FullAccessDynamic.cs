using System.Dynamic;
using System.Reflection;

namespace LaquaiLib.Dynamic;

/// <summary>
/// Represents a dynamic object that allows access to all properties and methods of the wrapped object as if they were <see langword="public"/>, regardless of their actual access level.
/// <para/>Note that all dynamically retrieved members are also instances of <see cref="FullAccessDynamic{T}"/> to allow for further dynamic access. The only value explicitly propagated to allow <c>?.</c> <see langword="null"/> propagation is <see langword="null"/>. This does <b>not</b> work directly on an object of type <see cref="FullAccessDynamic{T}"/>, i.e. the following method invocation will always take place:
/// <code language="csharp">
/// MyClass? myInstance = null;
/// var myFullAccessDynamic = FullAccessDynamic.Create(typeof(MyClass), myInstance);
/// // This incovation will happen no matter if the underlying object myInstance is null or not, because the null propagation will check the FullAccessDynamic instance, rather than the underlying object
/// myFullAccessDynamic?.MyMethod();
/// // These ones will not, however, if MyProperty is null or MyNullReturningMethod returns null
/// myFullAccessDynamic.MyProperty?.MyMethod();
/// myFullAccessDynamic.MyNullReturningMethod()?.MyMethod();
/// </code>
/// <para/><b>Warning!</b> Nothing prevents the underlying object instance of <typeparamref name="T"/> from being <see langword="null"/>. As such, <see cref="Unwrap"/> may return <see langword="null"/>.
/// </summary>
/// <typeparam name="T">The type of the object to wrap.</typeparam>
public class FullAccessDynamic<T> : DynamicObject
{
    private readonly T _instance;
    private readonly Type _instanceType = typeof(T);
    private const BindingFlags publicInstance = BindingFlags.Instance | BindingFlags.Public;
    private const BindingFlags anyInstance = BindingFlags.Instance | BindingFlags.NonPublic;
    private const BindingFlags publicStatic = BindingFlags.Static | BindingFlags.Public;
    private const BindingFlags anyStatic = BindingFlags.Static | BindingFlags.NonPublic;
    private const BindingFlags bindingFlags = publicInstance | anyInstance | publicStatic | anyStatic;

    private static readonly Dictionary<string, MemberInfo> _memberCache = [];

    internal FullAccessDynamic() => _instance = Activator.CreateInstance<T>();
    internal FullAccessDynamic(T instance) => _instance = instance;
    /// <inheritdoc/>
    public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
    {
        if (_instance is null)
        {
            result = null;
            return false;
        }

        // Proxy all the object-inherited methods back to the instance
        switch (binder.Name)
        {
            case "ToString":
                result = _instance.ToString();
                return true;
            case "GetHashCode":
                result = _instance.GetHashCode();
                return true;
            case "GetType":
                result = _instance.GetType();
                return true;
            case "Equals" when args is not null and { Length: 1 }:
                result = _instance.Equals(args[0]);
                return true;
            case "Unwrap":
                result = Unwrap();
                return true;
            default:
                break;
        }

        // Attempt to find the method with the specified name and parameter types.
        var key = _instanceType.Namespace + '.' + _instanceType.Name + '.' + binder.Name + '(' + string.Join(", ", Array.ConvertAll(args, static item =>
        {
            var itemType = item.GetType();
            return itemType.Namespace + '.' + itemType.Name;
        })) + ')';
        MethodInfo method;
        if (_memberCache.TryGetValue(key, out var value) && value is MethodInfo info)
        {
            method = info;
        }
        else
        {
            method = FindMethod(binder, args);
            _memberCache[key] = method;
        }

        if (method is not null)
        {
            result = FullAccessDynamicFactory.Create(method.ReturnType, method.Invoke(_instance, args));
            return true;
        }

        result = null;
        return false;
    }
    /// <inheritdoc/>
    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
        var key = _instanceType.Namespace + '.' + _instanceType.Name + '.' + binder.Name;

        PropertyInfo prop = null;
        FieldInfo field = null;
        if (_memberCache.TryGetValue(key, out var value))
        {
            switch (value)
            {
                case PropertyInfo propInfo:
                    prop = propInfo;
                    break;
                case FieldInfo fieldInfo:
                    field = fieldInfo;
                    break;
                default:
                    break;
            }
        }

        try
        {
            prop ??= FindProperty(binder);
            if (prop is not null)
            {
                var propValue = prop.GetValue(_instance);
                result = propValue is null ? null : FullAccessDynamicFactory.Create(prop.PropertyType, propValue);
                _memberCache[key] = prop;
                return true;
            }
        }
        catch
        {
        }

        try
        {
            field ??= FindField(binder);
            if (field is not null)
            {
                var fieldValue = field.GetValue(_instance);
                result = fieldValue is null ? null : FullAccessDynamicFactory.Create(field.FieldType, fieldValue);
                _memberCache[key] = field;
                return true;
            }
        }
        catch
        {
        }

        // Before failing (which base.TryGetMember will do once we call it), check if the property or field we were searching for was not null
        // This means the property exists, but either _instance is null or the member's value is null
        if (_instance is null && (prop is not null || field is not null))
        {
            result = null;
            return true;
        }

        result = null;
        return false;
    }
    /// <inheritdoc/>
    public override bool TrySetMember(SetMemberBinder binder, object value)
    {
        if (_instance is null)
        {
            return false;
        }

        PropertyInfo prop = null;
        FieldInfo field = null;
        if (_memberCache.TryGetValue(binder.Name, out var member))
        {
            if (member is PropertyInfo propInfo)
            {
                prop = propInfo;
            }
            else if (member is FieldInfo fieldInfo)
            {
                field = fieldInfo;
            }
        }

        prop ??= FindProperty(binder);
        if (prop is not null)
        {
            prop.SetValue(_instance, value);
            return true;
        }

        field ??= FindField(binder);
        if (field is not null)
        {
            field.SetValue(_instance, value);
            return true;
        }

        return false;
    }
    /// <inheritdoc/>
    public override bool TryConvert(ConvertBinder binder, out object result)
    {
        if (binder.Type.IsAssignableFrom(_instanceType))
        {
            result = _instance;
            return true;
        }

        throw new InvalidCastException($"Cannot cast object of type '{_instanceType.FullName}' to '{binder.Type.FullName}'.");
    }
    /// <inheritdoc/>
    public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
    {
        if (_instance is null)
        {
            result = null;
            return false;
        }

        var itemProp = FindIndexer(binder, indexes);
        if (itemProp is not null)
        {
            var itemValue = itemProp.GetValue(_instance, indexes);
            result = itemValue is null
                ? null
                : (object)FullAccessDynamicFactory.Create(itemProp.PropertyType, itemValue);
            return true;
        }

        result = null;
        return false;
    }
    /// <inheritdoc/>
    public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
    {
        if (_instance is null)
        {
            return false;
        }

        var itemProp = FindIndexer(binder, indexes);
        if (itemProp is not null)
        {
            itemProp.SetValue(_instance, value, indexes);
            return true;
        }

        return false;
    }
    /// <inheritdoc/>
    public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
    {
        switch (_instance)
        {
            case MethodInfo methodInfo:
                result = methodInfo.Invoke(methodInfo.IsStatic ? null : _instance, args);
                return true;
            case Delegate delegateType:
                result = delegateType.DynamicInvoke(args);
                return true;
            default:
                throw new InvalidOperationException($"Object of type '{_instanceType.FullName}' is not invocable, expected '{typeof(MethodInfo).FullName}' or any '{typeof(Delegate)}'-like type.");
        }
    }
    /// <inheritdoc/>
    public override IEnumerable<string> GetDynamicMemberNames() => _instanceType.GetMembers(bindingFlags).Select(static p => p.Name);

    /// <summary>
    /// Returns the underlying <typeparamref name="T"/> instance.
    /// </summary>
    /// <returns>The underlying <typeparamref name="T"/> instance.</returns>
    public T Unwrap() => _instance;

    /// <summary>
    /// Executes a <see cref="Func{T, TResult}"/> that is passed each <see cref="BindingFlags"/> value in order of preference, and returns the result of the first non-null invocation.
    /// </summary>
    /// <param name="func">The function to execute.</param>
    /// <returns>The result of the first non-null invocation of <paramref name="func"/> or <see langword="null"/> if all invocations return <see langword="null"/>.</returns>
    private static TMember GetFirstNonNull<TMember>(Func<BindingFlags, TMember> func)
    {
        var member =
            func(publicStatic)
            ?? func(anyStatic)
            ?? func(publicInstance)
            ?? func(anyInstance)
            ?? func(bindingFlags | BindingFlags.FlattenHierarchy);

        return member;
    }
    private MethodInfo FindMethod(InvokeMemberBinder binder, object[] args)
    {
        var targetType = _instanceType;
        while (targetType != null)
        {
            var method = GetFirstNonNull(bf => targetType.GetMethod(binder.Name, bf, null, Array.ConvertAll(args, item => item.GetType()), null));
            if (method is not null)
            {
                return method;
            }
            targetType = targetType.BaseType;
        }
        return null;
    }
    private FieldInfo FindField(GetMemberBinder binder)
    {
        var targetType = _instanceType;
        while (targetType != null)
        {
            var field = GetFirstNonNull(bf => targetType.GetField(binder.Name, bf));
            if (field is not null)
            {
                return field;
            }
            targetType = targetType.BaseType;
        }
        return null;
    }
    private PropertyInfo FindProperty(GetMemberBinder binder)
    {
        var targetType = _instanceType;
        while (targetType != null)
        {
            var property = GetFirstNonNull(bf => targetType.GetProperty(binder.Name, bf));
            if (property is not null)
            {
                return property;
            }
            targetType = targetType.BaseType;
        }
        return null;
    }
    private FieldInfo FindField(SetMemberBinder binder)
    {
        var targetType = _instanceType;
        while (targetType != null)
        {
            var field = GetFirstNonNull(bf => targetType.GetField(binder.Name, bf));
            if (field is not null)
            {
                return field;
            }
            targetType = targetType.BaseType;
        }
        return null;
    }
    private PropertyInfo FindProperty(SetMemberBinder binder)
    {
        var targetType = _instanceType;
        while (targetType != null)
        {
            var property = GetFirstNonNull(bf => targetType.GetProperty(binder.Name, bf));
            if (property is not null)
            {
                return property;
            }
            targetType = targetType.BaseType;
        }
        return null;
    }
    private PropertyInfo FindIndexer(GetIndexBinder binder, object[] indexes)
    {
        var targetType = _instanceType;
        while (targetType != null)
        {
            var indexer = GetFirstNonNull(bf => targetType.GetProperty("Item", bf, null, binder.ReturnType, Array.ConvertAll(indexes, item => item.GetType()), null));
            if (indexer is not null)
            {
                return indexer;
            }
            targetType = targetType.BaseType;
        }
        return null;
    }
    private PropertyInfo FindIndexer(SetIndexBinder binder, object[] indexes)
    {
        var targetType = _instanceType;
        while (targetType != null)
        {
            var indexer = GetFirstNonNull(bf => targetType.GetProperty("Item", bf, null, binder.ReturnType, Array.ConvertAll(indexes, item => item.GetType()), null));
            if (indexer is not null)
            {
                return indexer;
            }
            targetType = targetType.BaseType;
        }
        return null;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj) => Equals(Unwrap(), obj);
    /// <inheritdoc/>
    public static bool operator ==(FullAccessDynamic<T> left, object right) => Equals(left.Unwrap(), right);
    /// <inheritdoc/>
    public static bool operator !=(FullAccessDynamic<T> left, object right) => !(left == right);
    /// <inheritdoc/>
    public override int GetHashCode() => Unwrap()?.GetHashCode() ?? 0;
    /// <inheritdoc/>
    public override string ToString() => _instance.ToString();
}
