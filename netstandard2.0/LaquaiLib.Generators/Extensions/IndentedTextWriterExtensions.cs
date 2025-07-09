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
}
