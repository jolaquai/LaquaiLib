using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Numerics.Tensors;

namespace LaquaiLib.Numerics;

/// <remarks>
/// A growable arbitrary-width bit container with two interpretation modes controlled by <see cref="Signed"/>:
/// <list type="bullet">
/// <item><description>When <see cref="Signed"/> is <see langword="false"/>, the value is a non-negative magnitude with bit 0 as the LSB.</description></item>
/// <item><description>When <see cref="Signed"/> is <see langword="true"/>, the value is a two's-complement integer at width <see cref="Capacity"/>; the bit at index <c><see cref="Capacity"/> - 1</c> is the sign bit.</description></item>
/// </list>
/// Identity (<see cref="Equals(BitArray)"/>, <see cref="GetHashCode"/>, <see cref="CompareTo(BitArray)"/>) is by numeric value: the <see cref="Signed"/> flag participates only when it actually makes the value negative (see <see cref="IsNegative"/>). Two instances with identical bits but different <see cref="Signed"/> flags are therefore equal when both are non-negative (the flag does not change a non-negative value) and unequal when one is negative (e.g. signed <c>-5</c> differs from the unsigned magnitude with the same bits).
/// </remarks>
public class BitArray : IEquatable<BitArray>, IComparable<BitArray>, ICloneable, ISpanFormattable
{
    private ulong[] _data;
    private BitArray() { _data = []; }

    /// <summary>
    /// Initializes a new <see cref="BitArray"/> with the specified capacity in bits, initialized to all zeros. <paramref name="capacity"/> is rounded up to the next multiple of 64 (the backing word size), so e.g. <c>CreateWithCapacity(65)</c> yields an instance with <see cref="Capacity"/> equal to 128.
    /// </summary>
    /// <param name="capacity">The minimum number of bits the <see cref="BitArray"/> can hold; rounded up to a multiple of 64.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitArray CreateWithCapacity(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        var words = checked((int)((capacity + 63) / 64));
        return new BitArray { _data = new ulong[words] };
    }
    /// <summary>
    /// Initializes a new <see cref="BitArray"/> with the specified number of lower (least significant) bits set to 1.
    /// </summary>
    /// <param name="bits">The number of lower bits to set to 1.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitArray WithLowerNSet(int bits)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bits);
        var ba = CreateWithCapacity(bits);
        ba.SetLower(bits, true);
        return ba;
    }
    /// <summary>
    /// Initializes a new <see cref="BitArray"/> with the specified number of upper (most significant) bits set to 1. The capacity is rounded up to the next multiple of 64; bits are set at the top of the rounded capacity, so <c>WithUpperNSet(65)</c> produces a 128-bit value with bits [63..127] set.
    /// </summary>
    /// <param name="bits">The number of upper bits to set to 1.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitArray WithUpperNSet(int bits)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bits);
        var ba = CreateWithCapacity(bits);
        ba.SetUpper(bits, true);
        return ba;
    }
    /// <summary>
    /// Initializes a new <see cref="BitArray"/> by parsing either a <c>0x</c>/<c>0b</c> numeric literal (always unsigned) or a decimal integer with an optional leading sign. A leading <c>-</c> produces a <see cref="Signed"/> two's-complement value; otherwise the result is unsigned. <c>_</c> digit separators are accepted in <c>0x</c>/<c>0b</c> literals. This is the inverse of the default (decimal) <see cref="ToString()"/> rendering.
    /// </summary>
    /// <param name="str">The <see langword="string"/> to parse.</param>
    /// <returns>The parsed <see cref="BitArray"/>.</returns>
    public static BitArray Parse(string str)
    {
        ArgumentNullException.ThrowIfNull(str);
        var span = str.AsSpan().Trim();
        if (span.IsEmpty)
            throw new FormatException("The input string was empty.");

        // 0x / 0b literal (always a non-negative magnitude).
        if (span.Length >= 2 && span[0] == '0' && span[1] is 'x' or 'X' or 'b' or 'B')
        {
            var bitsPerDigit = span[1] is 'x' or 'X' ? 4 : 1;
            var digits = span[2..];
            var digitCount = 0;
            foreach (var c in digits)
            {
                if (c != '_')
                    digitCount++;
            }
            if (digitCount == 0)
                throw new FormatException($"'{str}' contains no digits.");

            var ba = CreateWithCapacity(digitCount * bitsPerDigit);
            var data = ba._data;
            // Walk right-to-left (LSB first), packing each digit's bits into the words at the running bit position.
            var pos = 0;
            for (var i = digits.Length - 1; i >= 0; i--)
            {
                var c = digits[i];
                if (c == '_')
                    continue;
                ulong val;
                if (bitsPerDigit == 4)
                {
                    val = (ulong)(c switch
                    {
                        >= '0' and <= '9' => c - '0',
                        >= 'a' and <= 'f' => c - 'a' + 10,
                        >= 'A' and <= 'F' => c - 'A' + 10,
                        _ => throw new FormatException($"'{c}' is not a valid hexadecimal digit."),
                    });
                }
                else
                {
                    val = c switch
                    {
                        '0' => 0UL,
                        '1' => 1UL,
                        _ => throw new FormatException($"'{c}' is not a valid binary digit."),
                    };
                }
                if (val != 0)
                {
                    var word = pos >> 6;
                    var off = pos & 63;
                    data[word] |= val << off;
                    // A hex digit at offset >60 straddles the word boundary; spill the high bits into the next word.
                    if (off > 60 && bitsPerDigit == 4)
                        data[word + 1] |= val >> (64 - off);
                }
                pos += bitsPerDigit;
            }
            return ba;
        }

        // Decimal integer with optional leading sign. BigInteger does the heavy lifting.
        var value = BigInteger.Parse(span, NumberStyles.Integer, CultureInfo.InvariantCulture);
        if (value.IsZero)
            return CreateWithCapacity(1);

        var negative = value.Sign < 0;
        var byteCount = value.GetByteCount(isUnsigned: !negative);
        var result = CreateWithCapacity(Math.Max((byteCount + 7) / 8, 1) * 64);
        // For negatives, pre-fill the backing words with the sign-extension (all-ones); TryWriteBytes only
        // emits the minimal two's-complement bytes, so the unwritten high words must already carry the sign.
        if (negative)
            result._data.AsSpan().Fill(ulong.MaxValue);
        value.TryWriteBytes(MemoryMarshal.AsBytes(result._data.AsSpan()), out _, isUnsigned: !negative, isBigEndian: false);
        result.Signed = negative;
        return result;
    }
    /// <summary>
    /// Initializes a new <see cref="BitArray"/> with the specified data. <paramref name="data"/>[0] holds bits [0..63] (least significant), <paramref name="data"/>[1] holds bits [64..127], and so on.
    /// </summary>
    /// <param name="data">The initial data for the <see cref="BitArray"/>.</param>
    public BitArray(params ReadOnlySpan<ulong> data)
    {
        _data = data.IsEmpty ? new ulong[1] : data.ToArray();
    }
    /// <summary>
    /// Initializes a new <see cref="BitArray"/> by copying the contents of another <see cref="BitArray"/>. The new instance will have the same length and bit values as the original.
    /// </summary>
    /// <param name="other">The <see cref="BitArray"/> to copy.</param>
    public BitArray(BitArray other)
    {
        ArgumentNullException.ThrowIfNull(other);
        _data = other._data.ToArray();
        Signed = other.Signed;
    }

    /// <summary>
    /// The total number of bits currently allocated in this <see cref="BitArray"/>'s backing storage (always a multiple of 64). This does not trigger growth.
    /// </summary>
    public int Capacity => _data.Length << 6;
    /// <summary>
    /// The logical length of this <see cref="BitArray"/>, defined as the index of the highest set bit plus one. Returns 0 when no bits are set.
    /// </summary>
    public int Length => FindLastSetBit() + 1;

    /// <summary>
    /// Controls whether this <see cref="BitArray"/> is interpreted as a signed (two's-complement) value at width <see cref="Capacity"/>. When <see langword="true"/>, the bit at index <c><see cref="Capacity"/> - 1</c> is the sign bit; <see cref="ShiftRightArithmetic(int)"/> replicates it into vacated high bits, and <see cref="EnsureCapacityBits(int)"/> sign-extends on growth when that bit is set. When <see langword="false"/>, the value is a non-negative magnitude and growth zero-extends. Freely settable; toggling does not modify stored bits. It affects identity (<see cref="Equals(BitArray)"/>, <see cref="GetHashCode"/>, <see cref="CompareTo(BitArray)"/>) only insofar as it changes the numeric value — i.e. only when the sign bit is set (see <see cref="IsNegative"/>); toggling this flag on a value whose sign bit is clear is identity-preserving.
    /// </summary>
    public bool Signed { get; set; }
    /// <summary>
    /// Returns <see langword="true"/> when this instance is logically negative: <see cref="Signed"/> is <see langword="true"/> and the sign bit (the bit at index <c><see cref="Capacity"/> - 1</c>, i.e. the top bit of the highest backing word) is set. An unsigned instance is never negative, regardless of its bits.
    /// </summary>
    public bool IsNegative => Signed && _data.Length > 0 && (_data[^1] & (1UL << 63)) != 0;

    /// <summary>
    /// Ensures the backing buffer can hold at least <paramref name="bitCount"/> bits, growing it if necessary. Newly allocated bits are zero, except when <see cref="Signed"/> is <see langword="true"/> and the value is currently negative (top bit of the highest word set), in which case they are sign-extended to 1. This is intentional: a <see cref="Signed"/> instance with its top bit set represents a negative two's-complement value, and growing the buffer must preserve that value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureCapacityBits(int bitCount)
    {
        var needed = (bitCount + 63) >> 6;
        if (needed <= _data.Length)
            return;
        var arr = new ulong[needed];
        _data.AsSpan().CopyTo(arr);
        if (IsNegative)
            arr.AsSpan(_data.Length).Fill(ulong.MaxValue);
        _data = arr;
    }

    /// <summary>
    /// Gets or sets the bit value at the specified index. Indices are zero-based from the LSB (index 0 is the least significant bit). From-end indices (<c>^k</c>) are resolved against the current <see cref="Capacity"/>, not <see cref="Length"/>, so <c>^1</c> is always the top bit of the highest backing word.
    /// <para>
    /// The getter throws <see cref="ArgumentOutOfRangeException"/> when the resolved bit index is outside <c>[0, <see cref="Capacity"/>)</c>; it never grows the buffer. The setter grows the buffer when a positive index lies beyond the current <see cref="Capacity"/>; from-end indices never grow. When growth occurs on a <see cref="Signed"/> instance whose current top bit is set (i.e. the value is logically negative), newly allocated high bits are sign-extended to 1 <em>before</em> the requested bit is written. This preserves the two's-complement value across the resize, but means writing a single bit beyond capacity on a signed-negative instance also materializes that sign extension into the buffer.
    /// </para>
    /// </summary>
    /// <param name="index">The index of the bit to get or set.</param>
    /// <returns><see langword="true"/> if the bit at the specified index is set; otherwise, <see langword="false"/>.</returns>
    public bool this[Index index]
    {
        get
        {
            var v = index.IsFromEnd ? (_data.Length << 6) - index.Value : index.Value;
            if (!TryGet(v, out var bit))
                throw new ArgumentOutOfRangeException(nameof(index));
            return bit;
        }
        set
        {
            var v = ResolveForWrite(index);
            ref var word = ref _data[v >> 6];
            var mask = 1UL << (v & 63);
            if (value)
                word |= mask;
            else
                word &= ~mask;
        }
    }
    /// <summary>
    /// Returns a new <see cref="BitArray"/> containing the bits in the specified range, packed starting at bit 0 of the result. From-end indices are resolved against the current <see cref="Capacity"/>. Does not grow this instance. Use <see cref="Set(Range, bool)"/> to assign a value across a range.
    /// </summary>
    public BitArray this[Range range]
    {
        get
        {
            var totalBits = _data.Length << 6;
            var start = range.Start;
            var end = range.End;
            var startBit = start.IsFromEnd ? totalBits - start.Value : start.Value;
            var endBit = end.IsFromEnd ? totalBits - end.Value : end.Value;
            ArgumentOutOfRangeException.ThrowIfNegative(startBit);
            ArgumentOutOfRangeException.ThrowIfLessThan(endBit, startBit);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(endBit, totalBits);
            var length = endBit - startBit;
            if (length == 0)
                return CreateWithCapacity(1);
            var result = CreateWithCapacity(length);

            // Bit-aligned copy from this._data[startBit .. endBit) into result._data[0 ..).
            var srcWord = startBit >> 6;
            var srcBit = startBit & 63;
            var dst = result._data.AsSpan();
            if (srcBit == 0)
            {
                var fullWords = length >> 6;
                _data.AsSpan(srcWord, fullWords).CopyTo(dst);
                var tail = length & 63;
                if (tail > 0)
                    dst[fullWords] = _data[srcWord + fullWords] & ((1UL << tail) - 1);
            }
            else
            {
                var inv = 64 - srcBit;
                var outWords = (length + 63) >> 6;
                for (var i = 0; i < outWords; i++)
                {
                    var lo = _data[srcWord + i] >> srcBit;
                    var hi = (srcWord + i + 1 < _data.Length) ? _data[srcWord + i + 1] << inv : 0UL;
                    dst[i] = lo | hi;
                }
                // Mask off bits beyond `length` in the last destination word.
                var tail = length & 63;
                if (tail > 0)
                    dst[outWords - 1] &= (1UL << tail) - 1;
            }
            return result;
        }
    }
    /// <summary>
    /// Attempts to read the bit at <paramref name="index"/>. Returns <see langword="true"/> when <paramref name="index"/> lies within <c>[0, <see cref="Capacity"/>)</c> and stores the bit in <paramref name="value"/>; otherwise returns <see langword="false"/> and stores <see langword="false"/> in <paramref name="value"/>. Never grows the buffer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet(int index, out bool value)
    {
        if ((uint)index >= (uint)(_data.Length << 6))
        {
            value = false;
            return false;
        }
        value = (_data[index >> 6] & (1UL << (index & 63))) != 0;
        return true;
    }
    /// <summary>
    /// Attempts to write <paramref name="value"/> to the bit at <paramref name="index"/>. Returns <see langword="true"/> when <paramref name="index"/> lies within <c>[0, <see cref="Capacity"/>)</c> and the bit is written; otherwise returns <see langword="false"/> with no side effects. Never grows the buffer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TrySet(int index, bool value)
    {
        if ((uint)index >= (uint)(_data.Length << 6))
            return false;
        ref var word = ref _data[index >> 6];
        var mask = 1UL << (index & 63);
        if (value)
            word |= mask;
        else
            word &= ~mask;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int ResolveForWrite(Index index)
    {
        int v;
        if (index.IsFromEnd)
        {
            v = (_data.Length << 6) - index.Value;
            ArgumentOutOfRangeException.ThrowIfNegative(v);
        }
        else
        {
            v = index.Value;
            ArgumentOutOfRangeException.ThrowIfNegative(v);
            EnsureCapacityBits(v + 1);
        }
        return v;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void And(BitArray right)
    {
        ArgumentNullException.ThrowIfNull(right);
        var len = Math.Min(_data.Length, right._data.Length);
        var dst = _data.AsSpan(0, len);
        TensorPrimitives.BitwiseAnd(dst, right._data.AsSpan(0, len), dst);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Or(BitArray right)
    {
        ArgumentNullException.ThrowIfNull(right);
        var len = Math.Min(_data.Length, right._data.Length);
        var dst = _data.AsSpan(0, len);
        TensorPrimitives.BitwiseOr(dst, right._data.AsSpan(0, len), dst);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Xor(BitArray right)
    {
        ArgumentNullException.ThrowIfNull(right);
        var len = Math.Min(_data.Length, right._data.Length);
        var dst = _data.AsSpan(0, len);
        TensorPrimitives.Xor(dst, right._data.AsSpan(0, len), dst);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Not() => TensorPrimitives.OnesComplement(_data, _data);

    /// <summary>
    /// Sets the lower (least significant) <paramref name="bits"/> to the bitwise NOT of their current value. Grows the buffer if <paramref name="bits"/> exceeds the current <see cref="Capacity"/>.
    /// </summary>
    /// <param name="bits">The number of lower bits to NOT.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Not(int bits)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(bits);
        if (bits == 0)
            return;
        EnsureCapacityBits(bits);

        var fullWords = bits >> 6;
        var remBits = bits & 63;
        var span = _data.AsSpan(0, fullWords);
        TensorPrimitives.OnesComplement(span, span);
        if (remBits > 0)
        {
            var mask = (1UL << remBits) - 1;
            _data[fullWords] ^= mask;
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int PopCount()
    {
        var pop = 0;
        foreach (var w in _data)
            pop += BitOperations.PopCount(w);
        return pop;
    }
    /// <summary>
    /// Sets the upper (most significant) <paramref name="bits"/> of the current <see cref="Capacity"/> to <paramref name="value"/>. Grows the buffer to hold at least <paramref name="bits"/> bits first, then operates relative to the (possibly newly grown) capacity.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetUpper(int bits, bool value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(bits);
        EnsureCapacityBits(bits);
        Set(^bits.., value);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetLower(int bits, bool value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(bits);
        EnsureCapacityBits(bits);
        Set(..bits, value);
    }
    public void Set(Range range, bool value)
    {
        var start = range.Start;
        var end = range.End;
        var totalBits = _data.Length << 6;
        var startBit = start.IsFromEnd ? totalBits - start.Value : start.Value;
        var endBit = end.IsFromEnd ? totalBits - end.Value : end.Value;
        ArgumentOutOfRangeException.ThrowIfNegative(startBit);
        ArgumentOutOfRangeException.ThrowIfLessThan(endBit, startBit);
        Set(startBit, endBit - startBit, value);
    }
    public void Set(int offset, int length, bool value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        if (length == 0)
            return;
        EnsureCapacityBits(offset + length);
        var totalBits = _data.Length << 6;
        if (offset == 0 && length == totalBits)
        {
            SetAll(value);
            return;
        }

        var startWord = offset >> 6;
        var startBit = offset & 63;
        var endExclusive = offset + length;
        var endWord = endExclusive >> 6;
        var endBit = endExclusive & 63;

        // Single-word range
        if (startWord == endWord)
        {
            var mask = ((1UL << (endBit - startBit)) - 1) << startBit;
            if (value)
                _data[startWord] |= mask;
            else
                _data[startWord] &= ~mask;
            return;
        }

        // Head: bits [startBit, 64) of startWord
        {
            var mask = ulong.MaxValue << startBit;
            if (value)
                _data[startWord] |= mask;
            else
                _data[startWord] &= ~mask;
        }

        // Middle: fully covered words [startWord+1, endWord). Span.Fill is SIMD-accelerated.
        var midLen = endWord - startWord - 1;
        if (midLen > 0)
            _data.AsSpan(startWord + 1, midLen).Fill(value ? ulong.MaxValue : 0UL);

        // Tail: bits [0, endBit) of endWord (skipped when range ends on a word boundary)
        if (endBit > 0)
        {
            var mask = (1UL << endBit) - 1;
            if (value)
                _data[endWord] |= mask;
            else
                _data[endWord] &= ~mask;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Clear() => _data.AsSpan().Clear();
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public void SetAll(bool value) => _data.AsSpan().Fill(value ? ulong.MaxValue : 0UL);

    /// <summary>
    /// Returns <see langword="true"/> if every bit is zero.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool IsAllZero() => !_data.AsSpan().ContainsAnyExcept(0UL);
    /// <summary>
    /// Returns <see langword="true"/> if every bit is one.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool IsAllSet() => !_data.AsSpan().ContainsAnyExcept(ulong.MaxValue);
    /// <summary>
    /// Returns <see langword="true"/> if at least one bit is set.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool Any() => _data.AsSpan().ContainsAnyExcept(0UL);
    /// <summary>
    /// Returns <see langword="true"/> if no bit is set.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool None() => !Any();

    /// <summary>
    /// Returns the index of the lowest set bit, or -1 if no bit is set.
    /// </summary>
    public int FindFirstSetBit()
    {
        var idx = _data.AsSpan().IndexOfAnyExcept(0UL);
        if (idx < 0)
            return -1;
        return (idx << 6) + BitOperations.TrailingZeroCount(_data[idx]);
    }
    /// <summary>
    /// Returns the index of the highest set bit, or -1 if no bit is set.
    /// </summary>
    public int FindLastSetBit()
    {
        var idx = _data.AsSpan().LastIndexOfAnyExcept(0UL);
        if (idx < 0)
            return -1;
        return (idx << 6) + BitOperations.Log2(_data[idx]);
    }

    /// <summary>
    /// Shifts every bit toward higher indices (toward the MSB) by <paramref name="n"/> positions. Bits shifted past the end are lost; vacated low bits are zero-filled.
    /// </summary>
    public void ShiftLeft(int n)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(n);
        ShiftLeftSpan(_data, n);
    }
    public static BitArray operator <<(BitArray value, int n)
    {
        ArgumentNullException.ThrowIfNull(value);
        var clone = value.Clone();
        clone.ShiftLeft(n);
        return clone;
    }
    public void operator <<=(int n) => ShiftLeft(n);
    /// <summary>
    /// Shifts every bit toward lower indices (toward the LSB) by <paramref name="n"/> positions. Bits shifted past index 0 are lost; vacated high bits are zero-filled.
    /// </summary>
    public void ShiftRight(int n)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(n);
        ShiftRightSpan(_data, n, 0UL);
    }
    public static BitArray operator >>(BitArray value, int n)
    {
        ArgumentNullException.ThrowIfNull(value);
        var clone = value.Clone();
        clone.ShiftRight(n);
        return clone;
    }
    public void operator >>=(int n) => ShiftRight(n);

    /// <summary>
    /// Shifts every bit toward lower indices (toward the LSB) by <paramref name="n"/> positions. When <see cref="Signed"/> is <see langword="true"/>, vacated high bits are filled with the sign bit (the bit at index <c><see cref="Capacity"/> - 1</c>); otherwise vacated high bits are zero-filled (equivalent to <see cref="ShiftRight(int)"/>).
    /// </summary>
    public void ShiftRightArithmetic(int n)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(n);
        ulong fill = 0UL;
        if (IsNegative)
            fill = ulong.MaxValue;
        ShiftRightSpan(_data, n, fill);
    }
    public static BitArray operator >>>(BitArray value, int n)
    {
        ArgumentNullException.ThrowIfNull(value);
        var clone = value.Clone();
        clone.ShiftRightArithmetic(n);
        return clone;
    }
    public void operator >>>=(int n) => ShiftRightArithmetic(n);

    /// <summary>
    /// Rotates every bit toward higher indices by <paramref name="n"/> positions; bits shifted past the end wrap around to index 0. Negative <paramref name="n"/> rotates right.
    /// </summary>
    public void RotateLeft(int n)
    {
        var totalBits = _data.Length << 6;
        n = (int)(((long)n % totalBits + totalBits) % totalBits);
        if (n == 0)
            return;

        ulong[] arr = null;
        scoped Span<ulong> buf;
        if (_data.Length <= 128)
            buf = stackalloc ulong[_data.Length];
        else
        {
            arr = ArrayPool<ulong>.Shared.Rent(_data.Length);
            buf = arr.AsSpan(0, _data.Length);
        }

        try
        {
            scoped var copy = buf;
            _data.AsSpan().CopyTo(copy);
            ShiftLeftSpan(_data, n);
            ShiftRightSpan(copy, totalBits - n, 0UL);
            TensorPrimitives.BitwiseOr(_data, copy, _data);
        }
        finally
        {
            if (arr is not null)
                ArrayPool<ulong>.Shared.Return(arr);
        }
    }
    /// <summary>
    /// Rotates every bit toward lower indices by <paramref name="n"/> positions; bits shifted past index 0 wrap around to the end. Negative <paramref name="n"/> rotates left.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RotateRight(int n) => RotateLeft(-n);

    private static void ShiftLeftSpan(Span<ulong> data, int n)
    {
        var totalBits = data.Length << 6;
        if (n == 0)
            return;
        if (n >= totalBits)
        { data.Clear(); return; }

        var wordShift = n >> 6;
        var bitShift = n & 63;

        if (bitShift == 0)
        {
            // Pure word displacement; CopyTo is SIMD-vectorized and handles overlap correctly.
            data.Slice(0, data.Length - wordShift).CopyTo(data.Slice(wordShift));
            data.Slice(0, wordShift).Clear();
            return;
        }

        var inv = 64 - bitShift;
        // Walk high -> low so the in-place writes don't clobber unread sources.
        for (var i = data.Length - 1; i > wordShift; i--)
            data[i] = (data[i - wordShift] << bitShift) | (data[i - wordShift - 1] >> inv);
        data[wordShift] = data[0] << bitShift;
        data.Slice(0, wordShift).Clear();
    }
    private static void ShiftRightSpan(Span<ulong> data, int n, ulong fill)
    {
        var totalBits = data.Length << 6;
        if (n == 0)
            return;
        if (n >= totalBits)
        { data.Fill(fill); return; }

        var wordShift = n >> 6;
        var bitShift = n & 63;
        var last = data.Length - 1;

        if (bitShift == 0)
        {
            data.Slice(wordShift).CopyTo(data.Slice(0, data.Length - wordShift));
            data.Slice(data.Length - wordShift).Fill(fill);
            return;
        }

        var inv = 64 - bitShift;
        // Walk low -> high.
        for (var i = 0; i < last - wordShift; i++)
            data[i] = (data[i + wordShift] >> bitShift) | (data[i + wordShift + 1] << inv);
        // Top in-range word: no further source, splice in the fill.
        data[last - wordShift] = (data[last] >> bitShift) | (fill << inv);
        data.Slice(last - wordShift + 1).Fill(fill);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitArray Clone() => new BitArray(this);
    object ICloneable.Clone() => Clone();

    /// <summary>
    /// Reinterprets the lowest <c>sizeof(<typeparamref name="T"/>)</c> bytes of this instance's backing storage as a value of type <typeparamref name="T"/>, using the same LSB-first word layout as the <see cref="BitArray(ReadOnlySpan{ulong})"/> constructor (word 0 holds the least significant bytes). This is a raw bit reinterpretation, <em>not</em> a numeric conversion: it ignores <see cref="Signed"/> and never sign-extends. When the backing storage holds fewer than <c>sizeof(<typeparamref name="T"/>)</c> bytes, the missing high bytes are treated as zero.
    /// </summary>
    /// <typeparam name="T">The unmanaged type to reinterpret the bits as.</typeparam>
    /// <returns>The reinterpreted value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T As<T>() where T : unmanaged
    {
        // An if-elif ladder over typeof(T): for a given instantiation the comparisons fold to constants and
        // the JIT drops every dead branch, leaving a direct read for the common primitives. Casts read the
        // low bytes of word 0; `unchecked` keeps the narrowing conversions as pure bit truncations.
        unchecked
        {
            var lo = _data[0];
            if (typeof(T) == typeof(bool))
            { var v = (byte)lo; return Unsafe.As<byte, T>(ref v); }
            else if (typeof(T) == typeof(byte))
            { var v = (byte)lo; return Unsafe.As<byte, T>(ref v); }
            else if (typeof(T) == typeof(sbyte))
            { var v = (sbyte)lo; return Unsafe.As<sbyte, T>(ref v); }
            else if (typeof(T) == typeof(short))
            { var v = (short)lo; return Unsafe.As<short, T>(ref v); }
            else if (typeof(T) == typeof(ushort))
            { var v = (ushort)lo; return Unsafe.As<ushort, T>(ref v); }
            else if (typeof(T) == typeof(char))
            { var v = (char)lo; return Unsafe.As<char, T>(ref v); }
            else if (typeof(T) == typeof(int))
            { var v = (int)lo; return Unsafe.As<int, T>(ref v); }
            else if (typeof(T) == typeof(uint))
            { var v = (uint)lo; return Unsafe.As<uint, T>(ref v); }
            else if (typeof(T) == typeof(long))
            { var v = (long)lo; return Unsafe.As<long, T>(ref v); }
            else if (typeof(T) == typeof(ulong))
            { var v = lo; return Unsafe.As<ulong, T>(ref v); }
            else if (typeof(T) == typeof(nint))
            { var v = (nint)lo; return Unsafe.As<nint, T>(ref v); }
            else if (typeof(T) == typeof(nuint))
            { var v = (nuint)lo; return Unsafe.As<nuint, T>(ref v); }
            else if (typeof(T) == typeof(Half))
            { var v = BitConverter.UInt16BitsToHalf((ushort)lo); return Unsafe.As<Half, T>(ref v); }
            else if (typeof(T) == typeof(float))
            { var v = BitConverter.UInt32BitsToSingle((uint)lo); return Unsafe.As<float, T>(ref v); }
            else if (typeof(T) == typeof(double))
            { var v = BitConverter.UInt64BitsToDouble(lo); return Unsafe.As<double, T>(ref v); }
            else if (typeof(T) == typeof(Int128))
            { var v = new Int128(_data.Length > 1 ? _data[1] : 0UL, lo); return Unsafe.As<Int128, T>(ref v); }
            else if (typeof(T) == typeof(UInt128))
            { var v = new UInt128(_data.Length > 1 ? _data[1] : 0UL, lo); return Unsafe.As<UInt128, T>(ref v); }
            else
            {
                // Generic fallback for any other unmanaged T (Guid, decimal, user structs, SIMD vectors, ...).
                var size = Unsafe.SizeOf<T>();
                var bytes = MemoryMarshal.AsBytes(_data.AsSpan());
                if (bytes.Length >= size)
                    return MemoryMarshal.Read<T>(bytes);
                // Backing store is shorter than T; zero-extend the high bytes.
                Span<byte> buffer = stackalloc byte[size];
                buffer.Clear();
                bytes.CopyTo(buffer);
                return MemoryMarshal.Read<T>(buffer);
            }
        }
    }
    /// <summary>
    /// Like <see cref="As{T}"/>, but throws <see cref="OverflowException"/> when the reinterpretation would drop information — i.e. when any bit at or above index <c>sizeof(<typeparamref name="T"/>) * 8</c> is set. This is a lossless <em>bit-width</em> guard and is sign-agnostic, exactly like <see cref="As{T}"/>: a <see cref="Signed"/> negative value will throw, because its sign-extension bits count as set high bits. For "does the numeric value fit in <typeparamref name="T"/>" semantics, use a numeric conversion instead.
    /// </summary>
    /// <typeparam name="T">The unmanaged type to reinterpret the bits as.</typeparam>
    /// <returns>The reinterpreted value.</returns>
    /// <exception cref="OverflowException">A set bit lies at or beyond <c>sizeof(<typeparamref name="T"/>) * 8</c>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T AsExact<T>() where T : unmanaged
    {
        if (!TryAsExact(out T result))
            ThrowOverflow(Unsafe.SizeOf<T>(), typeof(T));
        return result;
    }
    [DoesNotReturn, MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowOverflow(int size, Type t) =>throw new OverflowException($"The value has set bits beyond the {size} byte(s) of {t} and cannot be reinterpreted without loss.");

    /// <summary>
    /// Like <see cref="AsExact{T}"/>, but throws <see cref="OverflowException"/> when the reinterpretation would drop information — i.e. when any bit at or above index <c>sizeof(<typeparamref name="T"/>) * 8</c> is set. This is a lossless <em>bit-width</em> guard and is sign-agnostic, exactly like <see cref="As{T}"/>: a <see cref="Signed"/> negative value will throw, because its sign-extension bits count as set high bits. For "does the numeric value fit in <typeparamref name="T"/>" semantics, use a numeric conversion instead.
    /// </summary>
    /// <typeparam name="T">The unmanaged type to reinterpret the bits as.</typeparam>
    /// <returns>The reinterpreted value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryAsExact<T>(out T result) where T : unmanaged
    {
        if (FindLastSetBit() >= Unsafe.SizeOf<T>() * 8)
        {
            result = default;
            return false;
        }
        result = As<T>();
        return true;
    }

    /// <summary>
    /// Returns the number of backing words needed to represent the logical value under this instance's <see cref="Signed"/> interpretation. For non-negative values (unsigned, or signed with top bit clear) trailing all-zero words are trimmed. For signed-negative values (top bit of the highest word set), trailing <see cref="ulong.MaxValue"/> words are trimmed so long as the next-lower word still has its top bit set, preserving the negative two's-complement semantic at the smaller width. Returns 0 only when the value is zero (non-negative case); always returns at least 1 for signed-negative.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EffectiveWordCount()
    {
        if (IsNegative)
        {
            var n = _data.Length;
            while (n > 1 && _data[n - 1] == ulong.MaxValue && (_data[n - 2] & (1UL << 63)) != 0)
                n--;
            return n;
        }
        return _data.AsSpan().LastIndexOfAnyExcept(0UL) + 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(BitArray other)
    {
        if (ReferenceEquals(this, other))
            return true;
        if (other is null)
            return false;
        // Signed changes the numeric value only when it actually makes the value negative, so it gates
        // equality solely through IsNegative: two non-negative instances are equal regardless of their
        // Signed flags, while a negative value never equals a non-negative one with the same bits.
        if (IsNegative != other.IsNegative)
            return false;
        var n = EffectiveWordCount();
        if (n != other.EffectiveWordCount())
            return false;
        return _data.AsSpan(0, n).SequenceEqual(other._data.AsSpan(0, n));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object obj) => Equals(obj as BitArray);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
    {
        var n = EffectiveWordCount();
        var hc = new HashCode();
        // Hash IsNegative (not Signed) so equal values hash equally: a non-negative value hashes the same
        // whether or not Signed is set, keeping the hash contract consistent with Equals.
        hc.Add(IsNegative);
        hc.AddBytes(MemoryMarshal.AsBytes(_data.AsSpan(0, n)));
        return hc.ToHashCode();
    }
    /// <summary>
    /// Compares this <see cref="BitArray"/> with another by numeric value under each operand's own interpretation (per <see cref="Signed"/>). A logically-negative instance (see <see cref="IsNegative"/>) is less than any non-negative instance. Within the same sign category, comparison is by magnitude (high-to-low word scan, ignoring trailing all-zero words for non-negative operands). Numerically equal operands compare equal regardless of their <see cref="Signed"/> flags, so <c>CompareTo == 0</c> exactly matches <see cref="Equals(BitArray)"/>. <see langword="null"/> is considered less than any non-<see langword="null"/> instance.
    /// </summary>
    public int CompareTo(BitArray other)
    {
        if (ReferenceEquals(this, other))
            return 0;
        if (other is null)
            return 1;

        var thisNeg = IsNegative;
        var otherNeg = other.IsNegative;
        if (thisNeg != otherNeg)
            return thisNeg ? -1 : 1;

        int cmp;
        if (thisNeg)
        {
            // Both negative two's-complement. Unsigned-larger == signed-larger when both are negative,
            // so scan high-to-low across the full backing of both operands, treating missing high words
            // as all-ones (sign-extension of a negative value).
            var maxLen = Math.Max(_data.Length, other._data.Length);
            cmp = 0;
            for (var i = maxLen - 1; i >= 0; i--)
            {
                var lw = i < _data.Length ? _data[i] : ulong.MaxValue;
                var rw = i < other._data.Length ? other._data[i] : ulong.MaxValue;
                if (lw != rw)
                {
                    cmp = lw < rw ? -1 : 1;
                    break;
                }
            }
        }
        else
        {
            // Both non-negative magnitudes. Trim trailing zero words on each side.
            var ln = EffectiveWordCount();
            var rn = other.EffectiveWordCount();
            if (ln != rn)
                return ln < rn ? -1 : 1;
            cmp = 0;
            for (var i = ln - 1; i >= 0; i--)
            {
                var lw = _data[i];
                var rw = other._data[i];
                if (lw != rw)
                {
                    cmp = lw < rw ? -1 : 1;
                    break;
                }
            }
        }

        // cmp == 0 means numerically equal. Signed is not a tiebreaker — when neither operand is negative
        // the flag does not change the value — so CompareTo == 0 stays consistent with Equals.
        return cmp;
    }

    private enum FormatKind { Decimal, Hex, Binary, Raw }

    /// <summary>
    /// Returns this <see cref="BitArray"/>'s value as a decimal integer, exactly as a primitive numeric type would render: the magnitude in base 10, prefixed with <c>-</c> only when this instance is <see cref="Signed"/> and logically negative. The <see cref="Signed"/> flag and backing width are otherwise not reflected in the output, so two instances that differ only by <see cref="Signed"/> while both non-negative render identically. Use <see cref="ToString(string, IFormatProvider)"/> with a format specifier for hexadecimal, binary, or the raw backing-word dump.
    /// </summary>
    public override string ToString() => ToString(null, null);
    public string ToString(string format) => ToString(format, null);
    /// <summary>
    /// Returns a textual representation of this <see cref="BitArray"/>'s value. All renderings are sign-magnitude: the magnitude is formatted in the requested base and prefixed with <c>-</c> when this instance is <see cref="Signed"/> and logically negative. Supported <paramref name="format"/> specifiers:
    /// <list type="bullet">
    /// <item><description><see langword="null"/>/empty, <c>"D"</c>/<c>"d"</c>, <c>"G"</c>/<c>"g"</c>: decimal (the default).</description></item>
    /// <item><description><c>"X"</c>/<c>"x"</c>: hexadecimal (upper/lower case), no leading zeros.</description></item>
    /// <item><description><c>"B"</c>/<c>"b"</c>: binary, no leading zeros.</description></item>
    /// <item><description><c>"R"</c>/<c>"r"</c>: the raw, round-trippable backing-word dump — most-significant word first, prefixed with <c>"s:"</c>/<c>"u:"</c>, each word 16 hex digits (upper/lower case), trailing zero words (or sign-extension words for signed-negative values) trimmed.</description></item>
    /// </list>
    /// Unlike the BCL, hexadecimal and binary of a negative value use sign-magnitude (a <c>-</c> prefix), not two's-complement. <paramref name="formatProvider"/> is ignored since none of these renderings are culture-sensitive.
    /// </summary>
    public string ToString(string format, IFormatProvider formatProvider)
    {
        var kind = ParseFormat(format, out var lowercase);
        if (kind == FormatKind.Decimal)
            // BigInteger already emits sign-magnitude decimal ("-N"); reading our words as two's-complement
            // when Signed (else as an unsigned magnitude) reproduces the logical value exactly.
            return ToBigInteger().ToString(CultureInfo.InvariantCulture);

        // Hex/binary/raw all have computable lengths; format into a scratch buffer, then materialize once.
        var words = Math.Max(_data.Length, 1);
        var maxLength = kind switch
        {
            FormatKind.Hex => 1 + (16 * words),
            FormatKind.Binary => 1 + (64 * words),
            _ => (words * 17) + 1, // Raw
        };
        char[] rented = null;
        var buffer = maxLength <= 256
            ? stackalloc char[maxLength]
            : (rented = ArrayPool<char>.Shared.Rent(maxLength)).AsSpan(0, maxLength);
        try
        {
            TryFormatCore(buffer, out var written, kind, lowercase);
            return new string(buffer[..written]);
        }
        finally
        {
            if (rented is not null)
                ArrayPool<char>.Shared.Return(rented);
        }
    }
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)
    {
        var kind = ParseFormat(format, out var lowercase);
        if (kind == FormatKind.Decimal)
            return ToBigInteger().TryFormat(destination, out charsWritten, default, CultureInfo.InvariantCulture);
        return TryFormatCore(destination, out charsWritten, kind, lowercase);
    }

    /// <summary>
    /// Reinterprets the backing words as a <see cref="BigInteger"/> carrying this instance's logical value: two's-complement at the full backing width when <see cref="Signed"/>, otherwise an unsigned magnitude.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private BigInteger ToBigInteger()
        => _data.Length == 0
            ? BigInteger.Zero
            : new BigInteger(MemoryMarshal.AsBytes(_data.AsSpan()), isUnsigned: !Signed, isBigEndian: false);

    /// <summary>
    /// Maps a format specifier to a <see cref="FormatKind"/> and whether hex/raw digits should be lowercase. Throws <see cref="FormatException"/> for any unsupported specifier.
    /// </summary>
    private static FormatKind ParseFormat(ReadOnlySpan<char> format, out bool lowercase)
    {
        lowercase = false;
        if (format.IsEmpty)
            return FormatKind.Decimal;
        if (format.Length == 1)
        {
            switch (format[0])
            {
                case 'D' or 'd' or 'G' or 'g':
                    return FormatKind.Decimal;
                case 'x':
                    lowercase = true;
                    goto case 'X';
                case 'X':
                    return FormatKind.Hex;
                case 'B' or 'b':
                    return FormatKind.Binary;
                case 'r':
                    lowercase = true;
                    goto case 'R';
                case 'R':
                    return FormatKind.Raw;
            }
        }
        throw new FormatException($"The '{format.ToString()}' format string is not supported.");
    }

    /// <summary>
    /// Computes the sign-magnitude of this instance into <paramref name="magnitude"/> (which must be at least <see cref="_data"/>.Length long), returning the number of significant magnitude words (0 for a zero value) and reporting whether the value is logically negative.
    /// </summary>
    private int GetMagnitude(Span<ulong> magnitude, out bool negative)
    {
        negative = IsNegative;
        if (!negative)
        {
            var n = EffectiveWordCount();
            _data.AsSpan(0, n).CopyTo(magnitude);
            return n;
        }
        // Two's-complement negate (~x + 1) over the full width, then trim trailing zero words.
        ulong carry = 1;
        for (var i = 0; i < _data.Length; i++)
        {
            var sum = ~_data[i] + carry;
            magnitude[i] = sum;
            carry = sum < carry ? 1UL : 0UL;
        }
        var m = _data.Length;
        while (m > 0 && magnitude[m - 1] == 0)
            m--;
        return m;
    }

    private bool TryFormatCore(Span<char> destination, out int charsWritten, FormatKind kind, bool lowercase)
    {
        if (kind == FormatKind.Raw)
            return TryFormatRaw(destination, out charsWritten, lowercase);

        // Hex/binary: sign-magnitude.
        var words = Math.Max(_data.Length, 1);
        ulong[] rented = null;
        var magnitude = words <= 64
            ? stackalloc ulong[words]
            : (rented = ArrayPool<ulong>.Shared.Rent(words)).AsSpan(0, words);
        try
        {
            var m = GetMagnitude(magnitude, out var negative);
            if (m == 0)
            {
                if (destination.IsEmpty)
                {
                    charsWritten = 0;
                    return false;
                }
                destination[0] = '0';
                charsWritten = 1;
                return true;
            }

            var topBits = 64 - BitOperations.LeadingZeroCount(magnitude[m - 1]);
            var perWord = kind == FormatKind.Hex ? 16 : 64;
            var topDigits = kind == FormatKind.Hex ? (topBits + 3) / 4 : topBits;
            var length = (negative ? 1 : 0) + topDigits + (perWord * (m - 1));
            if (destination.Length < length)
            {
                charsWritten = 0;
                return false;
            }

            var pos = 0;
            if (negative)
                destination[pos++] = '-';
            ReadOnlySpan<char> topFormat, wordFormat;
            if (kind == FormatKind.Hex)
            {
                topFormat = lowercase ? "x" : "X";
                wordFormat = lowercase ? "x16" : "X16";
            }
            else
            {
                topFormat = "B";
                wordFormat = "B64";
            }
            // Most-significant word with no padding (trims leading zeros), then the rest zero-padded to full width.
            magnitude[m - 1].TryFormat(destination[pos..], out var written, topFormat, CultureInfo.InvariantCulture);
            pos += written;
            for (var i = m - 2; i >= 0; i--)
            {
                magnitude[i].TryFormat(destination[pos..], out written, wordFormat, CultureInfo.InvariantCulture);
                pos += written;
            }
            charsWritten = pos;
            return true;
        }
        finally
        {
            if (rented is not null)
                ArrayPool<ulong>.Shared.Return(rented);
        }
    }

    private bool TryFormatRaw(Span<char> destination, out int charsWritten, bool lowercase)
    {
        var n = EffectiveWordCount();
        if (n == 0)
        {
            if (destination.Length < 3)
            {
                charsWritten = 0;
                return false;
            }
            destination[0] = Signed ? 's' : 'u';
            destination[1] = ':';
            destination[2] = '0';
            charsWritten = 3;
            return true;
        }
        // 2 prefix chars + 16 hex per word + 1 separator between words.
        if (destination.Length < (n * 17) + 1)
        {
            charsWritten = 0;
            return false;
        }
        destination[0] = Signed ? 's' : 'u';
        destination[1] = ':';
        var pos = 2;
        ReadOnlySpan<char> wordFormat = lowercase ? "x16" : "X16";
        for (var i = n - 1; i >= 0; i--)
        {
            _data[i].TryFormat(destination[pos..], out var written, wordFormat, CultureInfo.InvariantCulture);
            pos += written;
            if (i > 0)
                destination[pos++] = '_';
        }
        charsWritten = pos;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(BitArray left, BitArray right) => left is null ? right is null : left.Equals(right);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(BitArray left, BitArray right) => !(left == right);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(BitArray left, BitArray right) => left is null ? right is not null : left.CompareTo(right) < 0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(BitArray left, BitArray right) => left is null || left.CompareTo(right) <= 0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(BitArray left, BitArray right) => left is not null && left.CompareTo(right) > 0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(BitArray left, BitArray right) => left is null ? right is null : left.CompareTo(right) >= 0;

    /// <summary>
    /// Returns a new <see cref="BitArray"/> containing the bitwise AND of <paramref name="left"/> and <paramref name="right"/>. The result's capacity equals the larger operand's capacity; high words missing from the shorter operand are treated as zero. The result's <see cref="Signed"/> is <c><paramref name="left"/>.Signed || <paramref name="right"/>.Signed</c> (symmetric in operand order).
    /// </summary>
    public static BitArray operator &(BitArray left, BitArray right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        var (big, small) = left._data.Length >= right._data.Length ? (left, right) : (right, left);
        var clone = big.Clone();
        clone.And(small);
        // Any words in clone beyond small._data.Length must become zero (AND with implicit 0).
        if (clone._data.Length > small._data.Length)
            clone._data.AsSpan(small._data.Length).Clear();
        clone.Signed = left.Signed || right.Signed;
        return clone;
    }
    /// <summary>
    /// Returns a new <see cref="BitArray"/> containing the bitwise OR of <paramref name="left"/> and <paramref name="right"/>. The result's capacity equals the larger operand's capacity. The result's <see cref="Signed"/> is <c><paramref name="left"/>.Signed || <paramref name="right"/>.Signed</c> (symmetric in operand order).
    /// </summary>
    public static BitArray operator |(BitArray left, BitArray right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        var (big, small) = left._data.Length >= right._data.Length ? (left, right) : (right, left);
        var clone = big.Clone();
        clone.Or(small);
        clone.Signed = left.Signed || right.Signed;
        return clone;
    }
    /// <summary>
    /// Returns a new <see cref="BitArray"/> containing the bitwise XOR of <paramref name="left"/> and <paramref name="right"/>. The result's capacity equals the larger operand's capacity. The result's <see cref="Signed"/> is <c><paramref name="left"/>.Signed || <paramref name="right"/>.Signed</c> (symmetric in operand order).
    /// </summary>
    public static BitArray operator ^(BitArray left, BitArray right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        var (big, small) = left._data.Length >= right._data.Length ? (left, right) : (right, left);
        var clone = big.Clone();
        clone.Xor(small);
        clone.Signed = left.Signed || right.Signed;
        return clone;
    }
    /// <summary>
    /// Returns a new <see cref="BitArray"/> containing the bitwise complement of <paramref name="value"/> within its current <see cref="Capacity"/>. The result preserves <paramref name="value"/>'s <see cref="Signed"/> flag.
    /// </summary>
    public static BitArray operator ~(BitArray value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var clone = value.Clone();
        clone.Not();
        return clone;
    }
}
