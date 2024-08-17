using System.Buffers;
using System.Runtime.CompilerServices;

namespace LaquaiLib.Streams;

public class ObservableStream<T> : Stream
    where T : Stream
{
    private readonly T _underlying;

    public override bool CanRead => _underlying.CanRead;
    public override bool CanSeek => _underlying.CanSeek;
    public override bool CanWrite => _underlying.CanWrite;
    public override long Length => _underlying.Length;
    public override long Position {
        get => _underlying.Position;
        set
        {
            if (_underlying.Position == value)
            {
                return;
            }

            var oldPosition = _underlying.Position;
            _underlying.Position = value;
            Seeked?.Invoke(this, new SeekedEventArgs(oldPosition, Position));
        }
    }

    public override void Flush()
    {
        _underlying.Flush();
        Flushed?.Invoke(this, EventArgs.Empty);
    }
    public override async Task FlushAsync(CancellationToken cancellationToken)
    {
        await _underlying.FlushAsync(cancellationToken).ConfigureAwait(false);
        Flushed?.Invoke(this, EventArgs.Empty);
    }
    public override int Read(byte[] buffer, int offset, int count)
    {
        if (count <= 0)
        {
            return 0;
        }
        if (offset >= buffer.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset is greater than the buffer length.");
        }

        var oldPosition = Position;
        var read = _underlying.Read(buffer, offset, count);
        if (read > 0)
        {
            var memory = new ReadOnlyMemory<byte>(buffer, offset, read);
            DataRead?.Invoke(this, new ReadEventArgs(memory));
            Seeked?.Invoke(this, new SeekedEventArgs(oldPosition, Position));
        }

        return read;
    }
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
    {
        if (count <= 0)
        {
            return 0;
        }
        if (offset >= buffer.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset is greater than the buffer length.");
        }

        var oldPosition = Position;
        var read = await _underlying.ReadAsync(buffer.AsMemory(offset, count), cancellationToken).ConfigureAwait(false);
        if (read > 0)
        {
            var memory = new ReadOnlyMemory<byte>(buffer, offset, read);
            DataRead?.Invoke(this, new ReadEventArgs(memory));
            Seeked?.Invoke(this, new SeekedEventArgs(oldPosition, Position));
        }

        return read;
    }
    public override int Read(Span<byte> buffer)
    {
        if (buffer.Length == 0)
        {
            return 0;
        }

        var temp = ArrayPool<byte>.Shared.Rent(buffer.Length);
        var read = Read(temp, 0, buffer.Length);
        temp.AsSpan(0, read).CopyTo(buffer);
        ArrayPool<byte>.Shared.Return(temp);

        return read;
    }
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        await _underlying.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
        DataRead?.Invoke(this, new ReadEventArgs(buffer));
        return buffer.Length;
    }
    public override int ReadByte()
    {
        if (Position == Length)
        {
            return -1;
        }

        var prevPosition = Position;
        var theByte = _underlying.ReadByte();
        if (theByte != -1)
        {
            DataRead?.Invoke(this, new ReadEventArgs(new ReadOnlyMemory<byte>([(byte)theByte])));
            Seeked?.Invoke(this, new SeekedEventArgs(prevPosition, Position));
        }
        return theByte;
    }
    public override long Seek(long offset, SeekOrigin origin = SeekOrigin.Begin)
    {
        // Bail if the position doesn't actually change
        switch (origin)
        {
            case SeekOrigin.Begin when offset == Position:
            case SeekOrigin.Current when offset == 0:
            case SeekOrigin.End when Length - offset == Position:
                return Position;
            default:
                break;
        }

        var oldPos = Position;
        var newPos = _underlying.Seek(offset, origin);
        if (oldPos != newPos)
        {
            Seeked?.Invoke(this, new SeekedEventArgs(oldPos, newPos));
        }
        return newPos;
    }
    public override void SetLength(long value)
    {
        if (Length == value)
        {
            return;
        }

        var oldLength = Length;
        _underlying.SetLength(value);
        Resized?.Invoke(this, new ResizedEventArgs(oldLength, Length));
    }
    public override void Write(byte[] buffer, int offset, int count)
    {
        if (count <= 0)
        {
            return;
        }
        if (offset >= buffer.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset is greater than the buffer length.");
        }

        var oldPosition = Position;
        var oldLength = Length;
        _underlying.Write(buffer, offset, count);

        var memory = new ReadOnlyMemory<byte>(buffer, offset, count);
        DataWritten?.Invoke(this, new WrittenEventArgs(memory));
        Seeked?.Invoke(this, new SeekedEventArgs(oldPosition, Position));
        if (Length > oldLength)
        {
            Resized?.Invoke(this, new ResizedEventArgs(oldLength, Length));
        }
    }
    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
    {
        if (count <= 0)
        {
            return;
        }
        if (offset >= buffer.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset is greater than the buffer length.");
        }

        var oldPosition = Position;
        var oldLength = Length;
        await _underlying.WriteAsync(buffer.AsMemory(offset, count), cancellationToken).ConfigureAwait(false);

        var memory = new ReadOnlyMemory<byte>(buffer, offset, count);
        DataWritten?.Invoke(this, new WrittenEventArgs(memory));
        Seeked?.Invoke(this, new SeekedEventArgs(oldPosition, Position));
        if (Length > oldLength)
        {
            Resized?.Invoke(this, new ResizedEventArgs(oldLength, Length));
        }
    }
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        var temp = ArrayPool<byte>.Shared.Rent(buffer.Length);
        {
            var span = temp.AsSpan(0, buffer.Length);
            buffer.CopyTo(span);
            Write(temp, 0, buffer.Length);
            ArrayPool<byte>.Shared.Return(temp);
        }
    }
    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        await _underlying.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
        DataWritten?.Invoke(this, new WrittenEventArgs(buffer));
    }
    public override void WriteByte(byte value)
    {
        var prevPosition = Position;
        var oldLength = Length;
        _underlying.WriteByte(value);
        DataWritten?.Invoke(this, new WrittenEventArgs(new ReadOnlyMemory<byte>([value])));
        Seeked?.Invoke(this, new SeekedEventArgs(prevPosition, Position));
        if (Length > oldLength)
        {
            Resized?.Invoke(this, new ResizedEventArgs(oldLength, Length));
        }
    }
    public override void CopyTo(Stream destination, int bufferSize)
    {
        var temp = ArrayPool<byte>.Shared.Rent(bufferSize);
        while (Position != Length)
        {
            var read = Read(temp, 0, bufferSize);
            destination.Write(temp, 0, read);
        }
        ArrayPool<byte>.Shared.Return(temp);
    }
    public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken = default)
    {
        var temp = ArrayPool<byte>.Shared.Rent(bufferSize);
        while (Position != Length)
        {
            var read = await ReadAsync(temp.AsMemory(0, bufferSize), cancellationToken).ConfigureAwait(false);
            await destination.WriteAsync(temp.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
        }
        ArrayPool<byte>.Shared.Return(temp);
    }

    /// <inheritdoc/>
    public override string ToString() => $"{{{_underlying.GetType().FullName}, {Position}/{Length}}}";

    /// <summary>
    /// Occurs when the stream is flushed.
    /// </summary>
    public event EventHandler? Flushed;
    /// <summary>
    /// Occurs when data is read from the stream.
    /// </summary>
    public event EventHandler<ReadEventArgs>? DataRead;
    /// <summary>
    /// Occurs when the stream is seeked, irrespective of whether this was part of a write or read operation, a call to <see cref="Seek(long, SeekOrigin)"/> or a direct change of the <see cref="Position"/> property.
    /// </summary>
    public event EventHandler<SeekedEventArgs>? Seeked;
    /// <summary>
    /// Occurs when data is written to the stream.
    /// </summary>
    public event EventHandler<WrittenEventArgs>? DataWritten;
    /// <summary>
    /// Occurs when the stream is resized, that is, an operation causes <see cref="Length"/> to change.
    /// </summary>
    public event EventHandler<ResizedEventArgs>? Resized;

    /// <summary>
    /// Initializes a new <see cref="ObservableStream{T}"/> instance by wrapping an existing <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to wrap.</param>
    protected internal ObservableStream(T stream)
    {
        _underlying = stream;
    }

    private bool disposed;
    protected override void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            // managed
            _underlying.Dispose();
        }

        // unmanaged

        disposed = true;
    }
    public override async ValueTask DisposeAsync()
    {
        await _underlying.DisposeAsync().ConfigureAwait(false);

        disposed = true;
    }
}

/// <summary>
/// Provides factory methods for <see cref="ObservableStream{T}"/>.
/// </summary>
public static class ObservableStreamFactory
{
    /// <summary>
    /// Creates an <see cref="ObservableStream{T}"/> by creating a new instance of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Stream"/> to create. Must have a public parameterless constructor.</typeparam>
    /// <returns>The created <see cref="ObservableStream{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ObservableStream<T> Create<T>() where T : Stream => new ObservableStream<T>(Activator.CreateInstance<T>());
    /// <summary>
    /// Creates an <see cref="ObservableStream{T}"/> from an existing <see cref="Stream"/>.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="Stream"/> to wrap.</typeparam>
    /// <param name="stream">The <see cref="Stream"/> to wrap.</param>
    /// <returns>The created <see cref="ObservableStream{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ObservableStream<T> Create<T>(T stream) where T : Stream => new ObservableStream<T>(stream);
    /// <summary>
    /// Creates an <see cref="ObservableStream{T}"/> from a portion of a <see cref="byte"/> array.
    /// </summary>
    /// <param name="data">The <see cref="byte"/> array to create the <see cref="MemoryStream"/> from.</param>
    /// <param name="offset">The offset in the <see cref="byte"/> array to start reading from.</param>
    /// <param name="length">The number of bytes to read from the <see cref="byte"/> array.</param>
    /// <param name="canResize">Whether the underlying <see cref="MemoryStream"/> should be made resizable.</param>
    /// <returns>The created <see cref="ObservableStream{T}"/>.</returns>
    public static ObservableStream<MemoryStream> Create(byte[] data, int offset = 0, int length = int.MinValue, bool canResize = false)
    {
        if (length == int.MinValue)
        {
            length = data.Length - offset;
        }

        MemoryStream ms;
        if (canResize)
        {
            ms = new MemoryStream(data, offset, length);
        }
        else
        {
            ms = new MemoryStream();
            ms.Write(data.AsSpan(offset, length));
        }
        return new ObservableStream<MemoryStream>(ms);
    }
    /// <summary>
    /// Creates an <see cref="ObservableStream{T}"/> that wraps a <see cref="FileStream"/>.
    /// </summary>
    /// <param name="path">The path to the file to open.</param>
    /// <param name="fileMode">A <see cref="FileMode"/> enum value that specifies how the operating system should open the file.</param>
    /// <param name="fileAccess">A <see cref="FileAccess"/> enum value that specifies the access level.</param>
    /// <param name="fileShare">A <see cref="FileShare"/> enum value that specifies the sharing mode of the file.</param>
    /// <returns>The created <see cref="ObservableStream{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ObservableStream<FileStream> Create(string path, FileMode fileMode = default, FileAccess fileAccess = default, FileShare fileShare = default) => new ObservableStream<FileStream>(File.Open(path, fileMode, fileAccess, fileShare));
    /// <summary>
    /// Creates an <see cref="ObservableStream{T}"/> that wraps a <see cref="FileStream"/>.
    /// </summary>
    /// <param name="path">The path to the file to open.</param>
    /// <param name="fileStreamOptions">A <see cref="FileStreamOptions"/> instance that specifies how the <see cref="FileStream"/> is opened.</param>
    /// <returns>The created <see cref="ObservableStream{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ObservableStream<FileStream> Create(string path, FileStreamOptions fileStreamOptions) => new ObservableStream<FileStream>(File.Open(path, fileStreamOptions));
}
