using System.IO;

namespace LaquaiLib.Util;

/// <summary>
/// Represents a temporary directory that is automatically deleted when its wrapper object is disposed.
/// </summary>
public class TempDir : IDisposable
{
    /// <summary>
    /// Instantiates a new <see cref="TempDir"/> with a fully random name.
    /// </summary>
    public TempDir()
        : this(System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString())) { }

    /// <summary>
    /// Instantiates a new <see cref="TempDir"/> as a wrapper around the specified directory. If the target directory does not exist, it is created. A deletion attempt is still made when the wrapping <see cref="TempDir"/> is disposed.
    /// </summary>
    /// <param name="path">The path to the directory to wrap with this <see cref="TempDir"/>.</param>
    public TempDir(string path)
    {
        _path = path;
        if (!Directory.Exists(_path))
        {
            Directory.CreateDirectory(_path);
        }
    }

    private string _path;

    /// <summary>
    /// The path to the file this <see cref="TempDir"/> wraps.
    /// </summary>
    public string Path {
        get {
            ObjectDisposedException.ThrowIf(string.IsNullOrWhiteSpace(_path), _path);
            return _path;
        }
    }
    /// <summary>
    /// Whether this <see cref="TempDir"/> has been disposed.
    /// </summary>
    public bool IsDisposed => string.IsNullOrWhiteSpace(_path);

    #region Dispose pattern
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

    ~TempDir()
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
