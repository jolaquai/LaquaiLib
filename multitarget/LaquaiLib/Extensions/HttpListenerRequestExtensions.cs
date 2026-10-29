using System.Net;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extensions for the <see cref="HttpListenerRequest"/> type.
/// </summary>
public static class HttpListenerRequestExtensions
{
    extension(HttpListenerRequest request)
    {
        /// <summary>
        /// Returns the route that the request was made to.
        /// <para/>For example, if the request was made to <c>http://localhost:8080/api/v1/endpoint?param1=1&amp;param2=2</c>, this method would return <c>/api/v1/endpoint</c>.
        /// </summary>
        /// <returns>The route that the request was made to.</returns>
        public string Route
        {
            get
            {
                var rawUrl = request.RawUrl;
                var queryIndex = rawUrl.IndexOf('?');
                // No query string means the whole RawUrl is the route.
                return queryIndex < 0 ? rawUrl : rawUrl[..queryIndex];
            }
        }
    }
}
