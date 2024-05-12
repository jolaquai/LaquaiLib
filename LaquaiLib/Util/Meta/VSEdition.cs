namespace LaquaiLib.Util.Meta;
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
