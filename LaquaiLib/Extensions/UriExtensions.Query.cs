namespace LaquaiLib.Extensions;

public partial class UriExtensions
{
    /// <summary>
    /// Creates a new <see cref="Uri"/> with the specified query parameter set.
    /// </summary>
    /// <param name="uri">The <see cref="Uri"/> to set the query parameter in.</param>
    /// <param name="name">The name of the query parameter.</param>
    /// <param name="value">The value of the query parameter. If <see langword="null"/>, removes all values associated with the specified <paramref name="name"/> from the query.</param>
    public static Uri SetQueryParameter(this Uri uri, string name, object value)
    {
        var builder = new QueryBuilder(uri);
        builder[name] = value;
        return builder.Build();
    }
    /// <summary>
    /// Creates a new <see cref="Uri"/> with the specified query parameters set.
    /// </summary>
    /// <param name="uri">The <see cref="Uri"/> to begin with.</param>
    /// <param name="parameters">The parameters to set in the query.</param>
    /// <returns>A new <see cref="Uri"/> with the specified query parameters set.</returns>
    public static Uri SetQueryParameters(this Uri uri, params ReadOnlySpan<(string, object)> parameters)
    {
        var builder = new QueryBuilder(uri);
        foreach (var (name, value) in parameters)
        {
            builder[name] = value;
        }
        return builder.Build();
    }
    /// <summary>
    /// Creates a new <see cref="Uri"/> with the specified query parameters set.
    /// </summary>
    /// <param name="uri">The <see cref="Uri"/> to begin with.</param>
    /// <param name="parameters">The parameters to set in the query. This must be an even number of strings, beginning with a name and followed by a value (which may be another <see langword="string"/> or <see langword="null"/>). <see langword="null"/> causes existing pairs with the same name to be removed.</param>
    /// <returns>A new <see cref="Uri"/> with the specified query parameters set.</returns>
    /// <exception cref="ArgumentException">Thrown when the number of parameters is not even or is less than or equal to zero.</exception>
    public static Uri SetQueryParameters(this Uri uri, params ReadOnlySpan<string> parameters)
    {
        if (parameters.Length <= 0 || parameters.Length % 2 != 0)
        {
            throw new ArgumentException("The number of parameters must be even and greater than zero.", nameof(parameters));
        }
        var builder = new QueryBuilder(uri);
        for (var i = 0; i < parameters.Length; i += 2)
        {
            builder[parameters[i]] = parameters[i + 1];
        }
        return builder.Build();
    }
}

internal readonly struct QueryBuilder
{
    private readonly Uri _uri;
    private readonly Dictionary<string, string> _components;
    private readonly bool _disableEscaping;

    public readonly object this[string name]
    {
        get => _components[name];
        set
        {
            if (value is null)
            {
                _ = _components.Remove(name);
            }
            else
            {
                var data = value as string ?? value.ToString();
                _components[name] = _disableEscaping ? data : Uri.EscapeDataString(data);
            }
        }
    }

    /// <summary>
    /// Allows creation of a <see cref="QueryBuilder"/> that does not escape values for query parameters when setting them.
    /// </summary>
    internal QueryBuilder(Uri uri, bool disableEscaping)
    {
        _uri = uri;
        _components = [];
        if (uri.Query.Length > 1)
        {
            var span = uri.Query.AsSpan()[1..];
            foreach (var range in span.Split('&'))
            {
                var pair = span[range];
                var equalsIndex = pair.IndexOf('=');
                var name = pair[..equalsIndex];
                var value = pair[(equalsIndex + 1)..];
                _components[name.ToString()] = value.ToString();
            }
        }

        _disableEscaping = disableEscaping;
    }
    /// <summary>
    /// Creates a <see cref="QueryBuilder"/> that escapes values for query parameters when setting them.
    /// Assumes correct escaping for existing query parameters, otherwise the % signs would be escaped.
    /// </summary>
    public QueryBuilder(Uri uri) : this(uri, false)
    {
    }

    public readonly Uri Build()
    {
        var query = _components.Count == 0 ? "" : '?' + string.Join('&', _components.Select(static kvp => $"{kvp.Key}={kvp.Value}"));
        return new Uri(_uri.GetLeftPart(UriPartial.Path) + query);
    }
}
