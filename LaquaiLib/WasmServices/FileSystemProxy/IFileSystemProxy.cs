namespace LaquaiLib.WasmServices.FileSystemProxy;

/// <summary>
/// Exposes access to the file system by proxy in a Blazor WebAssembly application.
/// </summary>
public interface IFileSystemProxy
{
    public IFile File { get; }
    public IDirectory Directory { get; }
    public IPath Path { get; }
}
