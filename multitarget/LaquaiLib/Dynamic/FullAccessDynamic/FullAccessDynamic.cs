using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

using LaquaiLib.Extensions;

namespace LaquaiLib.Dynamic;

/// <summary>
/// Represents a dynamic object that allows access to all properties and methods of the wrapped object as if they were <see langword="public"/>, regardless of their actual access level.
/// <para/>For more information on this type, see the readme in the repository.
/// <para/><b>Warning!</b> Nothing prevents the underlying object instance of <typeparamref name="T"/> from being <see langword="null"/>. As such, <see cref="Unwrap"/> may return <see langword="null"/>.
/// </summary>
/// <typeparam name="T">The type of the object to wrap.</typeparam>
[Obsolete($"{nameof(FullAccessDynamic<>)} has been obsoleted in favor of the source generator using {nameof(LaquaiLib.Analyzers.Shared.Attributes.FullAccessProxyAttribute)}.")]
public class FullAccessDynamic<T> : DynamicObject, IEquatable<FullAccessDynamic<T>>, IEquatable<T>
{
    private static readonly ConcurrentDictionary<string, MemberInfo> _memberCache = [];
    private static readonly ConcurrentDictionary<MethodInfo, Delegate> _delegateCache = [];

    private readonly T _instance;
    private readonly Type _instanceType = typeof(T);
    private const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    internal FullAccessDynamic() : this(Activator.CreateInstance<T>()) { }
    internal FullAccessDynamic(T instance) => _instance = instance;
    /// <inheritdoc/>
    public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
    {
        // Proxy all the object-inherited methods back to the instance
        switch (binder.Name)
        {
            case "ToString":
                result = _instance?.ToString();
                return true;
            case "GetHashCode":
                result = _instance?.GetHashCode();
                return true;
            case "Equals" when args is not null and { Length: 1 }:
                result = _instance?.Equals(args[0]);
                return true;
            case "Unwrap":
                result = _instance;
                return true;
        }

        // Attempt to find the method with the specified name and parameter types.
        var key = $"{_instanceType.Namespace}.{_instanceType.Name}.{binder.Name}({string.Join(',', args.Select(o => o.GetType().GetFriendlyName()))})";
        MethodInfo method;
        if (_memberCache.TryGetValue(key, out var value) && value is MethodInfo methodInfo)
        {
            method = methodInfo;
        }
        else
        {
            var targetType = _instanceType;
            var argTypes = args.Length == 0 ? Type.EmptyTypes : Array.ConvertAll(args, item => item.GetType());
            method = null;
            while (targetType is not null && method is null)
            {
                method = targetType.GetMethod(binder.Name, bindingFlags, null, argTypes, null);
                method ??= targetType.GetMethod(binder.Name, bindingFlags | BindingFlags.FlattenHierarchy, null, argTypes, null);
                targetType = targetType.BaseType;
            }
        }

        if (method is null)
        {
            result = null;
            return false;
        }
        _memberCache[key] = method;

        if (!method.IsStatic && _instance is null)
        {
            throw new NullReferenceException($"Cannot invoke instance method '{method.Name}' on a null instance of type '{_instanceType.FullName}'.");
        }

        result = FullAccessDynamicFactory.Create(method.ReturnType, method.Invoke(method.IsStatic ? null : _instance, args));
        return true;
    }
    /// <inheritdoc/>
    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
        var key = _instanceType.Namespace + '.' + _instanceType.Name + '.' + binder.Name;

        if (_memberCache.TryGetValue(key, out var member))
        {
            goto memberAssigned;
        }

        var members = _instanceType.GetMember(binder.Name, bindingFlags);
        if (members?.Length is 0 or null)
        {
            members = _instanceType.GetMember(binder.Name, bindingFlags | BindingFlags.FlattenHierarchy);
        }
        switch (members.Length)
        {
            case 0:
                result = null;
                return false;
            case > 1:
                throw new AmbiguousMatchException($"The member '{binder.Name}' is ambiguous in type '{_instanceType.FullName}'.");
        }

        member = members[0];
        memberAssigned:
        switch (member)
        {
            case PropertyInfo propInfo:
            {
                result = GetProp(propInfo);
                return result is not null;
            }
            case FieldInfo fieldInfo:
            {
                result = GetField(fieldInfo);
                return result is not null;
            }
            case MethodInfo methodInfo:
            {
                result = GetMethodDelegate(methodInfo, key);
                return result is not null;
            }
        }

        result = null;
        return true;

        object GetProp(PropertyInfo prop)
        {
            var getter = prop.GetGetMethod(true);
            if (getter is null)
            {
                // I've decided against making this a binding failure since, technically, the binding was successful, the property exists, but it just has no getter
                throw new MissingMethodException($"The property '{prop.Name}' in type '{_instanceType.FullName}' does not have a getter.");
            }

            if (!getter.IsStatic && _instance is null)
            {
                throw new NullReferenceException($"Cannot invoke instance property getter '{getter.Name}' on a null instance of type '{_instanceType.FullName}'.");
            }

            var propValue = prop.GetValue(getter.IsStatic ? null : _instance);
            var result = FullAccessDynamicFactory.Create(prop.PropertyType, propValue);
            _memberCache[key] = prop;
            return result;
        }

        object GetField(FieldInfo field)
        {
            if (!field.IsStatic && _instance is null)
            {
                throw new NullReferenceException($"Cannot get value of instance field '{field.Name}' on a null instance of type '{_instanceType.FullName}'.");
            }

            var fieldValue = field.GetValue(field.IsStatic ? null : _instance);
            var result = FullAccessDynamicFactory.Create(field.FieldType, fieldValue);
            _memberCache[key] = field;
            return result;
        }
    }
    /// <inheritdoc/>
    public override bool TrySetMember(SetMemberBinder binder, object value)
    {
        var key = _instanceType.Namespace + '.' + _instanceType.Name + '.' + binder.Name;

        if (_memberCache.TryGetValue(key, out var member))
        {
            goto memberAssigned;
        }

        var members = _instanceType.GetMember(binder.Name, bindingFlags);
        if (members?.Length is 0 or null)
        {
            members = _instanceType.GetMember(binder.Name, bindingFlags | BindingFlags.FlattenHierarchy);
        }
        switch (members.Length)
        {
            case 0:
                return false;
            case > 1:
                throw new AmbiguousMatchException($"The member '{binder.Name}' is ambiguous in type '{_instanceType.FullName}'.");
        }

        member = members[0];
        memberAssigned:
        switch (member)
        {
            case PropertyInfo propInfo:
            {
                SetProp(propInfo);
                return true;
            }
            case FieldInfo fieldInfo:
            {
                SetField(fieldInfo);
                return true;
            }
            case MethodInfo:
            {
                // Still cache the MethodInfo to prevent future reflection lookups
                _memberCache[key] = member;
                throw new InvalidOperationException($"Cannot set member '{binder.Name}' of type '{_instanceType.FullName}' because it resolved to a method.");
            }
        }

        return false;

        void SetProp(PropertyInfo prop)
        {
            var setter = prop.GetSetMethod(true);
            if (setter is null)
            {
                // I've decided against making this a binding failure since, technically, the binding was successful, the property exists, but it just has no getter
                throw new MissingMethodException($"The property '{prop.Name}' in type '{_instanceType.FullName}' does not have a sgetter.");
            }
            if (!setter.IsStatic && _instance is null)
            {
                throw new NullReferenceException($"Cannot invoke instance property setter '{setter.Name}' on a null instance of type '{_instanceType.FullName}'.");
            }

            prop.SetValue(setter.IsStatic ? null : _instance, value);
            _memberCache[key] = prop;
        }

        void SetField(FieldInfo field)
        {
            if (!field.IsStatic && _instance is null)
            {
                throw new NullReferenceException($"Cannot set value of instance field '{field.Name}' on a null instance of type '{_instanceType.FullName}'.");
            }

            field.SetValue(field.IsStatic ? null : _instance, value);
            _memberCache[key] = field;
        }
    }
    /// <inheritdoc/>
    public override bool TryConvert(ConvertBinder binder, out object result)
    {
        if (_instanceType.IsAssignableTo(binder.Type))
        {
            result = _instance;
            return true;
        }
        if (binder.Type == typeof(FullAccessDynamic<T>))
        {
            result = this;
            return true;
        }

        throw new InvalidCastException($"Cannot cast object of type '{_instanceType.FullName}' to '{binder.Type.FullName}'.");
    }
    /// <inheritdoc/>
    public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
    {
        // Static indexers don't exist
        if (_instance is null)
        {
            result = null;
            return false;
        }

        var itemProp = FindIndexer(indexes);
        if (itemProp is not null)
        {
            var itemValue = itemProp.GetValue(_instance, indexes);
            result = FullAccessDynamicFactory.Create(itemProp.PropertyType, itemValue);
            return true;
        }

        result = null;
        return false;
    }
    /// <inheritdoc/>
    public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
    {
        // Static indexers don't exist
        if (_instance is null)
        {
            return false;
        }

        var itemProp = FindIndexer(indexes);
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
            case null:
                throw new NullReferenceException($"Cannot perform an invocation on a null instance of type '{_instanceType.FullName}' (and invocations on a type have no meaning).");
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

    // We can cache these since we're not like ExpandoObject
    private HashSet<string> _dynamicMemberNames;
    /// <inheritdoc/>
    public override IEnumerable<string> GetDynamicMemberNames() => _dynamicMemberNames ??= _instanceType.GetMembers(bindingFlags).Select(static p => p.Name).ToHashSet();

    /// <summary>
    /// Returns the underlying <typeparamref name="T"/> instance.
    /// </summary>
    /// <returns>The underlying <typeparamref name="T"/> instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Unwrap() => _instance;
    /// <summary>
    /// Gets the <see cref="System.Type"/> of the wrapped instance.
    /// </summary>
    public Type Type
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _instanceType;
    }

    /// <summary>
    /// Returns a new <see cref="FullAccessDynamic{T}"/> with the same underlying instance as the current instance, but with <typeparamref name="TCast"/> as the type argument.
    /// <typeparamref name="T"/> must be assignable to <typeparamref name="TCast"/>; otherwise, an <see cref="InvalidCastException"/> is thrown.
    /// </summary>
    /// <typeparam name="TCast">The type to cast the underlying instance to.</typeparam>
    /// <returns>The new <see cref="FullAccessDynamic{TCast}"/> instance with the same underlying instance as the current instance.</returns>
    public FullAccessDynamic<TCast> Cast<TCast>()
    {
        // If we have a value, we can check for the cast the easy way
        if (_instance is TCast || (_instance is null && _instanceType.IsAssignableTo(typeof(TCast))))
        {
            return FullAccessDynamicFactory.Create(typeof(TCast), _instance);
        }
        throw new InvalidCastException($"Cannot cast object of type '{_instanceType.FullName}' to '{typeof(TCast).FullName}'.");
    }

    /// <summary>
    /// Finds the correct <see cref="Delegate"/>-derived <see cref="Type"/> for the specified <see cref="MethodInfo"/>.
    /// </summary>
    private static Type GetDelegateType(MethodInfo method)
    {
        var paramTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();

        if (method.ReturnType == typeof(void))
        {
            return Expression.GetActionType(paramTypes);
        }
        else
        {
            return Expression.GetFuncType([.. paramTypes, method.ReturnType]);
        }
    }
    private Delegate GetMethodDelegate(MethodInfo methodInfo, string key)
    {
        if (!_delegateCache.TryGetValue(methodInfo, out var delg))
        {
            if (!methodInfo.IsStatic && _instance is null)
            {
                throw new InvalidOperationException($"Cannot create delegate for instance method '{methodInfo.Name}' on a null instance of type '{_instanceType.FullName}'.");
            }

            var delegateType = GetDelegateType(methodInfo);
            _memberCache[key] = methodInfo;
            delg = _delegateCache[methodInfo] = methodInfo.CreateDelegate(delegateType, methodInfo.IsStatic ? null : _instance);
        }

        return delg;
    }

    /// <summary>
    /// Finds an overloaded indexer property defined in the type of the wrapped instance using its argument's types.
    /// </summary>
    private PropertyInfo FindIndexer(object[] indexes)
    {
        var targetType = _instanceType;
        var indexTypes = Array.ConvertAll(indexes, item => item.GetType());
        while (targetType != null)
        {
            // The return type may not make it in here (as in, mangled to object), so try without a constrained return type
            var indexer = targetType.GetProperty("Item", bindingFlags, null, null, indexTypes, null);
            indexer ??= targetType.GetProperty("Item", bindingFlags | BindingFlags.FlattenHierarchy, null, null, indexTypes, null);
            if (indexer is not null)
            {
                return indexer;
            }
            targetType = targetType.BaseType;
        }
        return null;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object obj) => Equals(obj as FullAccessDynamic<T>) || (obj is T t && Equals(t));
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(T other) => _instance.Equals(other);
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(FullAccessDynamic<T> other) => _instance.Equals(other._instance);
    /// <summary>
    /// Determines whether the underlying value of the specified <see cref="FullAccessDynamic{T}"/> instance is equal to the specified object.
    /// </summary>
    /// <param name="left">The left <see cref="FullAccessDynamic{T}"/> instance.</param>
    /// <param name="right">The right object to compare against.</param>
    /// <returns><see langword="true"/> if the underlying value of the specified <see cref="FullAccessDynamic{T}"/> instance compares equal to the specified object; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(FullAccessDynamic<T> left, object right) => left.Equals(right);
    /// <summary>
    /// Determines whether the underlying values of the specified <see cref="FullAccessDynamic{T}"/> instances are equal.
    /// </summary>
    /// <param name="left">The left <see cref="FullAccessDynamic{T}"/> instance.</param>
    /// <param name="right">The right <see cref="FullAccessDynamic{T}"/> instance.</param>
    /// <returns><see langword="true"/> if the underlying values of the specified <see cref="FullAccessDynamic{T}"/> instances compare equal; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(FullAccessDynamic<T> left, FullAccessDynamic<T> right) => left.Equals(right);
    /// <summary>
    /// Determines whether the underlying value of the specified <see cref="FullAccessDynamic{T}"/> instance is equal to the specified value of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="left">The left <see cref="FullAccessDynamic{T}"/> instance.</param>
    /// <param name="right">The right value of type <typeparamref name="T"/>.</param>
    /// <returns><see langword="true"/> if the underlying value of the specified <see cref="FullAccessDynamic{T}"/> instance compares equal to the specified value of type <typeparamref name="T"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(FullAccessDynamic<T> left, T right) => left.Equals(right);
    /// <summary>
    /// Determines whether the underlying value of the specified <see cref="FullAccessDynamic{T}"/> instance is not equal to the specified object.
    /// </summary>
    /// <param name="left">The left <see cref="FullAccessDynamic{T}"/> instance.</param>
    /// <param name="right">The right object to compare against.</param>
    /// <returns><see langword="true"/> if the underlying value of the specified <see cref="FullAccessDynamic{T}"/> instance compares not equal to the specified object; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(FullAccessDynamic<T> left, object right) => !(left == right);
    /// <summary>
    /// Determines whether the underlying values of the specified <see cref="FullAccessDynamic{T}"/> instances are not equal.
    /// </summary>
    /// <param name="left">The left <see cref="FullAccessDynamic{T}"/> instance.</param>
    /// <param name="right">The right <see cref="FullAccessDynamic{T}"/> instance.</param>
    /// <returns><see langword="true"/> if the underlying values of the specified <see cref="FullAccessDynamic{T}"/> instances compare not equal; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(FullAccessDynamic<T> left, FullAccessDynamic<T> right) => !(left == right);
    /// <summary>
    /// Determines whether the underlying value of the specified <see cref="FullAccessDynamic{T}"/> instance is not equal to the specified value of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="left">The left <see cref="FullAccessDynamic{T}"/> instance.</param>
    /// <param name="right">The right value of type <typeparamref name="T"/>.</param>
    /// <returns><see langword="true"/> if the underlying value of the specified <see cref="FullAccessDynamic{T}"/> instance compares not equal to the specified value of type <typeparamref name="T"/>; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(FullAccessDynamic<T> left, T right) => !(left == right);
    /// <summary>
    /// Gets the hash code of the underlying instance or its <see cref="Type"/> if it is <see langword="null"/>.
    /// </summary>
    /// <returns>The hash code of the underlying instance or its <see cref="Type"/> if it is <see langword="null"/>.</returns>
    public override int GetHashCode() => _instance?.GetHashCode() ?? _instanceType.GetHashCode();
    /// <summary>
    /// Returns the result of the underlying instance's <see cref="T.ToString"/> method or <see langword="null"/> if the instance is <see langword="null"/>.
    /// </summary>
    /// <returns>The underlying instance's <see langword="string"/> representation or <see langword="null"/> if the instance is <see langword="null"/>.</returns>
    public override string ToString() => _instance?.ToString();
}
