namespace LaquaiLib.Generators;

/// <summary>
/// Tracking names for every user-defined step in this assembly's incremental pipelines. Shared with the test project so a rename can't silently turn an incrementality assertion into a no-op.
/// </summary>
public static class GeneratorStepNames
{
    public const string FullAccessProxyModels = "FullAccessProxy.Models";
    public const string FullAccessProxyFiltered = "FullAccessProxy.Filtered";

    public const string InlineArrayModels = "InlineArray.Models";
    public const string InlineArrayFiltered = "InlineArray.Filtered";
    public const string InlineArrayCollected = "InlineArray.Collected";

    public const string EnumExpanderModels = "EnumExpander.Models";
    public const string EnumExpanderFiltered = "EnumExpander.Filtered";
    public const string EnumExpanderCollected = "EnumExpander.Collected";
}
