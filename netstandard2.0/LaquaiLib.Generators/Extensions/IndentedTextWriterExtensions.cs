using System.CodeDom.Compiler;

namespace LaquaiLib.Generators.Extensions;

internal static class IndentedTextWriterExtensions
{
    extension(IndentedTextWriter itw)
    {
        public IDisposable Scope
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new ScopeDisposable(itw);
        }

        public void WriteLines(string s)
        {
            var span = s.AsSpan();
            var ptr = 0;
            const char newLine = '\n';
            while (ptr < span.Length)
            {
                var nextNewLine = span.Slice(ptr).IndexOf(newLine);
                var slice = nextNewLine == -1 ? span.Slice(ptr) : span.Slice(ptr, nextNewLine);
                // a lone CR is a line terminator in C#, so leaving one behind splits every /// line into its own unterminated doc comment
                if (slice.Length > 0 && slice[slice.Length - 1] == '\r')
                {
                    slice = slice.Slice(0, slice.Length - 1);
                }
                for (var i = 0; i < slice.Length; i++)
                {
                    itw.Write(slice[i]);
                }
                itw.WriteLine();
                if (nextNewLine == -1)
                {
                    break;
                }
                ptr += nextNewLine + 1;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IDisposable Region(string name) => new RegionDisposable(itw, name);
    }

    private class ScopeDisposable : IDisposable
    {
        private readonly IndentedTextWriter _itw;
        public ScopeDisposable(IndentedTextWriter itw)
        {
            _itw = itw;
            _itw.WriteLine('{');
            _itw.Indent++;
        }
        public void Dispose()
        {
            _itw.Indent--;
            _itw.WriteLine('}');
        }
    }
    private class RegionDisposable : IDisposable
    {
        private readonly IndentedTextWriter _itw;
        private readonly string _regionName;
        private readonly System.Text.StringBuilder _sb;
        private readonly string _indent;
        private readonly int _start;

        public RegionDisposable(IndentedTextWriter itw, string regionName)
        {
            _itw = itw;
            _regionName = regionName;

            // defer emitting the directives until we know the body isn't empty; only possible when the sink is a StringBuilder
            _sb = (itw.InnerWriter as StringWriter)?.GetStringBuilder();
            if (_sb is null)
            {
                _start = -1;
                _itw.WriteLine($"#region {regionName}");
                return;
            }
            _start = _sb.Length;
            _indent = new string(' ', itw.Indent * IndentedTextWriter.DefaultTabString.Length);
        }
        public void Dispose()
        {
            if (_start < 0)
            {
                _itw.WriteLine($"#endregion {_regionName}");
                return;
            }
            if (_sb.Length == _start)
            {
                return;
            }
            _sb.Insert(_start, $"{_indent}#region {_regionName}{_itw.NewLine}");
            _itw.WriteLine($"#endregion {_regionName}");
        }
    }
}
