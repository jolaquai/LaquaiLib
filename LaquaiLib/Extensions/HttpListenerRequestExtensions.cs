using System.Net;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="HttpListenerRequest"/> Type.
/// </summary>
public static class HttpListenerRequestExtensions
{
    /// <summary>
    /// Returns the route that the request was made to.
    /// <para/>For example, if the request was made to <c>http://localhost:8080/api/v1/endpoint?param1=1&amp;param2=2</c>, this method would return <c>/api/v1/endpoint</c>.
    /// </summary>
    /// <param name="request">The <see cref="HttpListenerRequest"/> to get the route from.</param>
    /// <returns>The route that the request was made to.</returns>
    public static string GetRoute(this HttpListenerRequest request)
    {
        return request.RawUrl[..request.RawUrl.IndexOf('?')];
    }
}
