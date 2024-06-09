namespace LaquaiLib.Wrappers;

/// <summary>
/// Represents a temporary directory that is automatically deleted when its wrapper object is disposed.
/// </summary>
public class TempDirectory : IDisposable
{
    /// <summary>
    /// Initializes a new <see cref="TempDirectory"/> with a fully random name.
    /// </summary>
    public TempDirectory()
        : this(System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString())) { }

    /// <summary>
    /// Initializes a new <see cref="TempDirectory"/> as a wrapper around the specified directory. If the target directory does not exist, it is created. A deletion attempt is still made when the wrapping <see cref="TempDirectory"/> is disposed.
    /// </summary>
    /// <param name="path">The path to the directory to wrap with this <see cref="TempDirectory"/>.</param>
    public TempDirectory(string path)
    {
        _path = path;
        if (!Directory.Exists(_path))
        {
            Directory.CreateDirectory(_path);
        }
    }

    private string? _path;

    /// <summary>
    /// The path to the file this <see cref="TempDirectory"/> wraps.
    /// </summary>
    public string Path {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, _path!);
            return _path!;
        }
    }

    #region Dispose pattern
    /// <summary>
    /// Whether this <see cref="TempDirectory"/> has been disposed.
    /// </summary>
    public bool IsDisposed => string.IsNullOrWhiteSpace(_path);

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (!string.IsNullOrWhiteSpace(_path))
            {
                try
                {
                    Directory.Delete(_path, true);
                }
                catch { }
                _path = null;
            }
        }
    }

    /// <summary>
    /// Finalizes this <see cref="TempDirectory"/>.
    /// </summary>
    ~TempDirectory()
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
