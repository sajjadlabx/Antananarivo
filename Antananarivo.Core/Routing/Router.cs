using Antananarivo.Core.Http;
using HttpMethod = Antananarivo.Core.Http.HttpMethod;

namespace Antananarivo.Core.Routing;

public sealed class Router
{
    private readonly List<Route> _routes = new();

    public IReadOnlyList<Route> Routes => _routes;

    public Router Map(HttpMethod method, string path, RouteHandler handler)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException(
                "Route path must not be null or empty.", nameof(path));
        }

        if (!path.StartsWith('/'))
        {
            throw new ArgumentException(
                $"Route path must start with '/': {path}", nameof(path));
        }

        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        if (_routes.Any(route => route.Method == method && route.Path == path))
        {
            throw new ArgumentException(
                $"A route for {method} {path} is already registered.");
        }

        _routes.Add(new Route(method, path, handler));

        return this;
    }

    public Router MapGet(string path, RouteHandler handler) =>
        Map(HttpMethod.Get, path, handler);

    public Router MapPost(string path, RouteHandler handler) =>
        Map(HttpMethod.Post, path, handler);

    public Router MapPut(string path, RouteHandler handler) =>
        Map(HttpMethod.Put, path, handler);

    public Router MapDelete(string path, RouteHandler handler) =>
        Map(HttpMethod.Delete, path, handler);

    public Router MapHead(string path, RouteHandler handler) =>
        Map(HttpMethod.Head, path, handler);

    public Router MapOptions(string path, RouteHandler handler) =>
        Map(HttpMethod.Options, path, handler);

    public Router MapPatch(string path, RouteHandler handler) =>
        Map(HttpMethod.Patch, path, handler);

    public RouteMatchResult Match(HttpRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var pathExists = false;

        foreach (var route in _routes)
        {
            if (!string.Equals(route.Path, request.Path, StringComparison.Ordinal))
            {
                continue;
            }

            if (route.Method == request.Method)
            {
                return RouteMatchResult.Matched(route);
            }

            pathExists = true;
        }

        return pathExists
            ? RouteMatchResult.MethodMismatch()
            : RouteMatchResult.NotFound();
    }
}
