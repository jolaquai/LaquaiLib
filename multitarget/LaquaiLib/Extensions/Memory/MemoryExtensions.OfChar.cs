namespace LaquaiLib.Extensions;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

public static partial class MemoryExtensions
{
    extension(in Memory<char> memory)
    {
        /// <summary>
        /// Forcibly obtains a <see cref="ReadOnlySpan{T}"/> from <paramref name="memory"/>, assuming its backing store is a <see langword="string"/>.
        /// If this invariant is not true, this results in undefined behavior (likely access violations).
        /// </summary>
        /// <returns>The created <see cref="ReadOnlySpan{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<char> UnsafeStringSpan() => Unsafe.As<Memory<char>, ReadOnlyMemory<char>>(ref Unsafe.AsRef(in memory)).UnsafeStringSpan();
        /// <summary>
        /// Determines a value indicating whether the backing store of <paramref name="memory"/> is a <see langword="string"/>.
        /// </summary>
        public bool IsBackedByString
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ref readonly var shim = ref Unsafe.As<Memory<char>, MemoryMirror<char>>(ref Unsafe.AsRef(in memory));
                return shim.Object is string;
            }
        }
    }
    extension(in ReadOnlyMemory<char> memory)
    {
        /// <summary>
        /// Forcibly obtains a <see cref="Span{T}"/> from <paramref name="memory"/>, assuming its backing store is a <see langword="string"/>.
        /// If this invariant is not true, this results in undefined behavior (likely access violations).
        /// </summary>
        /// <returns>The created <see cref="Span{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<char> UnsafeStringSpan()
        {
            ref readonly var shim = ref Unsafe.As<ReadOnlyMemory<char>, MemoryMirror<char>>(ref Unsafe.AsRef(in memory));

            Debug.Assert(shim.Object is string);
            var str = Unsafe.As<string>(shim.Object);
            var index = shim.Index & RemoveFlagsBitMask;
            return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref MemoryMarshal.GetReference(str.AsSpan()), index), shim.Length);
        }
        /// <summary>
        /// Determines a value indicating whether the backing store of <paramref name="memory"/> is a <see langword="string"/>.
        /// </summary>
        public bool IsBackedByString
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ref readonly var shim = ref Unsafe.As<ReadOnlyMemory<char>, MemoryMirror<char>>(ref Unsafe.AsRef(in memory));
                return shim.Object is string;
            }
        }
    }
}
