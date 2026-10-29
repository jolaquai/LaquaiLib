using System.Buffers;

namespace LaquaiLib.Extensions;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

/// <summary>
/// Provides extensions for the <see cref="Span{T}"/>, <see cref="ReadOnlySpan{T}"/>, <see cref="Memory{T}"/> and <see cref="ReadOnlyMemory{T}"/> types.
/// </summary>
public static partial class MemoryExtensions
{
    [StructLayout(LayoutKind.Auto)]
    private readonly struct MemoryMirror<T>
    {
        public readonly object Object;
        public readonly int Index;
        public readonly int Length;
    }
    private const int RemoveFlagsBitMask = 0x7FFFFFFF;

    extension<T>(in ReadOnlySpan<T> span)
    {
        /// <summary>
        /// Invokes the specified <paramref name="action"/> for each element in the <see cref="ReadOnlySpan{T}"/>.
        /// </summary>
        /// <param name="action">The <see cref="Action{T}"/> to invoke for each element.</param>
        /// <returns>The original <see cref="ReadOnlySpan{T}"/>.</returns>
        public ReadOnlySpan<T> ForEach(Action<T> action)
        {
            ArgumentNullException.ThrowIfNull(action);
            for (var i = 0; i < span.Length; i++)
                action(span[i]);
            return span;
        }
        /// <summary>
        /// Invokes the specified <paramref name="action"/> for each element in the <see cref="ReadOnlySpan{T}"/>, passing the element and its index.
        /// </summary>
        /// <param name="action">The <see cref="Action{T1, T2}"/> to invoke for each element.</param>
        /// <returns>The original <see cref="ReadOnlySpan{T}"/>.</returns>
        public ReadOnlySpan<T> ForEach(Action<T, int> action)
        {
            ArgumentNullException.ThrowIfNull(action);
            for (var i = 0; i < span.Length; i++)
                action(span[i], i);
            return span;
        }

        /// <summary>
        /// Splits the specified <paramref name="span"/> into the specified destination <see cref="Span{T}"/>s based on the given <paramref name="predicate"/>.
        /// </summary>
        /// <param name="whereTrue">The <see cref="Span{T}"/> that will contain all elements that match the given <paramref name="predicate"/>.</param>
        /// <param name="whereFalse">The <see cref="Span{T}"/> that will contain all elements that do not match the given <paramref name="predicate"/>.</param>
        /// <param name="predicate">The <see cref="Predicate{T}"/> that checks each element for a condition.</param>
        /// <remarks>
        /// <paramref name="whereTrue"/> and <paramref name="whereFalse"/>'s lengths are not checked against <paramref name="span"/>'s length.
        /// If they are too small, an <see cref="IndexOutOfRangeException"/> will be thrown by the runtime.
        /// </remarks>
        public void Split(Span<T> whereTrue, Span<T> whereFalse, Func<T, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(predicate);

            var trueIndex = 0;
            var falseIndex = 0;
            for (var i = 0; i < span.Length; i++)
                if (predicate(span[i]))
                {
                    whereTrue[trueIndex] = span[i];
                    trueIndex++;
                }
                else
                {
                    whereFalse[falseIndex] = span[i];
                    falseIndex++;
                }
        }
    }
    extension<T>(in Span<T> span)
    {
        /// <summary>
        /// Invokes the specified <paramref name="action"/> for each element in the <see cref="Span{T}"/>.
        /// </summary>
        /// <param name="action">The <see cref="Action{T}"/> to invoke for each element.</param>
        /// <returns>The original <see cref="Span{T}"/>.</returns>
        public Span<T> ForEach(Action<T> action)
        {
            ArgumentNullException.ThrowIfNull(action);
            for (var i = 0; i < span.Length; i++)
                action(span[i]);
            return span;
        }
        /// <summary>
        /// Invokes the specified <paramref name="action"/> for each element in the <see cref="Span{T}"/>, passing the element and its index.
        /// </summary>
        /// <param name="action">The <see cref="Action{T1, T2}"/> to invoke for each element.</param>
        /// <returns>The original <see cref="Span{T}"/>.</returns>
        public Span<T> ForEach(Action<T, int> action)
        {
            ArgumentNullException.ThrowIfNull(action);
            for (var i = 0; i < span.Length; i++)
                action(span[i], i);
            return span;
        }
    }

    extension<T>(in Memory<T> memory)
    {
        /// <summary>
        /// Forcibly obtains a <see cref="Span{T}"/> from <paramref name="memory"/>, assuming its backing store is an array.
        /// If this invariant is not true, this results in undefined behavior (likely access violations).
        /// </summary>
        /// <returns>The created <see cref="Span{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> UnsafeArraySpan()
        {
            ref readonly var rom = ref Unsafe.As<Memory<T>, ReadOnlyMemory<T>>(ref Unsafe.AsRef(in memory));
            var span = rom.UnsafeArraySpan();
            return Unsafe.As<ReadOnlySpan<T>, Span<T>>(ref Unsafe.AsRef(in span));
        }
        /// <summary>
        /// Determines a value indicating whether the backing store of <paramref name="memory"/> is an array.
        /// </summary>
        public bool IsBackedByArray
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ref readonly var shim = ref Unsafe.As<Memory<T>, MemoryMirror<T>>(ref Unsafe.AsRef(in memory));
                return shim.Object is T[];
            }
        }

        /// <summary>
        /// Forcibly obtains a <see cref="Span{T}"/> from <paramref name="memory"/>, assuming its backing store is a <see cref="MemoryManager{T}"/>.
        /// To offer performance similar to <see cref="UnsafeArraySpan{T}(in Memory{T})"/> and <see cref="UnsafeStringSpan(in Memory{char})"/>, <typeparamref name="TManager"/> must be the exact type of the backing <see cref="MemoryManager{T}"/>, otherwise <see cref="MemoryManager{T}.GetSpan"/> will still incur a virtual call.
        /// </summary>
        /// <typeparam name="TManager">The type of the backing <see cref="MemoryManager{T}"/>.</typeparam>
        /// <returns>The <see cref="Span{T}"/> created by the backing <see cref="MemoryManager{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> UnsafeManagerSpan<TManager>() where TManager : MemoryManager<T>
        {
            ref readonly var rom = ref Unsafe.As<Memory<T>, ReadOnlyMemory<T>>(ref Unsafe.AsRef(in memory));
            var span = rom.UnsafeManagerSpan<T, TManager>();
            return Unsafe.As<ReadOnlySpan<T>, Span<T>>(ref Unsafe.AsRef(in span));
        }
        /// <summary>
        /// Determines whether the backing store of <paramref name="memory"/> is a <see cref="MemoryManager{T}"/> of type <typeparamref name="TManager"/>.
        /// </summary>
        /// <typeparam name="TManager">The type of the backing <see cref="MemoryManager{T}"/>.</typeparam>
        /// <returns><see langword="true"/> if the backing store is a <see cref="MemoryManager{T}"/> of type <typeparamref name="TManager"/>; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBackedByManager<TManager>() where TManager : MemoryManager<T>
        {
            ref readonly var shim = ref Unsafe.As<Memory<T>, MemoryMirror<T>>(ref Unsafe.AsRef(in memory));
            return shim.Object is TManager;
        }
    }
    extension<T>(in ReadOnlyMemory<T> memory)
    {
        /// <summary>
        /// Forcibly obtains a <see cref="ReadOnlySpan{T}"/> from <paramref name="memory"/>, assuming its backing store is an array.
        /// If this invariant is not true, this results in undefined behavior (likely access violations).
        /// </summary>
        /// <returns>The created <see cref="ReadOnlySpan{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> UnsafeArraySpan()
        {
            ref readonly var shim = ref Unsafe.As<ReadOnlyMemory<T>, MemoryMirror<T>>(ref Unsafe.AsRef(in memory));
            Debug.Assert(shim.Object is T[]);

            var arr = Unsafe.As<T[]>(shim.Object);
            var index = shim.Index & RemoveFlagsBitMask;
            return MemoryMarshal.CreateSpan(ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(arr), index), shim.Length);
        }
        /// <summary>
        /// Determines a value indicating whether the backing store of <paramref name="memory"/> is an array.
        /// </summary>
        public bool IsBackedByArray
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ref readonly var shim = ref Unsafe.As<ReadOnlyMemory<T>, MemoryMirror<T>>(ref Unsafe.AsRef(in memory));
                return shim.Object is T[];
            }
        }

        /// <summary>
        /// Forcibly obtains a <see cref="ReadOnlySpan{T}"/> from <paramref name="memory"/>, assuming its backing store is a <see cref="MemoryManager{T}"/>.
        /// To offer performance similar to <see cref="UnsafeArraySpan{T}(in Memory{T})"/> and <see cref="UnsafeStringSpan(in Memory{char})"/>, <typeparamref name="TManager"/> must be the exact type of the backing <see cref="MemoryManager{T}"/>, otherwise <see cref="MemoryManager{T}.GetSpan"/> will still incur a virtual call.
        /// </summary>
        /// <typeparam name="TManager">The type of the backing <see cref="MemoryManager{T}"/>.</typeparam>
        /// <returns>The <see cref="ReadOnlySpan{T}"/> created by the backing <see cref="MemoryManager{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> UnsafeManagerSpan<TManager>() where TManager : MemoryManager<T>
        {
            ref readonly var shim = ref Unsafe.As<ReadOnlyMemory<T>, MemoryMirror<T>>(ref Unsafe.AsRef(in memory));
            Debug.WriteLineIf(shim.Object.GetType() == typeof(TManager),
                $"Expected {nameof(memory)}'s backing store to be exactly of type {typeof(TManager)}, but got {shim.Object.GetType()} instead. TManager.GetSpan() cannot be devirtualized.");

            var mgr = Unsafe.As<TManager>(shim.Object);
            var index = shim.Index & RemoveFlagsBitMask;
            return mgr.GetSpan().Slice(index, shim.Length);
        }
        /// <summary>
        /// Determines whether the backing store of <paramref name="memory"/> is a <see cref="MemoryManager{T}"/> of type <typeparamref name="TManager"/>.
        /// </summary>
        /// <typeparam name="TManager">The type of the backing <see cref="MemoryManager{T}"/>.</typeparam>
        /// <returns><see langword="true"/> if the backing store is a <see cref="MemoryManager{T}"/> of type <typeparamref name="TManager"/>; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBackedByManager<TManager>() where TManager : MemoryManager<T>
        {
            ref readonly var shim = ref Unsafe.As<ReadOnlyMemory<T>, MemoryMirror<T>>(ref Unsafe.AsRef(in memory));
            return shim.Object is TManager;
        }
    }
}
