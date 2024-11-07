using System.Runtime.CompilerServices;

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
        var parallelOptions = new ParallelOptions()
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = maxDegreeOfParallelism
        };

        if (copy)
        {
            Parallel.ForEach(
                partitioner,
                parallelOptions,
                newPath =>
                {
                    var newFilePath = newPath.Replace(source, newTopPath);
                    File.Copy(newPath, newFilePath, allowExisting);
                }
            );
        }
        else
        {
            Parallel.ForEach(
                partitioner,
                parallelOptions,
                newPath =>
                {
                    var newFilePath = newPath.Replace(source, newTopPath);
                    File.Move(newPath, newFilePath, allowExisting);
                }
            );
        }
    }

    /// <summary>
    /// Asynchronously enumerates the file system and attempts to find a directory structure that matches the one specified.
    /// Generally, the more specific the directory structure, the faster the search will complete.
    /// </summary>
    /// <param name="dirStructure">The directory structure to search for, for example <c>@"MyFolder\MySubFolder"</c>. Must be a well-formed relative path to a directory.</param>
    /// <param name="root">The absolute (rooted) path to the directory to start the search from. If <see langword="null"/>, the search starts from the root of each drive and returns all matches. This process may take a significant amount of time.</param>
    /// <param name="driveType">If <paramref name="root"/> is <see langword="null"/>, allows specifying which kinds of drives to search. The default is <see cref="DriveType.Fixed"/>. Note that searching network drives may take a significant amount of time.</param>
    /// <param name="maxRecursionDepth">Limits the depth of recursion when searching for the directory structure. The default is <see cref="int.MaxValue"/>.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> that enumerates the full paths of directories that match the specified structure.</returns>
    /// <exception cref="IOException">Thrown when the root directory does not exist.</exception>
    /// <exception cref="ArgumentException">Thrown when the directory structure or root directory is invalid.</exception>
    public static IAsyncEnumerable<string> EnumerateDirectoryStructureMatches(
        string dirStructure,
        string root = null,
        DriveType? driveType = null,
        int maxRecursionDepth = int.MaxValue
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dirStructure);
        if (Path.IsPathRooted(dirStructure)) throw new ArgumentException("The directory structure must be a well-formed relative path to a directory.", nameof(dirStructure));

        if (driveType is not null && !string.IsNullOrWhiteSpace(root))
        {
            throw new ArgumentException("Cannot specify both a drive type and a root directory.", nameof(driveType));
        }

        static IAsyncEnumerable<string> ExamineRootImpl(string dirStructure, string root, int maxRecursionDepth)
        {
            var dir = new DirectoryInfo(root);
            if (!dir.Exists) return AsyncEnumerableWrapper<string>.Empty;

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
            if (!Path.IsPathRooted(root)) throw new ArgumentException("The root directory must be a well-formed absolute path to a directory.", nameof(root));
            if (!Directory.Exists(root)) throw new IOException($"Directory '{root}' does not exist.");

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
}
