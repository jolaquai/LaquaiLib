using System.Buffers;

using LaquaiLib.Extensions;
using LaquaiLib.Util.Misc;

namespace LaquaiLib.Util;

/// <summary>
/// Provides methods and events for working with files and directories.
/// </summary>
public static class FileSystemHelper
{
    /// <summary>
    /// In parallel, migrates the contents of a directory from one location to another.
    /// </summary>
    /// <param name="source">The directory to move.</param>
    /// <param name="to">The directory that becomes the new parent directory of <paramref name="source"/>.</param>
    /// <param name="copy">Replicate the directory and its contents at the source location instead of moving it.</param>
    /// <param name="allowExisting">Whether to allow the destination directory to already exist and contain files, and whether to overwrite existing files when <paramref name="copy"/> is <see langword="true"/>.</param>
    /// <param name="maxDegreeOfParallelism">The maximum number of concurrent operations to allow.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
    /// <remarks>
    /// This method uses <see cref="FileSizePartitioner"/> to create partitions for parallel processing that accounts for the size of the files in the directories.
    /// </remarks>
    public static void MigrateDirectory(
        string source,
        string to,
        bool copy = false,
        bool allowExisting = false,
        int maxDegreeOfParallelism = -1,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(to);
        if (maxDegreeOfParallelism < 0)
        {
            maxDegreeOfParallelism = Environment.ProcessorCount;
        }

        var newTopPath = Path.Combine(to, Path.GetFileName(source));

        if (!Directory.Exists(source))
        {
            throw new ArgumentException($"Directory {source} does not exist.", nameof(source));
        }
        if (!Directory.Exists(to))
        {
            throw new ArgumentException($"Directory {to} does not exist.", nameof(to));
        }
        if (!allowExisting && Directory.Exists(newTopPath))
        {
            throw new ArgumentException($"Directory {newTopPath} already exists.", nameof(to));
        }
        if (source.Equals(to, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Source and destination are the same.");
        }
        if (source.Contains(to, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Destination is a subdirectory of the source.", nameof(to));
        }
        if (to.Contains(source, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Source is a subdirectory of the destination.", nameof(source));
        }

        Directory.CreateDirectory(newTopPath);

        foreach (var dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
        {
            var newDirPath = dirPath.Replace(source, newTopPath);
            Directory.CreateDirectory(newDirPath);
        }

        var partitioner = new FileSizePartitioner(Directory.GetFiles(source, "*", SearchOption.AllDirectories));
        var partitions = partitioner.GetPartitions(maxDegreeOfParallelism);

        if (copy)
        {
            _ = Parallel.ForEach(
                partitions,
                pathEnumerator =>
                {
                    while (pathEnumerator.MoveNext())
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var current = pathEnumerator.Current;
                        var newFilePath = current.Replace(source, newTopPath);
                        File.Copy(current, newFilePath, allowExisting);
                    }
                }
            );
        }
        else
        {
            _ = Parallel.ForEach(
                partitions,
                pathEnumerator =>
                {
                    while (pathEnumerator.MoveNext())
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var current = pathEnumerator.Current;
                        var newFilePath = current.Replace(source, newTopPath);
                        File.Move(current, newFilePath, allowExisting);
                    }
                }
            );
        }
    }

    private static readonly Lazy<char[]> _invalidPathChars = new Lazy<char[]>(Path.GetInvalidFileNameChars);
    /// <summary>
    /// Asynchronously enumerates the file system and attempts to find a directory structure that matches the one specified.
    /// Generally, the more specific the directory structure, the faster the search will complete.
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
        if (Path.IsPathRooted(dirStructure) || dirStructure.AsSpan().IndexOfAny(_invalidPathChars.Value) > -1)
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
                    MaxRecursionDepth = maxRecursionDepth
                })
                .AsParallel()
                .Where(d => d.FullName.EndsWith(dirStructure, StringComparison.OrdinalIgnoreCase))
                .Select(d => d.FullName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
            );
        }

        if (!string.IsNullOrWhiteSpace(root))
        {
            if (!Path.IsPathRooted(root))
            {
                throw new ArgumentException("The root directory must be a well-formed absolute path to a directory.", nameof(root));
            }

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

        var names = files.Select(f => Path.GetFileName(f)).ToArray();
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
        ((ReadOnlySpan<char>)['\"', '<', '>', ':', '*', '?', '\\', '/']).CopyTo(destination);
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
        } ((ReadOnlySpan<char>)['|', '\0',
            (char)1, (char)2, (char)3, (char)4, (char)5, (char)6, (char)7, (char)8, (char)9, (char)10,
            (char)11, (char)12, (char)13, (char)14, (char)15, (char)16, (char)17, (char)18, (char)19, (char)20,
            (char)21, (char)22, (char)23, (char)24, (char)25, (char)26, (char)27, (char)28, (char)29, (char)30,
            (char)31
        ]).CopyTo(destination);
    }
}
