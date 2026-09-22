using LaquaiLib.IO;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extensions for the <see cref="DirectoryInfo"/> type.
/// </summary>
public static partial class FileSystemModelExtensions
{
    extension(DirectoryInfo di)
    {
        /// <summary>
        /// Creates a new <see cref="DirectoryInfo"/> instance for the subdirectory identified by <paramref name="name"/>.
        /// </summary>
        /// <param name="name">A relative directory path to search for under the directory represented by <paramref name="di"/>.</param>
        /// <returns>A <see cref="DirectoryInfo"/> instance for the subdirectory identified by <paramref name="name"/>.</returns>
        public DirectoryInfo Directory(string name)
        {
            ArgumentNullException.ThrowIfNull(di);
            return new DirectoryInfo(Path.Combine(di.FullName, name));
        }
        /// <summary>
        /// Creates a new <see cref="DirectoryInfo"/> instance for the subdirectory identified by a path consisting of subdirectory <paramref name="names"/>.
        /// </summary>
        /// <param name="names">Any number of directory names to join and search for under the directory represented by <paramref name="di"/>.</param>
        /// <returns>A <see cref="DirectoryInfo"/> instance for the subdirectory identified by a path consisting of subdirectory <paramref name="names"/>.</returns>
        public DirectoryInfo Directory(params ReadOnlySpan<string> names) => di.Directory(Path.Combine(names));
        /// <summary>
        /// Creates a new <see cref="DirectoryInfo"/> instance for the subdirectory identified by <paramref name="name"/>, creating the entire directory structure if it does not exist.
        /// </summary>
        /// <param name="name">A relative directory path to search for under the directory represented by <paramref name="di"/>.</param>
        /// <returns>A <see cref="DirectoryInfo"/> instance for the subdirectory identified by <paramref name="name"/>.</returns>
        public DirectoryInfo MakeDirectory(string name)
        {
            ArgumentNullException.ThrowIfNull(di);
            var directory = di.Directory(name);
            if (!directory.Exists)
                directory.Create();
            return directory;
        }
        /// <summary>
        /// Creates a new <see cref="DirectoryInfo"/> instance for the subdirectory identified by a path consisting of subdirectory <paramref name="names"/>, creating the entire directory structure if it does not exist.
        /// </summary>
        /// <param name="names">Any number of directory names to join and search for under the directory represented by <paramref name="di"/>.</param>
        /// <returns>A <see cref="DirectoryInfo"/> instance for the subdirectory identified by a path consisting of subdirectory <paramref name="names"/>.</returns>
        public DirectoryInfo MakeDirectory(params ReadOnlySpan<string> names) => di.MakeDirectory(Path.Combine(names));

        /// <summary>
        /// Creates a <see cref="FileInfo"/> instance for a file identified by <paramref name="name"/>.
        /// </summary>
        /// <param name="name">A relative file path to search for under the directory represented by <paramref name="di"/>.</param>
        /// <returns>A <see cref="FileInfo"/> instance for a file identified by <paramref name="name"/>.</returns>
        public FileInfo File(string name)
        {
            ArgumentNullException.ThrowIfNull(di);
            return new FileInfo(Path.Combine(di.FullName, name));
        }
        /// <summary>
        /// Creates a <see cref="FileInfo"/> instance for a file identified by <paramref name="names"/>.
        /// </summary>
        /// <param name="names">Any number of directory names to join and search for under the directory represented by <paramref name="di"/>.</param>
        /// <returns>A <see cref="FileInfo"/> instance for a file identified by <paramref name="names"/>.</returns>
        public FileInfo File(params ReadOnlySpan<string> names) => di.File(Path.Combine(names));

        /// <summary>
        /// Determines whether the directory represented by <paramref name="di"/> is a base of the directory represented by <paramref name="comp"/>.
        /// </summary>
        /// <param name="comp">The <see cref="DirectoryInfo"/> instance to compare against.</param>
        /// <returns><see langword="true"/> if <paramref name="di"/> is a base of <paramref name="comp"/>, otherwise <see langword="false"/>.</returns>
        public bool IsBaseOf(DirectoryInfo comp)
        {
            ArgumentNullException.ThrowIfNull(di);
            ArgumentNullException.ThrowIfNull(comp);
            return FileSystemHelper.IsBaseOf(di.FullName, comp.FullName);
        }
        /// <summary>
        /// Determines whether the directory represented by <paramref name="di"/> is a base of the directory represented by <paramref name="comp"/>.
        /// </summary>
        /// <param name="comp">The path of the directory to compare against.</param>
        /// <returns><see langword="true"/> if <paramref name="di"/> is a base of <paramref name="comp"/>, otherwise <see langword="false"/>.</returns>
        public bool IsBaseOf(string comp)
        {
            ArgumentNullException.ThrowIfNull(di);
            ArgumentNullException.ThrowIfNull(comp);
            return FileSystemHelper.IsBaseOf(di.FullName, comp);
        }
    }
}
