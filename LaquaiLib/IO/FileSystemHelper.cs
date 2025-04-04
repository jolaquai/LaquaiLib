using System.Buffers;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.AccessControl;

using LaquaiLib.Core;
using LaquaiLib.Extensions;
using LaquaiLib.IO;
using LaquaiLib.Unsafe;
using LaquaiLib.Util.Misc;

namespace LaquaiLib.IO;

#pragma warning disable CA1416

// This partial part implements general-purpose methods and some custom stuff.
/// <summary>
/// Provides methods and events for working with files and directories.
/// </summary>
public static partial class FileSystemHelper
{
    /// <summary>
    /// In parallel, migrates the contents of a directory from one location to another.
    /// </summary>
    /// <param name="source">The directory to move.</param>
    /// <param name="destination">The directory that <paramref name="source"/> will be moved or copied to.</param>
    /// <param name="copy">Replicate the directory and its contents at the source location instead of moving it.</param>
    /// <param name="allowExisting">Whether to allow the destination directory to already exist and contain files and whether to allow overwriting existing files.</param>
    /// <param name="maxDegreeOfParallelism">The maximum number of concurrent operations to allow. Defaults to the number of logical processors on the machine.</param>
    /// <param name="bufferSize">The size of the buffer to use for copying files. If unset, this is calculated individually for each chunk of files.</param>
    /// <param name="progressSink">An <see cref="IProgress{T}"/> instance to report progress to. The value is a tuple of the number of files processed and the total number of files.</param>
    /// <param name="restorePermissionsAndAttributes">Whether to restore the permissions and attributes of the files and directories after moving or copying them. Defaults to <see langword="false"/> and may incur a large performance penalty if <see langword="true"/>.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
    /// <remarks>
    /// This method uses <see cref="FileSizePartitioner"/> to create partitions for parallel processing that accounts for the size of the files in the directories.
    /// <para/>The process requires approximately <c><paramref name="maxDegreeOfParallelism"/> * 2^17 KB</c> of memory.
    /// </remarks>
    public static Task MigrateDirectoryAsync(
        string source,
        string destination,
        bool copy = false,
        bool allowExisting = false,
        int maxDegreeOfParallelism = -1,
        int bufferSize = -1,
        IProgress<(int, int)> progressSink = null,
        bool restorePermissionsAndAttributes = false,
        CancellationToken cancellationToken = default
    )
    {
        ValidateMigrateArguments(source, destination, allowExisting, ref maxDegreeOfParallelism);

        // Create the directory structure first
        Directory.CreateDirectory(destination);
        var directories = Directory.GetDirectories(source, "*", SearchOption.AllDirectories);
        for (var i = 0; i < directories.Length; i++)
        {
            var dirPath = directories[i];
            var newDirPath = dirPath.Replace(source, destination);
            Directory.CreateDirectory(newDirPath);

            if (OperatingSystem.IsWindows())
            {
                if (restorePermissionsAndAttributes)
                {
                    var srcSecurity = new DirectoryInfo(dirPath).GetAccessControl();
                    var di = new DirectoryInfo(newDirPath);
                    ReplacePermissions(di, srcSecurity);
                }
            }
        }

        var partitioner = new FileSizePartitioner(Directory.GetFiles(source, "*", SearchOption.AllDirectories));
        var partitions = partitioner.GetPartitions(maxDegreeOfParallelism);

        var filesCompleted = 0;
        return Task.WhenAll(partitions.Select(p => Task.Run(async () =>
        {
            // Local copy so the reference doesn't change from under us
            var pathEnumerator = p;
            while (pathEnumerator.MoveNext())
            {
                var fileSrc = pathEnumerator.Current;
                var fileDest = fileSrc.Replace(source, destination);

                if (!allowExisting && File.Exists(fileDest))
                {
                    throw new IOException($"Destination file '{fileDest}' already exists.");
                }

                cancellationToken.ThrowIfCancellationRequested();
                {
                    var sourceFs = File.OpenRead(fileSrc);
                    var destFs = File.Create(fileDest);
                    await using (sourceFs.ConfigureAwait(false))
                    await using (destFs.ConfigureAwait(false))
                    {
                        const int baseCutoff = 1 << 18;
                        int buffer;
                        if (sourceFs.Length < baseCutoff)
                        {
                            buffer = (int)sourceFs.Length;
                        }
                        else
                        {
                            buffer = bufferSize > 0 ? bufferSize : baseCutoff;
                        }

                        await sourceFs.CopyToAsync(destFs, buffer, cancellationToken).ConfigureAwait(false);
                    }
                }
                cancellationToken.ThrowIfCancellationRequested();

                // If requested, restore the permissions and attributes
                if (restorePermissionsAndAttributes)
                {
                    var srcAttribs = File.GetAttributes(fileSrc);
                    File.SetAttributes(fileDest, srcAttribs);

                    if (OperatingSystem.IsWindows())
                    {
                        var srcSecurity = new FileInfo(fileSrc).GetAccessControl();
                        var fi = new FileInfo(fileDest);
                        ReplacePermissions(fi, srcSecurity);
                    }
                }

                if (!copy)
                {
                    File.Delete(fileSrc);
                }

                var prog = Interlocked.Increment(ref filesCompleted);
                progressSink?.Report((prog, partitioner.TotalCount));
            }
        }, cancellationToken))).ContinueWith(_ =>
        {
            if (!copy)
            {
                Directory.Delete(source, true);
            }
        }, cancellationToken);
        // I know that's ugly as hell but I'm not making this method async for a single meaningless await
    }
    /// <summary>
    /// Migrates the contents of a directory from one location to another while employing the common trick of first compressing, then moving and decompressing the data.
    /// </summary>
    /// <param name="source">The directory to move.</param>
    /// <param name="destination">The directory that <paramref name="source"/> will be moved or copied to.</param>
    /// <param name="copy">Replicate the directory and its contents at the source location instead of moving it.</param>
    /// <param name="allowExisting">Whether to allow the destination directory to already exist and contain files and whether to allow overwriting existing files.</param>
    /// <param name="maxDegreeOfParallelism">The maximum number of concurrent operations to allow. Defaults to the number of logical processors on the machine.</param>
    /// <param name="progressSink">An <see cref="IProgress{T}"/> instance to report progress to. The value is the completion progress in percent.</param>
    /// <param name="restorePermissionsAndAttributes">Whether to restore the permissions and attributes of the files and directories after moving or copying them. Defaults to <see langword="false"/> and may incur a large performance penalty if <see langword="true"/>.</param>
    /// <param name="compressionLevel">The level of compression to apply to the files. Defaults to <see cref="CompressionLevel.Optimal"/>.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
    /// <remarks>
    /// This method uses <see cref="FileSizePartitioner"/> to create partitions for parallel processing that accounts for the size of the files in the directories.
    /// <para/>There are few cases where this method could realistically perform better than <see cref="MigrateDirectoryAsync(string, string, bool, bool, int, bool, CancellationToken)"/>; the additional compression/decompression overhead will likely only pay off when transferring over a slow network or to or from very slow storage media (that is, any situation where I/O is the bottleneck, instead of CPU).
    /// </remarks>
    public static Task MigrateDirectoryAsArchiveAsync(
        string source,
        string destination,
        bool copy = false,
        bool allowExisting = false,
        int maxDegreeOfParallelism = -1,
        IProgress<(int, int)> progressSink = null,
        bool restorePermissionsAndAttributes = false,
        CompressionLevel compressionLevel = CompressionLevel.Optimal,
        CancellationToken cancellationToken = default
    )
    {
        ValidateMigrateArguments(source, destination, allowExisting, ref maxDegreeOfParallelism);
        cancellationToken.ThrowIfCancellationRequested();

        // First, create the directory structure
        Directory.CreateDirectory(destination);
        var directories = Directory.GetDirectories(source, "*", SearchOption.AllDirectories);
        for (var i = 0; i < directories.Length; i++)
        {
            var dirPath = directories[i];
            var newDirPath = dirPath.Replace(source, destination);
            Directory.CreateDirectory(newDirPath);
            if (OperatingSystem.IsWindows())
            {
                if (restorePermissionsAndAttributes)
                {
                    var srcSecurity = new DirectoryInfo(dirPath).GetAccessControl();
                    var di = new DirectoryInfo(newDirPath);
                    ReplacePermissions(di, srcSecurity);
                }
            }
        }

        var partitioner = new FileSizePartitioner(Directory.GetFiles(source, "*", SearchOption.AllDirectories));
        var partitions = partitioner.GetPartitions(maxDegreeOfParallelism);

        var filesCompleted = 0;
        return Task.WhenAll(partitions.Select(p => Task.Run(async () =>
        {
            // Use a single intermediary stream for all files in this partition, since nobody but us will touch it
            var intermediary = new MemoryStream();
            // Local copy so the reference doesn't change from under us
            var pathEnumerator = p;

            while (pathEnumerator.MoveNext())
            {
                var fileSrc = pathEnumerator.Current;
                var fileDest = fileSrc.Replace(source, destination);

                if (!allowExisting && File.Exists(fileDest))
                {
                    throw new IOException($"Destination file '{fileDest}' already exists.");
                }

                cancellationToken.ThrowIfCancellationRequested();
                // Lots of stream overhead + compression and decompression for every file, but (ideally) less data to move around
                intermediary.SetLength(0);
                var sourceFs = File.OpenRead(fileSrc);
                var destFs = File.Create(fileDest);
                await using (sourceFs.ConfigureAwait(false))
                await using (destFs.ConfigureAwait(false))
                {
                    var deflateStream = new DeflateStream(intermediary, compressionLevel, true);
                    await using (deflateStream.ConfigureAwait(false))
                    {
                        await sourceFs.CopyToAsync(deflateStream, cancellationToken).ConfigureAwait(false);
                    }
                    intermediary.Position = 0;
                    deflateStream = new DeflateStream(intermediary, CompressionMode.Decompress);
                    await using (deflateStream.ConfigureAwait(false))
                    {
                        await deflateStream.CopyToAsync(destFs, cancellationToken).ConfigureAwait(false);
                    }
                    await sourceFs.CopyToAsync(destFs, cancellationToken).ConfigureAwait(false);
                }
                cancellationToken.ThrowIfCancellationRequested();

                // If requested, restore the permissions and attributes
                if (restorePermissionsAndAttributes)
                {
                    var srcAttribs = File.GetAttributes(fileSrc);
                    File.SetAttributes(fileDest, srcAttribs);

                    if (OperatingSystem.IsWindows())
                    {
                        var srcSecurity = new FileInfo(fileSrc).GetAccessControl();
                        var fi = new FileInfo(fileDest);
                        ReplacePermissions(fi, srcSecurity);
                    }
                }

                if (!copy)
                {
                    File.Delete(fileSrc);
                }

                var prog = Interlocked.Increment(ref filesCompleted);
                progressSink?.Report((prog, partitioner.TotalCount));
            }
        }, cancellationToken))).ContinueWith(_ =>
        {
            if (!copy)
            {
                Directory.Delete(source, true);
            }
        }, cancellationToken);
    }

    private static void ValidateMigrateArguments(string source, string destination, bool allowExisting, ref int maxDegreeOfParallelism)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentOutOfRangeException.ThrowIfZero(maxDegreeOfParallelism);
        if (maxDegreeOfParallelism < 0)
        {
            maxDegreeOfParallelism = Debugger.IsAttached ? 1 : Environment.ProcessorCount;
        }

        if (!Directory.Exists(source))
        {
            throw new ArgumentException($"Directory '{source}' does not exist.", nameof(source));
        }
        if (!allowExisting && Directory.Exists(destination))
        {
            throw new ArgumentException($"Directory '{destination}' already exists.", nameof(destination));
        }
        if (source.Equals(destination, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Source and destination are the same.");
        }

        // We can't use string.Contains to check for subdirectory relationship since one being prefixed by the other is not the same as being a subdirectory
        if (IsBaseOf(source, destination))
        {
            throw new ArgumentException("Source is a subdirectory of the destination.", nameof(source));
        }
        if (IsBaseOf(destination, source))
        {
            throw new ArgumentException("Destination is a subdirectory of the source.", nameof(destination));
        }
    }

    /// <summary>
    /// Removes all access rules and audit rules from the specified <see cref="FileSystemInfo"/> and replaces them with the rules from the specified <see cref="FileSystemSecurity"/>.
    /// </summary>
    /// <param name="fsi">A <see cref="FileInfo"/> or <see cref="DirectoryInfo"/> instance to modify.</param>
    /// <param name="takePermissionsFrom">The <see cref="FileSystemSecurity"/> instance to copy the permissions from.</param>
    /// <exception cref="PlatformNotSupportedException">Thrown when the method is called on a non-Windows platform.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="fsi"/> or <paramref name="takePermissionsFrom"/> is not a valid type or the combination is invalid.</exception>
    public static void ReplacePermissions(FileSystemInfo fsi, FileSystemSecurity takePermissionsFrom)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("This method is only supported on Windows.");
        }

        switch (fsi)
        {
            case FileInfo fi when takePermissionsFrom is FileSecurity fs:
                ReplacePermissionsImpl(fi, fs);
                break;
            case DirectoryInfo di when takePermissionsFrom is DirectorySecurity ds:
                ReplacePermissionsImpl(di, ds);
                break;
            default:
                throw new ArgumentException("Invalid FileSystemInfo or FileSystemSecurity type or the combination passed is invalid.");
        }
    }
    private static void ReplacePermissionsImpl(FileInfo target, FileSecurity copyFrom)
    {
        ArgumentNullException.ThrowIfNull(target);

        var sec = target.GetAccessControl();
        foreach (var rule in sec.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)).OfType<FileSystemAccessRule>())
        {
            sec.RemoveAccessRule(rule);
        }
        foreach (var rule in sec.GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier)).OfType<FileSystemAccessRule>())
        {
            sec.RemoveAccessRule(rule);
        }
        foreach (var rule in sec.GetAuditRules(true, true, typeof(System.Security.Principal.NTAccount)).Cast<FileSystemAuditRule>())
        {
            sec.RemoveAuditRule(rule);
        }
        foreach (var rule in sec.GetAuditRules(true, true, typeof(System.Security.Principal.SecurityIdentifier)).Cast<FileSystemAuditRule>())
        {
            sec.RemoveAuditRule(rule);
        }

        // Now copy the rules from the source
        foreach (var rule in copyFrom.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)).OfType<FileSystemAccessRule>())
        {
            sec.AddAccessRule(rule);
        }
        foreach (var rule in copyFrom.GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier)).OfType<FileSystemAccessRule>())
        {
            sec.AddAccessRule(rule);
        }
        foreach (var rule in copyFrom.GetAuditRules(true, true, typeof(System.Security.Principal.NTAccount)).Cast<FileSystemAuditRule>())
        {
            sec.AddAuditRule(rule);
        }
        foreach (var rule in copyFrom.GetAuditRules(true, true, typeof(System.Security.Principal.SecurityIdentifier)).Cast<FileSystemAuditRule>())
        {
            sec.AddAuditRule(rule);
        }

        if (copyFrom.GetSecurityDescriptorBinaryForm() is byte[] secDesc)
        {
            sec.SetSecurityDescriptorBinaryForm(secDesc);
        }

        // Run this anyway to cover anything not explicitly copied
        target.SetAccessControl(sec);
    }
    private static void ReplacePermissionsImpl(DirectoryInfo target, DirectorySecurity copyFrom)
    {
        ArgumentNullException.ThrowIfNull(target);

        var sec = target.GetAccessControl();
        foreach (var rule in sec.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)).OfType<FileSystemAccessRule>())
        {
            sec.RemoveAccessRule(rule);
        }
        foreach (var rule in sec.GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier)).OfType<FileSystemAccessRule>())
        {
            sec.RemoveAccessRule(rule);
        }
        foreach (var rule in sec.GetAuditRules(true, true, typeof(System.Security.Principal.NTAccount)).Cast<FileSystemAuditRule>())
        {
            sec.RemoveAuditRule(rule);
        }
        foreach (var rule in sec.GetAuditRules(true, true, typeof(System.Security.Principal.SecurityIdentifier)).Cast<FileSystemAuditRule>())
        {
            sec.RemoveAuditRule(rule);
        }

        // Now copy the rules from the source
        foreach (var rule in copyFrom.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)).OfType<FileSystemAccessRule>())
        {
            sec.AddAccessRule(rule);
        }
        foreach (var rule in copyFrom.GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier)).OfType<FileSystemAccessRule>())
        {
            sec.AddAccessRule(rule);
        }
        foreach (var rule in copyFrom.GetAuditRules(true, true, typeof(System.Security.Principal.NTAccount)).Cast<FileSystemAuditRule>())
        {
            sec.AddAuditRule(rule);
        }
        foreach (var rule in copyFrom.GetAuditRules(true, true, typeof(System.Security.Principal.SecurityIdentifier)).Cast<FileSystemAuditRule>())
        {
            sec.AddAuditRule(rule);
        }

        if (copyFrom.GetSecurityDescriptorBinaryForm() is byte[] secDesc)
        {
            sec.SetSecurityDescriptorBinaryForm(secDesc);
        }

        // Run this anyway to cover anything not explicitly copied
        target.SetAccessControl(sec);
    }

    /// <summary>
    /// Reads the file at <paramref name="path"/> into a <see cref="MemoryStream"/>, then deletes the file. It then only exists in memory.
    /// </summary>
    /// <param name="path">The path to the file to cut.</param>
    /// <returns>A <see cref="MemoryStream"/> containing the file data.</returns>
    public static MemoryStream CutFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var ms = new MemoryStream();
        {
            using var fileStream = File.OpenRead(path);
            fileStream.CopyTo(ms);
        }
        File.Delete(path);
        return ms;
    }
    /// <summary>
    /// Asynchronously reads the file at <paramref name="path"/> into a <see cref="MemoryStream"/>, then deletes the file. It then only exists in memory.
    /// </summary>
    /// <param name="path">The path to the file to cut.</param>
    /// <returns>A <see cref="MemoryStream"/> containing the file data.</returns>
    public static async Task<MemoryStream> CutFileAsync(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var ms = new MemoryStream();
        {
            var fileStream = File.OpenRead(path);
            await using (fileStream.ConfigureAwait(false))
            {
                await fileStream.CopyToAsync(ms).ConfigureAwait(false);
            }
        }
        File.Delete(path);
        return ms;
    }
    /// <summary>
    /// Reads the file at <paramref name="path"/> into the specified <paramref name="stream"/>, then deletes the file. It then only exists in memory.
    /// </summary>
    /// <param name="path">The path to the file to cut.</param>
    public static void CutFile(string path, Stream stream)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        using (var fileStream = File.OpenRead(path))
        {
            fileStream.CopyTo(stream);
        }
        File.Delete(path);
    }
    /// <summary>
    /// Asynchronously reads the file at <paramref name="path"/> into the specified <paramref name="stream"/>, then deletes the file. It then only exists in memory.
    /// </summary>
    /// <param name="path">The path to the file to cut.</param>
    /// <returns>A <see cref="Task"/> that completes when the operation is finished.</returns>
    public static async Task CutFileAsync(string path, Stream stream)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var fileStream = File.OpenRead(path);
        await using (fileStream.ConfigureAwait(false))
        {
            await fileStream.CopyToAsync(stream).ConfigureAwait(false);
        }
        File.Delete(path);
    }

    /// <summary>
    /// Asynchronously enumerates the file system and attempts to find a directory structure that matches the one specified.
    /// Generally, the more specific the directory structure, the faster the search will complete. However, this largely depends on the where the search is rooted, if at all.
    /// </summary>
    /// <param name="dirStructure">The directory structure to search for, for example <c>@"MyFolder\MySubFolder"</c>. Must be a well-formed relative path to a directory.</param>
    /// <param name="root">The absolute (rooted) path to the directory to start the search from. If <see langword="null"/>, the search starts from the root of each drive and returns all matches. This process may take a significant amount of time.</param>
    /// <param name="driveType">If <paramref name="root"/> is <see langword="null"/>, allows specifying which kinds of drives to search. The default is <see cref="DriveType.Fixed"/>. Note that searching network drives may take a significant amount of time.</param>
    /// <param name="maxRecursionDepth">Limits the depth of recursion when searching for the directory structure. The default is <see cref="int.MaxValue"/>.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> that enumerates the full paths of directories that match the specified structure.</returns>
    /// <exception cref="IOException">Thrown if the root directory does not exist.</exception>
    /// <exception cref="ArgumentException">Thrown if the directory structure or root directory is invalid.</exception>
    public static IAsyncEnumerable<string> EnumerateDirectoryStructureMatches(
        string dirStructure,
        string root = null,
        DriveType? driveType = null,
        int maxRecursionDepth = int.MaxValue
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dirStructure);
        if (Path.IsPathRooted(dirStructure) || dirStructure.AsSpan().IndexOfAny(InvalidPathChars) > -1)
        {
            throw new ArgumentException("The directory structure must be a well-formed relative path to a directory.", nameof(dirStructure));
        }

        if (driveType is not null && !string.IsNullOrWhiteSpace(root))
        {
            throw new ArgumentException("Cannot specify both a drive type and a root directory.", nameof(driveType));
        }

        static IAsyncEnumerable<string> ExamineRootImpl(string dirStructure, string root, int maxRecursionDepth)
        {
            var dir = new DirectoryInfo(root);
            if (!dir.Exists)
            {
                return AsyncEnumerableWrapper<string>.Empty;
            }

            return new AsyncEnumerableWrapper<string>(dir
                .EnumerateDirectories("*", new EnumerationOptions()
                {
                    IgnoreInaccessible = true,
                    RecurseSubdirectories = maxRecursionDepth > 0,
                    MaxRecursionDepth = maxRecursionDepth,
                    AttributesToSkip = FileAttributes.None
                })
                .AsParallel()
                .Where(d => d.FullName.EndsWith(dirStructure, StringComparison.OrdinalIgnoreCase))
                .Select(d => d.FullName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
            );
        }

        if (!string.IsNullOrWhiteSpace(root))
        {
            root = Path.GetFullPath(root);

            if (!Directory.Exists(root))
            {
                throw new IOException($"Directory '{root}' does not exist.");
            }

            return ExamineRootImpl(dirStructure, root, maxRecursionDepth);
        }

        driveType ??= DriveType.Fixed;

        return IAsyncEnumerableExtensions.Concat(
            DriveInfo.GetDrives()
                .Where(d => d.DriveType == driveType)
                .Select(d => ExamineRootImpl(dirStructure, d.Name, maxRecursionDepth))
                .ToArray()
        );
    }

    /// <summary>
    /// Enumerates all subdirectories of the specified <paramref name="directory"/> and moves the entire contents to the specified root.
    /// </summary>
    /// <param name="directory">The directory to process.</param>
    /// <param name="overwrite">Whether to allow overwriting files (not caused by the move) in the destination directory.</param>
    /// <param name="overwriteFromSubdirectories">If files with the same names exist on multiple levels of the directory structure, whether to allow files from more deeply nested directories to overwrite files from less deeply nested directories.</param>
    /// <returns>A <see cref="Task"/> that completes when the operation is finished.</returns>
    public static Task UnpackDirectory(string directory, bool overwrite = false, bool overwriteFromSubdirectories = false)
    {
        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"Directory '{directory}' does not exist.");
        }

        var files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories)
            .Except(Directory.GetFiles(directory))
            .OrderBy(f => f.Length)
            .ToArray();
        if (files.Length == 0)
        {
            return Task.CompletedTask;
        }

        var names = files.Select(Path.GetFileName).ToArray();
        var newPaths = names.Select(n => Path.Combine(directory, n)).ToArray();
        if (!overwrite && newPaths.FirstOrDefault(File.Exists) is string existing)
        {
            throw new IOException($"The file '{existing}' already exists. Move cannot be completed.");
        }

        if (!overwriteFromSubdirectories && names.Distinct().Count() < names.Length)
        {
            throw new IOException("Multiple files with the same name exist in the directory structure.");
        }

        return Task.Run(() =>
        {
            var filesLoc = files;
            for (var i = 0; i < filesLoc.Length; i++)
            {
                var file = filesLoc[i];
                var newFile = newPaths[i];
                MoveFile(file, newFile, overwriteFromSubdirectories);
                DeleteIfEmpty(Path.GetDirectoryName(file), true);
            }
        });
    }

    /// <summary>
    /// Determines whether the specified <paramref name="path"/> is a base of the specified <paramref name="potentialBase"/> path.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <param name="potentialBase">The path to compare against.</param>
    /// <returns><see langword="true"/> if <paramref name="path"/> is a base of <paramref name="potentialBase"/>, otherwise <see langword="false"/>.</returns>
    public static bool IsBaseOf(string potentialBase, string path)
    {
        var pathUri = new Uri(Path.EndsInDirectorySeparator(path) ? path : path + Path.DirectorySeparatorChar);
        var compUri = new Uri(Path.EndsInDirectorySeparator(potentialBase) ? potentialBase : potentialBase + Path.DirectorySeparatorChar);
        return compUri.IsBaseOf(pathUri);
    }
    /// <summary>
    /// Determines whether the directory identified by the specified <paramref name="directoryPath"/> is empty.
    /// </summary>
    /// <param name="directoryPath">The path to the directory.</param>
    /// <param name="allowEmptyDirectories">Whether a directory is considered empty if it contains only empty directories (that is, only files are considered for the check).</param>
    /// <returns><see langword="true"/> if a directory at the specified path exists and is empty according to the passed arguments, <see langword="false"/> otherwise.</returns>
    public static bool IsEmpty(string directoryPath, bool allowEmptyDirectories = false)
        => IsEmpty(new DirectoryInfo(directoryPath), allowEmptyDirectories);
    /// <summary>
    /// Determines whether the directory identified by the specified <paramref name="directoryInfo"/> is empty.
    /// </summary>
    /// <param name="directoryInfo">A <see cref="DirectoryInfo"/> instance that identifies the directory.</param>
    /// <param name="allowEmptyDirectories">Whether a directory is considered empty if it contains only empty directories (that is, only files are considered for the check).</param>
    /// <returns><see langword="true"/> if a directory at the specified path exists and is empty according to the passed arguments, <see langword="false"/> otherwise.</returns>
    public static bool IsEmpty(this DirectoryInfo directoryInfo, bool allowEmptyDirectories = false)
    {
        if (!directoryInfo.Exists)
        {
            return false;
        }
        if (allowEmptyDirectories)
        {
            return directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories).Any();
        }
        return directoryInfo.EnumerateFileSystemInfos("*", SearchOption.AllDirectories).Any();
    }
    /// <summary>
    /// Deletes the directory at the specified <paramref name="directoryPath"/> if it is empty.
    /// </summary>
    /// <param name="directoryPath">The path to the directory.</param>
    /// <param name="allowEmptyDirectories">Whether to delete the directory if it contains only empty directories (that is, only files are considered for the check).</param>
    public static void DeleteIfEmpty(string directoryPath, bool allowEmptyDirectories = false)
        => IsEmpty(new DirectoryInfo(directoryPath), allowEmptyDirectories);
    /// <summary>
    /// Deletes the directory identified by the specified <paramref name="directoryInfo"/> if it is empty.
    /// </summary>
    /// <param name="directoryInfo">A <see cref="DirectoryInfo"/> instance that identifies the directory.</param>
    /// <param name="allowEmptyDirectories">Whether to delete the directory if it contains only empty directories (that is, only files are considered for the check).</param>
    public static void DeleteIfEmpty(this DirectoryInfo directoryInfo, bool allowEmptyDirectories = false)
    {
        if (IsEmpty(directoryInfo, allowEmptyDirectories))
        {
            directoryInfo.Delete(allowEmptyDirectories);
        }
    }

    private static partial class LockedFileInterop
    {
        [LibraryImport("kernel32.dll", SetLastError = true)]
        private static partial nint CreateFile([MarshalAs(UnmanagedType.LPStr)] string lpFileName, uint dwDesiredAccess, uint dwShareMode,
        nint lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, nint hTemplateFile);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool BackupRead(nint hFile, Span<byte> lpBuffer, uint nNumberOfBytesToRead, ref uint lpNumberOfBytesRead, [MarshalAs(UnmanagedType.Bool)] bool bAbort, [MarshalAs(UnmanagedType.Bool)] bool bProcessSecurity, ref nint lpContext);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool CloseHandle(nint hObject);

        [StructLayout(LayoutKind.Sequential)]
        private struct WIN32_STREAM_ID
        {
            public uint dwStreamId;
            public uint dwStreamAttributes;
            public long Size;
            public uint dwStreamNameSize;
        }

        public static bool CopyLockedFile(string sourceFile, string destFile)
        {
            const uint GENERIC_READ = 0x80000000;
            const uint FILE_SHARE_READ = 0x00000001;
            const uint FILE_SHARE_WRITE = 0x00000002;
            const uint FILE_SHARE_DELETE = 0x00000004;
            const uint OPEN_EXISTING = 3;
            const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
            const uint BACKUP_DATA = 1;

            var handle = CreateFile(sourceFile, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE, nint.Zero, OPEN_EXISTING, FILE_FLAG_BACKUP_SEMANTICS, nint.Zero);

            if (handle == new nint(-1))
            {
                return false;
            }

            try
            {
                using var destStream = new FileStream(destFile, FileMode.Create);

                var buffer = MemoryManager.CreateBuffer(1 << 16);
                uint bytesRead = 0;
                var context = nint.Zero;
                var inDataStream = false;
                long remainingDataSize = 0;

                while (BackupRead(handle, buffer, (uint)buffer.Length, ref bytesRead, false, false, ref context))
                {
                    if (bytesRead == 0)
                    {
                        break;
                    }

                    var offset = 0;
                    while (offset < bytesRead)
                    {
                        // Continue reading data from previous stream
                        if (inDataStream)
                        {
                            var dataChunkSize = (int)Math.Min(remainingDataSize, bytesRead - offset);
                            destStream.Write(buffer.Slice(offset, dataChunkSize));

                            remainingDataSize -= dataChunkSize;
                            offset += dataChunkSize;

                            if (remainingDataSize <= 0)
                            {
                                inDataStream = false;
                            }

                            continue;
                        }

                        // Not enough bytes left for a header
                        if (offset + 20 > bytesRead)
                        {
                            break;
                        }

                        var streamId = buffer.Read<int>(offset);
                        var streamAttributes = buffer.Read<uint>(offset + 4);
                        var streamSize = buffer.Read<long>(offset + 8);
                        var streamNameSize = buffer.Read<uint>(offset + 16);

                        var headerSize = 20 + (int)streamNameSize;

                        // Skip past header
                        offset += headerSize;

                        // Check if this is a data stream
                        if (streamId == BACKUP_DATA)
                        {
                            inDataStream = true;
                            remainingDataSize = streamSize;

                            // Process available data now
                            if (offset < bytesRead)
                            {
                                var dataChunkSize = (int)Math.Min(remainingDataSize, bytesRead - offset);
                                destStream.Write(buffer.Slice(offset, dataChunkSize));

                                remainingDataSize -= dataChunkSize;
                                offset += dataChunkSize;

                                if (remainingDataSize <= 0)
                                {
                                    inDataStream = false;
                                }
                            }
                        }
                        else
                        {
                            // Skip non-data stream
                            var skipSize = (int)Math.Min(streamSize, bytesRead - offset);
                            offset += skipSize;
                            remainingDataSize = streamSize - skipSize;

                            if (remainingDataSize > 0)
                            {
                                inDataStream = true;  // Will be skipped next time
                            }
                        }
                    }
                }

                // Final read with bAbort = true
                BackupRead(handle, null, 0, ref bytesRead, true, false, ref context);
                return true;
            }
            finally
            {
                CloseHandle(handle);
            }
        }
    }
    /// <summary>
    /// Attempts to copy the file at <paramref name="source"/> to <paramref name="destination"/> even if that file is locked by another process.
    /// </summary>
    /// <param name="source">The path to the file to copy.</param>
    /// <param name="destination">The path to copy the file to.</param>
    /// <param name="overwrite">Whether to overwrite the destination file if it already exists. Defaults to <see langword="false"/>.</param>
    /// <returns><see langword="true"/> if the file was copied successfully, otherwise <see langword="false"/>.</returns>
    public static bool TryCopyLockedFile(string source, string destination, bool overwrite = false)
    {
        if (!File.Exists(source))
        {
            return false;
        }
        if (!overwrite && File.Exists(destination))
        {
            return false;
        }

        return LockedFileInterop.CopyLockedFile(source, destination);
    }

    /// <summary>
    /// Deletes the <c>Zone.Identifier</c> alternate data stream from the specified file, if it exists.
    /// This has the effect of removing the "This file originated from the internet" warning (akin to right-clicking and selecting "Unblock" in Explorer).
    /// </summary>
    /// <param name="path">The path to the file to remove the zone identifier from.</param>
    public static void RemoveZoneIdentifier(string path)
    {
        path += ":Zone.Identifier";
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
    /// <summary>
    /// Opens the alternate content stream with the specified name on the specified file.
    /// </summary>
    /// <param name="file">The path to the file to open the alternate content stream on.</param>
    /// <param name="acsName">The name of the alternate content stream to open.</param>
    /// <param name="fileMode">A <see cref="FileMode"/> value that determines how the ACS is opened or created.</param>
    /// <param name="fileAccess">A <see cref="FileAccess"/> value that determines the access rights to the ACS.</param>
    /// <param name="fileShare">A <see cref="FileShare"/> value that determines how the ACS is shared.</param>
    /// <returns>A <see cref="Stream"/> that represents the alternate content stream.</returns>
    public static Stream OpenAlternateContentStream(string file, string acsName, FileMode fileMode = FileMode.OpenOrCreate, FileAccess fileAccess = FileAccess.ReadWrite, FileShare fileShare = FileShare.None)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(file);
        ArgumentException.ThrowIfNullOrWhiteSpace(acsName);
        if (acsName.Length > 255)
        {
            throw new ArgumentException("Alternate content stream name is too long, must be 255 characters or less.", nameof(acsName));
        }
        if (acsName.AsSpan().ContainsAny(InvalidPathCharsSearchValues))
        {
            throw new ArgumentException("Alternate content stream name contains invalid characters.", nameof(acsName));
        }

        var path = file + ":" + acsName;
        return new FileStream(path, fileMode, fileAccess, fileShare);
    }

    /// <summary>
    /// Changes the name of the target of a path <see langword="string"/>. Specifying an extension in <paramref name="newName"/> causes the old extension to be replaced.
    /// </summary>
    /// <param name="path">The path to change the name of.</param>
    /// <param name="newName">The new name to change the path to.</param>
    /// <returns>A new path <see langword="string"/> with the new name.</returns>
    public static string ChangeName(string path, string newName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(newName);

        var span = path.AsSpan().TrimEnd(Path.DirectorySeparatorChar).TrimEnd(Path.AltDirectorySeparatorChar);
        var lastDirSepExclusiveIndex = span.LastIndexOfAny(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + 1;
        var extIndexWithTrailingPeriod = span.LastIndexOf('.');

        var newNameHasExtension = newName.Contains('.');

        if (extIndexWithTrailingPeriod > lastDirSepExclusiveIndex && !newNameHasExtension)
        {
            var beforeName = span[..lastDirSepExclusiveIndex];
            var afterName = span[extIndexWithTrailingPeriod..];
            return string.Create(beforeName.Length + newName.Length + afterName.Length, new ChangeNameState(beforeName, newName, afterName), (newSpan, state) =>
            {
                state.s1.CopyTo(newSpan);
                state.s2.CopyTo(newSpan[state.s1.Length..(state.s1.Length + state.s2.Length)]);
                state.s3.CopyTo(newSpan[^state.s3.Length..]);
            });
        }
        else // Includes the case where there is no extension (extIndexWithTrailingPeriod == -1)
        {
            var beforeName = span[..lastDirSepExclusiveIndex];
            return string.Create(beforeName.Length + newName.Length, new ChangeNameState(beforeName, newName), (newSpan, state) =>
            {
                state.s1.CopyTo(newSpan);
                state.s2.CopyTo(newSpan[state.s1.Length..]);
            });
        }
    }
    // Encapsulates state for use in string.Create above
    private readonly ref struct ChangeNameState(ReadOnlySpan<char> s1, ReadOnlySpan<char> s2, ReadOnlySpan<char> s3 = default)
    {
        public readonly ReadOnlySpan<char> s1 = s1;
        public readonly ReadOnlySpan<char> s2 = s2;
        public readonly ReadOnlySpan<char> s3 = s3;
    }

    /// <summary>
    /// Gets a <see cref="SearchValues{T}"/> instance enabling efficient searching for the <see langword="char"/>s in <see cref="ControlCharacters"/>.
    /// </summary>
    public static SearchValues<char> ControlCharactersSearchValues => field ??= SearchValues.Create(ControlCharacters);
    /// <summary>
    /// Gets a <see cref="SearchValues{T}"/> instance enabling efficient searching for the <see langword="char"/>s in <see cref="InvalidFileNameChars"/>.
    /// </summary>
    public static SearchValues<char> InvalidFileNameCharsSearchValues => field ??= SearchValues.Create(InvalidFileNameChars);
    /// <summary>
    /// Gets a <see cref="SearchValues{T}"/> instance enabling efficient searching for the <see langword="char"/>s in <see cref="InvalidPathChars"/>.
    /// </summary>
    public static SearchValues<char> InvalidPathCharsSearchValues => field ??= SearchValues.Create(InvalidPathChars);
    /// <summary>
    /// Gets all ASCII control characters (0-31) as a <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    public static ReadOnlySpan<char> ControlCharacters =>
    [
        '\0', (char)1, (char)2, (char)3, (char)4, (char)5, (char)6, (char)7, (char)8, (char)9, (char)10,
        (char)11, (char)12, (char)13, (char)14, (char)15, (char)16, (char)17, (char)18, (char)19, (char)20,
        (char)21, (char)22, (char)23, (char)24, (char)25, (char)26, (char)27, (char)28, (char)29, (char)30,
        (char)31
    ];
    /// <summary>
    /// Gets a <see cref="ReadOnlySpan{T}"/> containing the characters that are not allowed in file names.
    /// </summary>
    public static ReadOnlySpan<char> InvalidFileNameChars => ['\"', '<', '>', ':', '*', '?', '\\', '/'];
    /// <summary>
    /// Gets a <see cref="ReadOnlySpan{T}"/> containing the characters that are not allowed in paths.
    /// </summary>
    public static ReadOnlySpan<char> InvalidPathChars =>
    [
        '|',
        '\0', (char)1, (char)2, (char)3, (char)4, (char)5, (char)6, (char)7, (char)8, (char)9, (char)10,
        (char)11, (char)12, (char)13, (char)14, (char)15, (char)16, (char)17, (char)18, (char)19, (char)20,
        (char)21, (char)22, (char)23, (char)24, (char)25, (char)26, (char)27, (char)28, (char)29, (char)30,
        (char)31
    ];
    /// <summary>
    /// Fills the specified <see cref="Span{T}"/> with the characters that are not allowed in file names. Must be at least 8 characters long.
    /// </summary>
    /// <param name="destination">The span to fill.</param>
    public static void FillInvalidFileNameChars(Span<char> destination)
    {
        if (destination.Length < 8)
        {
            throw new ArgumentException("Destination span is too short.", nameof(destination));
        }

        InvalidFileNameChars.CopyTo(destination[..8]);
    }
    /// <summary>
    /// Fills the specified <see cref="Span{T}"/> with the characters that are not allowed in path names. Must be at least 33 characters long.
    /// </summary>
    /// <param name="destination">The span to fill.</param>
    public static void FillInvalidPathChars(Span<char> destination)
    {
        if (destination.Length < 33)
        {
            throw new ArgumentException("Destination span is too short.", nameof(destination));
        }
        InvalidPathChars.CopyTo(destination);
    }
}
