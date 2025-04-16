using System.ComponentModel;

namespace LaquaiLib.Util.Meta;

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
    /// <summary>
    /// Indicates that <c>dumpbin.exe</c> should be searched for.
    /// </summary>
    [Description("dumpbin.exe")]
    Dumpbin
}
