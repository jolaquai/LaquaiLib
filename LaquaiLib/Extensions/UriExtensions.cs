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
    public static string GetRoute(this Uri request)
    {
        return request.AbsolutePath;
    }
}