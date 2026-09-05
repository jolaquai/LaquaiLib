using System.Collections.Frozen;
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
    private static readonly char[] _escapeChars = [.. _xmlEscapeDict.Keys];

    extension(string str)
    {
        public string XmlEscape()
        {
            var i = str.IndexOfAny(_escapeChars);
            if (i < 0)
                return str;

            var sb = new StringBuilder((int)(str.Length * 1.2));
            var start = 0;
            while (i >= 0)
            {
                _ = sb.Append(str, start, i - start);
                _ = sb.Append(_xmlEscapeDict[str[i]]);
                start = i + 1;
                i = str.IndexOfAny(_escapeChars, start);
            }
            _ = sb.Append(str, start, str.Length - start);
            return sb.ToString();
        }
    }
}