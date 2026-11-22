namespace LaquaiLib.Analyzers;

internal struct DiagnosticDescriptorBuilder
{
    /// <summary>
    /// The original ID of the diagnostic descriptor. This is the only immutable property of this builder.
    /// </summary>
    public readonly string Id { get; }
    public LocalizableString Title { get; set; }
    public LocalizableString MessageFormat { get; set; }
    public string Category { get; set; }
    public DiagnosticSeverity DefaultSeverity { get; set; }
    public bool IsEnabledByDefault { get; set; }

    public LocalizableString Description { get; set; }
    public string HelpLinkUri { get; set; }
    public List<string> CustomTags { get; set; }

    public DiagnosticDescriptorBuilder() { }
    public DiagnosticDescriptorBuilder(DiagnosticDescriptor existing)
    {
        Id = existing.Id;
        Title = existing.Title;
        MessageFormat = existing.MessageFormat;
        Category = existing.Category;
        DefaultSeverity = existing.DefaultSeverity;
        IsEnabledByDefault = existing.IsEnabledByDefault;

        Description = existing.Description;
        HelpLinkUri = existing.HelpLinkUri;
        CustomTags = [.. existing.CustomTags];
    }
    public DiagnosticDescriptorBuilder(string id, LocalizableString title, LocalizableString messageFormat, string category, DiagnosticSeverity defaultSeverity, bool isEnabledByDefault, LocalizableString description = null, string helpLinkUri = null, params List<string> customTags)
    {
        Id = id;
        Title = title;
        MessageFormat = messageFormat;
        Category = category;
        DefaultSeverity = defaultSeverity;
        IsEnabledByDefault = isEnabledByDefault;

        Description = description;
        HelpLinkUri = helpLinkUri;
        CustomTags = customTags;
    }
    public DiagnosticDescriptorBuilder(string id, string title, string messageFormat, string category, DiagnosticSeverity defaultSeverity, bool isEnabledByDefault, string description = null, string helpLinkUri = null, params List<string> customTags)
    {
        Id = id;
        Title = title;
        MessageFormat = messageFormat;
        Category = category;
        DefaultSeverity = defaultSeverity;
        IsEnabledByDefault = isEnabledByDefault;

        Description = description;
        HelpLinkUri = helpLinkUri;
        CustomTags = customTags;
    }

    // Does not finalize the builder, allowing for further modifications
    // EACH CALL RETURNS A NEW INSTANCE
    public readonly DiagnosticDescriptor Build() => new DiagnosticDescriptor(
        id: Id,
        title: Title,
        messageFormat: MessageFormat,
        category: Category,
        defaultSeverity: DefaultSeverity,
        isEnabledByDefault: IsEnabledByDefault,
        description: Description,
        helpLinkUri: HelpLinkUri,
        customTags: [.. CustomTags]
    );
}
