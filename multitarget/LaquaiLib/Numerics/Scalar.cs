using System.Diagnostics.CodeAnalysis;

namespace LaquaiLib.Numerics;

#pragma warning disable IDE0010 // Add missing cases
#pragma warning disable CA1000 // Do not declare static members on generic types

/// <summary>
/// Contains utility methods for manipulating scalar values similar to <c>System.Runtime.Intrinsics.Scalar&lt;T&gt;</c> because that's internal and has tons of ugly casting.
/// </summary>
internal static unsafe class Scalar<T> where T : unmanaged
{
    [DoesNotReturn]
    private static void ThrowForUnsupportedType() => throw new NotSupportedException($"The type '{typeof(T).FullName}' is not supported.");

    private static readonly int _sizeofT = sizeof(T);
    private static readonly uint _sizeofTUnsigned = unchecked((uint)sizeof(T));
    private static readonly bool _isFloat = typeof(T) == typeof(float);
    private static readonly bool _isDouble = typeof(T) == typeof(double);
    private static readonly bool _isHalf = typeof(T) == typeof(Half);
    private static readonly bool _isDecimal = typeof(T) == typeof(decimal);
    private static readonly bool _isBool = typeof(T) == typeof(bool);
    private static readonly bool _isFloatingPoint = _isFloat || _isDouble || _isHalf || _isDecimal;
    private static readonly bool _isSigned = typeof(T) == typeof(sbyte) || typeof(T) == typeof(short) || typeof(T) == typeof(int) || typeof(T) == typeof(long) || typeof(T) == typeof(nint) || typeof(T) == typeof(Int128) || _isFloatingPoint;
    private static readonly bool _isUnsigned = !_isSigned;

    static Scalar()
    {
        if (!(typeof(T).IsPrimitive
            || typeof(T) == typeof(decimal)
            || typeof(T) == typeof(Half)
            || typeof(T) == typeof(Int128)
            || typeof(T) == typeof(UInt128)))
        {
            ThrowForUnsupportedType();
        }
    }

    /// <summary>
    /// Gets a value of <typeparamref name="T"/> with all bits set to 1.
    /// For unsigned types, this is the maximum value.
    /// For signed types, this is the equivalent of -1.
    /// </summary>
    public static T AllBitsSet
    {
        get
        {
            T value = default;
            Unsafe.InitBlock(&value, 0xFF, _sizeofTUnsigned);
            return value;
        }
    }

    /// <summary>
    /// Gets a value of <typeparamref name="T"/> with a value equivalent to 1.
    /// For numeric primitives, this is 1.
    /// For <see langword="bool"/>, this is <see langword="true"/>.
    /// </summary>
    public static T One
    {
        get
        {
            if (_isFloat)
            {
                var val = 1f;
                return Unsafe.As<float, T>(ref val);
            }
            if (_isDouble)
            {
                var val = 1.0;
                return Unsafe.As<double, T>(ref val);
            }
            if (_isHalf)
            {
                var val = (Half)1;
                return Unsafe.As<Half, T>(ref val);
            }
            if (_isDecimal)
            {
                var val = 1m;
                return Unsafe.As<decimal, T>(ref val);
            }
            if (_isBool)
            {
                var val = true;
                return Unsafe.As<bool, T>(ref val);
            }

            T result = default;
            if (BitConverter.IsLittleEndian)
            {
                Unsafe.As<T, byte>(ref result) = 1;
            }
            else
            {
                // Unsafe.Add to the 'last' byte
                Unsafe.Add(ref Unsafe.As<T, byte>(ref result), _sizeofT - 1) = 1;
            }
            return result;
        }
    }

    /// <summary>
    /// Gets the absolute value of a value of <typeparamref name="T"/>.
    /// </summary>
    public static T Abs(T value)
    {
        if (_isUnsigned)
        {
            return value;
        }
        if (_isFloatingPoint)
        {
            switch (_sizeofT)
            {
                case 2:
                {
                    var bits = Unsafe.As<T, ushort>(ref value);
                    bits &= 0x7FFF; // Clear sign bit
                    return Unsafe.As<ushort, T>(ref bits);
                }
                case 4:
                {
                    var bits = Unsafe.As<T, uint>(ref value);
                    bits &= 0x7FFFFFFF; // Clear sign bit
                    return Unsafe.As<uint, T>(ref bits);
                }
                case 8:
                {
                    var bits = Unsafe.As<T, ulong>(ref value);
                    bits &= 0x7FFFFFFFFFFFFFFF; // Clear sign bit
                    return Unsafe.As<ulong, T>(ref bits);
                }
                case 16:
                {
                    var dec = Unsafe.As<T, decimal>(ref value);
                    var abs = decimal.Abs(dec);
                    return Unsafe.As<decimal, T>(ref abs);
                }
            }
        }
        // Signed integer types
        switch (_sizeofT)
        {
            case 1:
            {
                var sb = Unsafe.As<T, sbyte>(ref value);
                var result = (sbyte)(sb < 0 ? -sb : sb);
                return Unsafe.As<sbyte, T>(ref result);
            }
            case 2:
            {
                var s = Unsafe.As<T, short>(ref value);
                var result = (short)(s < 0 ? -s : s);
                return Unsafe.As<short, T>(ref result);
            }
            case 4:
            {
                var i = Unsafe.As<T, int>(ref value);
                var result = i < 0 ? -i : i;
                return Unsafe.As<int, T>(ref result);
            }
            case 8:
            {
                var l = Unsafe.As<T, long>(ref value);
                var result = l < 0 ? -l : l;
                return Unsafe.As<long, T>(ref result);
            }
            case 16:
            {
                var abs = Int128.Abs(Unsafe.As<T, Int128>(ref value));
                return Unsafe.As<Int128, T>(ref abs);
            }
        }

        ThrowForUnsupportedType();
        return default;
    }

    /// <summary>
    /// Adds two values of <typeparamref name="T"/>, allowing overflows.
    /// </summary>
    public static T Add(T left, T right)
    {
        switch (_sizeofT)
        {
            case 1:
                return *(T*)(byte)(*(byte*)&left + *(byte*)&right);
            case 2:
                return *(T*)(ushort)(*(ushort*)&left + *(ushort*)&right);
            case 4:
                return *(T*)(*(uint*)&left + *(uint*)&right);
            case 8:
                return *(T*)(*(ulong*)&left + *(ulong*)&right);
            case 16:
                var sum = Unsafe.As<T, UInt128>(ref left) + Unsafe.As<T, UInt128>(ref right);
                return Unsafe.As<UInt128, T>(ref sum);
            default:
                ThrowForUnsupportedType();
                return default;
        }
    }

    /// <summary>
    /// Adds two values of <typeparamref name="T"/>, saturating at that type's extrema instead of wrapping around.
    /// </summary>
    public static T AddSaturate(T left, T right)
    {
        if (_isFloatingPoint)
        {
            return Add(left, right);
        }

        if (_isUnsigned)
        {
            switch (_sizeofT)
            {
                case 1:
                {
                    var l = Unsafe.As<T, byte>(ref left);
                    var r = Unsafe.As<T, byte>(ref right);
                    var result = (byte)(l > byte.MaxValue - r ? byte.MaxValue : l + r);
                    return Unsafe.As<byte, T>(ref result);
                }
                case 2:
                {
                    var l = Unsafe.As<T, ushort>(ref left);
                    var r = Unsafe.As<T, ushort>(ref right);
                    var result = (ushort)(l > ushort.MaxValue - r ? ushort.MaxValue : l + r);
                    return Unsafe.As<ushort, T>(ref result);
                }
                case 4:
                {
                    var l = Unsafe.As<T, uint>(ref left);
                    var r = Unsafe.As<T, uint>(ref right);
                    var result = l > uint.MaxValue - r ? uint.MaxValue : l + r;
                    return Unsafe.As<uint, T>(ref result);
                }
                case 8:
                {
                    var l = Unsafe.As<T, ulong>(ref left);
                    var r = Unsafe.As<T, ulong>(ref right);
                    var result = l > ulong.MaxValue - r ? ulong.MaxValue : l + r;
                    return Unsafe.As<ulong, T>(ref result);
                }
                case 16:
                {
                    var l = Unsafe.As<T, UInt128>(ref left);
                    var r = Unsafe.As<T, UInt128>(ref right);
                    var result = l > UInt128.MaxValue - r ? UInt128.MaxValue : l + r;
                    return Unsafe.As<UInt128, T>(ref result);
                }
            }
        }
        else
        {
            // Signed integers
            switch (_sizeofT)
            {
                case 1:
                {
                    var l = Unsafe.As<T, sbyte>(ref left);
                    var r = Unsafe.As<T, sbyte>(ref right);
                    var result = (l > 0 && r > 0 && l > sbyte.MaxValue - r) ? sbyte.MaxValue :
                                (l < 0 && r < 0 && l < sbyte.MinValue - r) ? sbyte.MinValue :
                                (sbyte)(l + r);
                    return Unsafe.As<sbyte, T>(ref result);
                }
                case 2:
                {
                    var l = Unsafe.As<T, short>(ref left);
                    var r = Unsafe.As<T, short>(ref right);
                    var result = (l > 0 && r > 0 && l > short.MaxValue - r) ? short.MaxValue :
                                (l < 0 && r < 0 && l < short.MinValue - r) ? short.MinValue :
                                (short)(l + r);
                    return Unsafe.As<short, T>(ref result);
                }
                case 4:
                {
                    var l = Unsafe.As<T, int>(ref left);
                    var r = Unsafe.As<T, int>(ref right);
                    var result = (l > 0 && r > 0 && l > int.MaxValue - r) ? int.MaxValue :
                                (l < 0 && r < 0 && l < int.MinValue - r) ? int.MinValue :
                                l + r;
                    return Unsafe.As<int, T>(ref result);
                }
                case 8:
                {
                    var l = Unsafe.As<T, long>(ref left);
                    var r = Unsafe.As<T, long>(ref right);
                    var result = (l > 0 && r > 0 && l > long.MaxValue - r) ? long.MaxValue :
                                (l < 0 && r < 0 && l < long.MinValue - r) ? long.MinValue :
                                l + r;
                    return Unsafe.As<long, T>(ref result);
                }
                case 16:
                {
                    var l = Unsafe.As<T, Int128>(ref left);
                    var r = Unsafe.As<T, Int128>(ref right);
                    var result = (l > 0 && r > 0 && l > Int128.MaxValue - r) ? Int128.MaxValue :
                                (l < 0 && r < 0 && l < Int128.MinValue - r) ? Int128.MinValue :
                                l + r;
                    return Unsafe.As<Int128, T>(ref result);
                }
            }
        }

        ThrowForUnsupportedType();
        return default;
    }

    /// <summary>
    /// Returns the smallest integral value that is greater than or equal to the specified value.
    /// For integer types, returns the value unchanged.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Ceiling(T value)
    {
        if (_isFloatingPoint)
        {
            switch (_sizeofT)
            {
                case 2:
                {
                    var h = Unsafe.As<T, Half>(ref value);
                    var result = Half.Ceiling(h);
                    return Unsafe.As<Half, T>(ref result);
                }
                case 4:
                {
                    var f = Unsafe.As<T, float>(ref value);
                    var result = MathF.Ceiling(f);
                    return Unsafe.As<float, T>(ref result);
                }
                case 8:
                {
                    var d = Unsafe.As<T, double>(ref value);
                    var result = Math.Ceiling(d);
                    return Unsafe.As<double, T>(ref result);
                }
                case 16:
                {
                    var dec = Unsafe.As<T, decimal>(ref value);
                    var result = decimal.Ceiling(dec);
                    return Unsafe.As<decimal, T>(ref result);
                }
            }
        }
        else
        {
            // Integer types - ceiling is a no-op
            return value;
        }

        ThrowForUnsupportedType();
        return default;
    }

    /// <summary>
    /// Converts an <see cref="int"/> value to type <typeparamref name="T"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Convert(int value)
    {
        if (_isFloatingPoint)
        {
            if (_sizeofT == 4)
            {
                var result = (float)value;
                return Unsafe.As<float, T>(ref result);
            }
            if (_sizeofT == 8)
            {
                var result = (double)value;
                return Unsafe.As<double, T>(ref result);
            }
        }

        if (_isUnsigned)
        {
            switch (_sizeofT)
            {
                case 1:
                {
                    var result = (byte)value;
                    return Unsafe.As<byte, T>(ref result);
                }
                case 2:
                {
                    var result = (ushort)value;
                    return Unsafe.As<ushort, T>(ref result);
                }
                case 4:
                {
                    var result = (uint)value;
                    return Unsafe.As<uint, T>(ref result);
                }
                case 8:
                {
                    var result = (ulong)value;
                    return Unsafe.As<ulong, T>(ref result);
                }
                case 16:
                {
                    var result = (UInt128)value;
                    return Unsafe.As<UInt128, T>(ref result);
                }
            }
        }
        else
        {
            switch (_sizeofT)
            {
                case 1:
                {
                    var result = (sbyte)value;
                    return Unsafe.As<sbyte, T>(ref result);
                }
                case 2:
                {
                    var result = (short)value;
                    return Unsafe.As<short, T>(ref result);
                }
                case 4:
                {
                    return Unsafe.As<int, T>(ref value);
                }
                case 8:
                {
                    var result = (long)value;
                    return Unsafe.As<long, T>(ref result);
                }
                case 16:
                {
                    var result = (Int128)value;
                    return Unsafe.As<Int128, T>(ref result);
                }
            }
        }

        ThrowForUnsupportedType();
        return default;
    }

    /// <summary>
    /// Returns a value with the magnitude of <paramref name="value"/> and the sign of <paramref name="sign"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T CopySign(T value, T sign)
    {
        if (_isFloatingPoint)
        {
            switch (_sizeofT)
            {
                case 2:
                {
                    var valueBits = Unsafe.As<T, ushort>(ref value);
                    var signBits = Unsafe.As<T, ushort>(ref sign);
                    var result = (ushort)((valueBits & 0x7FFF) | (signBits & 0x8000));
                    return Unsafe.As<ushort, T>(ref result);
                }
                case 4:
                {
                    var valueBits = Unsafe.As<T, uint>(ref value);
                    var signBits = Unsafe.As<T, uint>(ref sign);
                    var result = (valueBits & 0x7FFFFFFF) | (signBits & 0x80000000);
                    return Unsafe.As<uint, T>(ref result);
                }
                case 8:
                {
                    var valueBits = Unsafe.As<T, ulong>(ref value);
                    var signBits = Unsafe.As<T, ulong>(ref sign);
                    var result = (valueBits & 0x7FFFFFFFFFFFFFFF) | (signBits & 0x8000000000000000);
                    return Unsafe.As<ulong, T>(ref result);
                }
                case 16:
                {
                    var valueDec = Unsafe.As<T, decimal>(ref value);
                    var signDec = Unsafe.As<T, decimal>(ref sign);
                    var result = decimal.CopySign(valueDec, signDec);
                    return Unsafe.As<decimal, T>(ref result);
                }
            }
        }
        else if (_isUnsigned)
        {
            // Unsigned types can't be negative, return value unchanged
            return value;
        }
        else
        {
            // Signed integers
            var absValue = Abs(value);
            var isSignNegative = LessThan(sign, default);
            return isSignNegative ? Negate(absValue) : absValue;
        }

        ThrowForUnsupportedType();
        return default;
    }

    public static T Divide(T left, T right) { }

    public static bool Equals(T left, T right)
    {
        if (_isBool)
        {
            var lb = Unsafe.As<T, bool>(ref left);
            var rb = Unsafe.As<T, bool>(ref right);
            return lb == rb;
        }
        return left.Equals(right);
    }

    public static uint ExtractMostSignificantBit(T value)
    {
        // This operation makes basically no sense for floating-point types...
        switch (_sizeofT)
        {
            case 1:
            {
                var b = Unsafe.As<T, byte>(ref value);
                return (uint)(b >> 7);
            }
            case 2:
            {
                var s = Unsafe.As<T, ushort>(ref value);
                return (uint)(s >> 15);
            }
            case 4:
            {
                var i = Unsafe.As<T, uint>(ref value);
                return i >> 31;
            }
            case 8:
            {
                var l = Unsafe.As<T, ulong>(ref value);
                return (uint)(l >> 63);
            }
            case 16:
            {
                var u128 = Unsafe.As<T, UInt128>(ref value);
                return (uint)u128 >> 127;
            }
        }

        ThrowForUnsupportedType();
        return default;
    }

    public static T Floor(T value) { }

    public static bool GreaterThan(T left, T right) { }

    public static bool GreaterThanOrEqual(T left, T right) { }

    public static bool LessThan(T left, T right) { }

    public static bool LessThanOrEqual(T left, T right) { }

    public static T Max(T left, T right) { }

    public static T MaxMagnitude(T left, T right) { }

    public static T MaxMagnitudeNumber(T left, T right) { }

    public static T MaxNumber(T left, T right) { }

    public static T Min(T left, T right) { }

    public static T MinMagnitude(T left, T right) { }

    public static T MinMagnitudeNumber(T left, T right) { }

    public static T MinNumber(T left, T right) { }

    public static T Multiply(T left, T right) { }

    public static T MultiplyAddEstimate(T left, T right, T addend) { }

    public static T Negate(T value)
    {
        if (_isUnsigned)
        {
            // Unsigned types can't be negative
            ThrowForUnsupportedType();
        }

        if (_isFloatingPoint)
        {
            switch (_sizeofT)
            {
                case 2:
                {
                    var bits = Unsafe.As<T, ushort>(ref value);
                    bits ^= 0x8000; // Flip sign bit
                    return Unsafe.As<ushort, T>(ref bits);
                }
                case 4:
                {
                    var bits = Unsafe.As<T, uint>(ref value);
                    bits ^= 0x80000000; // Flip sign bit
                    return Unsafe.As<uint, T>(ref bits);
                }
                case 8:
                {
                    var bits = Unsafe.As<T, ulong>(ref value);
                    bits ^= 0x8000000000000000; // Flip sign bit
                    return Unsafe.As<ulong, T>(ref bits);
                }
                case 16:
                {
                    var dec = Unsafe.As<T, decimal>(ref value);
                    var neg = decimal.Negate(dec);
                    return Unsafe.As<decimal, T>(ref neg);
                }
            }
        }
    }

    public static bool ObjectEquals(T left, T right) { }

    public static T Round(T value) { }

    public static T ShiftLeft(T value, int shiftCount) { }

    public static T ShiftRightArithmetic(T value, int shiftCount) { }

    public static T ShiftRightLogical(T value, int shiftCount) { }

    public static T Sqrt(T value) { }

    public static T Subtract(T left, T right) { }

    public static T SubtractSaturate(T left, T right) { }

    public static T Truncate(T value) { }
}