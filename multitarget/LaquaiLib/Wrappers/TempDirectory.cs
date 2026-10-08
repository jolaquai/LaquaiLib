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
        Path = path;
        if (!Directory.Exists(path))
            _ = Directory.CreateDirectory(path);
    }

    /// <summary>
    /// The path to the file this <see cref="TempDirectory"/> wraps.
    /// </summary>
    public string Path
    {
        get
        {
            ObjectDisposedException.ThrowIf(disposed == 1, this);
            return field;
        }
        private set;
    }

    private int disposed;
    /// <inheritdoc/>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) == 1)
            return;

        try
        {
            Directory.Delete(Path, true);
        }
        catch { }
        GC.SuppressFinalize(this);
    }
}
