using LaquaiLib.Collections;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extensions for all Types.
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
        /// Changes the type of a reference to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to cast <paramref name="obj"/> to.</typeparam>
        /// <returns><paramref name="obj"/> reinterpreted as an instance of <typeparamref name="T"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T As<T>() where T : class => Unsafe.As<T>(obj);
        /// <summary>
        /// Gets a pinnable reference to the underlying data of <paramref name="obj"/>.
        /// <para/>If <paramref name="obj"/> is <see langword="null"/>, the reference is a <see langword="null"/> reference.
        /// <para/>If <paramref name="obj"/> is a non-zero-length <see langword="string"/>, the reference points to the first character of the string.
        /// <para/>If <paramref name="obj"/> is a non-zero-length array, the reference points to the first element of the array.
        /// <para/>In any other case, the reference points to the first field of the object.
        /// </summary>
        /// <returns>The pinnable reference to the underlying data of <paramref name="obj"/>, retyped as <typeparamref name="T"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetPinnableReference<T>() where T : unmanaged
        {
            if (obj is null)
                return ref Unsafe.NullRef<T>();
            if (obj is string s)
            {
                if (s.Length == 0)
                    return ref Unsafe.NullRef<T>();
                return ref Unsafe.As<char, T>(ref Unsafe.AsRef(in s.GetPinnableReference()));
            }
            if (obj is Array a)
            {
                if (a.Length == 0)
                    return ref Unsafe.NullRef<T>();
                return ref Unsafe.As<byte, T>(ref MemoryMarshal.GetArrayDataReference(a));
            }
            return ref Unsafe.As<byte, T>(ref Unsafe.As<RawData>(obj).Zero);
        }
    }
    private sealed class RawData { public byte Zero; }
}
