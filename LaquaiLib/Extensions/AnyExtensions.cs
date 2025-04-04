using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides Extension Methods for all Types.
/// </summary>
public static class AnyExtensions
{
    /// <summary>
    /// Checks whether a number of objects are all equal to each other. If any of the passed objects are <see langword="null"/>, all others must also be <see langword="null"/>.
    /// Comparing to an empty collection is considered equal.
    /// </summary>
    /// <typeparam name="T">The Type of the objects to compare.</typeparam>
    /// <param name="source">The first object to use for the comparison.</param>
    /// <param name="other">The remaining objects to use for the comparison.</param>
    /// <returns><see langword="true"/> if all passed objects are equal, otherwise <see langword="false"/>.</returns>
    public static bool AllEqual<T>(this T source, params ReadOnlySpan<T> other)
    {
        if (other.Length == 0)
        {
            return true;
        }

        for (var i = 0; i < other.Length; i++)
        {
            var elem = other[i];
            if (source is null && elem is not null)
            {
                return false;
            }

            if (!source.Equals(elem))
            {
                return false;
            }
        }
        return true;
    }
    /// <summary>
    /// Checks whether a number of objects are all equal to each other. If any of the passed objects are <see langword="null"/>, all others must also be <see langword="null"/>.
    /// Comparing to an empty collection is considered equal.
    /// </summary>
    /// <typeparam name="T">The Type of the objects to compare.</typeparam>
    /// <param name="source">The first object to use for the comparison.</param>
    /// <param name="enumerable">The objects to use for the comparison. Enumeration will cease if an object is encountered that is not equal to <paramref name="source"/>.</param>
    /// <returns><see langword="true"/> if all passed objects are equal, otherwise <see langword="false"/>.</returns>
    public static bool AllEqual<T>(this T source, IEnumerable<T> enumerable)
    {
        // Carry the extra bool around to check for an empty enumerable
        var compared = false;
        foreach (var elem in enumerable)
        {
            compared = true;
            if (source is null && elem is not null)
            {
                return false;
            }
            else if (!source.Equals(elem))
            {
                return false;
            }
        }
        return compared;
    }

    /// <summary>
    /// Invokes a <paramref name="transform"/> function on a <paramref name="source"/> and any <paramref name="other"/> objects and checks whether the results are all equal to each other. If any of the passed objects are <see langword="null"/>, all others must also be <see langword="null"/>. In this case, <paramref name="transform"/> is never invoked.
    /// </summary>
    /// <typeparam name="T">The Type of the input objects.</typeparam>
    /// <typeparam name="TCompare">The Type of the results <paramref name="transform"/> yields.</typeparam>
    /// <param name="source">The first object to use for the comparison.</param>
    /// <param name="transform">The transform function to invoke on each object before performing the comparison.</param>
    /// <param name="other">The remaining objects to use for the comparison.</param>
    /// <returns><see langword="true"/> if all the results produced by <paramref name="transform"/> are all equal, otherwise <see langword="false"/>.</returns>
    public static bool EqualBy<T, TCompare>(this T source, Func<T, TCompare> transform, params ReadOnlySpan<T> other)
    {
        if (other.Length == 0)
        {
            return true;
        }

        var sourceTransformed = transform(source);
        for (var i = 0; i < other.Length; i++)
        {
            var elem = other[i];
            if (sourceTransformed is null && elem is not null)
            {
                return false;
            }

            if (!sourceTransformed.Equals(elem))
            {
                return false;
            }
        }
        return true;
    }
    /// <summary>
    /// Invokes a <paramref name="transform"/> function on a <paramref name="source"/> and any other objects and checks whether the results are all equal to each other. If any of the passed objects are <see langword="null"/>, all others must also be <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the input objects.</typeparam>
    /// <typeparam name="TCompare">The Type of the results <paramref name="transform"/> yields.</typeparam>
    /// <param name="source">The first object to use for the comparison.</param>
    /// <param name="transform">The transform function to invoke on each object before performing the comparison.</param>
    /// <param name="enumerable">The remaining objects to use for the comparison.</param>
    /// <returns><see langword="true"/> if all the results produced by <paramref name="transform"/> are all equal, otherwise <see langword="false"/>.</returns>
    public static bool EqualBy<T, TCompare>(this T source, Func<T, TCompare> transform, IEnumerable<T> enumerable)
    {
        // Carry the extra bool around to check for an empty enumerable
        var compared = false;
        var sourceTransformed = transform(source);
        foreach (var elem in enumerable)
        {
            compared = true;
            var elemTransformed = transform(elem);
            if (sourceTransformed is null && elemTransformed is not null)
            {
                return false;
            }
            else if (!sourceTransformed.Equals(elemTransformed))
            {
                return false;
            }
        }
        return compared;
    }

    /// <summary>
    /// Checks whether a given input object is <see langword="null"/>. If not, it is marked to the compiler as non-<see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the input object.</typeparam>
    /// <param name="source">The input object.</param>
    /// <returns><see langword="true"/> if <paramref name="source"/> is <see langword="null"/>, otherwise <see langword="false"/>.</returns>
    public static bool IsNull<T>([NotNullWhen(false)] this T source) => source is null;

    /// <summary>
    /// Invokes an <paramref name="action"/> that is passed the <paramref name="source"/> object.
    /// </summary>
    /// <typeparam name="T">The Type of the object to execute the <paramref name="action"/> on.</typeparam>
    /// <param name="source">The object to execute the <paramref name="action"/> on.</param>
    /// <param name="action">The action to execute on the <paramref name="source"/> object.</param>
    /// <returns>A reference to <paramref name="source"/> itself after <paramref name="action"/> has returned.</returns>
    /// <remarks>
    /// While not tremendously useful, this method can be used to effectively limit variable scopes or chain calls to the same object like when using a builder pattern.
    /// </remarks>
    public static T With<T>(this T source, Action<T> action)
    {
        action(source);
        return source;
    }
    /// <summary>
    /// Invokes an asynchronous <paramref name="action"/> that is passed the <paramref name="source"/> object.
    /// </summary>
    /// <typeparam name="T">The Type of the object to execute the <paramref name="action"/> on.</typeparam>
    /// <param name="source">The object to execute the <paramref name="action"/> on.</param>
    /// <param name="action">The action to execute on the <paramref name="source"/> object.</param>
    /// <returns>A reference to <paramref name="source"/> itself after <paramref name="action"/> has returned.</returns>
    /// <remarks>
    /// While not tremendously useful, this method can be used to effectively limit variable scopes or chain calls to the same object like when using a builder pattern.
    /// </remarks>
    public static async Task<T> With<T>(this T source, Func<T, Task> action)
    {
        await action(source).ConfigureAwait(false);
        return source;
    }

    /// <summary>
    /// Changes the type of a reference to a <see langword="class"/> instance to <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to cast <paramref name="obj"/> to.</typeparam>
    /// <param name="obj">The <see cref="object"/> to cast.</param>
    /// <returns><paramref name="obj"/> reinterpreted as an instance of <typeparamref name="T"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T As<T>(this object obj) where T : class => System.Runtime.CompilerServices.Unsafe.As<T>(obj);

    private static readonly MethodInfo _deepCopyTemplate = typeof(AnyExtensions).GetMethod(nameof(DeepCopy), BindingFlags.Static | BindingFlags.Public);
    private static readonly Type _typeofString = typeof(string);
    private static readonly Type _typeofObject = typeof(object);
    /// <summary>
    /// Creates an exact copy of an object, including all nested objects, that is independent of the original object.
    /// </summary>
    /// <typeparam name="T">The Type of the object to copy.</typeparam>
    /// <param name="source">The object to copy.</param>
    /// <param name="depth">The maximum recursion depth to copy nested objects.</param>
    /// <param name="useRoundTripSerialization">Whether to use a serialization-deserialization round trip to create the copy instead of employing reflection. Note that this may behave differently for certain types if certain fields or properties are not serializable or were marked with <see cref="NonSerializedAttribute"/>, <see cref="JsonIgnoreAttribute"/> or similar.</param>
    /// <returns>An exact copy of <paramref name="source"/>.</returns>
    public static T DeepCopy<T>(this T source, int depth, bool useRoundTripSerialization = false)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (depth < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(depth), "Reached maximum recursion depth.");
        }

        if (source is null)
        {
            return (T)(object)null;
        }

        if (source is ICloneable cloneable)
        {
            return (T)cloneable.Clone();
        }

        var typeofT = typeof(T);
        T clone;
    getUninitializedInstance:
        if (typeofT.IsValueType)
        {
            System.Runtime.CompilerServices.Unsafe.SkipInit(out clone);
        }
        else if (typeofT.IsInterface || typeofT.IsAbstract)
        {
            typeofT = source.GetType();
            goto getUninitializedInstance;
        }
        else
        {
            clone = (T)RuntimeHelpers.GetUninitializedObject(typeofT);
        }

        var fields = typeofT.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToHashSet();
        var baseType = typeofT.BaseType;
        while (baseType != _typeofObject)
        {
            fields.UnionWith(baseType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
            baseType = baseType.BaseType;
        }
        var fieldMap = fields.ToDictionary(f => f, f => f.DeclaringType);

        foreach (var (field, asType) in fieldMap)
        {
            object assignValue = null;
            if (field.FieldType.IsArray)
            {
                var arrayUnderlyingType = field.FieldType.GetElementType();
                // Allocate an array of the same type and deep copy each element
                if (arrayUnderlyingType.IsValueType || arrayUnderlyingType == _typeofString)
                {
                    // ...except for structs because of copy assignment
                    var array = field.GetValue(source) as Array;
                    if (array is not null)
                    {
                        assignValue = array.Clone();
                    }
                }
                else
                {
                    var array = (Array)field.GetValue(source);
                    var arrayClone = (Array)array.Clone();
                    // Since we can't make assumptions about the rank or length(s) of the Array, we have to defensively iterate the entire thing
                    using (var read = array.GetReadOnlySpanProvider<object>())
                    using (var write = arrayClone.GetSpanProvider<object>())
                    {
                        var readSpan = read.ReadOnlySpan;
                        var writeSpan = write.Span;

                        for (var i = 0; i < readSpan.Length; i++)
                        {
                            writeSpan[i] = DeepCopy(readSpan[i], depth - 1);
                        }
                    }

                    assignValue = arrayClone;
                }
            }
            else if (field.FieldType.IsValueType || field.FieldType == _typeofString)
            {
                assignValue = field.GetValue(source);
            }
            else
            {
                var value = field.GetValue(source);
                var generic = _deepCopyTemplate.MakeGenericMethod(field.FieldType);
                assignValue = generic.Invoke(null, [value, depth - 1, useRoundTripSerialization]);
            }

            if (typeofT != asType)
            {
                var setter = CreateFieldSetter(field);
                setter(clone, assignValue);
            }
            else
            {
                field.SetValue(clone, assignValue);
            }
        }

        return clone;
    }
    private static Action<object, object> CreateFieldSetter(FieldInfo field)
    {
        ArgumentNullException.ThrowIfNull(field);

        var dm = new DynamicMethod(
            "Set" + field.Name,
            null,
            [typeof(object), typeof(object)],
            field.DeclaringType,
            true);

        var il = dm.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Castclass, field.DeclaringType);
        il.Emit(OpCodes.Ldarg_1);
        if (field.FieldType.IsValueType)
        {
            il.Emit(OpCodes.Unbox_Any, field.FieldType);
        }
        else
        {
            il.Emit(OpCodes.Castclass, field.FieldType);
        }

        il.Emit(OpCodes.Stfld, field);
        il.Emit(OpCodes.Ret);

        return dm.CreateDelegate<Action<object, object>>();
    }
}
