using System.Reflection;

using LaquaiLib.Interfaces;

namespace LaquaiLib.Collections.Enumeration;

/// <summary>
/// Implements the enumerator pattern to enumerate the elements of a (potentially) multidimensional array sequentially.
/// Instances hold a tracked reference to the array instead of pinning it, so <see cref="Dispose"/> releases nothing; it only marks the instance unusable.
/// </summary>
/// <typeparam name="T">
/// The type to view the array's elements as. It must be the same size as the array's element type, which permits lateral reinterpretation such as viewing an enum array as its underlying type.
/// Use <see cref="Reinterpret(Array)"/> to view an array as a differently sized <typeparamref name="T"/>, or <see cref="ReinterpretUnsafe(Array)"/> to additionally view a reference array as a different reference type.
/// </typeparam>
public sealed class MultiDimArrayEnumerable<T> : IEnumerable<T>, ISpanProvider<T>
{
    private enum Relaxation
    {
        None,
        Size,
        SizeAndType
    }

    private readonly Array _array;
    private readonly int _length;
    private int _disposed;

    /// <summary>
    /// Initializes a new <see cref="MultiDimArrayEnumerable{T}"/> with the specified <paramref name="array"/>.
    /// </summary>
    /// <param name="array">The array to enumerate.</param>
    public MultiDimArrayEnumerable(Array array) : this(array, Relaxation.None)
    {
    }
    private MultiDimArrayEnumerable(Array array, Relaxation relaxation)
    {
        ArgumentNullException.ThrowIfNull(array);

        var elementType = array.GetType().GetElementType();
        var elementSize = RuntimeHelpers.SizeOf(elementType.TypeHandle);
        var targetSize = Unsafe.SizeOf<T>();

        if (elementType != typeof(T))
        {
            var targetTracked = RuntimeHelpers.IsReferenceOrContainsReferences<T>();
            // Viewing GC-tracked slots as raw data or the reverse makes the GC walk arbitrary values as object pointers
            if (targetTracked != ArrayElementLayout.ContainsReferences(elementType))
            {
                throw new ArgumentException($"{typeof(T)} and the array's element type {elementType} disagree on whether they contain managed references.", nameof(array));
            }

            if (targetTracked)
            {
                // Only plain reference slots have a layout independent of their type; reference-holding structs do not
                if (typeof(T).IsValueType || elementType.IsValueType)
                {
                    throw new ArgumentException($"{typeof(T)} and {elementType} hold managed references at possibly differing offsets and cannot be viewed as one another.", nameof(array));
                }
                if (relaxation != Relaxation.SizeAndType)
                {
                    throw new ArgumentException($"Viewing an array of {elementType} as {typeof(T)} defeats the store checks that array covariance performs. Use {nameof(ReinterpretUnsafe)} if that is intended.", nameof(array));
                }
            }
            else if (elementSize != targetSize && relaxation == Relaxation.None)
            {
                throw new ArgumentException($"An array of {elementSize}-byte elements cannot be viewed as {typeof(T)} ({targetSize} bytes). Use {nameof(Reinterpret)} to reinterpret the array's memory instead.", nameof(array));
            }
        }

        var length = (long)array.Length * elementSize / targetSize;
        if (length > int.MaxValue)
        {
            throw new ArgumentException($"The array viewed as {typeof(T)} would contain {length} elements, which exceeds the maximum length of a {nameof(Span<>)}.", nameof(array));
        }

        _array = array;
        _length = (int)length;
    }

    /// <summary>
    /// Initializes a new <see cref="MultiDimArrayEnumerable{T}"/> that views the memory of <paramref name="array"/> as <typeparamref name="T"/> even if the array's element type differs in size.
    /// The element count becomes the array's total byte length divided by the size of <typeparamref name="T"/>; a trailing partial element is not observable.
    /// Neither type may contain managed references.
    /// </summary>
    /// <param name="array">The array whose memory to reinterpret.</param>
    /// <returns>The created <see cref="MultiDimArrayEnumerable{T}"/>.</returns>
    public static MultiDimArrayEnumerable<T> Reinterpret(Array array) => new MultiDimArrayEnumerable<T>(array, Relaxation.Size);
    /// <summary>
    /// Initializes a new <see cref="MultiDimArrayEnumerable{T}"/> that additionally views an array of one reference type as an array of another, bypassing the store checks that array covariance would normally perform.
    /// Reading an element that is not actually a <typeparamref name="T"/>, or writing an element the underlying array cannot hold, is not detected here and will surface as a cast failure or corruption elsewhere.
    /// Viewing GC-tracked slots as raw data or the reverse remains rejected.
    /// </summary>
    /// <param name="array">The array whose elements to view as <typeparamref name="T"/>.</param>
    /// <returns>The created <see cref="MultiDimArrayEnumerable{T}"/>.</returns>
    public static MultiDimArrayEnumerable<T> ReinterpretUnsafe(Array array) => new MultiDimArrayEnumerable<T>(array, Relaxation.SizeAndType);

    private bool IsDisposed => Volatile.Read(ref _disposed) != 0;

    /// <summary>
    /// Gets a <see cref="Span{T}"/> over the entire array.
    /// </summary>
    public Span<T> Span
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, typeof(MultiDimArrayEnumerable<T>));
            return MemoryMarshal.CreateSpan(ref Unsafe.As<byte, T>(ref MemoryMarshal.GetArrayDataReference(_array)), _length);
        }
    }

    /// <summary>
    /// Returns an <see cref="Enumerator"/> over the elements of the array.
    /// </summary>
    /// <returns>The created <see cref="Enumerator"/>.</returns>
    public Enumerator GetEnumerator() => new Enumerator(this);
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Marks this <see cref="MultiDimArrayEnumerable{T}"/> as disposed.
    /// The target array is referenced rather than pinned, so nothing is released; subsequent access to <see cref="Span"/> or to any <see cref="Enumerator"/>'s <see cref="Enumerator.Current"/> throws.
    /// </summary>
    public void Dispose() => Volatile.Write(ref _disposed, 1);

    /// <summary>
    /// Enables enumeration of the items of the <see cref="Array"/> a <see cref="MultiDimArrayEnumerable{T}"/> wraps.
    /// </summary>
    public struct Enumerator : IEnumerator<T>
    {
        private readonly MultiDimArrayEnumerable<T> _parent;
        private readonly int _length;
        private int _index;

        internal Enumerator(MultiDimArrayEnumerable<T> parent)
        {
            _parent = parent;
            _length = parent._length;
            _index = -1;
        }

        /// <inheritdoc/>
        public bool MoveNext()
        {
            var next = _index + 1;
            if (next >= _length)
            {
                // Clamp instead of running away; repeated calls past the end must stay false
                _index = _length;
                return false;
            }
            _index = next;
            return true;
        }
        /// <inheritdoc/>
        public void Reset() => _index = -1;
        /// <inheritdoc/>
        public readonly T Current
        {
            get
            {
                ObjectDisposedException.ThrowIf(_parent.IsDisposed, typeof(MultiDimArrayEnumerable<T>));
                return (uint)_index < (uint)_length
                    ? Unsafe.Add(ref Unsafe.As<byte, T>(ref MemoryMarshal.GetArrayDataReference(_parent._array)), _index)
                    : throw new InvalidOperationException("Enumeration has either not started or has already finished.");
            }
        }
        readonly object IEnumerator.Current => Current;
        /// <inheritdoc/>
        public readonly void Dispose() { }
    }
}

static file class ArrayElementLayout
{
    private static readonly MethodInfo _isReferenceOrContainsReferences = typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.IsReferenceOrContainsReferences));
    private static readonly ConcurrentDictionary<Type, bool> _cache = [];

    // No non-generic equivalent of RuntimeHelpers.IsReferenceOrContainsReferences exists, so ask the runtime once per type
    public static bool ContainsReferences(Type type) => _cache.GetOrAdd(type, static t => !t.IsPointer
        && !t.IsFunctionPointer
        && (!t.IsValueType || (bool)_isReferenceOrContainsReferences.MakeGenericMethod(t).Invoke(null, null)));
}
