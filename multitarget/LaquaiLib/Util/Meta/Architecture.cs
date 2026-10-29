namespace LaquaiLib.Util.Meta;
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
