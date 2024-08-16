using System.Text;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Uri"/> Type.
/// </summary>
public static class UriExtensions
{
    /// <summary>
    /// Returns the route component of the <see cref="Uri"/>.
    /// <para/>For example, if the request was made to <c>http://localhost:8080/api/v1/endpoint?param1=1&amp;param2=2</c>, this method would return <c>/api/v1/endpoint</c>.
    /// </summary>
    /// <param name="request">The <see cref="Uri"/> to get the route from.</param>
    /// <returns>The route component of the <see cref="Uri"/>.</returns>
    public static string GetRoute(this Uri request) => request.AbsolutePath;

    /// <summary>
    /// Combines this <see cref="Uri"/> instance with multiple relative paths or <see cref="Uri"/>.
    /// This is intended to replicate <see cref="Path.Combine(string[])"/> for <see cref="Uri"/> instances.
    /// </summary>
    /// <param name="baseUri">The base <see cref="Uri"/> to combine with. May also be relative.</param>
    /// <param name="relative">Any number of <see langword="string"/>s or relative <see cref="Uri"/>s to combine with the base <see cref="Uri"/>. Must be relative.</param>
    /// <returns>A new <see cref="Uri"/> instance that is the result of combining the base <see cref="Uri"/> with the relative paths or <see cref="Uri"/>s.</returns>
    public static Uri Combine(this Uri baseUri, params ReadOnlySpan<object> relative)
    {
        if (relative.Length == 0)
        {
            return baseUri;
        }

        var sb = new StringBuilder(baseUri.ToString().TrimEnd('/') + '/');
        foreach (var r in relative)
        {
            if (r is not ((Uri and { IsAbsoluteUri: false }) or string))
            {
                throw new ArgumentException($"One of the specified arguments was not of type {typeof(Uri)} or string, or specified an absolute URI.");
            }
            sb.Append(r.ToString().TrimEnd('/') + '/');
        }
        return new Uri(sb.ToString().TrimEnd('/'));
    }
}