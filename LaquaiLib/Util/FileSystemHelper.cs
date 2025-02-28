using System.Buffers;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.AccessControl;

using LaquaiLib.Extensions;
using LaquaiLib.Util.Misc;

namespace LaquaiLib.Util;

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
        bool restorePermissionsAndAttributes = false,
        CancellationToken cancellationToken = default
    )
    {
        ValidateMigrateArguments(source, destination, allowExisting, ref maxDegreeOfParallelism);

        // Create the directory structure first
        _ = Directory.CreateDirectory(destination);
        foreach (var dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
        {
            var newDirPath = dirPath.Replace(source, destination);
            _ = Directory.CreateDirectory(newDirPath);

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
                await using (var sourceFs = File.OpenRead(fileSrc))
                await using (var destFs = File.Create(fileDest))
                {
                    await sourceFs.CopyToAsync(destFs, cancellationToken);
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
        bool restorePermissionsAndAttributes = false,
        CompressionLevel compressionLevel = CompressionLevel.Optimal,
        CancellationToken cancellationToken = default
    )
    {
        ValidateMigrateArguments(source, destination, allowExisting, ref maxDegreeOfParallelism);
        cancellationToken.ThrowIfCancellationRequested();

        // First, create the directory structure
        _ = Directory.CreateDirectory(destination);
        foreach (var dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
        {
            var newDirPath = dirPath.Replace(source, destination);
            _ = Directory.CreateDirectory(newDirPath);
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
                await using (var sourceFs = File.OpenRead(fileSrc))
                await using (var destFs = File.Create(fileDest))
                {
                    await using (var compStream = new DeflateStream(intermediary, compressionLevel, true))
                    {
                        await sourceFs.CopyToAsync(compStream, cancellationToken);
                    }
                    intermediary.Position = 0;
                    await using (var decompStream = new DeflateStream(intermediary, CompressionMode.Decompress))
                    {
                        await decompStream.CopyToAsync(destFs, cancellationToken);
                    }
                    await sourceFs.CopyToAsync(destFs, cancellationToken);
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
            _ = sec.RemoveAccessRule(rule);
        }
        foreach (var rule in sec.GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier)).OfType<FileSystemAccessRule>())
        {
            _ = sec.RemoveAccessRule(rule);
        }
        foreach (var rule in sec.GetAuditRules(true, true, typeof(System.Security.Principal.NTAccount)).Cast<FileSystemAuditRule>())
        {
            _ = sec.RemoveAuditRule(rule);
        }
        foreach (var rule in sec.GetAuditRules(true, true, typeof(System.Security.Principal.SecurityIdentifier)).Cast<FileSystemAuditRule>())
        {
            _ = sec.RemoveAuditRule(rule);
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
            _ = sec.RemoveAccessRule(rule);
        }
        foreach (var rule in sec.GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier)).OfType<FileSystemAccessRule>())
        {
            _ = sec.RemoveAccessRule(rule);
        }
        foreach (var rule in sec.GetAuditRules(true, true, typeof(System.Security.Principal.NTAccount)).Cast<FileSystemAuditRule>())
        {
            _ = sec.RemoveAuditRule(rule);
        }
        foreach (var rule in sec.GetAuditRules(true, true, typeof(System.Security.Principal.SecurityIdentifier)).Cast<FileSystemAuditRule>())
        {
            _ = sec.RemoveAuditRule(rule);
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
            using (var fileStream = File.OpenRead(path))
            {
                fileStream.CopyTo(ms);
            }
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
                await fileStream.CopyToAsync(ms);
            }
        }
        File.Delete(path);
        return ms;
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
    /// <returns>A <see cref="Task"/> that completes when the operation is finished.</returns>
    public static Task UnpackDirectory(string directory)
    {
        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"Directory '{directory}' does not exist.");
        }

        var files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
        // Filter out the files that are already in the root directory
        files = [.. files.Except(Directory.GetFiles(directory))];
        if (files.Length == 0)
        {
            return Task.CompletedTask;
        }

        var names = files.Select(Path.GetFileName).ToArray();
        var newPaths = names.Select(n => Path.Combine(directory, n)).ToArray();
        if (newPaths.FirstOrDefault(File.Exists) is string existing)
        {
            throw new IOException($"The file '{existing}' already exists. Move cannot be completed.");
        }

        if (names.Distinct().Count() < names.Length)
        {
            throw new IOException("Multiple files with the same name exist in the directory structure.");
        }

        return Task.Run(() => Parallel.For(0, files.Length, i =>
        {
            var file = files[i];
            var newFile = newPaths[i];
            File.Move(file, newFile);
        }));
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
                using (var destStream = new FileStream(destFile, FileMode.Create))
                {
                    Span<byte> buffer = stackalloc byte[1 << 16];
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
                    _ = BackupRead(handle, null, 0, ref bytesRead, true, false, ref context);
                }
                return true;
            }
            finally
            {
                _ = CloseHandle(handle);
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

    private static ReadOnlySpan<char> InvalidFileNameChars => ['\"', '<', '>', ':', '*', '?', '\\', '/'];
    private static ReadOnlySpan<char> InvalidPathChars => ['|', '\0',
        (char)1, (char)2, (char)3, (char)4, (char)5, (char)6, (char)7, (char)8, (char)9, (char)10,
        (char)11, (char)12, (char)13, (char)14, (char)15, (char)16, (char)17, (char)18, (char)19, (char)20,
        (char)21, (char)22, (char)23, (char)24, (char)25, (char)26, (char)27, (char)28, (char)29, (char)30,
        (char)31
    ];
    /// <summary>
    /// Fills the specified <see cref="Span{T}"/> with the characters that are not allowed in file names. Must be at least 41 characters long.
    /// </summary>
    /// <param name="destination">The span to fill.</param>
    public static void FillInvalidFileNameChars(Span<char> destination)
    {
        if (destination.Length < 41)
        {
            throw new ArgumentException("Destination span is too short.", nameof(destination));
        }

        FillInvalidPathChars(destination[..33]);
        InvalidFileNameChars.CopyTo(destination[33..41]);
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
