using System.ComponentModel;
using System.Diagnostics;

using LaquaiLib.Extensions;

namespace LaquaiLib.Util;

/// <summary>
/// Provides utility methods for meta-metaprogramming, finding Visual Studio paths, etc.
/// </summary>
public static class MetaHelpers
{
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

    /// <summary>
    /// Finds the executable path of the currently running Visual Studio instance.
    /// This does not handle multiple instances of Visual Studio running at the same time, and will return the path of the first instance found.
    /// </summary>
    /// <returns>The path of the currently running Visual Studio instance or <see langword="null"/> if none is found.</returns>
    public static string? GetCurrentVisualStudioLocation()
    {
        return Process.GetProcessesByName("devenv").FirstOrDefault()?.MainModule?.FileName;
    }
}
/// <summary>
/// Specifies the tool that should be searched for.
/// </summary>
public enum MetaTool
{
    /// <summary>
    /// Indicates that <c>msbuild.exe</c> should be searched for.
    /// </summary>
    [Description("msbuild.exe")]
    MSBuild,
}
/// <summary>
/// Specifies the version(s) of Visual Studio that should be included in the search.
/// </summary>
[Flags]
public enum VSVersion
{
    /// <summary>
    /// Indicates that Visual Studio 2017 installations should be included in the search.
    /// </summary>
    [Description("2017")]
    VS2017 = 0b1,
    /// <summary>
    /// Indicates that Visual Studio 2019 installations should be included in the search.
    /// </summary>
    [Description("2019")]
    VS2019 = 1 << 1,
    /// <summary>
    /// Indicates that Visual Studio 2022 installations should be included in the search.
    /// </summary>
    [Description("2022")]
    VS2022 = 1 << 2,
    /// <summary>
    /// Indicates that any version of Visual Studio is included in the search.
    /// </summary>
    Any = VS2017 | VS2019 | VS2022
}
/// <summary>
/// Specifies the edition(s) of Visual Studio that should be included in the search.
/// </summary>
[Flags]
public enum VSEdition
{
    /// <summary>
    /// Indicates that Community editions of Visual Studio should be included in the search.
    /// </summary>
    Community = 0b1,
    /// <summary>
    /// Indicates that Professional editions of Visual Studio should be included in the search.
    /// </summary>
    Professional = 1 << 1,
    /// <summary>
    /// Indicates that Enterprise editions of Visual Studio should be included in the search.
    /// </summary>
    Enterprise = 1 << 2,
    /// <summary>
    /// Indicates that Preview editions of Visual Studio should be included in the search.
    /// </summary>
    Preview = 1 << 3,
    /// <summary>
    /// Indicates that any edition of Visual Studio is included in the search.
    /// </summary>
    Any = Community | Professional | Enterprise | Preview
}
/// <summary>
/// Specifies the architecture(s) of an app or tool that should be included in the search.
/// </summary>
[Flags]
public enum Architecture
{
    /// <summary>
    /// Indicates that x86 apps or tools should be included in the search.
    /// </summary>
    x86 = 0b1,
    /// <summary>
    /// Indicates that x64 apps or tools should be included in the search.
    /// </summary>
    x64 = 1 << 1,
    /// <summary>
    /// Indicates that any architecture is included in the search.
    /// </summary>
    Any = x86 | x64
}
