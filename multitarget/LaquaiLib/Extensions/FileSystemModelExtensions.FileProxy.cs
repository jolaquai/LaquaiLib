namespace LaquaiLib.Extensions;

// This partial type proxies all static methods of System.IO.File class as extension methods to System.IO.FileInfo
public static partial class FileSystemModelExtensions
{
    extension(FileInfo fileInfo)
    {
        #region Append*
        /// <inheritdoc cref="File.AppendAllBytes(string, byte[])" />
        public void AppendAllBytes(byte[] bytes) => System.IO.File.AppendAllBytes(fileInfo.FullName, bytes);
        /// <inheritdoc cref="File.AppendAllBytesAsync(string, ReadOnlyMemory{byte}, CancellationToken)" />
        public void AppendAllBytes(ReadOnlySpan<byte> bytes) => System.IO.File.AppendAllBytes(fileInfo.FullName, bytes);
        /// <inheritdoc cref="File.AppendAllBytesAsync(string, byte[], CancellationToken)" />
        public Task AppendAllBytesAsync(byte[] bytes, CancellationToken cancellationToken) => System.IO.File.AppendAllBytesAsync(fileInfo.FullName, bytes, cancellationToken);
        /// <inheritdoc cref="File.AppendAllBytesAsync(string, ReadOnlyMemory{byte}, CancellationToken)" />
        public Task AppendAllBytesAsync(ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken) => System.IO.File.AppendAllBytesAsync(fileInfo.FullName, bytes, cancellationToken);
        /// <inheritdoc cref="File.AppendAllLines(string, IEnumerable{string})" />
        public void AppendAllLines(IEnumerable<string> contents) => System.IO.File.AppendAllLines(fileInfo.FullName, contents);
        /// <inheritdoc cref="File.AppendAllLines(string, IEnumerable{string}, Encoding)" />
        public void AppendAllLines(IEnumerable<string> contents, Encoding encoding) => System.IO.File.AppendAllLines(fileInfo.FullName, contents, encoding);
        /// <inheritdoc cref="File.AppendAllLinesAsync(string, IEnumerable{string}, CancellationToken)" />
        public Task AppendAllLinesAsync(IEnumerable<string> contents, CancellationToken cancellationToken) => System.IO.File.AppendAllLinesAsync(fileInfo.FullName, contents, cancellationToken);
        /// <inheritdoc cref="File.AppendAllLinesAsync(string, IEnumerable{string}, Encoding, CancellationToken)" />
        public Task AppendAllLinesAsync(IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken) => System.IO.File.AppendAllLinesAsync(fileInfo.FullName, contents, encoding, cancellationToken);
        /// <inheritdoc cref="File.AppendAllText(string, string)" />
        public void AppendAllText(string contents) => System.IO.File.AppendAllText(fileInfo.FullName, contents);
        /// <inheritdoc cref="File.AppendAllText(string, ReadOnlySpan{char})" />
        public void AppendAllText(ReadOnlySpan<char> contents) => System.IO.File.AppendAllText(fileInfo.FullName, contents);
        /// <inheritdoc cref="File.AppendAllText(string, string, Encoding)" />
        public void AppendAllText(string contents, Encoding encoding) => System.IO.File.AppendAllText(fileInfo.FullName, contents, encoding);
        /// <inheritdoc cref="File.AppendAllText(string, ReadOnlySpan{char}, Encoding)" />
        public void AppendAllText(ReadOnlySpan<char> contents, Encoding encoding) => System.IO.File.AppendAllText(fileInfo.FullName, contents, encoding);
        /// <inheritdoc cref="File.AppendAllTextAsync(string, string, CancellationToken)" />
        public Task AppendAllTextAsync(string contents, CancellationToken cancellationToken) => System.IO.File.AppendAllTextAsync(fileInfo.FullName, contents, cancellationToken);
        /// <inheritdoc cref="File.AppendAllTextAsync(string, ReadOnlyMemory{char}, CancellationToken)" />
        public Task AppendAllTextAsync(ReadOnlyMemory<char> contents, CancellationToken cancellationToken) => System.IO.File.AppendAllTextAsync(fileInfo.FullName, contents, cancellationToken);
        /// <inheritdoc cref="File.AppendAllTextAsync(string, string, Encoding, CancellationToken)" />
        public Task AppendAllTextAsync(string contents, Encoding encoding, CancellationToken cancellationToken) => System.IO.File.AppendAllTextAsync(fileInfo.FullName, contents, encoding, cancellationToken);
        /// <inheritdoc cref="File.AppendAllTextAsync(string, ReadOnlyMemory{char}, Encoding, CancellationToken)" />
        public Task AppendAllTextAsync(ReadOnlyMemory<char> contents, Encoding encoding, CancellationToken cancellationToken) => System.IO.File.AppendAllTextAsync(fileInfo.FullName, contents, encoding, cancellationToken);
        #endregion

        /// <inheritdoc cref="File.Copy(string, string, bool)" />
        public void Copy(FileInfo destFileInfo) => System.IO.File.Copy(fileInfo.FullName, destFileInfo.FullName);
        /// <inheritdoc cref="File.Copy(string, string, bool)" />
        public void Copy(FileInfo destFileInfo, bool overwrite) => System.IO.File.Copy(fileInfo.FullName, destFileInfo.FullName, overwrite);
        /// <inheritdoc cref="File.Create(string, int)" />
        public FileStream Create(int bufferSize) => System.IO.File.Create(fileInfo.FullName, bufferSize);
        /// <inheritdoc cref="File.Create(string, int, FileOptions)" />
        public FileStream Create(int bufferSize, FileOptions options) => System.IO.File.Create(fileInfo.FullName, bufferSize, options);
        /// <inheritdoc cref="File.Move(string, string)" />
        public void Move(FileInfo destFileInfo) => System.IO.File.Move(fileInfo.FullName, destFileInfo.FullName);
        /// <inheritdoc cref="File.Move(string, string, bool)" />
        public void Move(FileInfo destFileInfo, bool overwrite) => System.IO.File.Move(fileInfo.FullName, destFileInfo.FullName, overwrite);
        /// <inheritdoc cref="File.Replace(string, string, string)" />
        public void Replace(FileInfo destinationFileInfo, FileInfo destinationBackupFileInfo) => System.IO.File.Replace(fileInfo.FullName, destinationFileInfo.FullName, destinationBackupFileInfo.FullName);
        /// <inheritdoc cref="File.Replace(string, string, string, bool)" />
        public void Replace(FileInfo destinationFileInfo, FileInfo destinationBackupFileInfo, bool ignoreMetadataErrors) => System.IO.File.Replace(fileInfo.FullName, destinationFileInfo.FullName, destinationBackupFileInfo.FullName, ignoreMetadataErrors);

        #region Properties
        /// <inheritdoc cref="File.GetAttributes(string)" />
        public FileAttributes Attributes
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => System.IO.File.GetAttributes(fileInfo.FullName);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => System.IO.File.SetAttributes(fileInfo.FullName, value);
        }
        /// <inheritdoc cref="File.GetCreationTime(string)" />
        public DateTime CreationTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => System.IO.File.GetCreationTime(fileInfo.FullName);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => System.IO.File.SetCreationTime(fileInfo.FullName, value);
        }
        /// <inheritdoc cref="File.GetCreationTimeUtc(string)" />
        public DateTime CreationTimeUtc
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => System.IO.File.GetCreationTimeUtc(fileInfo.FullName);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => System.IO.File.SetCreationTimeUtc(fileInfo.FullName, value);
        }
        /// <inheritdoc cref="File.GetLastAccessTime(string)" />
        public DateTime LastAccessTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => System.IO.File.GetLastAccessTime(fileInfo.FullName);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => System.IO.File.SetLastAccessTime(fileInfo.FullName, value);
        }
        /// <inheritdoc cref="File.GetLastAccessTimeUtc(string)" />
        public DateTime LastAccessTimeUtc
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => System.IO.File.GetLastAccessTimeUtc(fileInfo.FullName);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => System.IO.File.SetLastAccessTimeUtc(fileInfo.FullName, value);
        }
        /// <inheritdoc cref="File.GetLastWriteTime(string)" />
        public DateTime LastWriteTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => System.IO.File.GetLastWriteTime(fileInfo.FullName);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => System.IO.File.SetLastWriteTime(fileInfo.FullName, value);
        }
        /// <inheritdoc cref="File.GetLastWriteTimeUtc(string)" />
        public DateTime LastWriteTimeUtc
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => System.IO.File.GetLastWriteTimeUtc(fileInfo.FullName);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => System.IO.File.SetLastWriteTimeUtc(fileInfo.FullName, value);
        }
        /// <inheritdoc cref="File.GetUnixFileMode(string)" />
        public UnixFileMode UnixFileMode
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => System.IO.File.GetUnixFileMode(fileInfo.FullName);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => System.IO.File.SetUnixFileMode(fileInfo.FullName, value);
        }
        #endregion

        #region ReadAll*
        /// <inheritdoc cref="File.ReadAllBytes(string)" />
        public byte[] ReadAllBytes() => System.IO.File.ReadAllBytes(fileInfo.FullName);
        /// <inheritdoc cref="File.ReadAllBytesAsync(string, CancellationToken)" />
        public Task<byte[]> ReadAllBytesAsync(CancellationToken cancellationToken) => System.IO.File.ReadAllBytesAsync(fileInfo.FullName, cancellationToken);
        /// <inheritdoc cref="File.ReadAllLines(string)" />
        public string[] ReadAllLines() => System.IO.File.ReadAllLines(fileInfo.FullName);
        /// <inheritdoc cref="File.ReadAllLines(string, Encoding)" />
        public string[] ReadAllLines(Encoding encoding) => System.IO.File.ReadAllLines(fileInfo.FullName, encoding);
        /// <inheritdoc cref="File.ReadAllLinesAsync(string, CancellationToken)" />
        public Task<string[]> ReadAllLinesAsync(CancellationToken cancellationToken) => System.IO.File.ReadAllLinesAsync(fileInfo.FullName, cancellationToken);
        /// <inheritdoc cref="File.ReadAllLinesAsync(string, Encoding, CancellationToken)" />
        public Task<string[]> ReadAllLinesAsync(Encoding encoding, CancellationToken cancellationToken) => System.IO.File.ReadAllLinesAsync(fileInfo.FullName, encoding, cancellationToken);
        /// <inheritdoc cref="File.ReadAllText(string)" />
        public string ReadAllText() => System.IO.File.ReadAllText(fileInfo.FullName);
        /// <inheritdoc cref="File.ReadAllText(string, Encoding)" />
        public string ReadAllText(Encoding encoding) => System.IO.File.ReadAllText(fileInfo.FullName, encoding);
        /// <inheritdoc cref="File.ReadAllTextAsync(string, CancellationToken)" />
        public Task<string> ReadAllTextAsync(CancellationToken cancellationToken) => System.IO.File.ReadAllTextAsync(fileInfo.FullName, cancellationToken);
        /// <inheritdoc cref="File.ReadAllTextAsync(string, Encoding, CancellationToken)" />
        public Task<string> ReadAllTextAsync(Encoding encoding, CancellationToken cancellationToken) => System.IO.File.ReadAllTextAsync(fileInfo.FullName, encoding, cancellationToken);
        #endregion

        #region Rest of ReadLine*
        /// <inheritdoc cref="File.ReadLines(string)" />
        public IEnumerable<string> ReadLines() => System.IO.File.ReadLines(fileInfo.FullName);
        /// <inheritdoc cref="File.ReadLines(string, Encoding)" />
        public IEnumerable<string> ReadLines(Encoding encoding) => System.IO.File.ReadLines(fileInfo.FullName, encoding);
        /// <inheritdoc cref="File.ReadLinesAsync(string, CancellationToken)" />
        public IAsyncEnumerable<string> ReadLinesAsync(CancellationToken cancellationToken) => System.IO.File.ReadLinesAsync(fileInfo.FullName, cancellationToken);
        /// <inheritdoc cref="File.ReadLinesAsync(string, Encoding, CancellationToken)" />
        public IAsyncEnumerable<string> ReadLinesAsync(Encoding encoding, CancellationToken cancellationToken) => System.IO.File.ReadLinesAsync(fileInfo.FullName, encoding, cancellationToken);
        #endregion

        #region WriteAll*
        /// <inheritdoc cref="File.WriteAllBytes(string, byte[])" />
        public void WriteAllBytes(byte[] bytes) => System.IO.File.WriteAllBytes(fileInfo.FullName, bytes);
        /// <inheritdoc cref="File.WriteAllBytes(string, ReadOnlySpan{byte})" />
        public void WriteAllBytes(ReadOnlySpan<byte> bytes) => System.IO.File.WriteAllBytes(fileInfo.FullName, bytes);
        /// <inheritdoc cref="File.WriteAllBytesAsync(string, byte[], CancellationToken)" />
        public Task WriteAllBytesAsync(byte[] bytes, CancellationToken cancellationToken) => System.IO.File.WriteAllBytesAsync(fileInfo.FullName, bytes, cancellationToken);
        /// <inheritdoc cref="File.WriteAllBytesAsync(string, ReadOnlyMemory{byte}, CancellationToken)" />
        public Task WriteAllBytesAsync(ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken) => System.IO.File.WriteAllBytesAsync(fileInfo.FullName, bytes, cancellationToken);
        /// <inheritdoc cref="File.WriteAllLines(string, IEnumerable{string})" />
        public void WriteAllLines(string[] contents) => System.IO.File.WriteAllLines(fileInfo.FullName, contents);
        /// <inheritdoc cref="File.WriteAllLines(string, IEnumerable{string})" />
        public void WriteAllLines(IEnumerable<string> contents) => System.IO.File.WriteAllLines(fileInfo.FullName, contents);
        /// <inheritdoc cref="File.WriteAllLines(string, IEnumerable{string}, Encoding)" />
        public void WriteAllLines(string[] contents, Encoding encoding) => System.IO.File.WriteAllLines(fileInfo.FullName, contents, encoding);
        /// <inheritdoc cref="File.WriteAllLines(string, IEnumerable{string}, Encoding)" />
        public void WriteAllLines(IEnumerable<string> contents, Encoding encoding) => System.IO.File.WriteAllLines(fileInfo.FullName, contents, encoding);
        /// <inheritdoc cref="File.WriteAllLinesAsync(string, IEnumerable{string}, CancellationToken)" />
        public Task WriteAllLinesAsync(IEnumerable<string> contents, CancellationToken cancellationToken) => System.IO.File.WriteAllLinesAsync(fileInfo.FullName, contents, cancellationToken);
        /// <inheritdoc cref="File.WriteAllLinesAsync(string, IEnumerable{string}, Encoding, CancellationToken)" />
        public Task WriteAllLinesAsync(IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken) => System.IO.File.WriteAllLinesAsync(fileInfo.FullName, contents, encoding, cancellationToken);
        /// <inheritdoc cref="File.WriteAllText(string, string)" />
        public void WriteAllText(string contents) => System.IO.File.WriteAllText(fileInfo.FullName, contents);
        /// <inheritdoc cref="File.WriteAllText(string, ReadOnlySpan{char})" />
        public void WriteAllText(ReadOnlySpan<char> contents) => System.IO.File.WriteAllText(fileInfo.FullName, contents);
        /// <inheritdoc cref="File.WriteAllText(string, string, Encoding)" />
        public void WriteAllText(string contents, Encoding encoding) => System.IO.File.WriteAllText(fileInfo.FullName, contents, encoding);
        /// <inheritdoc cref="File.WriteAllText(string, ReadOnlySpan{char}, Encoding)" />
        public void WriteAllText(ReadOnlySpan<char> contents, Encoding encoding) => System.IO.File.WriteAllText(fileInfo.FullName, contents, encoding);
        /// <inheritdoc cref="File.WriteAllTextAsync(string, string, CancellationToken)" />
        public Task WriteAllTextAsync(string contents, CancellationToken cancellationToken) => System.IO.File.WriteAllTextAsync(fileInfo.FullName, contents, cancellationToken);
        /// <inheritdoc cref="File.WriteAllTextAsync(string, ReadOnlyMemory{char}, CancellationToken)" />
        public Task WriteAllTextAsync(ReadOnlyMemory<char> contents, CancellationToken cancellationToken) => System.IO.File.WriteAllTextAsync(fileInfo.FullName, contents, cancellationToken);
        /// <inheritdoc cref="File.WriteAllTextAsync(string, string, Encoding, CancellationToken)" />
        public Task WriteAllTextAsync(string contents, Encoding encoding, CancellationToken cancellationToken) => System.IO.File.WriteAllTextAsync(fileInfo.FullName, contents, encoding, cancellationToken);
        /// <inheritdoc cref="File.WriteAllTextAsync(string, ReadOnlyMemory{char}, Encoding, CancellationToken)" />
        public Task WriteAllTextAsync(ReadOnlyMemory<char> contents, Encoding encoding, CancellationToken cancellationToken) => System.IO.File.WriteAllTextAsync(fileInfo.FullName, contents, encoding, cancellationToken);
        #endregion
    }
}
