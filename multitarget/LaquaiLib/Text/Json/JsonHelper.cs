using System.Text.Json.Serialization.Metadata;

namespace LaquaiLib.Text.Json;

internal static class JsonHelper
{
    public static bool IsReflectionAllowed(JsonSerializerOptions options)
    {
        if (!JsonSerializer.IsReflectionEnabledByDefault)
            return false;
        if (options is null)
            return true;
        var resolver = options.TypeInfoResolver;
        if (resolver is null or DefaultJsonTypeInfoResolver)
            return true;
        foreach (var r in options.TypeInfoResolverChain)
            if (r is DefaultJsonTypeInfoResolver)
                return true;
        return false;
    }
}