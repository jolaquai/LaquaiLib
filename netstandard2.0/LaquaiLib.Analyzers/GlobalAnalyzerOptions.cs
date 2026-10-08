namespace LaquaiLib.Analyzers;

internal static class GlobalAnalyzerOptions
{
    /// <summary>
    /// Checks common analyzer options for the analyzer that reports diagnostics using the specified <see cref="DiagnosticDescriptor"/>.
    /// Options may cause that descriptor to become outdated.
    /// </summary>
    /// <returns>A set of <see cref="DiagnosticDescriptor"/> if the analyzer should continue analyzing in the current context. If <see langword="null"/>, the analyzer should immediately return without reporting any diagnostics.</returns>
    public static ImmutableDictionary<string, DiagnosticDescriptor> CheckAnalyzer(AnalyzerConfigOptions analyzerConfigOptions, params ReadOnlySpan<DiagnosticDescriptor> diagnosticDescriptors)
    {
        var dict = new Dictionary<string, DiagnosticDescriptor>(diagnosticDescriptors.Length);

        for (var i = 0; i < diagnosticDescriptors.Length; i++)
        {
            var diagnosticDescriptor = diagnosticDescriptors[i];
            var id = diagnosticDescriptor.Id;

            var builder = new DiagnosticDescriptorBuilder(diagnosticDescriptor);

            if (analyzerConfigOptions.TryGetValue($"dotnet_diagnostic.{id}.enabled", out var value) && value.Equals("false", StringComparison.OrdinalIgnoreCase))
                return null; // Analyzer is disabled

            if (analyzerConfigOptions.TryGetValue($"dotnet_diagnostic.{id}.severity", out var severity))
            {
                if (Enum.TryParse<DiagnosticSeverity>(severity, true, out var parsed))
                    builder.DefaultSeverity = parsed;
                else if (int.TryParse(severity, out var parsedNum) && Enum.IsDefined(typeof(DiagnosticSeverity), parsedNum))
                    builder.DefaultSeverity = (DiagnosticSeverity)parsedNum;
                // else keep default severity
            }

            dict.Add(builder.Id, builder.Build());
        }

        return dict.ToImmutableDictionary();
    }
}
