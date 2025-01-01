using LaquaiLib.Util;
using LaquaiLib.Util.ExceptionManagement;
using LaquaiLib.Util.Misc;

namespace TestConsole;

/// <summary>
/// [Entry point] Represents a test console application for <see cref="LaquaiLib"/>.
/// </summary>
public partial class TestConsole
{
    [STAThread]
    private static void Main()
    {
        // FirstChanceExceptionHandlers.RegisterAll();

        using (var scope = TestCore.TestCore.GetScope().ConfigureAwait(false).GetAwaiter().GetResult())
        {
            ActualMain(scope.ServiceProvider).ConfigureAwait(false).GetAwaiter().GetResult();
            // Debugger.Break();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void cw(object obj) => Console.WriteLine(obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void cw<T>(IEnumerable<T> enumerable) => Console.WriteLine($"<{typeof(T).Namespace + '.' + typeof(T).Name}>[{string.Join(", ", enumerable)}]");
    public static async Task ActualMain(IServiceProvider serviceProvider)
    {
        var client = serviceProvider.GetRequiredService<HttpClient>();
        ;
    }
}

public class DiscordWebhookApiClient : HttpClient
{
    private readonly string _query;
    public DiscordWebhookApiClient(string webhookUrl)
    {
        if (webhookUrl.IndexOf('?') is var index and > -1)
        {
            _query = webhookUrl[index..];
            webhookUrl = webhookUrl[..index];
        }
        BaseAddress = new Uri(webhookUrl);
    }

    public async Task<webhookmessage> CreateMessageAsync(string content)
    {
        using var mfd = new MultipartFormDataContent()
        {
            { new StringContent(content), nameof(content) },
        };
        using var response = await PostAsync(_query, mfd).ConfigureAwait(false);
        await using var resStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

        return await JsonSerializer.DeserializeAsync<webhookmessage>(resStream).ConfigureAwait(false);
    }

#pragma warning disable IDE1006 // Naming Styles
    public class webhookmessage
    {
        public double type { get; init; }
        public string channel_id { get; init; }
        public string content { get; init; }
        public dynamic[] attachments { get; init; }
        public dynamic[] embeds { get; init; }
        public string timestamp { get; init; }
        public dynamic edited_timestamp { get; init; }
        public double flags { get; init; }
        public dynamic[] components { get; init; }
        public string id { get; init; }
        public user author { get; init; }
        public dynamic[] mentions { get; init; }
        public dynamic[] mention_roles { get; init; }
        public bool pinned { get; init; }
        public bool mention_everyone { get; init; }
        public bool tts { get; init; }
        public string webhook_id { get; init; }

        public async ValueTask<bool> DeleteAsync(DiscordWebhookApiClient client)
        {
            var uri = client.BaseAddress.Combine("messages", id);
            using var response = await client.DeleteAsync(uri).ConfigureAwait(false);

            return response.IsSuccessStatusCode;
        }

        public async ValueTask<bool> EditAsync(string content, DiscordWebhookApiClient client)
        {
            var uri = client.BaseAddress.Combine("messages", id);
            using var mfd = new MultipartFormDataContent()
            {
                { new StringContent(content), nameof(content) },
            };
            using var response = await client.PatchAsync(uri, mfd).ConfigureAwait(false);

            return response.IsSuccessStatusCode;
        }
    }

    public class user
    {
        public string id { get; init; }
        public string username { get; init; }
        public dynamic avatar { get; init; }
        public string discriminator { get; init; }
        public double public_flags { get; init; }
        public double flags { get; init; }
        public bool bot { get; init; }
        public dynamic global_name { get; init; }
        public dynamic clan { get; init; }
    }
#pragma warning restore IDE1006 // Naming Styles
}

/// <summary>
/// Dynamically builds C# record type definition source code from a <see cref="JsonObject"/>.
/// </summary>
public class DynamicTypeBuilder(JsonObject root)
{
    public static string Build(JsonObject root) => new DynamicTypeBuilder(root).GenerateRecords();

    private class JsonProperty(string propKey, JsonObject act, bool alreadyDeserialized) : IEquatable<JsonProperty>
    {
        public string PropKey { get; init; } = propKey;
        public JsonObject Act { get; init; } = act;
        public bool AlreadyDeserialized { get; set; } = alreadyDeserialized;

        public bool Equals(JsonProperty other) => PropKey == other?.PropKey;
        public override bool Equals(object obj) => Equals(obj as JsonProperty);
        public override int GetHashCode() => PropKey.GetHashCode();
    }

    private static string GenerateRecordsImpl(JsonObject node, HashSet<JsonProperty> furtherTypesToConstruct)
    {
        var sb = new StringBuilder();
        sb.AppendLine("public class __reflected{{{n}}}");
        sb.AppendLine("{");

        foreach (var (name, value) in node)
        {
            sb.AppendLine($"    public {GetNodeType(value, furtherTypesToConstruct)} {name} {{ get; init; }}");
        }

        sb.AppendLine("}");

        sb.AppendLine();
        for (var i = 0; i < furtherTypesToConstruct.Count; i++)
        {
            var jProp = furtherTypesToConstruct.FirstOrDefault();
            if (jProp?.Act is not null && !jProp.AlreadyDeserialized)
            {
                jProp.AlreadyDeserialized = true;
                sb.AppendLine(GenerateRecordsImpl(jProp.Act, furtherTypesToConstruct));
            }
        }

        return sb.ToString();
    }
    public string GenerateRecords()
    {
        HashSet<JsonProperty> furtherTypesToConstruct = [];
        var str = GenerateRecordsImpl(root, furtherTypesToConstruct);
        var i = 0;
        return str.Replace("{{{n}}}", () =>
        {
            var ret = i.ToString();
            i++;
            return ret;
        });
    }
    private static string GetNodeType(JsonNode node, HashSet<JsonProperty> furtherTypes)
    {
        if (new StackTrace().FrameCount > 100)
        {
            throw new InvalidOperationException("Stack depth is above 100.");
        }

        switch (node?.GetValueKind())
        {
            case JsonValueKind.String:
                return "string";
            case JsonValueKind.Number:
                return "double";
            case JsonValueKind.True:
            case JsonValueKind.False:
                return "bool";
            case JsonValueKind.Array:
                return GetNodeType(node?.AsArray().FirstOrDefault(), furtherTypes) + "[]";
            case JsonValueKind.Object:
                furtherTypes.Add(new JsonProperty(node.GetPath(), node?.AsObject(), false));
                return node.GetPropertyName();
            case null:
                return "dynamic?";
            default:
                return "dynamic";
        }
    }
}
