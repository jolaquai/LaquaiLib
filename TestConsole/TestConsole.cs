﻿using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

using DiscUtils.BootConfig;

using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;

using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;

using DocumentFormat.OpenXml.Spreadsheet;

using LaquaiLib.Collections.LimitedCollections;
using LaquaiLib.Extensions.ALinq;
using LaquaiLib.Util.Threading;

using Microsoft.Diagnostics.Tracing.Parsers.Clr;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TestConsole;

/// <summary>
/// [Entry point] Represents a test console application for <see cref="LaquaiLib"/>.
/// </summary>
public static partial class TestConsole
{
    [STAThread]
    private static void Main()
    {
        // FirstChanceExceptionHandlers.RegisterAll();

        using (var scope = TestCore.TestCore.GetScope().ConfigureAwait(false).GetAwaiter().GetResult())
        {
            ActualMain(scope.ServiceProvider);//.ConfigureAwait(false).GetAwaiter().GetResult();
            // Debugger.Break();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void cw(this object obj) => Console.WriteLine(obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void cw<T>(this IEnumerable<T> enumerable) => Console.WriteLine($"<{typeof(T).Namespace + '.' + typeof(T).Name}>[{string.Join(", ", enumerable)}]");
    public static void ActualMain(IServiceProvider serviceProvider)
    {
        var client = serviceProvider.GetRequiredService<HttpClient>();

        #region
        //var path = @"E:\PROGRAMMING\Projects\C#\LaquaiLib\LaquaiLib\Extensions\ALinq\";
        //var linqMethods = typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static)
        //    .GroupBy(m => m.Name)
        //    .IntersectBy([
        //        "Aggregate",
        //        "AggregateBy",
        //        "All",
        //        "Any",
        //        "Average",
        //        "Contains",
        //        "Count",
        //        "CountBy",
        //        "DefaultIfEmpty",
        //        "ElementAt",
        //        "ElementAtOrDefault",
        //        "First",
        //        "FirstOrDefault",
        //        "Last",
        //        "LastOrDefault",
        //        "LongCount",
        //        "Max",
        //        "MaxBy",
        //        "Min",
        //        "MinBy",
        //        "SequenceEqual",
        //        "Single",
        //        "SingleOrDefault",
        //        "Sum",
        //        "ToArray",
        //        "ToDictionary",
        //        "ToHashSet",
        //        "ToList",
        //        "ToLookup"], g => g.Key)
        //    .ToArray();
        //// Create a cs file with a template for each method in the Enumerable class
        //if (Directory.Exists(path))
        //{
        //    Directory.Delete(path, true);
        //}
        //Directory.CreateDirectory(path);
        //foreach (var (name, overloads) in linqMethods)
        //{
        //    var template = $$"""
        //        namespace LaquaiLib.Extensions.ALinq;

        //        // Provides parallel extensions for the System.Linq.Enumerable.{{name}} family of methods.
        //        public static partial class IEnumerableExtensions
        //        {
        //        {{string.Join(Environment.NewLine, overloads.Select(static o => LaquaiLib.Extensions.MethodInfoExtensions.RebuildMethod(o,
        //            returnTypeTransform: t => $"System.Threading.Tasks.Task<{t}>",
        //            nameTransform: t => $"{t}Async",
        //            parametersTransform: static list =>
        //            {
        //                var copy = list[0];
        //                copy.Item1 = "this " + list[0].Item1;
        //                list[0] = copy;
        //                list.Add(("System.Threading.CancellationToken", "cancellationToken", "default"));
        //            },
        //            bodyGenerator: static (writer, accessibility, modifiers, returnType, methodName, genericParameters, parameters) =>
        //            {
        //                writer.Write($"""

        //                            => System.Threading.Tasks.Task.Run(() => {parameters[0].Name}.{methodName[..^5]}({string.Join(", ", parameters.Skip(1).Take(parameters.Count - 2).Select(p => p.Name))}), cancellationToken);
        //                    """);
        //            }
        //            )).Select(static str => $"    {str}"))}}
        //        }
        //        """;

        //    await File.WriteAllTextAsync(Path.Combine(path, $"{name}.cs"), template).ConfigureAwait(false);
        //} 
        #endregion

        ;
    }
}

#region Discord
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
#pragma warning disable CS8981 // Naming Styles
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
#pragma warning restore CS8981 // Naming Styles
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
        _ = sb.AppendLine("public class __reflected{{{n}}}");
        _ = sb.AppendLine("{");

        foreach (var (name, value) in node)
        {
            _ = sb.AppendLine($"    public {GetNodeType(value, furtherTypesToConstruct)} {name} {{ get; init; }}");
        }

        _ = sb.AppendLine("}");

        _ = sb.AppendLine();
        for (var i = 0; i < furtherTypesToConstruct.Count; i++)
        {
            var jProp = furtherTypesToConstruct.FirstOrDefault();
            if (jProp?.Act is not null && !jProp.AlreadyDeserialized)
            {
                jProp.AlreadyDeserialized = true;
                _ = sb.AppendLine(GenerateRecordsImpl(jProp.Act, furtherTypesToConstruct));
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
                _ = furtherTypes.Add(new JsonProperty(node.GetPath(), node?.AsObject(), false));
                return node.GetPropertyName();
            case null:
                return "dynamic?";
            default:
                return "dynamic";
        }
    }
}

#endregion