using System.Management;

namespace LaquaiLib.Streams;

/// <summary>
/// Represents a <see cref="Stream"/> that can be either a <see cref="MemoryStream"/> or a <see cref="FileStream"/>, depending on the size of the data expected to be written to it.
/// </summary>
public class MemoryOrFileStream : Stream, IDisposable
{
    /// <summary>
    /// The number of bytes at which the stream will switch from a <see cref="MemoryStream"/> to a <see cref="FileStream"/>.
    /// </summary>
    /// <remarks>
    /// You may freely change this value at runtime. Its initial value is 1/64th of the total physical memory of the system (e.g., if your system has 32 GB of total physical memory, this will initially have the value <c>32768 / 64 = 512 MB</c>). If an exception is thrown during retrieval of the total physical memory, the value will default to 64 MB.
    /// </remarks>
    public static int Cutoff { get; set; }

    /// <summary>
    /// Resets the <see cref="Cutoff"/> to the initial value. See the documentation of <see cref="Cutoff"/> for more information.
    /// </summary>
    /// <returns>The new value of <see cref="Cutoff"/>.</returns>
    public static int ResetCutoff()
    {
        try
        {
            using (var computerSystem = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
            {
                foreach (var obj in computerSystem.Get())
                {
                    return Cutoff = Convert.ToInt32(obj["TotalPhysicalMemory"]) / 64;
                }
            }
        }
        catch
        {
        }
        return Cutoff = 64 * 1024 * 1024; // 64 MB
    }

    /// <summary>
    /// Initializes the <see cref="MemoryOrFileStream"/> Type.
    /// </summary>
    static MemoryOrFileStream()
    {
        _ = ResetCutoff();
    }

    /// <summary>
    /// The wrapped <see cref="Stream"/>.
    /// </summary>
    public Stream Stream { get; }

    /// <summary>
    /// The actual <see cref="Type"/> of the wrapped <see cref="Stream"/>, either <see cref="MemoryStream"/> or <see cref="FileStream"/>.
    /// </summary>
    public Type StreamType => Stream.GetType();

    /// <summary>
    /// Initializes a new <see cref="MemoryOrFileStream"/> with the given expected payload size.
    /// </summary>
    /// <param name="payloadSize">The expected size of the payload to be written to this stream. If it exceeds a set <see cref="Cutoff"/>, the internal <see cref="Stream"/> is created as a <see cref="FileStream"/>.</param>
    public MemoryOrFileStream(int payloadSize)
    {
        Stream = payloadSize >= Cutoff
            ? new FileStream(Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.Asynchronous)
            : new MemoryStream(payloadSize);
    }

    /// <inheritdoc/>
    public override bool CanRead => Stream.CanRead;
    /// <inheritdoc/>
    public override bool CanSeek => Stream.CanSeek;
    /// <inheritdoc/>
    public override bool CanWrite => Stream.CanWrite;
    /// <inheritdoc/>
    public override long Length => Stream.Length;
    /// <inheritdoc/>
    public override long Position {
        get => Stream.Position;
        set => Stream.Position = value;
    }
    /// <inheritdoc/>
    public override void Flush() => Stream.Flush();
    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count) => Stream.Read(buffer, offset, count);
    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin) => Stream.Seek(offset, origin);
    /// <inheritdoc/>
    public override void SetLength(long value) => Stream.SetLength(value);
    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count) => Stream.Write(buffer, offset, count);

    #region Dispose pattern
    private bool _disposed;

    /// <summary>
    /// Releases the unmanaged and optionally the managed resources used by this <see cref="MemoryOrFileStream"/> instance.
    /// </summary>
    /// <param name="disposing">Whether to release the managed resources used by this <see cref="MemoryOrFileStream"/> instance.</param>
    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose of managed resources (Streams etc.)
                Stream.Dispose();
            }

            // Dispose of unmanaged resources (native allocated memory etc.)

            _disposed = true;
        }
    }

    /// <summary>
    /// Finalizes this <see cref="MemoryOrFileStream"/> instance, releasing any unmanaged resources.
    /// </summary>
    ~MemoryOrFileStream()
    {
        Dispose(false);
    }

    /// <summary>
    /// Releases the managed and unmanaged resources used by this <see cref="MemoryOrFileStream"/> instance.
    /// </summary>
    public new void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
