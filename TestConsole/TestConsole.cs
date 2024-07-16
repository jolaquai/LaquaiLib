using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

using LaquaiLib.Extensions;
using LaquaiLib.Util.ExceptionManagement;

using MediaDevices;

using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore;

using Potato.Fastboot;

namespace TestConsole;

/// <summary>
/// [Entry point] Represents a test console application for <see cref="LaquaiLib"/>.
/// </summary>
public partial class TestConsole
{
    [DebuggerStepThrough]
    private static async Task Main()
    {
        FirstChanceExceptionHandlers.RegisterAll();

        await using (var scope = await TestCore.TestCore.GetScope())
        {
            await ActualMain(scope.ServiceProvider);
        }
    }
    private static void cw<T>(T obj) => Console.WriteLine(obj);
    [STAThread]
    public static async ValueTask ActualMain(IServiceProvider serviceProvider)
    {
        _ = serviceProvider;

        var fb = new Fastboot();
        try
        {
            fb.Connect();
        }
        catch
        {
            Process.Start("cmd", "/c adb reboot bootloader").WaitForExit();
            Console.WriteLine($"Press any key once your device is in the bootloader.");
            Console.ReadKey();

            while (true)
            {
                try
                {
                    fb.Connect();
                    break;
                }
                catch
                {
                    await Task.Delay(1000);
                }
            }
        }
        AppDomain.CurrentDomain.ProcessExit += (s, e) => fb.Disconnect();

        // const long imei = 862680041740255; // top one
        const long imei = 862680041776267; // bottom one
        var unlocker = new BootloaderUnlocker(fb, imei);
        unlocker.TryUnlock();

        await Task.Delay(-1);
    }

    private class BootloaderUnlocker(Fastboot fastboot, long imei)
    {
        private long lastCheckedCode = 1000000000000000;
        private long imeiChecksum = LuhnChecksum(imei);
        private string lastPrefix;
        private long? nextIncr;

        public long TryUnlock()
        {
            nextIncr ??= (long)(imeiChecksum + Math.Sqrt(imei) * 1024);
            var code = lastCheckedCode;

            while (true)
            {
                var response = fastboot.Command($"fastboot oem unlock {code}");
                response.Payload = response.Payload.Trim();
                if (response.Payload == "Command not allowed")
                {
                }
                else if (response.Payload.Contains("success"))
                {
                    File.WriteAllText("oem.txt", $"Bootloader unlock code: {code}");
                    return code;
                }
                else if (response.Payload.Contains("reboot"))
                {
                    Console.WriteLine("There's weird stuff going on with your bootloader, I can't unlock it :c");
                }

                var prefix = code.ToString()[..2];
                if (lastPrefix != prefix)
                {
                    lastPrefix = prefix;
                    Console.WriteLine($"Surpassed new prefix '{lastPrefix}'");
                }

                lastCheckedCode = code;
                NextChecksum(ref code);
            }
        }

        private void NextChecksum(ref long genOEMcode)
        {
            genOEMcode += nextIncr.Value;
        }

        private static long LuhnChecksum(long imei)
        {
            static int[] DigitsOf(string str) => str.Select(static d => int.Parse(d.ToString())).ToArray();

            var digits = DigitsOf(imei.ToString());
            var oddDigits = digits.Reverse().Where((d, i) => i % 2 == 0).ToArray();
            var evenDigits = digits.Reverse().Where((d, i) => i % 2 != 0).ToArray();

            long checksum = oddDigits.Sum();
            foreach (var i in evenDigits)
            {
                checksum += DigitsOf((i * 2).ToString()).Sum();
            }

            return checksum % 10;
        }
    }
}
public static class Ext
{
    public readonly struct MediaDeviceHandle(MediaDevice device) : IDisposable
    {
        public void Dispose() => device.Disconnect();
    }
    public static MediaDeviceHandle Use(this MediaDevice device)
    {
        device.Connect();
        return new MediaDeviceHandle(device);
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
        using var response = await PostAsync(_query, mfd);
        await using var resStream = await response.Content.ReadAsStreamAsync();

        return await JsonSerializer.DeserializeAsync<webhookmessage>(resStream);
    }

    public class webhookmessage
    {
        public double type { get; init; }
        public string channel_id { get; init; }
        public string content { get; init; }
        public dynamic?[] attachments { get; init; }
        public dynamic?[] embeds { get; init; }
        public string timestamp { get; init; }
        public dynamic? edited_timestamp { get; init; }
        public double flags { get; init; }
        public dynamic?[] components { get; init; }
        public string id { get; init; }
        public user author { get; init; }
        public dynamic?[] mentions { get; init; }
        public dynamic?[] mention_roles { get; init; }
        public bool pinned { get; init; }
        public bool mention_everyone { get; init; }
        public bool tts { get; init; }
        public string webhook_id { get; init; }

        public async ValueTask<bool> DeleteAsync(DiscordWebhookApiClient client)
        {
            var uri = client.BaseAddress.Combine("messages", id);
            using var response = await client.DeleteAsync(uri);

            return response.IsSuccessStatusCode;
        }

        public async ValueTask<bool> EditAsync(string content, DiscordWebhookApiClient client)
        {
            var uri = client.BaseAddress.Combine("messages", id);
            using var mfd = new MultipartFormDataContent()
            {
                { new StringContent(content), nameof(content) },
            };
            using var response = await client.PatchAsync(uri, mfd);

            return response.IsSuccessStatusCode;
        }
    }

    public class user
    {
        public string id { get; init; }
        public string username { get; init; }
        public dynamic? avatar { get; init; }
        public string discriminator { get; init; }
        public double public_flags { get; init; }
        public double flags { get; init; }
        public bool bot { get; init; }
        public dynamic? global_name { get; init; }
        public dynamic? clan { get; init; }
    }
}

/// <summary>
/// Dynamically builds C# record type definition source code from a <see cref="JsonObject"/>.
/// </summary>
public class DynamicTypeBuilder(JsonObject root)
{
    public static string Build(JsonObject root) => new DynamicTypeBuilder(root).GenerateRecords();

    private class JsonProperty(string propKey, JsonObject? act, bool alreadyDeserialized) : IEquatable<JsonProperty>
    {
        public string PropKey { get; init; } = propKey;
        public JsonObject? Act { get; init; } = act;
        public bool AlreadyDeserialized { get; set; } = alreadyDeserialized;

        public bool Equals(JsonProperty? other) => PropKey == other?.PropKey;
        public override bool Equals(object? obj) => Equals(obj as JsonProperty);
        public override int GetHashCode() => PropKey.GetHashCode();
    }

    private string GenerateRecordsImpl(JsonObject node, HashSet<JsonProperty> furtherTypesToConstruct)
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
    private static string GetNodeType(JsonNode? node, HashSet<JsonProperty> furtherTypes)
    {
        if (new StackTrace().FrameCount > 100)
        {
            throw new InvalidOperationException("Stack depth is above 100.");
        }

        var type = node?.GetValueKind();
        switch (type)
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
