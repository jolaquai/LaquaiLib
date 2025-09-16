using System.Text.RegularExpressions;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Regex"/> Types.
/// </summary>
public static class RegexExtensions
{
    private static class Accessors
    {
        [UnsafeAccessor(UnsafeAccessorKind.Field)]
        public static extern ref Regex _regex(Match _);
        [UnsafeAccessor(UnsafeAccessorKind.Method)]
        public static extern string get_Text(Capture _);
    }

    extension(Match match)
    {
        /// <summary>
        /// Retrieves the <see cref="Regex"/> instance that produced this <see cref="Match"/>.
        /// </summary>
        /// <returns>The <see cref="Regex"/> instance that produced this <see cref="Match"/>.</returns>
        public Regex Regex
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ArgumentNullException.ThrowIfNull(match);
                return Accessors._regex(match);
            }
        }
    }

    extension(Capture capture)
    {
        /// <summary>
        /// Retrieves the original <see cref="string"/> that was matched by a <see cref="Regex"/> instance to produce this <paramref name="capture"/>.
        /// </summary>
        /// <returns>The original <see cref="string"/> that was matched by a <see cref="Regex"/> instance to produce this <paramref name="capture"/>.</returns>
        public string Text
        {
            get
            {
                ArgumentNullException.ThrowIfNull(capture);
                return Accessors.get_Text(capture);
            }
        }
    }

    extension(ReadOnlySpan<char> span)
    {
        /// <summary>
        /// Creates a slice from the source span using the range specified by the <see cref="ValueMatch"/>.
        /// </summary>
        /// <param name="vm">The <see cref="ValueMatch"/> specifying the range to slice.</param>
        /// <returns>The formed slice.</returns>
        public ReadOnlySpan<char> Slice(ValueMatch vm) => span.Slice(vm.Index, vm.Length);
    }
    extension(Span<char> span)
    {
        /// <summary>
        /// Creates a slice from the source span using the range specified by the <see cref="ValueMatch"/>.
        /// </summary>
        /// <param name="vm">The <see cref="ValueMatch"/> specifying the range to slice.</param>
        /// <returns>The formed slice.</returns>
        public ReadOnlySpan<char> Slice(ValueMatch vm) => span.Slice(vm.Index, vm.Length);
    }
}
