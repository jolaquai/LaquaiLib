using System.Collections.Frozen;
using System.Runtime.InteropServices;
using System.Text;

namespace LaquaiLib.Analyzers.Shared;

public static class StringExtensions
{
    private static readonly FrozenDictionary<char, string> _xmlEscapeDict = new Dictionary<char, string>()
    {
        { '<', "&lt;" },
        { '>', "&gt;" },
        { '&', "&amp;" },
        { '"', "&quot;" },
    }.ToFrozenDictionary();
    private static readonly char[] _escapeChars = _xmlEscapeDict.Keys.ToArray();

    extension(string str)
    {
        public unsafe string XmlEscape()
        {
            var sb = new StringBuilder((int)(str.Length * 1.2));
            var length = str.Length;
            var span = str.AsSpan();
            var ptr = (char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
            var start = ptr;
            while (start - ptr < span.Length)
            {
                var index = new ReadOnlySpan<char>(start, length - (int)(start - ptr)).IndexOfAny(_escapeChars);
                if (index < 0)
                {
                    _ = sb.Append(start, (int)(span.Length - (start - ptr)));
                    break;
                }
                else
                {
                    _ = sb.Append(start, index);
                    var escapeChar = start[index];
                    _ = sb.Append(_xmlEscapeDict[escapeChar]);
                    start += index + 1;
                }
            }
            return sb.ToString();
        }
    }
}