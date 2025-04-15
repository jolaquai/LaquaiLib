using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace LaquaiLib.Util;

/// <summary>
/// Implements a <see cref="StringComparer"/> equivalent for <see cref="char"/>.
/// </summary>
public abstract class CharComparer : IEqualityComparer<char>, IComparer<char>
{
    public int Compare(char x, char y)
    {
        // Ensure these are stackalloc'd
        ReadOnlySpan<char> left = stackalloc char[1] { x };
        ReadOnlySpan<char> right = stackalloc char[1] { y };
        return CompareCore(x, y, left, right);
    }
    public bool Equals(char x, char y) => Compare(x, y) == 0;
    public int GetHashCode([DisallowNull] char obj)
    {
        // Ensure this is stackalloc'd
        ReadOnlySpan<char> value = stackalloc char[1] { obj };
        return GetHashCodeCore(obj, value);
    }
    protected abstract int CompareCore(char x, char y, ReadOnlySpan<char> left, ReadOnlySpan<char> right);
    protected virtual bool EqualsCore(char x, char y, ReadOnlySpan<char> left, ReadOnlySpan<char> right) => CompareCore(x, y, left, right) == 0;
    protected abstract int GetHashCodeCore([DisallowNull] char obj, ReadOnlySpan<char> span);

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
    public static CharComparer CurrentCulture => CurrentCultureCharComparer.Instance;
    /// <summary>
    /// Gets a <see cref="CharComparer"/> that compares according to the rules of the current culture, ignoring case.
    /// </summary>
    public static CharComparer CurrentCultureIgnoreCase => CurrentCultureIgnoreCaseCharComparer.Instance;
    /// <summary>
    /// Gets a <see cref="CharComparer"/> that compares according to the rules of the invariant culture.
    /// </summary>
    public static CharComparer InvariantCulture => InvariantCultureCharComparer.Instance;
    /// <summary>
    /// Gets a <see cref="CharComparer"/> that compares according to the rules of the invariant culture, ignoring case.
    /// </summary>
    public static CharComparer InvariantCultureIgnoreCase => InvariantCultureIgnoreCaseCharComparer.Instance;
    /// <summary>
    /// Gets a <see cref="CharComparer"/> that performs a case-sensitive ordinal comparison.
    /// </summary>
    public static CharComparer Ordinal => OrdinalCharComparer.Instance;
    /// <summary>
    /// Gets a <see cref="CharComparer"/> that performs a case-insensitive ordinal comparison.
    /// </summary>
    public static CharComparer OrdinalIgnoreCase => OrdinalIgnoreCaseCharComparer.Instance;

    #region Implementations
    internal class CurrentCultureCharComparer : CharComparer
    {
        private CompareInfo _compareInfo = CultureInfo.CurrentCulture.CompareInfo;

        private CurrentCultureCharComparer() { }
        /// <summary>
        /// Gets the singleton instance of the <see cref="CurrentCultureCharComparer"/>.
        /// </summary>
        public static CharComparer Instance => field ??= new CurrentCultureCharComparer();

        protected override int CompareCore(char x, char y, ReadOnlySpan<char> left, ReadOnlySpan<char> right) => _compareInfo.Compare(left, right);
        protected override int GetHashCodeCore([DisallowNull] char obj, ReadOnlySpan<char> span) => _compareInfo.GetHashCode(span, CompareOptions.None);
    }
    internal class CurrentCultureIgnoreCaseCharComparer : CharComparer
    {
        private readonly CompareInfo _compareInfo = CultureInfo.CurrentCulture.CompareInfo;

        private CurrentCultureIgnoreCaseCharComparer() { }
        /// <summary>
        /// Gets the singleton instance of the <see cref="CurrentCultureIgnoreCaseCharComparer"/>.
        /// </summary>
        public static CharComparer Instance => field ??= new CurrentCultureIgnoreCaseCharComparer();

        protected override int CompareCore(char x, char y, ReadOnlySpan<char> left, ReadOnlySpan<char> right) => _compareInfo.Compare(left, right, CompareOptions.IgnoreCase);
        protected override int GetHashCodeCore([DisallowNull] char obj, ReadOnlySpan<char> span) => _compareInfo.GetHashCode(span, CompareOptions.IgnoreCase);
    }
    internal class InvariantCultureCharComparer : CharComparer
    {
        private CompareInfo _compareInfo = CultureInfo.InvariantCulture.CompareInfo;

        private InvariantCultureCharComparer() { }
        /// <summary>
        /// Gets the singleton instance of the <see cref="InvariantCultureCharComparer"/>.
        /// </summary>
        public static CharComparer Instance => field ??= new InvariantCultureCharComparer();

        protected override int CompareCore(char x, char y, ReadOnlySpan<char> left, ReadOnlySpan<char> right) => _compareInfo.Compare(left, right);
        protected override int GetHashCodeCore([DisallowNull] char obj, ReadOnlySpan<char> span) => _compareInfo.GetHashCode(span, CompareOptions.None);
    }
    internal class InvariantCultureIgnoreCaseCharComparer : CharComparer
    {
        private readonly CompareInfo _compareInfo = CultureInfo.InvariantCulture.CompareInfo;

        private InvariantCultureIgnoreCaseCharComparer() { }
        /// <summary>
        /// Gets the singleton instance of the <see cref="InvariantCultureIgnoreCaseCharComparer"/>.
        /// </summary>
        public static CharComparer Instance => field ??= new InvariantCultureIgnoreCaseCharComparer();

        protected override int CompareCore(char x, char y, ReadOnlySpan<char> left, ReadOnlySpan<char> right) => _compareInfo.Compare(left, right, CompareOptions.IgnoreCase);
        protected override int GetHashCodeCore([DisallowNull] char obj, ReadOnlySpan<char> span) => _compareInfo.GetHashCode(span, CompareOptions.IgnoreCase);
    }
    internal class OrdinalCharComparer : CharComparer
    {
        private OrdinalCharComparer() { }
        /// <summary>
        /// Gets the singleton instance of the <see cref="OrdinalCharComparer"/>.
        /// </summary>
        public static CharComparer Instance => field ??= new OrdinalCharComparer();

        protected override int CompareCore(char x, char y, ReadOnlySpan<char> left, ReadOnlySpan<char> right) => x.CompareTo(y);
        protected override bool EqualsCore(char x, char y, ReadOnlySpan<char> left, ReadOnlySpan<char> right) => x == y;
        protected override int GetHashCodeCore([DisallowNull] char obj, ReadOnlySpan<char> span) => obj.GetHashCode();
    }
    internal class OrdinalIgnoreCaseCharComparer : CharComparer
    {
        private OrdinalIgnoreCaseCharComparer() { }
        /// <summary>
        /// Gets the singleton instance of the <see cref="OrdinalIgnoreCaseCharComparer"/>.
        /// </summary>
        public static CharComparer Instance => field ??= new OrdinalIgnoreCaseCharComparer();
        protected override int CompareCore(char x, char y, ReadOnlySpan<char> left, ReadOnlySpan<char> right) => char.ToUpperInvariant(x).CompareTo(char.ToUpperInvariant(y));
        protected override bool EqualsCore(char x, char y, ReadOnlySpan<char> left, ReadOnlySpan<char> right) => char.ToUpperInvariant(x) == char.ToUpperInvariant(y);
        protected override int GetHashCodeCore([DisallowNull] char obj, ReadOnlySpan<char> span) => char.ToUpperInvariant(obj).GetHashCode();
    }
    #endregion
}
