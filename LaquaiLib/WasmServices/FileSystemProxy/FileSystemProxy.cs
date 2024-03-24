namespace LaquaiLib.WasmServices.FileSystemProxy;

/// <summary>
/// Implements the <see cref="IFileSystemProxy"/> interface, which allows direct file system access from a Blazor WebAssembly application by proxying functionality from <see cref="System.IO"/>.
/// </summary>
public class FileSystemProxy : IFileSystemProxy
{
    internal FileSystemProxy(IFile file, IDirectory directory, IPath path)
    {
        File = file;
        Directory = directory;
        Path = path;
    }

    /// <summary>
    /// Gets an <see cref="IFile"/> implementation that proxies functionality from <see cref="System.IO.File"/>.
    /// </summary>
    public IFile File { get; }
    /// <summary>
    /// Gets an <see cref="IDirectory"/> implementation that proxies functionality from <see cref="System.IO.Directory"/>.
    /// </summary>
    public IDirectory Directory { get; }
    /// <summary>
    /// Gets an <see cref="IPath"/> implementation that proxies functionality from <see cref="System.IO.Path"/>.
    /// </summary>
    public IPath Path { get; }
}
