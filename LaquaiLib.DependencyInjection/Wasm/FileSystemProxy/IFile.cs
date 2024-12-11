using System.Text;

namespace LaquaiLib.DependencyInjection.Wasm.FileSystemProxy;

/// <summary>
/// Exposes access to the <see cref="System.IO.File"/> class by proxy in a Blazor WebAssembly application.
/// </summary>
public interface IFile
{
    /// <inheritdoc cref="System.IO.File.AppendAllLines(string, IEnumerable{string})" />
    public void AppendAllLines(string path, IEnumerable<string> contents);
    /// <inheritdoc cref="System.IO.File.AppendAllLines(string, IEnumerable{string}, Encoding)" />
    public void AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding);
    /// <inheritdoc cref="System.IO.File.AppendAllLinesAsync(string, IEnumerable{string}, CancellationToken)" />
    public Task AppendAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken);
    /// <inheritdoc cref="System.IO.File.AppendAllLinesAsync(string, IEnumerable{string}, Encoding, CancellationToken)" />
    public Task AppendAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken);
    /// <inheritdoc cref="System.IO.File.AppendAllText(string, string)" />
    public void AppendAllText(string path, string contents);
    /// <inheritdoc cref="System.IO.File.AppendAllText(string, string, Encoding)" />
    public void AppendAllText(string path, string contents, Encoding encoding);
    /// <inheritdoc cref="System.IO.File.AppendAllTextAsync(string, string, CancellationToken)" />
    public Task AppendAllTextAsync(string path, string contents, CancellationToken cancellationToken);
    /// <inheritdoc cref="System.IO.File.AppendAllTextAsync(string, string, Encoding, CancellationToken)" />
    public Task AppendAllTextAsync(string path, string contents, Encoding encoding, CancellationToken cancellationToken);
    /// <inheritdoc cref="System.IO.File.AppendText(string)" />
    public StreamWriter AppendText(string path);
    /// <inheritdoc cref="System.IO.File.Copy(string, string)" />
    public void Copy(string sourceFileName, string destFileName);
    /// <inheritdoc cref="System.IO.File.Copy(string, string, bool)" />
    public void Copy(string sourceFileName, string destFileName, bool overwrite);
    /// <inheritdoc cref="System.IO.File.Create(string)" />
    public FileStream Create(string path);
    /// <inheritdoc cref="System.IO.File.Create(string, int)" />
    public FileStream Create(string path, int bufferSize);
    /// <inheritdoc cref="System.IO.File.Create(string, int, FileOptions)" />
    public FileStream Create(string path, int bufferSize, FileOptions options);
    /// <inheritdoc cref="System.IO.File.CreateSymbolicLink(string, string)" />
    public FileSystemInfo CreateSymbolicLink(string path, string pathToTarget);
    /// <inheritdoc cref="System.IO.File.CreateText(string)" />
    public StreamWriter CreateText(string path);
    /// <inheritdoc cref="System.IO.File.Decrypt(string)" />
    public void Decrypt(string path);
    /// <inheritdoc cref="System.IO.File.Delete(string)" />
    public void Delete(string path);
    /// <inheritdoc cref="System.IO.File.Encrypt(string)" />
    public void Encrypt(string path);
    /// <inheritdoc cref="System.IO.File.Exists(string)" />
    public bool Exists(string path);
    /// <inheritdoc cref="System.IO.File.GetAttributes(string)" />
    public FileAttributes GetAttributes(string path);
    /// <inheritdoc cref="System.IO.File.GetCreationTime(string)" />
    public DateTime GetCreationTime(string path);
    /// <inheritdoc cref="System.IO.File.GetCreationTimeUtc(string)" />
    public DateTime GetCreationTimeUtc(string path);
    /// <inheritdoc cref="System.IO.File.GetLastAccessTime(string)" />
    public DateTime GetLastAccessTime(string path);
    /// <inheritdoc cref="System.IO.File.GetLastAccessTimeUtc(string)" />
    public DateTime GetLastAccessTimeUtc(string path);
    /// <inheritdoc cref="System.IO.File.GetLastWriteTime(string)" />
    public DateTime GetLastWriteTime(string path);
    /// <inheritdoc cref="System.IO.File.GetLastWriteTimeUtc(string)" />
    public DateTime GetLastWriteTimeUtc(string path);
    /// <inheritdoc cref="System.IO.File.Move(string, string)" />
    public void Move(string sourceFileName, string destFileName);
    /// <inheritdoc cref="System.IO.File.Move(string, string, bool)" />
    public void Move(string sourceFileName, string destFileName, bool overwrite);
    /// <inheritdoc cref="System.IO.File.Open(string, FileMode)" />
    public FileStream Open(string path, FileMode mode);
    /// <inheritdoc cref="System.IO.File.Open(string, FileMode, FileAccess)" />
    public FileStream Open(string path, FileMode mode, FileAccess access);
    /// <inheritdoc cref="System.IO.File.Open(string, FileMode, FileAccess, FileShare)" />
    public FileStream Open(string path, FileMode mode, FileAccess access, FileShare share);
    /// <inheritdoc cref="System.IO.File.Open(string, FileStreamOptions)" />
    public FileStream Open(string path, FileStreamOptions options);
    /// <inheritdoc cref="System.IO.File.OpenHandle(string, FileMode, FileAccess, FileShare, FileOptions, long)" />
    public Microsoft.Win32.SafeHandles.SafeFileHandle OpenHandle(string path, FileMode mode, FileAccess access, FileShare share, FileOptions options, long preallocationSize);
    /// <inheritdoc cref="System.IO.File.OpenRead(string)" />
    public FileStream OpenRead(string path);
    /// <inheritdoc cref="System.IO.File.OpenText(string)" />
    public StreamReader OpenText(string path);
    /// <inheritdoc cref="System.IO.File.OpenWrite(string)" />
    public FileStream OpenWrite(string path);
    /// <inheritdoc cref="System.IO.File.ReadAllBytes(string)" />
    public byte[] ReadAllBytes(string path);
    /// <inheritdoc cref="System.IO.File.ReadAllBytesAsync(string, CancellationToken)" />
    public Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken);
    /// <inheritdoc cref="System.IO.File.ReadAllLines(string)" />
    public string[] ReadAllLines(string path);
    /// <inheritdoc cref="System.IO.File.ReadAllLines(string, Encoding)" />
    public string[] ReadAllLines(string path, Encoding encoding);
    /// <inheritdoc cref="System.IO.File.ReadAllLinesAsync(string, CancellationToken)" />
    public Task<string[]> ReadAllLinesAsync(string path, CancellationToken cancellationToken);
    /// <inheritdoc cref="System.IO.File.ReadAllLinesAsync(string, Encoding, CancellationToken)" />
    public Task<string[]> ReadAllLinesAsync(string path, Encoding encoding, CancellationToken cancellationToken);
    /// <inheritdoc cref="System.IO.File.ReadAllText(string)" />
    public string ReadAllText(string path);
    /// <inheritdoc cref="System.IO.File.ReadAllText(string, Encoding)" />
    public string ReadAllText(string path, Encoding encoding);
    /// <inheritdoc cref="System.IO.File.ReadAllTextAsync(string, CancellationToken)" />
    public Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken);
    /// <inheritdoc cref="System.IO.File.ReadAllTextAsync(string, Encoding, CancellationToken)" />
    public Task<string> ReadAllTextAsync(string path, Encoding encoding, CancellationToken cancellationToken);
    /// <inheritdoc cref="System.IO.File.ReadLines(string)" />
    public IEnumerable<string> ReadLines(string path);
    /// <inheritdoc cref="System.IO.File.ReadLines(string, Encoding)" />
    public IEnumerable<string> ReadLines(string path, Encoding encoding);
    /// <inheritdoc cref="System.IO.File.Replace(string, string, string)" />
    public void Replace(string sourceFileName, string destinationFileName, string destinationBackupFileName);
    /// <inheritdoc cref="System.IO.File.Replace(string, string, string, bool)" />
    public void Replace(string sourceFileName, string destinationFileName, string destinationBackupFileName, bool ignoreMetadataErrors);
    /// <inheritdoc cref="System.IO.File.ResolveLinkTarget(string, bool)" />
    public FileSystemInfo ResolveLinkTarget(string linkPath, bool returnFinalTarget);
    /// <inheritdoc cref="System.IO.File.SetAttributes(string, FileAttributes)" />
    public void SetAttributes(string path, FileAttributes fileAttributes);
    /// <inheritdoc cref="System.IO.File.SetCreationTime(string, DateTime)" />
    public void SetCreationTime(string path, DateTime creationTime);
    /// <inheritdoc cref="System.IO.File.SetCreationTimeUtc(string, DateTime)" />
    public void SetCreationTimeUtc(string path, DateTime creationTimeUtc);
    /// <inheritdoc cref="System.IO.File.SetLastAccessTime(string, DateTime)" />
    public void SetLastAccessTime(string path, DateTime lastAccessTime);
    /// <inheritdoc cref="System.IO.File.SetLastAccessTimeUtc(string, DateTime)" />
    public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc);
    /// <inheritdoc cref="System.IO.File.SetLastWriteTime(string, DateTime)" />
    public void SetLastWriteTime(string path, DateTime lastWriteTime);
    /// <inheritdoc cref="System.IO.File.SetLastWriteTimeUtc(string, DateTime)" />
    public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc);
    /// <inheritdoc cref="System.IO.File.WriteAllBytes(string, byte[])" />
    public void WriteAllBytes(string path, byte[] bytes);
    /// <inheritdoc cref="System.IO.File.WriteAllBytesAsync(string, byte[], CancellationToken)" />
    public Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken);
    /// <inheritdoc cref="System.IO.File.WriteAllLines(string, string[])" />
    public void WriteAllLines(string path, string[] contents);
    /// <inheritdoc cref="System.IO.File.WriteAllLines(string, IEnumerable{string})" />
    public void WriteAllLines(string path, IEnumerable<string> contents);
    /// <inheritdoc cref="System.IO.File.WriteAllLines(string, string[], Encoding)" />
    public void WriteAllLines(string path, string[] contents, Encoding encoding);
    /// <inheritdoc cref="System.IO.File.WriteAllLines(string, IEnumerable{string}, Encoding)" />
    public void WriteAllLines(string path, IEnumerable<string> contents, Encoding encoding);
    /// <inheritdoc cref="System.IO.File.WriteAllLinesAsync(string, IEnumerable{string}, CancellationToken)" />
    public Task WriteAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken);
    /// <inheritdoc cref="System.IO.File.WriteAllLinesAsync(string, IEnumerable{string}, Encoding, CancellationToken)" />
    public Task WriteAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken);
    /// <inheritdoc cref="System.IO.File.WriteAllText(string, string)" />
    public void WriteAllText(string path, string contents);
    /// <inheritdoc cref="System.IO.File.WriteAllText(string, string, Encoding)" />
    public void WriteAllText(string path, string contents, Encoding encoding);
    /// <inheritdoc cref="System.IO.File.WriteAllTextAsync(string, string, CancellationToken)" />
    public Task WriteAllTextAsync(string path, string contents, CancellationToken cancellationToken);
    /// <inheritdoc cref="System.IO.File.WriteAllTextAsync(string, string, Encoding, CancellationToken)" />
    public Task WriteAllTextAsync(string path, string contents, Encoding encoding, CancellationToken cancellationToken);
}
