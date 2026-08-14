using System.Reflection;

namespace LaquaiLib.Extensions;

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
    }
}
