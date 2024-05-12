namespace LaquaiLib.Wrappers;

/// <summary>
/// Represents a temporary file that is automatically deleted when its wrapper object is disposed.
/// </summary>
public class TempFile : IDisposable
{
    /// <summary>
    /// Initializes a new <see cref="TempFile"/> with the file name and extension being assigned by the OS. It is usually a <see cref="Guid"/> with the extension <c>.tmp</c>.
    /// </summary>
    public TempFile()
        : this(System.IO.Path.GetTempFileName(), null) { }

    /// <summary>
    /// Initializes a new <see cref="TempFile"/> with the file name being assigned by the OS (it is usually a <see cref="Guid"/>), and changes its extension to the one specified.
    /// </summary>
    /// <param name="fileExtension">The file extension for this <see cref="TempFile"/>.</param>
    public TempFile(string fileExtension)
        : this(System.IO.Path.GetTempFileName(), fileExtension) { }

    /// <summary>
    /// Initializes a new <see cref="TempFile"/> as a wrapper around the specified file path. If the target file does not exist, it is created. A deletion attempt is still made when the wrapping <see cref="TempFile"/> is disposed.
    /// </summary>
    /// <param name="path">The path to the file to wrap with this <see cref="TempFile"/>.</param>
    /// <param name="fileExtension">The file extension for this <see cref="TempFile"/>. This is the extension <paramref name="path"/> is changed to before opening the file stream. If <see langword="null"/> or white space, the existing extension in <paramref name="path"/> is kept.</param>
    public TempFile(string path, string? fileExtension)
    {
        _path = string.IsNullOrWhiteSpace(fileExtension) ? path : System.IO.Path.ChangeExtension(path, fileExtension);
        _stream = new(_path, FileMode.OpenOrCreate);
    }

    private string? _path;
    private FileStream? _stream;

    /// <summary>
    /// The path to the file this <see cref="TempFile"/> wraps.
    /// </summary>
    public string Path {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, _path!);
            return _path!;
        }
    }
    /// <summary>
    /// The <see cref="FileStream"/> for the file this <see cref="TempFile"/> wraps.
    /// </summary>
    public FileStream Stream {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, _stream!);
            return _stream!;
        }
    }

    #region Dispose pattern
    /// <summary>
    /// Whether this <see cref="TempFile"/> has been disposed.
    /// </summary>
    public bool IsDisposed => string.IsNullOrWhiteSpace(_path) && _stream is null;

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (!string.IsNullOrWhiteSpace(_path))
            {
                try
                {
                    File.Delete(_path);
                }
                catch { }
                _path = null;
            }
            if (_stream is not null)
            {
                _stream.Dispose();
                _stream = null;
            }
        }
    }

    /// <summary>
    /// Finalizes this <see cref="TempFile"/>.
    /// </summary>
    ~TempFile()
    {
        Dispose(false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose(true);
    }
    #endregion
}
