using System.Diagnostics.CodeAnalysis;
using System.Text;

using LaquaiLib.Extensions;

namespace LaquaiLib.Streams;

/// <summary>
/// Represents a wrapper for multiple <see cref="Stream"/> instances to be written to as one.
/// </summary>
public class MultiStream : Stream, IDisposable
{
    private readonly List<Stream> _streams;

    #region .ctors
    /// <summary>
    /// Initializes a new <see cref="MultiStream"/> with the given <see cref="Stream"/>s.
    /// </summary>
    /// <param name="streams">A collection of <see cref="Stream"/> instances that are to be written to simultaneously.</param>
    public MultiStream(params ReadOnlySpan<Stream> streams)
    {
        _streams = [.. streams];
    }

    /// <summary>
    /// Initializes a new <see cref="MultiStream"/> with the given <see cref="Stream"/>s.
    /// </summary>
    /// <param name="streams">A collection of <see cref="Stream"/> instances that are to be written to simultaneously.</param>
    public MultiStream(IEnumerable<Stream> streams)
    {
        _streams = streams.ToList();
    }

    /// <summary>
    /// Initializes a new <see cref="MultiStream"/> with the given number of <see cref="Stream"/>s.
    /// </summary>
    /// <param name="streamType">The type of <see cref="Stream"/>s to instantiate.</param>
    /// <param name="count">The number of <see cref="Stream"/>s to instantiate.</param>
    /// <param name="constructorParameters">A collection of parameters to pass to the constructor of the given <paramref name="streamType"/>. If no constructor with the passed parameter types exists, instantiation is attempted with the parameterless constructor.</param>
    /// <exception cref="ArgumentException">Thrown if the supplied <paramref name="streamType"/> does not inherit from <see cref="Stream"/>.</exception>
    public MultiStream(Type streamType, int count, params ReadOnlySpan<object> constructorParameters)
    {
        if (!typeof(Stream).IsAssignableFrom(streamType))
        {
            throw new ArgumentException($"The given type '{streamType.Name}' must inherit from '{nameof(Stream)}'.", nameof(streamType));
        }

        if (streamType.IsAssignableTo(typeof(MemoryStream)))
        {
            _streams = Enumerable.Range(0, count).Select(_ => (Stream)new MemoryStream()).ToList();
        }
        else if (streamType.IsAssignableTo(typeof(FileStream)))
        {
            _streams = Enumerable.Range(0, count).Select(_ => (Stream)new FileStream(Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.DeleteOnClose)).ToList();
        }
        else if (constructorParameters.Length > 1)
        {
            var parameters = constructorParameters.ToArray();
            _streams = Enumerable.Range(0, count).Select(_ => (Stream)Activator.CreateInstance(streamType, parameters)!).ToList();
        }
        else
        {
            try
            {
                _streams = Enumerable.Range(0, count).Select(_ => (Stream)Activator.CreateInstance(streamType)!).ToList();
            }
            catch (Exception)
            {
                throw new ArgumentException($"The given type '{streamType.Name}' could not be instantiated. The call was invalid.", nameof(streamType));
            }
        }
    }

    /// <summary>
    /// Initializes a new <see cref="MultiStream"/> with the given number of <see cref="Stream"/>s.
    /// <example>
    /// For example, to create a <see cref="MultiStream"/> with 5 <see cref="FileStream"/> instances that point to temporary files, use this constructor and something along the lines of:
    /// <code>
    /// using (var ms = new MultiStream(typeof(FileStream), 5, i =>
    /// {
    ///     return new object[]
    ///     {
    ///         Path.Combine(Path.GetTempPath(), $"multistream_test_{i}.txt"),
    ///         FileMode.Create
    ///     };
    /// }))
    /// {
    ///     // ...
    /// }
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="streamType">The type of <see cref="Stream"/>s to instantiate.</param>
    /// <param name="count">The number of <see cref="Stream"/>s to instantiate.</param>
    /// <param name="constructorParameterFactory">A <see cref="Func{T, TResult}"/> that takes an <see cref="int"/> and returns a collection of parameters to pass to the constructor of the given <paramref name="streamType"/>. If no constructor with the passed parameter types exists, instantiation is attempted with the parameterless constructor.</param>
    /// <exception cref="ArgumentException">Thrown if the supplied <paramref name="streamType"/> does not inherit from <see cref="Stream"/>.</exception>
    public MultiStream(Type streamType, int count, Func<int, object[]> constructorParameterFactory)
    {
        if (!typeof(Stream).IsAssignableFrom(streamType))
        {
            throw new ArgumentException($"The given type '{streamType.Name}' must inherit from '{nameof(Stream)}'.", nameof(streamType));
        }

        _streams = Enumerable.Range(0, count).Select(i => (Stream)Activator.CreateInstance(streamType, constructorParameterFactory(i))!).ToList()!;
    }
    #endregion

    /// <summary>
    /// A value that indicates whether all <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance can be read from.
    /// </summary>
    public override bool CanRead => _streams.Select(static stream => stream.CanRead).All();
    /// <summary>
    /// A value that indicates whether all <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance can be seeked.
    /// </summary>
    public override bool CanSeek => _streams.Select(static stream => stream.CanSeek).All();
    /// <summary>
    /// A value that indicates whether all <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance can be written to.
    /// </summary>
    public override bool CanWrite => _streams.Select(static stream => stream.CanWrite).All();
    /// <summary>
    /// A collection of <see cref="long"/>s that indicate the lengths of the <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance.
    /// </summary>
    public long[] Lengths => _streams.Select(static stream => stream.Length).ToArray();
    /// <inheritdoc/>
    public override long Length => throw new InvalidOperationException($"{nameof(LaquaiLib.Streams.MultiStream)} does not support using {nameof(Stream.Length)}. Use {nameof(Lengths)} instead.");
    /// <summary>
    /// A collection of <see cref="long"/>s taht indicate the current positions of the <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance.
    /// </summary>
    public long[] Positions => _streams.Select(static stream => stream.Position).ToArray();
    /// <inheritdoc/>
    public override long Position
    {
        get => throw new InvalidOperationException($"{nameof(LaquaiLib.Streams.MultiStream)} does not support using {nameof(Stream.Position)}. Use {nameof(Positions)} instead.");

        set => throw new InvalidOperationException($"{nameof(LaquaiLib.Streams.MultiStream)} does not support using {nameof(Stream.Position)}. Use {nameof(Positions)} instead.");
    }
    /// <summary>
    /// Flushes all <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance.
    /// </summary>
    public override void Flush() => _streams.ForEach(static stream => stream.Flush());
    /// <summary>
    /// Seeks all <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance.
    /// </summary>
    /// <param name="offset">The offset to seek by.</param>
    /// <param name="origin">A <see cref="SeekOrigin"/> value that indicates the reference point used to obtain the new position.</param>
    /// <returns>-1. Use <see cref="Positions"/> to obtain the new positions of the <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance.</returns>
    public override long Seek(long offset, SeekOrigin origin)
    {
        _streams.ForEach(stream => stream.Seek(offset, origin));
        return -1;
    }
    /// <summary>
    /// Sets a new length for all <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance.
    /// </summary>
    /// <param name="value">The new length for the <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance.</param>
    public void SetLengths(long value) => _streams.ForEach(stream => stream.SetLength(value));
    /// <summary>
    /// Writes a sequence of bytes to all <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance and advances the current position within the <see cref="Stream"/>s by the number of <see cref="byte"/>s written.
    /// </summary>
    /// <param name="buffer">The buffer containing the data to write.</param>
    /// <param name="offset">The offset in the buffer at which to begin writing.</param>
    /// <param name="count">The number of <see cref="byte"/>s to write.</param>
    public override void Write(byte[] buffer, int offset, int count) => _streams.ForEach(stream => stream.Write(buffer, offset, count));
    /// <summary>
    /// Writes a sequence of bytes to all <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance and advances the current position within the <see cref="Stream"/>s by the number of <see cref="byte"/>s written.
    /// </summary>
    /// <param name="buffer">A region of memory to copy to all <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance.</param>
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        foreach (var stream in _streams)
        {
            stream.Write(buffer);
        }
    }
    /// <summary>
    /// Writes <paramref name="text"/> to all <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance using the given <paramref name="encoding"/>.
    /// </summary>
    /// <param name="text">The text to write.</param>
    /// <param name="encoding">The <see cref="Encoding"/> to use to obtain the <see cref="byte"/>s of the characters.</param>
    public void Write(string text, Encoding encoding = null) => _streams.ForEach(stream => stream.Write((encoding ?? Encoding.Default).GetBytes(text)));
    /// <summary>
    /// Writes <paramref name="text"/>, followed by the current line terminator to all <see cref="Stream"/>s wrapped by this <see cref="MultiStream"/> instance using the given <paramref name="encoding"/>.
    /// </summary>
    /// <param name="text">The text to write.</param>
    /// <param name="encoding">The <see cref="Encoding"/> to use to obtain the <see cref="byte"/>s of the characters.</param>
    public void WriteLine(string text, Encoding encoding = null) => _streams.ForEach(stream => stream.Write((encoding ?? Encoding.Default).GetBytes(text + Environment.NewLine)));

    /// <summary>
    /// Unconditionally throws an <see cref="InvalidOperationException"/>.
    /// </summary>
    [DoesNotReturn]
    public override int Read(byte[] buffer, int offset, int count) => throw new InvalidOperationException($"{nameof(LaquaiLib.Streams.MultiStream)} does not support using {nameof(Stream.Read)}.");
    /// <summary>
    /// Unconditionally throws an <see cref="InvalidOperationException"/>.
    /// </summary>
    [DoesNotReturn]
    public override void SetLength(long value) => throw new InvalidOperationException($"{nameof(LaquaiLib.Streams.MultiStream)} does not support using {nameof(Stream.SetLength)}.");

    #region Dispose pattern
    private bool _disposed;

    /// <summary>
    /// Releases the unmanaged and optionally the managed resources used by this <see cref="MultiStream"/> instance.
    /// </summary>
    /// <param name="disposing">Whether to release the managed resources used by this <see cref="MultiStream"/> instance.</param>
    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose of managed resources (Streams etc.)
                _streams.Dispose();
            }

            // Dispose of unmanaged resources (native allocated memory etc.)

            _disposed = true;
        }
    }

    /// <summary>
    /// Finalizes this <see cref="MultiStream"/> instance, releasing any unmanaged resources.
    /// </summary>
    ~MultiStream()
    {
        Dispose(false);
    }

    /// <summary>
    /// Releases the managed and unmanaged resources used by this <see cref="MultiStream"/> instance.
    /// </summary>
    public new void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
