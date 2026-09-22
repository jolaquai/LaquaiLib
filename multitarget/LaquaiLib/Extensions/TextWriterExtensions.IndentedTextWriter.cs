using System.CodeDom.Compiler;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extensions for the <see cref="IndentedTextWriter"/> type.
/// </summary>
public static class IndentedTextWriterExtensions
{
    extension(IndentedTextWriter itw)
    {
        /// <summary>
        /// Creates and returns an <see cref="ItwIndent"/> that will increase the indentation level of the <see cref="IndentedTextWriter"/> by 1 and decrease it by 1 on disposal.
        /// </summary>
        public ItwIndent Indent => new(itw);
        /// <summary>
        /// Creates and returns an <see cref="ItwIndent"/> that will increase the indentation level of the <see cref="IndentedTextWriter"/> by the specified amount and decrease it by the same amount on disposal.
        /// </summary>
        /// <param name="by">The number of indentation levels to increase by.</param>
        /// <returns>The created <see cref="ItwIndent"/>.</returns>
        public ItwIndent IndentBy(int by = 1) => new(itw, by);
        /// <summary>
        /// Creates and returns an <see cref="ItwScope"/> that will write an opening brace and increase the indentation level of the <see cref="IndentedTextWriter"/> by 1 and close that scope on disposal.
        /// </summary>
        public ItwScope Scope => new(itw);
    }

    /// <summary>
    /// Represents an indentation level for an <see cref="IndentedTextWriter"/>.
    /// On creation, it will increase the indentation level by the specified amount.
    /// On disposal, it will decrease the indentation level by the same amount.
    /// </summary>
    public readonly struct ItwIndent : IDisposable
    {
        private readonly IndentedTextWriter _itw;
        private readonly int _by;

        /// <summary>
        /// Initializes a new <see cref="ItwIndent"/>.
        /// </summary>
        /// <param name="itw">The <see cref="IndentedTextWriter"/> to increase the indentation level of.</param>
        /// <param name="by">The number of indentation levels to increase by.</param>
        public ItwIndent(IndentedTextWriter itw, int by = 1)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(by);
            _itw = itw;
            _by = by;

            itw.Indent += by;
        }
        /// <inheritdoc/>
        public readonly void Dispose() => _itw.Indent -= _by;
    }
    /// <summary>
    /// Represents a scope for an <see cref="IndentedTextWriter"/> (that is, a brace plus an indentation level).
    /// On creation, it will write an opening brace and increase the indentation level by 1.
    /// On disposal, it will decrease the indentation level by 1 and write a closing brace.
    /// </summary>
    public readonly struct ItwScope : IDisposable
    {
        private readonly IndentedTextWriter _itw;
        /// <summary>
        /// Initializes a new <see cref="ItwScope"/>.
        /// </summary>
        /// <param name="itw">The <see cref="IndentedTextWriter"/> to increase the indentation level of.</param>
        public ItwScope(IndentedTextWriter itw)
        {
            _itw = itw;

            itw.WriteLine('{');
            itw.Indent++;
        }
        /// <inheritdoc/>
        public readonly void Dispose()
        {
            _itw.Indent--;
            _itw.WriteLine('}');
        }
    }
}
