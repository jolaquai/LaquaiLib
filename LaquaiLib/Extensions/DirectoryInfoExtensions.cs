namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="DirectoryInfo"/> Type.
/// </summary>
public static class DirectoryInfoExtensions
{
    /// <summary>
    /// Creates a new <see cref="DirectoryInfo"/> instance for the subdirectory identified by <paramref name="name"/> if it exists, otherwise returns <see langword="null"/>.
    /// </summary>
    /// <param name="di">The <see cref="DirectoryInfo"/> instance to use as the current directory.</param>
    /// <param name="name">A relative directory path to search for under the directory represented by <paramref name="di"/>.</param>
    /// <returns>A <see cref="DirectoryInfo"/> instance for the subdirectory identified by <paramref name="name"/> if it exists, otherwise <see langword="null"/>.</returns>
    public static DirectoryInfo? Directory(this DirectoryInfo di, string name)
    {
        ArgumentNullException.ThrowIfNull(di);
        if (di.Exists)
        {
            var directory = new DirectoryInfo(Path.Combine(di.FullName, name));
            if (directory.Exists)
            {
                return directory;
            }
        }
        return null;
    }
    /// <summary>
    /// Creates a new <see cref="DirectoryInfo"/> instance for the subdirectory identified by <paramref name="name"/>, creating the entire directory structure it does not exist.
    /// </summary>
    /// <param name="di">The <see cref="DirectoryInfo"/> instance to use as the current directory.</param>
    /// <param name="name">A relative directory path to search for under the directory represented by <paramref name="di"/>.</param>
    /// <returns>A <see cref="DirectoryInfo"/> instance for the subdirectory identified by <paramref name="name"/>.</returns>
    public static DirectoryInfo? MakeDirectory(this DirectoryInfo di, string name)
    {
        ArgumentNullException.ThrowIfNull(di);
        var directory = new DirectoryInfo(Path.Combine(di.FullName, name));
        if (!directory.Exists)
        {
            directory.Create();
        }
        return directory;
    }
    /// <summary>
    /// Creates a <see cref="FileInfo"/> instance for the file identified by <paramref name="name"/> if it exists, otherwise returns <see langword="null"/>.
    /// </summary>
    /// <param name="di">The <see cref="DirectoryInfo"/> instance to use as the current directory.</param>
    /// <param name="name">A relative file path to search for under the directory represented by <paramref name="di"/>.</param>
    /// <returns>A <see cref="FileInfo"/> instance for the file identified by <paramref name="name"/> if it exists, otherwise <see langword="null"/>.</returns>
    public static FileInfo? File(this DirectoryInfo di, string name)
    {
        ArgumentNullException.ThrowIfNull(di);
        if (di.Exists)
        {
            var file = new FileInfo(Path.Combine(di.FullName, name));
            if (file.Exists)
            {
                return file;
            }
        }
        return null;
    }
    /// <summary>
    /// Creates a <see cref="FileInfo"/> instance for the file identified by <paramref name="name"/>, creating the entire directory structure and the file itself if any part of it does not exist.
    /// </summary>
    /// <param name="di">The <see cref="DirectoryInfo"/> instance to use as the current directory.</param>
    /// <param name="name">A relative file path to construct for under the directory represented by <paramref name="di"/>.</param>
    /// <returns>A <see cref="FileInfo"/> instance for the file identified by <paramref name="name"/>.</returns>
    public static FileInfo? MakeFile(this DirectoryInfo di, string name)
    {
        ArgumentNullException.ThrowIfNull(di);
        var file = new FileInfo(Path.Combine(di.FullName, name));
        if (file.Directory?.Exists is false)
        {
            file.Directory.Create();
        }
        if (!file.Exists)
        {
            using (file.Create())
            {
            }
        }
        return file;
    }
}
