using System.Reflection;

namespace LaquaiLib.Extensions;

/// <inheritdoc/>
public static class RuntimeHelpersExtensions
{
    private static readonly MethodInfo _isReferenceOrContainsReferences = typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.IsReferenceOrContainsReferences));
    private static readonly ConcurrentDictionary<Type, bool> _cache = [];

    extension(RuntimeHelpers)
    {
        /// <summary>
        /// Calls <see cref="RuntimeHelpers.IsReferenceOrContainsReferences{T}"/> for the specified <paramref name="type"/> instance.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to check.</param>
        /// <returns><see langword="true"/> if the type is a reference type or contains references; otherwise, <see langword="false"/>.</returns>
        public static bool IsReferenceOrContainsReferences(Type type) => _cache.GetOrAdd(type, static t => !t.IsPointer
            && !t.IsFunctionPointer
            && (!t.IsValueType || (bool)_isReferenceOrContainsReferences.MakeGenericMethod(t).Invoke(null, null)));
        /// <summary>
        /// Calls <see cref="RuntimeHelpers.GetUninitializedObject(Type)"/> for the specified generic type parameter <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of object to create.</typeparam>
        /// <returns>An uninitialized object of type <typeparamref name="T"/>.</returns>
        public static T GetUninitializedObject<T>()
        {
            if (typeof(T).IsValueType)
                // Can't unbox any other way since Unsafe.Unbox<T>(object) is 'where T : struct'
                return default;
            var val = RuntimeHelpers.GetUninitializedObject(typeof(T));
            return Unsafe.As<object, T>(ref val);
        }
    }
}
