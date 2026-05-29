namespace LaquaiLib.Wrappers;

/// <summary>
/// Represents a temporary file that is automatically deleted when its wrapper object is disposed.
/// </summary>
public class TempFile : IDisposable
{
    /// <summary>
    /// Initializes a new <see cref="TempFile"/> with the file name and extension being assigned by the OS. It is usually a <see cref="Guid"/> with the extension <c>.tmp</c>.
    /// </summary>
    public TempFile() : this(System.IO.Path.GetTempFileName(), null) { }

    /// <summary>
    /// Initializes a new <see cref="TempFile"/> with the file name being assigned by the OS, and changes its extension to the one specified.
    /// </summary>
    /// <param name="fileExtension">The file extension for this <see cref="TempFile"/>.</param>
    public TempFile(string fileExtension) : this(System.IO.Path.GetTempFileName(), fileExtension) { }

    /// <summary>
    /// Initializes a new <see cref="TempFile"/> as a wrapper around the specified file path. If the target file does not exist, it is created. A deletion attempt is still made when the wrapping <see cref="TempFile"/> is disposed.
    /// </summary>
    /// <param name="path">The path to the file to wrap with this <see cref="TempFile"/>.</param>
    /// <param name="fileExtension">The file extension for this <see cref="TempFile"/>. This is the extension <paramref name="path"/> is changed to before opening the file stream. If <see langword="null"/> or white space, the existing extension in <paramref name="path"/> is kept.</param>
    public TempFile(string path, string fileExtension) : this(string.IsNullOrWhiteSpace(fileExtension) ? path : System.IO.Path.ChangeExtension(path, fileExtension), FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose | FileOptions.Asynchronous) { }
    /// <summary>
    /// Initializes a new <see cref="TempFile"/> as a wrapper around the specified file path. If the target file does not exist, it is created. A deletion attempt is still made when the wrapping <see cref="TempFile"/> is disposed.
    /// </summary>
    /// <param name="fullPath">The path to the file to wrap with this <see cref="TempFile"/>.</param>
    /// <param name="fileMode">A <see cref="FileMode"/> value that specifies how to open or create the file.</param>
    /// <param name="fileAccess">A <see cref="FileAccess"/> value that specifies the operations that can be performed on the file.</param>
    /// <param name="fileShare">A <see cref="FileShare"/> value that specifies the type of access other <see cref="FileStream"/> objects have to this file.</param>
    /// <param name="bufferSize">The size of the buffer to use for the file stream.</param>
    /// <param name="fileOptions">A <see cref="FileOptions"/> value that specifies additional options for the file stream.</param>
    public TempFile(string fullPath, FileMode fileMode, FileAccess fileAccess, FileShare fileShare, int bufferSize, FileOptions fileOptions)
    {
        Path = fullPath;
        Stream = new FileStream(Path, fileMode, fileAccess, fileShare, bufferSize, fileOptions);
    }

    /// <summary>
    /// The path to the file this <see cref="TempFile"/> wraps.
    /// </summary>
    public string Path { get; }
    /// <summary>
    /// The <see cref="FileStream"/> for the file this <see cref="TempFile"/> wraps.
    /// </summary>
    public FileStream Stream { get; }

    public void Dispose()
    {
        Stream.Dispose();
        GC.SuppressFinalize(this);
    }
}
