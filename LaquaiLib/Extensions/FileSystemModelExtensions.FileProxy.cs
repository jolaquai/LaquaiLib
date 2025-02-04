using System.Text;

namespace LaquaiLib.Extensions;

// This partial type proxies all static methods of System.IO.File class as extension methods to System.IO.FileInfo
public static partial class FileSystemModelExtensions
{
    /// <inheritdoc cref="File.AppendAllBytes(string, byte[])" />
    public static void AppendAllBytes(this FileInfo fileInfo, byte[] bytes)
        => System.IO.File.AppendAllBytes(fileInfo.FullName, bytes);
    /// <inheritdoc cref="File.AppendAllBytesAsync(string, ReadOnlyMemory{byte}, CancellationToken)" />
    public static void AppendAllBytes(this FileInfo fileInfo, ReadOnlySpan<byte> bytes)
        => System.IO.File.AppendAllBytes(fileInfo.FullName, bytes);
    /// <inheritdoc cref="File.AppendAllBytesAsync(string, byte[], CancellationToken)" />
    public static Task AppendAllBytesAsync(this FileInfo fileInfo, byte[] bytes, CancellationToken cancellationToken)
        => System.IO.File.AppendAllBytesAsync(fileInfo.FullName, bytes, cancellationToken);
    /// <inheritdoc cref="File.AppendAllBytesAsync(string, ReadOnlyMemory{byte}, CancellationToken)" />
    public static Task AppendAllBytesAsync(this FileInfo fileInfo, ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken)
        => System.IO.File.AppendAllBytesAsync(fileInfo.FullName, bytes, cancellationToken);
    /// <inheritdoc cref="File.AppendAllLines(string, IEnumerable{string})" />
    public static void AppendAllLines(this FileInfo fileInfo, IEnumerable<string> contents)
        => System.IO.File.AppendAllLines(fileInfo.FullName, contents);
    /// <inheritdoc cref="File.AppendAllLines(string, IEnumerable{string}, Encoding)" />
    public static void AppendAllLines(this FileInfo fileInfo, IEnumerable<string> contents, Encoding encoding)
        => System.IO.File.AppendAllLines(fileInfo.FullName, contents, encoding);
    /// <inheritdoc cref="File.AppendAllLinesAsync(string, IEnumerable{string}, CancellationToken)" />
    public static Task AppendAllLinesAsync(this FileInfo fileInfo, IEnumerable<string> contents, CancellationToken cancellationToken)
        => System.IO.File.AppendAllLinesAsync(fileInfo.FullName, contents, cancellationToken);
    /// <inheritdoc cref="File.AppendAllLinesAsync(string, IEnumerable{string}, Encoding, CancellationToken)" />
    public static Task AppendAllLinesAsync(this FileInfo fileInfo, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken)
        => System.IO.File.AppendAllLinesAsync(fileInfo.FullName, contents, encoding, cancellationToken);
    /// <inheritdoc cref="File.AppendAllText(string, string)" />
    public static void AppendAllText(this FileInfo fileInfo, string contents)
        => System.IO.File.AppendAllText(fileInfo.FullName, contents);
    /// <inheritdoc cref="File.AppendAllText(string, ReadOnlySpan{char})" />
    public static void AppendAllText(this FileInfo fileInfo, ReadOnlySpan<char> contents)
        => System.IO.File.AppendAllText(fileInfo.FullName, contents);
    /// <inheritdoc cref="File.AppendAllText(string, string, Encoding)" />
    public static void AppendAllText(this FileInfo fileInfo, string contents, Encoding encoding)
        => System.IO.File.AppendAllText(fileInfo.FullName, contents, encoding);
    /// <inheritdoc cref="File.AppendAllText(string, ReadOnlySpan{char}, Encoding)" />
    public static void AppendAllText(this FileInfo fileInfo, ReadOnlySpan<char> contents, Encoding encoding)
        => System.IO.File.AppendAllText(fileInfo.FullName, contents, encoding);
    /// <inheritdoc cref="File.AppendAllTextAsync(string, string, CancellationToken)" />
    public static Task AppendAllTextAsync(this FileInfo fileInfo, string contents, CancellationToken cancellationToken)
        => System.IO.File.AppendAllTextAsync(fileInfo.FullName, contents, cancellationToken);
    /// <inheritdoc cref="File.AppendAllTextAsync(string, ReadOnlyMemory{char}, CancellationToken)" />
    public static Task AppendAllTextAsync(this FileInfo fileInfo, ReadOnlyMemory<char> contents, CancellationToken cancellationToken)
        => System.IO.File.AppendAllTextAsync(fileInfo.FullName, contents, cancellationToken);
    /// <inheritdoc cref="File.AppendAllTextAsync(string, string, Encoding, CancellationToken)" />
    public static Task AppendAllTextAsync(this FileInfo fileInfo, string contents, Encoding encoding, CancellationToken cancellationToken)
        => System.IO.File.AppendAllTextAsync(fileInfo.FullName, contents, encoding, cancellationToken);
    /// <inheritdoc cref="File.AppendAllTextAsync(string, ReadOnlyMemory{char}, Encoding, CancellationToken)" />
    public static Task AppendAllTextAsync(this FileInfo fileInfo, ReadOnlyMemory<char> contents, Encoding encoding, CancellationToken cancellationToken)
        => System.IO.File.AppendAllTextAsync(fileInfo.FullName, contents, encoding, cancellationToken);
    /// <inheritdoc cref="File.AppendText(string)" />
    public static StreamWriter AppendText(this FileInfo fileInfo)
        => System.IO.File.AppendText(fileInfo.FullName);
    /// <inheritdoc cref="File.Copy(string, string)" />
    public static void Copy(this FileInfo fileInfo, string destFileName)
        => System.IO.File.Copy(fileInfo.FullName, destFileName);
    /// <inheritdoc cref="File.Copy(string, string, bool)" />
    public static void Copy(this FileInfo fileInfo, string destFileName, bool overwrite)
        => System.IO.File.Copy(fileInfo.FullName, destFileName, overwrite);
    /// <inheritdoc cref="File.Copy(string, string, bool)" />
    public static void Copy(this FileInfo fileInfo, FileInfo destFileInfo)
        => System.IO.File.Copy(fileInfo.FullName, destFileInfo.FullName);
    /// <inheritdoc cref="File.Copy(string, string, bool)" />
    public static void Copy(this FileInfo fileInfo, FileInfo destFileInfo, bool overwrite)
        => System.IO.File.Copy(fileInfo.FullName, destFileInfo.FullName, overwrite);
    /// <inheritdoc cref="File.Create(string)" />
    public static FileStream Create(this FileInfo fileInfo)
        => System.IO.File.Create(fileInfo.FullName);
    /// <inheritdoc cref="File.Create(string, int)" />
    public static FileStream Create(this FileInfo fileInfo, int bufferSize)
        => System.IO.File.Create(fileInfo.FullName, bufferSize);
    /// <inheritdoc cref="File.Create(string, int, FileOptions)" />
    public static FileStream Create(this FileInfo fileInfo, int bufferSize, FileOptions options)
        => System.IO.File.Create(fileInfo.FullName, bufferSize, options);
    /// <inheritdoc cref="File.CreateSymbolicLink(string, string)" />
    public static FileSystemInfo CreateSymbolicLink(this FileInfo fileInfo, string pathToTarget)
        => System.IO.File.CreateSymbolicLink(fileInfo.FullName, pathToTarget);
    /// <inheritdoc cref="File.CreateText(string)" />
    public static StreamWriter CreateText(this FileInfo fileInfo)
        => System.IO.File.CreateText(fileInfo.FullName);
    /// <inheritdoc cref="File.Decrypt(string)" />
    public static void Decrypt(this FileInfo fileInfo)
        => System.IO.File.Decrypt(fileInfo.FullName);
    /// <inheritdoc cref="File.Delete(string)" />
    public static void Delete(this FileInfo fileInfo)
        => System.IO.File.Delete(fileInfo.FullName);
    /// <inheritdoc cref="File.Encrypt(string)" />
    public static void Encrypt(this FileInfo fileInfo)
        => System.IO.File.Encrypt(fileInfo.FullName);
    /// <inheritdoc cref="File.Exists(string)" />
    public static bool Exists(this FileInfo fileInfo)
        => System.IO.File.Exists(fileInfo.FullName);
    /// <inheritdoc cref="File.GetAttributes(string)" />
    public static FileAttributes GetAttributes(this FileInfo fileInfo)
        => System.IO.File.GetAttributes(fileInfo.FullName);
    /// <inheritdoc cref="File.GetCreationTime(string)" />
    public static DateTime GetCreationTime(this FileInfo fileInfo)
        => System.IO.File.GetCreationTime(fileInfo.FullName);
    /// <inheritdoc cref="File.GetCreationTimeUtc(string)" />
    public static DateTime GetCreationTimeUtc(this FileInfo fileInfo)
        => System.IO.File.GetCreationTimeUtc(fileInfo.FullName);
    /// <inheritdoc cref="File.GetLastAccessTime(string)" />
    public static DateTime GetLastAccessTime(this FileInfo fileInfo)
        => System.IO.File.GetLastAccessTime(fileInfo.FullName);
    /// <inheritdoc cref="File.GetLastAccessTimeUtc(string)" />
    public static DateTime GetLastAccessTimeUtc(this FileInfo fileInfo)
        => System.IO.File.GetLastAccessTimeUtc(fileInfo.FullName);
    /// <inheritdoc cref="File.GetLastWriteTime(string)" />
    public static DateTime GetLastWriteTime(this FileInfo fileInfo)
        => System.IO.File.GetLastWriteTime(fileInfo.FullName);
    /// <inheritdoc cref="File.GetLastWriteTimeUtc(string)" />
    public static DateTime GetLastWriteTimeUtc(this FileInfo fileInfo)
        => System.IO.File.GetLastWriteTimeUtc(fileInfo.FullName);
    /// <inheritdoc cref="File.GetUnixFileMode(string)" />
    public static UnixFileMode GetUnixFileMode(this FileInfo fileInfo)
        => System.IO.File.GetUnixFileMode(fileInfo.FullName);
    /// <inheritdoc cref="File.Move(string, string)" />
    public static void Move(this FileInfo fileInfo, string destFileName)
        => System.IO.File.Move(fileInfo.FullName, destFileName);
    /// <inheritdoc cref="File.Move(string, string, bool)" />
    public static void Move(this FileInfo fileInfo, string destFileName, bool overwrite)
        => System.IO.File.Move(fileInfo.FullName, destFileName, overwrite);
    /// <inheritdoc cref="File.Move(string, string)" />
    public static void Move(this FileInfo fileInfo, FileInfo destFileInfo)
        => System.IO.File.Move(fileInfo.FullName, destFileInfo.FullName);
    /// <inheritdoc cref="File.Move(string, string, bool)" />
    public static void Move(this FileInfo fileInfo, FileInfo destFileInfo, bool overwrite)
        => System.IO.File.Move(fileInfo.FullName, destFileInfo.FullName, overwrite);
    /// <inheritdoc cref="File.Open(string, FileMode)" />
    public static FileStream Open(this FileInfo fileInfo, FileStreamOptions options)
        => System.IO.File.Open(fileInfo.FullName, options);
    /// <inheritdoc cref="File.Open(string, FileMode)" />
    public static FileStream Open(this FileInfo fileInfo, FileMode mode)
        => System.IO.File.Open(fileInfo.FullName, mode);
    /// <inheritdoc cref="File.Open(string, FileMode, FileAccess)" />
    public static FileStream Open(this FileInfo fileInfo, FileMode mode, FileAccess access)
        => System.IO.File.Open(fileInfo.FullName, mode, access);
    /// <inheritdoc cref="File.Open(string, FileMode, FileAccess, FileShare)" />
    public static FileStream Open(this FileInfo fileInfo, FileMode mode, FileAccess access, FileShare share)
        => System.IO.File.Open(fileInfo.FullName, mode, access, share);
    /// <inheritdoc cref="File.OpenRead(string)" />
    public static FileStream OpenRead(this FileInfo fileInfo)
        => System.IO.File.OpenRead(fileInfo.FullName);
    /// <inheritdoc cref="File.OpenText(string)" />
    public static StreamReader OpenText(this FileInfo fileInfo)
        => System.IO.File.OpenText(fileInfo.FullName);
    /// <inheritdoc cref="File.OpenWrite(string)" />
    public static FileStream OpenWrite(this FileInfo fileInfo)
        => System.IO.File.OpenWrite(fileInfo.FullName);
    /// <inheritdoc cref="File.ReadAllBytes(string)" />
    public static byte[] ReadAllBytes(this FileInfo fileInfo)
        => System.IO.File.ReadAllBytes(fileInfo.FullName);
    /// <inheritdoc cref="File.ReadAllBytesAsync(string, CancellationToken)" />
    public static Task<byte[]> ReadAllBytesAsync(this FileInfo fileInfo, CancellationToken cancellationToken)
        => System.IO.File.ReadAllBytesAsync(fileInfo.FullName, cancellationToken);
    /// <inheritdoc cref="File.ReadAllLines(string)" />
    public static string[] ReadAllLines(this FileInfo fileInfo)
        => System.IO.File.ReadAllLines(fileInfo.FullName);
    /// <inheritdoc cref="File.ReadAllLines(string, Encoding)" />
    public static string[] ReadAllLines(this FileInfo fileInfo, Encoding encoding)
        => System.IO.File.ReadAllLines(fileInfo.FullName, encoding);
    /// <inheritdoc cref="File.ReadAllLinesAsync(string, CancellationToken)" />
    public static Task<string[]> ReadAllLinesAsync(this FileInfo fileInfo, CancellationToken cancellationToken)
        => System.IO.File.ReadAllLinesAsync(fileInfo.FullName, cancellationToken);
    /// <inheritdoc cref="File.ReadAllLinesAsync(string, Encoding, CancellationToken)" />
    public static Task<string[]> ReadAllLinesAsync(this FileInfo fileInfo, Encoding encoding, CancellationToken cancellationToken)
        => System.IO.File.ReadAllLinesAsync(fileInfo.FullName, encoding, cancellationToken);
    /// <inheritdoc cref="File.ReadAllText(string)" />
    public static string ReadAllText(this FileInfo fileInfo)
        => System.IO.File.ReadAllText(fileInfo.FullName);
    /// <inheritdoc cref="File.ReadAllText(string, Encoding)" />
    public static string ReadAllText(this FileInfo fileInfo, Encoding encoding)
        => System.IO.File.ReadAllText(fileInfo.FullName, encoding);
    /// <inheritdoc cref="File.ReadAllTextAsync(string, CancellationToken)" />
    public static Task<string> ReadAllTextAsync(this FileInfo fileInfo, CancellationToken cancellationToken)
        => System.IO.File.ReadAllTextAsync(fileInfo.FullName, cancellationToken);
    /// <inheritdoc cref="File.ReadAllTextAsync(string, Encoding, CancellationToken)" />
    public static Task<string> ReadAllTextAsync(this FileInfo fileInfo, Encoding encoding, CancellationToken cancellationToken)
        => System.IO.File.ReadAllTextAsync(fileInfo.FullName, encoding, cancellationToken);
    /// <inheritdoc cref="File.ReadLines(string)" />
    public static IEnumerable<string> ReadLines(this FileInfo fileInfo)
        => System.IO.File.ReadLines(fileInfo.FullName);
    /// <inheritdoc cref="File.ReadLines(string, Encoding)" />
    public static IEnumerable<string> ReadLines(this FileInfo fileInfo, Encoding encoding)
        => System.IO.File.ReadLines(fileInfo.FullName, encoding);
    /// <inheritdoc cref="File.ReadLinesAsync(string, CancellationToken)" />
    public static IAsyncEnumerable<string> ReadLinesAsync(this FileInfo fileInfo, CancellationToken cancellationToken)
        => System.IO.File.ReadLinesAsync(fileInfo.FullName, cancellationToken);
    /// <inheritdoc cref="File.ReadLinesAsync(string, Encoding, CancellationToken)" />
    public static IAsyncEnumerable<string> ReadLinesAsync(this FileInfo fileInfo, Encoding encoding, CancellationToken cancellationToken)
        => System.IO.File.ReadLinesAsync(fileInfo.FullName, encoding, cancellationToken);
    /// <inheritdoc cref="File.Replace(string, string, string)" />
    public static void Replace(this FileInfo fileInfo, string destinationFileName, string destinationBackupFileName)
        => System.IO.File.Replace(fileInfo.FullName, destinationFileName, destinationBackupFileName);
    /// <inheritdoc cref="File.Replace(string, string, string, bool)" />
    public static void Replace(this FileInfo fileInfo, string destinationFileName, string destinationBackupFileName, bool ignoreMetadataErrors)
        => System.IO.File.Replace(fileInfo.FullName, destinationFileName, destinationBackupFileName, ignoreMetadataErrors);
    /// <inheritdoc cref="File.Replace(string, string, string)" />
    public static void Replace(this FileInfo fileInfo, FileInfo destinationFileInfo, FileInfo destinationBackupFileInfo)
        => System.IO.File.Replace(fileInfo.FullName, destinationFileInfo.FullName, destinationBackupFileInfo.FullName);
    /// <inheritdoc cref="File.Replace(string, string, string, bool)" />
    public static void Replace(this FileInfo fileInfo, FileInfo destinationFileInfo, FileInfo destinationBackupFileInfo, bool ignoreMetadataErrors)
        => System.IO.File.Replace(fileInfo.FullName, destinationFileInfo.FullName, destinationBackupFileInfo.FullName, ignoreMetadataErrors);
    /// <inheritdoc cref="File.ResolveLinkTarget(string, bool)" />
    public static FileSystemInfo ResolveLinkTarget(this FileInfo fileInfo, bool returnFinalTarget)
        => System.IO.File.ResolveLinkTarget(fileInfo.FullName, returnFinalTarget);
    /// <inheritdoc cref="File.SetAttributes(string, FileAttributes)" />
    public static void SetAttributes(this FileInfo fileInfo, FileAttributes fileAttributes)
        => System.IO.File.SetAttributes(fileInfo.FullName, fileAttributes);
    /// <inheritdoc cref="File.SetCreationTime(string, DateTime)" />
    public static void SetCreationTime(this FileInfo fileInfo, DateTime creationTime)
        => System.IO.File.SetCreationTime(fileInfo.FullName, creationTime);
    /// <inheritdoc cref="File.SetCreationTimeUtc(string, DateTime)" />
    public static void SetCreationTimeUtc(this FileInfo fileInfo, DateTime creationTimeUtc)
        => System.IO.File.SetCreationTimeUtc(fileInfo.FullName, creationTimeUtc);
    /// <inheritdoc cref="File.SetLastAccessTime(string, DateTime)" />
    public static void SetLastAccessTime(this FileInfo fileInfo, DateTime lastAccessTime)
        => System.IO.File.SetLastAccessTime(fileInfo.FullName, lastAccessTime);
    /// <inheritdoc cref="File.SetLastAccessTimeUtc(string, DateTime)" />
    public static void SetLastAccessTimeUtc(this FileInfo fileInfo, DateTime lastAccessTimeUtc)
        => System.IO.File.SetLastAccessTimeUtc(fileInfo.FullName, lastAccessTimeUtc);
    /// <inheritdoc cref="File.SetLastWriteTime(string, DateTime)" />
    public static void SetLastWriteTime(this FileInfo fileInfo, DateTime lastWriteTime)
        => System.IO.File.SetLastWriteTime(fileInfo.FullName, lastWriteTime);
    /// <inheritdoc cref="File.SetLastWriteTimeUtc(string, DateTime)" />
    public static void SetLastWriteTimeUtc(this FileInfo fileInfo, DateTime lastWriteTimeUtc)
        => System.IO.File.SetLastWriteTimeUtc(fileInfo.FullName, lastWriteTimeUtc);
    /// <inheritdoc cref="File.SetUnixFileMode(string, UnixFileMode)" />
    public static void SetUnixFileMode(this FileInfo fileInfo, UnixFileMode mode)
        => System.IO.File.SetUnixFileMode(fileInfo.FullName, mode);
    /// <inheritdoc cref="File.WriteAllBytes(string, byte[])" />
    public static void WriteAllBytes(this FileInfo fileInfo, byte[] bytes)
        => System.IO.File.WriteAllBytes(fileInfo.FullName, bytes);
    /// <inheritdoc cref="File.WriteAllBytes(string, ReadOnlySpan{byte})" />
    public static void WriteAllBytes(this FileInfo fileInfo, ReadOnlySpan<byte> bytes)
        => System.IO.File.WriteAllBytes(fileInfo.FullName, bytes);
    /// <inheritdoc cref="File.WriteAllBytesAsync(string, byte[], CancellationToken)" />
    public static Task WriteAllBytesAsync(this FileInfo fileInfo, byte[] bytes, CancellationToken cancellationToken)
        => System.IO.File.WriteAllBytesAsync(fileInfo.FullName, bytes, cancellationToken);
    /// <inheritdoc cref="File.WriteAllBytesAsync(string, ReadOnlyMemory{byte}, CancellationToken)" />
    public static Task WriteAllBytesAsync(this FileInfo fileInfo, ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken)
        => System.IO.File.WriteAllBytesAsync(fileInfo.FullName, bytes, cancellationToken);
    /// <inheritdoc cref="File.WriteAllLines(string, IEnumerable{string})" />
    public static void WriteAllLines(this FileInfo fileInfo, string[] contents)
        => System.IO.File.WriteAllLines(fileInfo.FullName, contents);
    /// <inheritdoc cref="File.WriteAllLines(string, IEnumerable{string})" />
    public static void WriteAllLines(this FileInfo fileInfo, IEnumerable<string> contents)
        => System.IO.File.WriteAllLines(fileInfo.FullName, contents);
    /// <inheritdoc cref="File.WriteAllLines(string, IEnumerable{string}, Encoding)" />
    public static void WriteAllLines(this FileInfo fileInfo, string[] contents, Encoding encoding)
        => System.IO.File.WriteAllLines(fileInfo.FullName, contents, encoding);
    /// <inheritdoc cref="File.WriteAllLines(string, IEnumerable{string}, Encoding)" />
    public static void WriteAllLines(this FileInfo fileInfo, IEnumerable<string> contents, Encoding encoding)
        => System.IO.File.WriteAllLines(fileInfo.FullName, contents, encoding);
    /// <inheritdoc cref="File.WriteAllLinesAsync(string, IEnumerable{string}, CancellationToken)" />
    public static Task WriteAllLinesAsync(this FileInfo fileInfo, IEnumerable<string> contents, CancellationToken cancellationToken)
        => System.IO.File.WriteAllLinesAsync(fileInfo.FullName, contents, cancellationToken);
    /// <inheritdoc cref="File.WriteAllLinesAsync(string, IEnumerable{string}, Encoding, CancellationToken)" />
    public static Task WriteAllLinesAsync(this FileInfo fileInfo, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken)
        => System.IO.File.WriteAllLinesAsync(fileInfo.FullName, contents, encoding, cancellationToken);
    /// <inheritdoc cref="File.WriteAllText(string, string)" />
    public static void WriteAllText(this FileInfo fileInfo, string contents)
        => System.IO.File.WriteAllText(fileInfo.FullName, contents);
    /// <inheritdoc cref="File.WriteAllText(string, ReadOnlySpan{char})" />
    public static void WriteAllText(this FileInfo fileInfo, ReadOnlySpan<char> contents)
        => System.IO.File.WriteAllText(fileInfo.FullName, contents);
    /// <inheritdoc cref="File.WriteAllText(string, string, Encoding)" />
    public static void WriteAllText(this FileInfo fileInfo, string contents, Encoding encoding)
        => System.IO.File.WriteAllText(fileInfo.FullName, contents, encoding);
    /// <inheritdoc cref="File.WriteAllText(string, ReadOnlySpan{char}, Encoding)" />
    public static void WriteAllText(this FileInfo fileInfo, ReadOnlySpan<char> contents, Encoding encoding)
        => System.IO.File.WriteAllText(fileInfo.FullName, contents, encoding);
    /// <inheritdoc cref="File.WriteAllTextAsync(string, string, CancellationToken)" />
    public static Task WriteAllTextAsync(this FileInfo fileInfo, string contents, CancellationToken cancellationToken)
        => System.IO.File.WriteAllTextAsync(fileInfo.FullName, contents, cancellationToken);
    /// <inheritdoc cref="File.WriteAllTextAsync(string, ReadOnlyMemory{char}, CancellationToken)" />
    public static Task WriteAllTextAsync(this FileInfo fileInfo, ReadOnlyMemory<char> contents, CancellationToken cancellationToken)
        => System.IO.File.WriteAllTextAsync(fileInfo.FullName, contents, cancellationToken);
    /// <inheritdoc cref="File.WriteAllTextAsync(string, string, Encoding, CancellationToken)" />
    public static Task WriteAllTextAsync(this FileInfo fileInfo, string contents, Encoding encoding, CancellationToken cancellationToken)
        => System.IO.File.WriteAllTextAsync(fileInfo.FullName, contents, encoding, cancellationToken);
    /// <inheritdoc cref="File.WriteAllTextAsync(string, ReadOnlyMemory{char}, Encoding, CancellationToken)" />
    public static Task WriteAllTextAsync(this FileInfo fileInfo, ReadOnlyMemory<char> contents, Encoding encoding, CancellationToken cancellationToken)
        => System.IO.File.WriteAllTextAsync(fileInfo.FullName, contents, encoding, cancellationToken);
}
