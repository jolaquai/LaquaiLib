using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

using LaquaiLib.Extensions;

namespace LaquaiLib.Util.Meta;

/// <summary>
/// Provides utility methods for meta-metaprogramming, finding Visual Studio paths, etc.
/// </summary>
public static class MetaHelpers
{
    /// <summary>
    /// A <see cref="BindingFlags"/> value that returns members regardless of access level and instance/static status.
    /// </summary>
    public const BindingFlags UNIVERSAL = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    /// <summary>
    /// Finds Visual Studio installations that are located in default installation directories, constrained by the specified parameters.
    /// </summary>
    /// <param name="versions">The version(s) of Visual Studio that should be included in the search.</param>
    /// <param name="edition">The edition(s) of Visual Studio that should be included in the search.</param>
    /// <param name="architecture">The architecture(s) of an app or tool that should be included in the search.</param>
    /// <returns>The paths of the Visual Studio installations that were found.</returns>
    public static IEnumerable<string> FindInstallations(
        VSVersion versions = VSVersion.Any,
        VSEdition edition = VSEdition.Any,
        Architecture architecture = Architecture.Any
    )
    {
        var drives = Array.ConvertAll(Array.FindAll(DriveInfo.GetDrives(), di => di.DriveType is DriveType.Fixed and not DriveType.Network), di => Path.TrimEndingDirectorySeparator(di.Name));
        static string GetUnrooted(string path) => path.StartsWith(Path.DirectorySeparatorChar) ? path[1..] : path[3..];
        string?[] programFiles = [architecture.HasFlag(Architecture.x64) ? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) : null, architecture.HasFlag(Architecture.x86) ? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) : null];
        programFiles = drives.CrossSelect(programFiles.Where(pf => pf is not null), (drive, programFile) => Path.Combine(drive, GetUnrooted(programFile))).ToArray();
        var vsVersions = Array.ConvertAll(versions.GetFlags(), flag => flag.GetDescription());
        var vsEditions = Array.ConvertAll(edition.GetFlags(), flag => flag.GetDescription());

        foreach (var programFile in programFiles)
        {
            foreach (var vsVersion in vsVersions)
            {
                foreach (var vsEdition in vsEditions)
                {
                    var msvc = Path.Combine(programFile, "Microsoft Visual Studio", vsVersion, vsEdition);
                    if (File.Exists(Path.Combine(msvc, "Common7", "IDE", "devenv.exe")))
                    {
                        yield return msvc;
                    }
                }
            }
        }
    }

    public static string[] FindTool(MetaTool tool, VSVersion versions = VSVersion.Any, VSEdition edition = VSEdition.Any, Architecture architecture = Architecture.Any)
    {
        return FindTool(tool.GetDescription(), versions, edition, architecture);
    }
    public static string[] FindTool(string fileName, VSVersion versions = VSVersion.Any, VSEdition edition = VSEdition.Any, Architecture architecture = Architecture.Any)
    {
        var drives = Array.ConvertAll(Array.FindAll(DriveInfo.GetDrives(), di => di.DriveType is DriveType.Fixed and not DriveType.Network), di => Path.TrimEndingDirectorySeparator(di.Name));
        static string GetUnrooted(string path) => path.StartsWith(Path.DirectorySeparatorChar) ? path[1..] : path[3..];
        string?[] programFiles = [architecture.HasFlag(Architecture.x64) ? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) : null, architecture.HasFlag(Architecture.x86) ? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) : null];
        programFiles = drives.CrossSelect(programFiles.Where(pf => pf is not null), (drive, programFile) => Path.Combine(drive, GetUnrooted(programFile))).ToArray();
        var vsVersions = Array.ConvertAll(versions.GetFlags(), flag => flag.GetDescription());
        var vsEditions = Array.ConvertAll(edition.GetFlags(), flag => flag.GetDescription());

        List<Task<string?>> tasks = [];
        foreach (var programFile in programFiles)
        {
            foreach (var vsVersion in vsVersions)
            {
                foreach (var vsEdition in vsEditions)
                {
                    var msvc = Path.Combine(programFile, "Microsoft Visual Studio", vsVersion, vsEdition);
                    if (Directory.Exists(msvc))
                    {
                        tasks.Add(Task.Run(() =>
                        {
                            foreach (var file in Directory.EnumerateFiles(msvc, fileName, SearchOption.AllDirectories))
                            {
                                return file;
                            }
                            return null;
                        }));
                    }
                }
            }
        }
        Task.WhenAll(tasks).Wait();

        return tasks.Where(t => t.Result is not null).Select(t => t.Result).ToArray();
    }
}
