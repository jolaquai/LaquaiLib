namespace LaquaiLib.WasmServices.FileSystemProxy;

/// <summary>
/// Exposes access to the file system by proxy in a Blazor WebAssembly application.
/// This only works when the Blazor WASM application is being hosted inside a WPF application, which, outside of any Blazor WASM context, provides the actual file system access.
/// This will not work, even if created from a class library project, because the Blazor WASM application is sandboxed and cannot access the file system directly, even by proxy.
/// </summary>
public interface IFileSystemProxy
{
    /// <summary>
    /// An <see cref="IFile"/> implementation that proxies functionality from <see cref="System.IO.File"/>.
    /// </summary>
    public IFile File { get; }
    /// <summary>
    /// An <see cref="IDirectory"/> implementation that proxies functionality from <see cref="System.IO.Directory"/>.
    /// </summary>
    public IDirectory Directory { get; }
    /// <summary>
    /// An <see cref="IPath"/> implementation that proxies functionality from <see cref="System.IO.Path"/>.
    /// </summary>
    public IPath Path { get; }
}
