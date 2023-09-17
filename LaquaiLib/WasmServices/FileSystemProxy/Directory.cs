﻿using System.IO;

namespace LaquaiLib.WasmServices.FileSystemProxy;

/// <summary>
/// Implements the <see cref="IDirectory"/> interface for use in the <see cref="IFileSystemProxy"/>.
/// </summary>
public class Directory : IDirectory
{
    /// <inheritdoc cref="System.IO.Directory.CreateDirectory(string)" />
    public DirectoryInfo CreateDirectory(string path) => System.IO.Directory.CreateDirectory(path);
    /// <inheritdoc cref="System.IO.Directory.CreateSymbolicLink(string, string)" />
    public FileSystemInfo CreateSymbolicLink(string path, string pathToTarget) => System.IO.Directory.CreateSymbolicLink(path, pathToTarget);
    /// <inheritdoc cref="System.IO.Directory.Delete(string)" />
    public void Delete(string path) => System.IO.Directory.Delete(path);
    /// <inheritdoc cref="System.IO.Directory.Delete(string, bool)" />
    public void Delete(string path, bool recursive) => System.IO.Directory.Delete(path, recursive);
    /// <inheritdoc cref="System.IO.Directory.EnumerateDirectories(string)" />
    public IEnumerable<string> EnumerateDirectories(string path) => System.IO.Directory.EnumerateDirectories(path);
    /// <inheritdoc cref="System.IO.Directory.EnumerateDirectories(string, string)" />
    public IEnumerable<string> EnumerateDirectories(string path, string searchPattern) => System.IO.Directory.EnumerateDirectories(path, searchPattern);
    /// <inheritdoc cref="System.IO.Directory.EnumerateDirectories(string, string, SearchOption)" />
    public IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption) => System.IO.Directory.EnumerateDirectories(path, searchPattern, searchOption);
    /// <inheritdoc cref="System.IO.Directory.EnumerateDirectories(string, string, EnumerationOptions)" />
    public IEnumerable<string> EnumerateDirectories(string path, string searchPattern, EnumerationOptions enumerationOptions) => System.IO.Directory.EnumerateDirectories(path, searchPattern, enumerationOptions);
    /// <inheritdoc cref="System.IO.Directory.EnumerateFiles(string)" />
    public IEnumerable<string> EnumerateFiles(string path) => System.IO.Directory.EnumerateFiles(path);
    /// <inheritdoc cref="System.IO.Directory.EnumerateFiles(string, string)" />
    public IEnumerable<string> EnumerateFiles(string path, string searchPattern) => System.IO.Directory.EnumerateFiles(path, searchPattern);
    /// <inheritdoc cref="System.IO.Directory.EnumerateFiles(string, string, SearchOption)" />
    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption) => System.IO.Directory.EnumerateFiles(path, searchPattern, searchOption);
    /// <inheritdoc cref="System.IO.Directory.EnumerateFiles(string, string, EnumerationOptions)" />
    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, EnumerationOptions enumerationOptions) => System.IO.Directory.EnumerateFiles(path, searchPattern, enumerationOptions);
    /// <inheritdoc cref="System.IO.Directory.EnumerateFileSystemEntries(string)" />
    public IEnumerable<string> EnumerateFileSystemEntries(string path) => System.IO.Directory.EnumerateFileSystemEntries(path);
    /// <inheritdoc cref="System.IO.Directory.EnumerateFileSystemEntries(string, string)" />
    public IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern) => System.IO.Directory.EnumerateFileSystemEntries(path, searchPattern);
    /// <inheritdoc cref="System.IO.Directory.EnumerateFileSystemEntries(string, string, SearchOption)" />
    public IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, SearchOption searchOption) => System.IO.Directory.EnumerateFileSystemEntries(path, searchPattern, searchOption);
    /// <inheritdoc cref="System.IO.Directory.EnumerateFileSystemEntries(string, string, EnumerationOptions)" />
    public IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, EnumerationOptions enumerationOptions) => System.IO.Directory.EnumerateFileSystemEntries(path, searchPattern, enumerationOptions);
    /// <inheritdoc cref="System.IO.Directory.Exists(string)" />
    public bool Exists(string path) => System.IO.Directory.Exists(path);
    /// <inheritdoc cref="System.IO.Directory.GetCreationTime(string)" />
    public DateTime GetCreationTime(string path) => System.IO.Directory.GetCreationTime(path);
    /// <inheritdoc cref="System.IO.Directory.GetCreationTimeUtc(string)" />
    public DateTime GetCreationTimeUtc(string path) => System.IO.Directory.GetCreationTimeUtc(path);
    /// <inheritdoc cref="System.IO.Directory.GetCurrentDirectory()" />
    public string GetCurrentDirectory() => System.IO.Directory.GetCurrentDirectory();
    /// <inheritdoc cref="System.IO.Directory.GetDirectories(string)" />
    public string[] GetDirectories(string path) => System.IO.Directory.GetDirectories(path);
    /// <inheritdoc cref="System.IO.Directory.GetDirectories(string, string)" />
    public string[] GetDirectories(string path, string searchPattern) => System.IO.Directory.GetDirectories(path, searchPattern);
    /// <inheritdoc cref="System.IO.Directory.GetDirectories(string, string, SearchOption)" />
    public string[] GetDirectories(string path, string searchPattern, SearchOption searchOption) => System.IO.Directory.GetDirectories(path, searchPattern, searchOption);
    /// <inheritdoc cref="System.IO.Directory.GetDirectories(string, string, EnumerationOptions)" />
    public string[] GetDirectories(string path, string searchPattern, EnumerationOptions enumerationOptions) => System.IO.Directory.GetDirectories(path, searchPattern, enumerationOptions);
    /// <inheritdoc cref="System.IO.Directory.GetDirectoryRoot(string)" />
    public string GetDirectoryRoot(string path) => System.IO.Directory.GetDirectoryRoot(path);
    /// <inheritdoc cref="System.IO.Directory.GetFiles(string)" />
    public string[] GetFiles(string path) => System.IO.Directory.GetFiles(path);
    /// <inheritdoc cref="System.IO.Directory.GetFiles(string, string)" />
    public string[] GetFiles(string path, string searchPattern) => System.IO.Directory.GetFiles(path, searchPattern);
    /// <inheritdoc cref="System.IO.Directory.GetFiles(string, string, SearchOption)" />
    public string[] GetFiles(string path, string searchPattern, SearchOption searchOption) => System.IO.Directory.GetFiles(path, searchPattern, searchOption);
    /// <inheritdoc cref="System.IO.Directory.GetFiles(string, string, EnumerationOptions)" />
    public string[] GetFiles(string path, string searchPattern, EnumerationOptions enumerationOptions) => System.IO.Directory.GetFiles(path, searchPattern, enumerationOptions);
    /// <inheritdoc cref="System.IO.Directory.GetFileSystemEntries(string)" />
    public string[] GetFileSystemEntries(string path) => System.IO.Directory.GetFileSystemEntries(path);
    /// <inheritdoc cref="System.IO.Directory.GetFileSystemEntries(string, string)" />
    public string[] GetFileSystemEntries(string path, string searchPattern) => System.IO.Directory.GetFileSystemEntries(path, searchPattern);
    /// <inheritdoc cref="System.IO.Directory.GetFileSystemEntries(string, string, SearchOption)" />
    public string[] GetFileSystemEntries(string path, string searchPattern, SearchOption searchOption) => System.IO.Directory.GetFileSystemEntries(path, searchPattern, searchOption);
    /// <inheritdoc cref="System.IO.Directory.GetFileSystemEntries(string, string, EnumerationOptions)" />
    public string[] GetFileSystemEntries(string path, string searchPattern, EnumerationOptions enumerationOptions) => System.IO.Directory.GetFileSystemEntries(path, searchPattern, enumerationOptions);
    /// <inheritdoc cref="System.IO.Directory.GetLastAccessTime(string)" />
    public DateTime GetLastAccessTime(string path) => System.IO.Directory.GetLastAccessTime(path);
    /// <inheritdoc cref="System.IO.Directory.GetLastAccessTimeUtc(string)" />
    public DateTime GetLastAccessTimeUtc(string path) => System.IO.Directory.GetLastAccessTimeUtc(path);
    /// <inheritdoc cref="System.IO.Directory.GetLastWriteTime(string)" />
    public DateTime GetLastWriteTime(string path) => System.IO.Directory.GetLastWriteTime(path);
    /// <inheritdoc cref="System.IO.Directory.GetLastWriteTimeUtc(string)" />
    public DateTime GetLastWriteTimeUtc(string path) => System.IO.Directory.GetLastWriteTimeUtc(path);
    /// <inheritdoc cref="System.IO.Directory.GetLogicalDrives()" />
    public string[] GetLogicalDrives() => System.IO.Directory.GetLogicalDrives();
    /// <inheritdoc cref="System.IO.Directory.GetParent(string)" />
    public DirectoryInfo? GetParent(string path) => System.IO.Directory.GetParent(path);
    /// <inheritdoc cref="System.IO.Directory.Move(string, string)" />
    public void Move(string sourceDirName, string destDirName) => System.IO.Directory.Move(sourceDirName, destDirName);
    /// <inheritdoc cref="System.IO.Directory.ResolveLinkTarget(string, bool)" />
    public FileSystemInfo? ResolveLinkTarget(string linkPath, bool returnFinalTarget) => System.IO.Directory.ResolveLinkTarget(linkPath, returnFinalTarget);
    /// <inheritdoc cref="System.IO.Directory.SetCreationTime(string, DateTime)" />
    public void SetCreationTime(string path, DateTime creationTime) => System.IO.Directory.SetCreationTime(path, creationTime);
    /// <inheritdoc cref="System.IO.Directory.SetCreationTimeUtc(string, DateTime)" />
    public void SetCreationTimeUtc(string path, DateTime creationTimeUtc) => System.IO.Directory.SetCreationTimeUtc(path, creationTimeUtc);
    /// <inheritdoc cref="System.IO.Directory.SetCurrentDirectory(string)" />
    public void SetCurrentDirectory(string path) => System.IO.Directory.SetCurrentDirectory(path);
    /// <inheritdoc cref="System.IO.Directory.SetLastAccessTime(string, DateTime)" />
    public void SetLastAccessTime(string path, DateTime lastAccessTime) => System.IO.Directory.SetLastAccessTime(path, lastAccessTime);
    /// <inheritdoc cref="System.IO.Directory.SetLastAccessTimeUtc(string, DateTime)" />
    public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc) => System.IO.Directory.SetLastAccessTimeUtc(path, lastAccessTimeUtc);
    /// <inheritdoc cref="System.IO.Directory.SetLastWriteTime(string, DateTime)" />
    public void SetLastWriteTime(string path, DateTime lastWriteTime) => System.IO.Directory.SetLastWriteTime(path, lastWriteTime);
    /// <inheritdoc cref="System.IO.Directory.SetLastWriteTimeUtc(string, DateTime)" />
    public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc) => System.IO.Directory.SetLastWriteTimeUtc(path, lastWriteTimeUtc);
}
