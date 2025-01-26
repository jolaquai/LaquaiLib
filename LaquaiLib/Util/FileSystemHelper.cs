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
    /// <param name="destination">The directory that <paramref name="source"/> will be moved or copied to.</param>
    /// <param name="copy">Replicate the directory and its contents at the source location instead of moving it.</param>
    /// <param name="allowExisting">Whether to allow the destination directory to already exist and contain files and whether to allow overwriting existing files.</param>
    /// <param name="maxDegreeOfParallelism">The maximum number of concurrent operations to allow.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
    /// <remarks>
    /// This method uses <see cref="FileSizePartitioner"/> to create partitions for parallel processing that accounts for the size of the files in the directories.
    /// <para/>When the files are not moved or copied cross-device (such as from one drive to another), the operation can be completed significantly faster.
    /// </remarks>
    public static void MigrateDirectory(
        string source,
        string destination,
        bool copy = false,
        bool allowExisting = false,
        int maxDegreeOfParallelism = -1,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);
        if (maxDegreeOfParallelism < 0)
        {
            maxDegreeOfParallelism = Environment.ProcessorCount;
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

        // Create the directory structure first
        _ = Directory.CreateDirectory(destination);
        foreach (var dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
        {
            var newDirPath = dirPath.Replace(source, destination);
            _ = Directory.CreateDirectory(newDirPath);
        }

        var partitioner = new FileSizePartitioner(Directory.GetFiles(source, "*", SearchOption.AllDirectories));
        var partitions = partitioner.GetPartitions(maxDegreeOfParallelism);

        _ = Parallel.ForEach(
            partitions,
            pathEnumerator =>
            {
                while (pathEnumerator.MoveNext())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var current = pathEnumerator.Current;
                    var newFilePath = current.Replace(source, destination);
                    if (copy)
                    {
                        File.Copy(current, newFilePath, allowExisting);
                    }
                    else
                    {
                        File.Move(current, newFilePath, allowExisting);
                    }
                }
            });
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
        if (Path.IsPathRooted(dirStructure) || dirStructure.AsSpan().IndexOfAny(_invalidPathChars) > -1)
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

    private static ReadOnlySpan<char> _invalidFileNameChars => ['\"', '<', '>', ':', '*', '?', '\\', '/'];
    private static ReadOnlySpan<char> _invalidPathChars => ['|', '\0',
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
        _invalidFileNameChars.CopyTo(destination[33..41]);
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
        _invalidPathChars.CopyTo(destination);
    }
}
