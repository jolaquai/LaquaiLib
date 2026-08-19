using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace LaquaiLib.Text;

/// <summary>
/// Implements a <see cref="StringComparer"/> equivalent for <see cref="char"/>.
/// </summary>
public abstract class CharComparer : IEqualityComparer<char>, IComparer<char>
{
    /// <summary>
    /// Compares two <see langword="char"/> values and returns a value indicating whether one is less than, equal to, or greater than the other according to the rules of this instance.
    /// </summary>
    /// <param name="x">The first <see langword="char"/> to compare.</param>
    /// <param name="y">The second <see langword="char"/> to compare.</param>
    /// <returns>A signed integer that indicates the relative values of <paramref name="x"/> and <paramref name="y"/>.</returns>
    public abstract int Compare(char x, char y);
    /// <summary>
    /// Determines whether two <see langword="char"/> values are equal according to the rules of this instance.
    /// </summary>
    /// <param name="x">The first <see langword="char"/> to compare.</param>
    /// <param name="y">The second <see langword="char"/> to compare.</param>
    /// <returns></returns>
    public abstract bool Equals(char x, char y);
    /// <summary>
    /// Returns a hash code for the specified <see langword="char"/> according to the rules of this instance.
    /// </summary>
    /// <param name="obj">The <see langword="char"/> for which to get a hash code.</param>
    /// <returns>The hash code for the specified <see langword="char"/>.</returns>
    public abstract int GetHashCode([DisallowNull] char obj);

    /// <summary>
    /// Gets a <see cref="CharComparer"/> that compares according to the rules of the specified <see cref="StringComparison"/>.
    /// </summary>
    /// <param name="comparison">The <see cref="StringComparison"/> to get a <see cref="CharComparer"/> for.</param>
    /// <returns>The <see cref="CharComparer"/> equivalent to the specified <see cref="StringComparison"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="comparison"/> is not a valid (named) <see cref="StringComparison"/>.</exception>
    public static CharComparer FromComparison(StringComparison comparison) => comparison switch
    {
        StringComparison.CurrentCulture => CurrentCulture,
        StringComparison.CurrentCultureIgnoreCase => CurrentCultureIgnoreCase,
        StringComparison.InvariantCulture => InvariantCulture,
        StringComparison.InvariantCultureIgnoreCase => InvariantCultureIgnoreCase,
        StringComparison.Ordinal => Ordinal,
        StringComparison.OrdinalIgnoreCase => OrdinalIgnoreCase,
        _ => throw new ArgumentOutOfRangeException(nameof(comparison))
    };

    /// <summary>
    /// Gets a <see cref="CharComparer"/> that compares according to the rules of the current culture.
    /// </summary>
    public static CharComparer CurrentCulture => ComparerImpl<CurrentCultureStrategy>.Instance;
    /// <summary>
    /// Gets a <see cref="CharComparer"/> that compares according to the rules of the current culture, ignoring case.
    /// </summary>
    public static CharComparer CurrentCultureIgnoreCase => ComparerImpl<CurrentCultureIgnoreCaseStrategy>.Instance;
    /// <summary>
    /// Gets a <see cref="CharComparer"/> that compares according to the rules of the invariant culture.
    /// </summary>
    public static CharComparer InvariantCulture => ComparerImpl<InvariantCultureStrategy>.Instance;
    /// <summary>
    /// Gets a <see cref="CharComparer"/> that compares according to the rules of the invariant culture, ignoring case.
    /// </summary>
    public static CharComparer InvariantCultureIgnoreCase => ComparerImpl<InvariantCultureIgnoreCaseStrategy>.Instance;
    /// <summary>
    /// Gets a <see cref="CharComparer"/> that performs a case-sensitive ordinal comparison.
    /// </summary>
    public static CharComparer Ordinal => ComparerImpl<OrdinalStrategy>.Instance;
    /// <summary>
    /// Gets a <see cref="CharComparer"/> that performs a case-insensitive ordinal comparison.
    /// </summary>
    public static CharComparer OrdinalIgnoreCase => ComparerImpl<OrdinalIgnoreCaseStrategy>.Instance;

    #region Implementations
    internal interface ICompareStrategy
    {
        public static abstract int Compare(char x, char y);
        public static abstract bool Equals(char x, char y);
        public static abstract int GetHashCode([DisallowNull] char obj);
    }
    internal readonly struct CurrentCultureStrategy : ICompareStrategy
    {
        private static readonly CompareInfo _compareInfo = CultureInfo.CurrentCulture.CompareInfo;
        public static int Compare(char x, char y)
        {
            var left = new ReadOnlySpan<char>(in x);
            var right = new ReadOnlySpan<char>(in y);
            return _compareInfo.Compare(left, right);
        }
        public static bool Equals(char x, char y) => Compare(x, y) == 0;
        public static int GetHashCode([DisallowNull] char obj)
        {
            var span = new ReadOnlySpan<char>(in obj);
            return _compareInfo.GetHashCode(span, CompareOptions.None);
        }
    }
    internal readonly struct CurrentCultureIgnoreCaseStrategy : ICompareStrategy
    {
        private static readonly CompareInfo _compareInfo = CultureInfo.CurrentCulture.CompareInfo;
        public static int Compare(char x, char y)
        {
            var left = new ReadOnlySpan<char>(in x);
            var right = new ReadOnlySpan<char>(in y);
            return _compareInfo.Compare(left, right, CompareOptions.IgnoreCase);
        }
        public static bool Equals(char x, char y) => Compare(x, y) == 0;
        public static int GetHashCode([DisallowNull] char obj)
        {
            var span = new ReadOnlySpan<char>(in obj);
            return _compareInfo.GetHashCode(span, CompareOptions.IgnoreCase);
        }
    }
    internal readonly struct InvariantCultureStrategy : ICompareStrategy
    {
        private static readonly CompareInfo _compareInfo = CultureInfo.InvariantCulture.CompareInfo;
        public static int Compare(char x, char y)
        {
            var left = new ReadOnlySpan<char>(in x);
            var right = new ReadOnlySpan<char>(in y);
            return _compareInfo.Compare(left, right);
        }
        public static bool Equals(char x, char y) => Compare(x, y) == 0;
        public static int GetHashCode([DisallowNull] char obj)
        {
            var span = new ReadOnlySpan<char>(in obj);
            return _compareInfo.GetHashCode(span, CompareOptions.None);
        }
    }
    internal readonly struct InvariantCultureIgnoreCaseStrategy : ICompareStrategy
    {
        private static readonly CompareInfo _compareInfo = CultureInfo.InvariantCulture.CompareInfo;
        public static int Compare(char x, char y)
        {
            var left = new ReadOnlySpan<char>(in x);
            var right = new ReadOnlySpan<char>(in y);
            return _compareInfo.Compare(left, right, CompareOptions.IgnoreCase);
        }
        public static bool Equals(char x, char y) => Compare(x, y) == 0;
        public static int GetHashCode([DisallowNull] char obj)
        {
            var span = new ReadOnlySpan<char>(in obj);
            return _compareInfo.GetHashCode(span, CompareOptions.IgnoreCase);
        }
    }
    internal readonly struct OrdinalStrategy : ICompareStrategy
    {
        public static int Compare(char x, char y) => x.CompareTo(y);
        public static bool Equals(char x, char y) => x == y;
        public static int GetHashCode([DisallowNull] char obj) => obj.GetHashCode();
    }
    internal readonly struct OrdinalIgnoreCaseStrategy : ICompareStrategy
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static char Fold(char c) => (uint)((c | 0x20) - 'a') <= 'z' - 'a' ? (char)(c | 0x20) : c;
        public static int Compare(char x, char y) => Fold(x).CompareTo(Fold(y));
        public static bool Equals(char x, char y) => Fold(x) == Fold(y);
        public static int GetHashCode([DisallowNull] char obj) => Fold(obj).GetHashCode();
    }
    internal sealed class ComparerImpl<T> : CharComparer where T : struct, ICompareStrategy
    {
        public static readonly ComparerImpl<T> Instance = new ComparerImpl<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int Compare(char x, char y) => T.Compare(x, y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(char x, char y) => T.Equals(x, y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode([DisallowNull] char obj) => T.GetHashCode(obj);
    }
    #endregion
}
