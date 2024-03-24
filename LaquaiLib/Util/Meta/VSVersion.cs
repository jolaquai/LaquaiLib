using System.ComponentModel;

namespace LaquaiLib.Util.Meta;
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
