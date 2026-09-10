using HttpMethod = Antananarivo.Core.Http.HttpMethod;

namespace Antananarivo.Core.Routing;

public sealed class Route
{
    public Route(HttpMethod method, string path, RouteHandler handler)
    {
        Method = method;
        Path = path;
        Handler = handler;
    }

    public HttpMethod Method { get; }

    public string Path { get; }

    public RouteHandler Handler { get; }
}
