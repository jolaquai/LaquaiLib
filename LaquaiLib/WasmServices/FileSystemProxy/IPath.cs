namespace LaquaiLib.WasmServices.FileSystemProxy;

/// <summary>
/// Exposes access to the <see cref="System.IO.Path"/> class by proxy in a Blazor WebAssembly application.
/// </summary>
public interface IPath
{
    /// <inheritdoc cref="System.IO.Path.ChangeExtension(string, string)" />
    public string ChangeExtension(string path, string extension);
    /// <inheritdoc cref="System.IO.Path.Combine(string, string)" />
    public string Combine(string path1, string path2);
    /// <inheritdoc cref="System.IO.Path.Combine(string, string, string)" />
    public string Combine(string path1, string path2, string path3);
    /// <inheritdoc cref="System.IO.Path.Combine(string, string, string, string)" />
    public string Combine(string path1, string path2, string path3, string path4);
    /// <inheritdoc cref="System.IO.Path.Combine(string[])" />
    public string Combine(params ReadOnlySpan<string> paths);
    /// <inheritdoc cref="System.IO.Path.EndsInDirectorySeparator(ReadOnlySpan{char})" />
    public bool EndsInDirectorySeparator(ReadOnlySpan<char> path);
    /// <inheritdoc cref="System.IO.Path.EndsInDirectorySeparator(string)" />
    public bool EndsInDirectorySeparator(string path);
    /// <inheritdoc cref="System.IO.Path.GetDirectoryName(string)" />
    public string? GetDirectoryName(string path);
    /// <inheritdoc cref="System.IO.Path.GetDirectoryName(ReadOnlySpan{char})" />
    public ReadOnlySpan<char> GetDirectoryName(ReadOnlySpan<char> path);
    /// <inheritdoc cref="System.IO.Path.GetExtension(string)" />
    public string GetExtension(string path);
    /// <inheritdoc cref="System.IO.Path.GetExtension(ReadOnlySpan{char})" />
    public ReadOnlySpan<char> GetExtension(ReadOnlySpan<char> path);
    /// <inheritdoc cref="System.IO.Path.GetFileName(string)" />
    public string GetFileName(string path);
    /// <inheritdoc cref="System.IO.Path.GetFileName(ReadOnlySpan{char})" />
    public ReadOnlySpan<char> GetFileName(ReadOnlySpan<char> path);
    /// <inheritdoc cref="System.IO.Path.GetFileNameWithoutExtension(string)" />
    public string GetFileNameWithoutExtension(string path);
    /// <inheritdoc cref="System.IO.Path.GetFileNameWithoutExtension(ReadOnlySpan{char})" />
    public ReadOnlySpan<char> GetFileNameWithoutExtension(ReadOnlySpan<char> path);
    /// <inheritdoc cref="System.IO.Path.GetFullPath(string)" />
    public string GetFullPath(string path);
    /// <inheritdoc cref="System.IO.Path.GetFullPath(string, string)" />
    public string GetFullPath(string path, string basePath);
    /// <inheritdoc cref="System.IO.Path.GetInvalidFileNameChars()" />
    public char[] GetInvalidFileNameChars();
    /// <inheritdoc cref="System.IO.Path.GetInvalidPathChars()" />
    public char[] GetInvalidPathChars();
    /// <inheritdoc cref="System.IO.Path.GetPathRoot(string)" />
    public string? GetPathRoot(string path);
    /// <inheritdoc cref="System.IO.Path.GetPathRoot(ReadOnlySpan{char})" />
    public ReadOnlySpan<char> GetPathRoot(ReadOnlySpan<char> path);
    /// <inheritdoc cref="System.IO.Path.GetRandomFileName()" />
    public string GetRandomFileName();
    /// <inheritdoc cref="System.IO.Path.GetRelativePath(string, string)" />
    public string GetRelativePath(string relativeTo, string path);
    /// <inheritdoc cref="System.IO.Path.GetTempFileName()" />
    public string GetTempFileName();
    /// <inheritdoc cref="System.IO.Path.GetTempPath()" />
    public string GetTempPath();
    /// <inheritdoc cref="System.IO.Path.HasExtension(string)" />
    public bool HasExtension(string path);
    /// <inheritdoc cref="System.IO.Path.HasExtension(ReadOnlySpan{char})" />
    public bool HasExtension(ReadOnlySpan<char> path);
    /// <inheritdoc cref="System.IO.Path.IsPathFullyQualified(string)" />
    public bool IsPathFullyQualified(string path);
    /// <inheritdoc cref="System.IO.Path.IsPathFullyQualified(ReadOnlySpan{char})" />
    public bool IsPathFullyQualified(ReadOnlySpan<char> path);
    /// <inheritdoc cref="System.IO.Path.IsPathRooted(string)" />
    public bool IsPathRooted(string path);
    /// <inheritdoc cref="System.IO.Path.IsPathRooted(ReadOnlySpan{char})" />
    public bool IsPathRooted(ReadOnlySpan<char> path);
    /// <inheritdoc cref="System.IO.Path.Join(ReadOnlySpan{char}, ReadOnlySpan{char})" />
    public string Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2);
    /// <inheritdoc cref="System.IO.Path.Join(ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char})" />
    public string Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3);
    /// <inheritdoc cref="System.IO.Path.Join(ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char})" />
    public string Join(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3, ReadOnlySpan<char> path4);
    /// <inheritdoc cref="System.IO.Path.Join(string, string)" />
    public string Join(string path1, string path2);
    /// <inheritdoc cref="System.IO.Path.Join(string, string, string)" />
    public string Join(string path1, string path2, string path3);
    /// <inheritdoc cref="System.IO.Path.Join(string, string, string)" />
    public string Join(string path1, string path2, string path3, string path4);
    /// <inheritdoc cref="System.IO.Path.Join(string[])" />
    public string Join(params ReadOnlySpan<string> paths);
    /// <inheritdoc cref="System.IO.Path.TrimEndingDirectorySeparator(string)" />
    public string TrimEndingDirectorySeparator(string path);
    /// <inheritdoc cref="System.IO.Path.TrimEndingDirectorySeparator(ReadOnlySpan{char})" />
    public ReadOnlySpan<char> TrimEndingDirectorySeparator(ReadOnlySpan<char> path);
    /// <inheritdoc cref="System.IO.Path.TryJoin(ReadOnlySpan{char}, ReadOnlySpan{char}, Span{char}, out int)" />
    public bool TryJoin(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, Span<char> destination, out int charsWritten);
    /// <inheritdoc cref="System.IO.Path.TryJoin(ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char}, Span{char}, out int)" />
    public bool TryJoin(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2, ReadOnlySpan<char> path3, Span<char> destination, out int charsWritten);
}
