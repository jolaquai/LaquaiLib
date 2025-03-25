using System.Dynamic;
using System.Reflection;

namespace LaquaiLib.Util.DynamicExtensions.FullAccessDynamic;

/// <summary>
/// Represents a dynamic object that allows access to all properties and methods of the wrapped object as if they were <see langword="public"/>, regardless of their actual access level. All binding failures and binding-related exceptions are swallowed and will result in <see langword="null"/> returns.
/// <para/>Note that all dynamically retrieved members are also instances of <see cref="SilentFullAccessDynamic{T}"/> to allow for further dynamic access. The only value explicitly propagated to allow <c>?.</c> <see langword="null"/> propagation is <see langword="null"/>. This does <b>not</b> work directly on an object of type <see cref="SilentFullAccessDynamic{T}"/>, i.e. the following method invocation will always take place:
/// <code language="csharp">
/// MyClass? myInstance = null;
/// var mySilentFullAccessDynamic = SilentFullAccessDynamic.Create(typeof(MyClass), myInstance);
/// // This incovation will happen no matter if the underlying object myInstance is null or not, because the null propagation will check the SilentFullAccessDynamic instance, rather than the underlying object
/// mySilentFullAccessDynamic?.MyMethod();
/// // These ones will not, however, if MyProperty is null or MyNullReturningMethod returns null
/// mySilentFullAccessDynamic.MyProperty?.MyMethod();
/// mySilentFullAccessDynamic.MyNullReturningMethod()?.MyMethod();
/// </code>
/// <para/><b>Warning!</b> Nothing prevents the underlying object instance of <typeparamref name="T"/> from being <see langword="null"/>. As such, <see cref="Unwrap"/> may return <see langword="null"/>.
/// </summary>
/// <typeparam name="T">The type of the object to wrap.</typeparam>
public class SilentFullAccessDynamic<T> : DynamicObject
{
    private readonly T _instance;
    private readonly Type _instanceType = typeof(T);
    private const BindingFlags publicInstance = BindingFlags.Instance | BindingFlags.Public;
    private const BindingFlags anyInstance = BindingFlags.Instance | BindingFlags.NonPublic;
    private const BindingFlags publicStatic = BindingFlags.Static | BindingFlags.Public;
    private const BindingFlags anyStatic = BindingFlags.Static | BindingFlags.NonPublic;
    private const BindingFlags bindingFlags = publicInstance | anyInstance | publicStatic | anyStatic;

    private static readonly Dictionary<string, MemberInfo> _memberCache = [];

    internal SilentFullAccessDynamic()
    {
        _instance = Activator.CreateInstance<T>();
    }
    internal SilentFullAccessDynamic(T instance)
    {
        _instance = instance;
    }
    /// <inheritdoc/>
    public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
    {
        if (_instance is null)
        {
            result = null;
            return true;
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
        var key = _instanceType.Namespace + '.' + _instanceType.Name + '.' + binder.Name + '(' + string.Join(", ", Array.ConvertAll(args, item =>
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
            method = GetFirstNonNull(bf => _instanceType.GetMethod(binder.Name, bf, null, Array.ConvertAll(args, item => item.GetType()), null));
            _memberCache[key] = method;
        }

        if (method is not null)
        {
            result = SilentFullAccessDynamicFactory.Create(method.ReturnType, method.Invoke(_instance, args));
            return true;
        }

        result = null;
        return true;
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
            prop ??= GetFirstNonNull(bf => _instanceType.GetProperty(binder.Name, bf));
            if (prop is not null)
            {
                var propValue = prop.GetValue(_instance);
                result = propValue is null ? null : (object)SilentFullAccessDynamicFactory.Create(prop.PropertyType, propValue);
                _memberCache[key] = prop;
                return true;
            }
        }
        catch
        {
        }

        try
        {
            field ??= GetFirstNonNull(bf => _instanceType.GetField(binder.Name, bf));
            if (field is not null)
            {
                var fieldValue = field.GetValue(_instance);
                result = fieldValue is null ? null : (object)SilentFullAccessDynamicFactory.Create(field.FieldType, fieldValue);
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
        return true;
    }
    /// <inheritdoc/>
    public override bool TrySetMember(SetMemberBinder binder, object value)
    {
        if (_instance is null)
        {
            return true;
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

        prop ??= GetFirstNonNull(bf => _instanceType.GetProperty(binder.Name, bf));
        if (prop is not null)
        {
            prop.SetValue(_instance, value);
            return true;
        }

        field ??= GetFirstNonNull(bf => _instanceType.GetField(binder.Name, bf));
        if (field is not null)
        {
            field.SetValue(_instance, value);
            return true;
        }

        return true;
    }
    /// <inheritdoc/>
    public override bool TryConvert(ConvertBinder binder, out object result)
    {
        if (binder.Type.IsAssignableFrom(_instanceType))
        {
            result = _instance;
            return true;
        }

        result = null;
        return true;
        //throw new InvalidCastException($"Cannot cast object of type '{_instanceType.FullName}' to '{binder.Type.FullName}'.");
    }
    /// <inheritdoc/>
    public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
    {
        if (_instance is null)
        {
            result = null;
            return true;
        }

        var itemProp = GetFirstNonNull(bf => _instanceType.GetProperty("Item", bf, null, binder.ReturnType, Array.ConvertAll(indexes, item => item.GetType()), null));
        if (itemProp is not null)
        {
            var itemValue = itemProp.GetValue(_instance, indexes);
            result = itemValue is null ? null : (object)SilentFullAccessDynamicFactory.Create(itemProp.PropertyType, itemValue);
            return true;
        }

        result = null;
        return true;
    }
    /// <inheritdoc/>
    public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
    {
        if (_instance is null)
        {
            return true;
        }

        var itemProp = GetFirstNonNull(bf => _instanceType.GetProperty("Item", bf, null, binder.ReturnType, Array.ConvertAll(indexes, item => item.GetType()), null));
        if (itemProp is not null)
        {
            itemProp.SetValue(_instance, value, indexes);
            return true;
        }

        return true;
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
                result = null;
                return true;
        }
    }
    /// <inheritdoc/>
    public override IEnumerable<string> GetDynamicMemberNames() => _instanceType.GetProperties(bindingFlags).Select(static p => p.Name);

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
        return func(publicStatic)
            ?? func(anyStatic)
            ?? func(publicInstance)
            ?? func(anyInstance);
    }
    /// <inheritdoc/>
    public override bool Equals(object obj) => Equals(Unwrap(), obj);
    /// <inheritdoc/>
    public static bool operator ==(SilentFullAccessDynamic<T> left, object right) => Equals(left.Unwrap(), right);
    /// <inheritdoc/>
    public static bool operator !=(SilentFullAccessDynamic<T> left, object right) => !(left == right);
    /// <inheritdoc/>
    public override int GetHashCode() => Unwrap()?.GetHashCode() ?? 0;
}
