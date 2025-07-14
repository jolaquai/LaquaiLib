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
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
            var newLine = Environment.NewLine;
            var newLineLength = newLine.Length;
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
            while (ptr < span.Length)
            {
                var nextNewLine = span.Slice(ptr).IndexOf(newLine);
                if (nextNewLine == -1)
                {
                    var slice = span.Slice(ptr);
                    for (var i = 0; i < slice.Length; i++)
                    {
                        itw.Write(slice[i]);
                    }
                    itw.WriteLine();
                    break;
                }
                else
                {
                    var slice = span.Slice(ptr, nextNewLine);
                    for (var i = 0; i < slice.Length; i++)
                    {
                        itw.Write(slice[i]);
                    }
                    itw.WriteLine();
                    ptr += nextNewLine + newLineLength;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IDisposable Region(string name) => new RegionDisposable(itw, name);
    }

    private readonly struct ScopeDisposable : IDisposable
    {
        private readonly IndentedTextWriter _itw;
        public ScopeDisposable(IndentedTextWriter itw)
        {
            _itw = itw;
            _itw.WriteLine('{');
            _itw.Indent++;
        }
        public readonly void Dispose()
        {
            _itw.Indent--;
            _itw.WriteLine('}');
        }
    }
    private readonly struct RegionDisposable : IDisposable
    {
        private readonly IndentedTextWriter _itw;
        private readonly string _regionName;

        public RegionDisposable(IndentedTextWriter itw, string regionName)
        {
            _itw = itw;
            _regionName = regionName;

            _itw.WriteLine($"#region {regionName}");
        }
        public readonly void Dispose()
        {
            _itw.WriteLine($"#endregion {_regionName}");
        }
    }
}
