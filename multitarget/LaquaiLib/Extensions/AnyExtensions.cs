using LaquaiLib.Collections;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides Extension Methods for all Types.
/// </summary>
public static class AnyExtensions
{
    private static readonly ConditionalWeakTable<object, ISet<object>> _annotations = [];

    extension<T>(T source) where T : class
    {
        /// <summary>
        /// Gets an <see cref="ISet{T}"/> that can be used to store annotations for the given value.
        /// Values are always compared using simple reference equality.
        /// All operations on the set are thread-safe. All annotation objects are weakly referenced and stay alive only as long as the value is alive.
        /// The return value of this extension property is never <see langword="null"/>; if no annotation set exists for the extended value, a new one is created.
        /// </summary>
        public ISet<object> Annotations => _annotations.GetValue(source, _ => new ConcurrentSet<object>());
    }
    extension(object obj)
    {
        /// <summary>
        /// Changes the type of a reference to a <see langword="class"/> instance to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to cast <paramref name="obj"/> to.</typeparam>
        /// <param name="obj">The <see cref="object"/> to cast.</param>
        /// <returns><paramref name="obj"/> reinterpreted as an instance of <typeparamref name="T"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T As<T>() where T : class => Unsafe.As<T>(obj);
    }
}
