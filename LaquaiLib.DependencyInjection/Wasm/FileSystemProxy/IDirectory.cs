namespace LaquaiLib.DependencyInjection.Wasm.FileSystemProxy;

/// <summary>
/// Exposes access to the <see cref="System.IO.Directory"/> class by proxy in a Blazor WebAssembly application.
/// </summary>
public interface IDirectory
{
    /// <inheritdoc cref="System.IO.Directory.CreateDirectory(string)" />
    public DirectoryInfo CreateDirectory(string path);
    /// <inheritdoc cref="System.IO.Directory.CreateSymbolicLink(string, string)" />
    public FileSystemInfo CreateSymbolicLink(string path, string pathToTarget);
    /// <inheritdoc cref="System.IO.Directory.Delete(string)" />
    public void Delete(string path);
    /// <inheritdoc cref="System.IO.Directory.Delete(string, bool)" />
    public void Delete(string path, bool recursive);
    /// <inheritdoc cref="System.IO.Directory.EnumerateDirectories(string)" />
    public IEnumerable<string> EnumerateDirectories(string path);
    /// <inheritdoc cref="System.IO.Directory.EnumerateDirectories(string, string)" />
    public IEnumerable<string> EnumerateDirectories(string path, string searchPattern);
    /// <inheritdoc cref="System.IO.Directory.EnumerateDirectories(string, string, SearchOption)" />
    public IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption);
    /// <inheritdoc cref="System.IO.Directory.EnumerateDirectories(string, string, EnumerationOptions)" />
    public IEnumerable<string> EnumerateDirectories(string path, string searchPattern, EnumerationOptions enumerationOptions);
    /// <inheritdoc cref="System.IO.Directory.EnumerateFiles(string)" />
    public IEnumerable<string> EnumerateFiles(string path);
    /// <inheritdoc cref="System.IO.Directory.EnumerateFiles(string, string)" />
    public IEnumerable<string> EnumerateFiles(string path, string searchPattern);
    /// <inheritdoc cref="System.IO.Directory.EnumerateFiles(string, string, SearchOption)" />
    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption);
    /// <inheritdoc cref="System.IO.Directory.EnumerateFiles(string, string, EnumerationOptions)" />
    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, EnumerationOptions enumerationOptions);
    /// <inheritdoc cref="System.IO.Directory.EnumerateFileSystemEntries(string)" />
    public IEnumerable<string> EnumerateFileSystemEntries(string path);
    /// <inheritdoc cref="System.IO.Directory.EnumerateFileSystemEntries(string, string)" />
    public IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern);
    /// <inheritdoc cref="System.IO.Directory.EnumerateFileSystemEntries(string, string, SearchOption)" />
    public IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, SearchOption searchOption);
    /// <inheritdoc cref="System.IO.Directory.EnumerateFileSystemEntries(string, string, EnumerationOptions)" />
    public IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, EnumerationOptions enumerationOptions);
    /// <inheritdoc cref="System.IO.Directory.Exists(string)" />
    public bool Exists(string path);
    /// <inheritdoc cref="System.IO.Directory.GetCreationTime(string)" />
    public DateTime GetCreationTime(string path);
    /// <inheritdoc cref="System.IO.Directory.GetCreationTimeUtc(string)" />
    public DateTime GetCreationTimeUtc(string path);
    /// <inheritdoc cref="System.IO.Directory.GetCurrentDirectory()" />
    public string GetCurrentDirectory();
    /// <inheritdoc cref="System.IO.Directory.GetDirectories(string)" />
    public string[] GetDirectories(string path);
    /// <inheritdoc cref="System.IO.Directory.GetDirectories(string, string)" />
    public string[] GetDirectories(string path, string searchPattern);
    /// <inheritdoc cref="System.IO.Directory.GetDirectories(string, string, SearchOption)" />
    public string[] GetDirectories(string path, string searchPattern, SearchOption searchOption);
    /// <inheritdoc cref="System.IO.Directory.GetDirectories(string, string, EnumerationOptions)" />
    public string[] GetDirectories(string path, string searchPattern, EnumerationOptions enumerationOptions);
    /// <inheritdoc cref="System.IO.Directory.GetDirectoryRoot(string)" />
    public string GetDirectoryRoot(string path);
    /// <inheritdoc cref="System.IO.Directory.GetFiles(string)" />
    public string[] GetFiles(string path);
    /// <inheritdoc cref="System.IO.Directory.GetFiles(string, string)" />
    public string[] GetFiles(string path, string searchPattern);
    /// <inheritdoc cref="System.IO.Directory.GetFiles(string, string, SearchOption)" />
    public string[] GetFiles(string path, string searchPattern, SearchOption searchOption);
    /// <inheritdoc cref="System.IO.Directory.GetFiles(string, string, EnumerationOptions)" />
    public string[] GetFiles(string path, string searchPattern, EnumerationOptions enumerationOptions);
    /// <inheritdoc cref="System.IO.Directory.GetFileSystemEntries(string)" />
    public string[] GetFileSystemEntries(string path);
    /// <inheritdoc cref="System.IO.Directory.GetFileSystemEntries(string, string)" />
    public string[] GetFileSystemEntries(string path, string searchPattern);
    /// <inheritdoc cref="System.IO.Directory.GetFileSystemEntries(string, string, SearchOption)" />
    public string[] GetFileSystemEntries(string path, string searchPattern, SearchOption searchOption);
    /// <inheritdoc cref="System.IO.Directory.GetFileSystemEntries(string, string, EnumerationOptions)" />
    public string[] GetFileSystemEntries(string path, string searchPattern, EnumerationOptions enumerationOptions);
    /// <inheritdoc cref="System.IO.Directory.GetLastAccessTime(string)" />
    public DateTime GetLastAccessTime(string path);
    /// <inheritdoc cref="System.IO.Directory.GetLastAccessTimeUtc(string)" />
    public DateTime GetLastAccessTimeUtc(string path);
    /// <inheritdoc cref="System.IO.Directory.GetLastWriteTime(string)" />
    public DateTime GetLastWriteTime(string path);
    /// <inheritdoc cref="System.IO.Directory.GetLastWriteTimeUtc(string)" />
    public DateTime GetLastWriteTimeUtc(string path);
    /// <inheritdoc cref="System.IO.Directory.GetLogicalDrives()" />
    public string[] GetLogicalDrives();
    /// <inheritdoc cref="System.IO.Directory.GetParent(string)" />
    public DirectoryInfo GetParent(string path);
    /// <inheritdoc cref="System.IO.Directory.Move(string, string)" />
    public void Move(string sourceDirName, string destDirName);
    /// <inheritdoc cref="System.IO.Directory.ResolveLinkTarget(string, bool)" />
    public FileSystemInfo ResolveLinkTarget(string linkPath, bool returnFinalTarget);
    /// <inheritdoc cref="System.IO.Directory.SetCreationTime(string, DateTime)" />
    public void SetCreationTime(string path, DateTime creationTime);
    /// <inheritdoc cref="System.IO.Directory.SetCreationTimeUtc(string, DateTime)" />
    public void SetCreationTimeUtc(string path, DateTime creationTimeUtc);
    /// <inheritdoc cref="System.IO.Directory.SetCurrentDirectory(string)" />
    public void SetCurrentDirectory(string path);
    /// <inheritdoc cref="System.IO.Directory.SetLastAccessTime(string, DateTime)" />
    public void SetLastAccessTime(string path, DateTime lastAccessTime);
    /// <inheritdoc cref="System.IO.Directory.SetLastAccessTimeUtc(string, DateTime)" />
    public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc);
    /// <inheritdoc cref="System.IO.Directory.SetLastWriteTime(string, DateTime)" />
    public void SetLastWriteTime(string path, DateTime lastWriteTime);
    /// <inheritdoc cref="System.IO.Directory.SetLastWriteTimeUtc(string, DateTime)" />
    public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc);
}
