using HttpMethod = Antananarivo.Core.Http.HttpMethod;

namespace Antananarivo.Core.Routing;

/// <summary>
/// A registered route: an HTTP method paired with a path pattern
/// (e.g. "/users/{id}") mapped to a handler. Parameter segments
/// ({name}) match exactly one non-empty path segment.
/// </summary>
public sealed class Route
{
    public Route(HttpMethod method, string path, RouteHandler handler)
    {
        Method = method;
        Path = path;
        Handler = handler;
        Segments = RoutePatternParser.Parse(path);
    }

    public HttpMethod Method { get; }

    public string Path { get; }

    public RouteHandler Handler { get; }

    public IReadOnlyList<RouteSegment> Segments { get; }

    public bool HasParameters => Segments.Any(s => s.IsParameter);

    /// <summary>
    /// The matching shape of this route, e.g. "/users/{id}" and
    /// "/users/{userId}" both produce "/users/{}".
    /// </summary>
    public string Structure => RoutePatternParser.GetStructure(Segments);

    /// <summary>
    /// Checks whether the given request path matches this route's
    /// pattern. On a match, extracted parameter values are written
    /// into the supplied dictionary.
    /// </summary>
    public bool TryMatchPath(string path, Dictionary<string, string> parameters)
    {
        parameters.Clear();

        if (Segments.Count == 0)
        {
            return path == "/";
        }

        var parts = path.Split('/');

        // "/users/42" splits to ["", "users", "42"], so a path with
        // N non-empty segments yields N + 1 parts.
        if (parts.Length != Segments.Count + 1)
        {
            return false;
        }

        for (int i = 0; i < Segments.Count; i++)
        {
            var segment = Segments[i];

            // parts[0] is the empty string before the leading '/',
            // so segment i corresponds to parts[i + 1].
            var value = parts[i + 1];

            if (segment.IsParameter)
            {
                if (value.Length == 0)
                {
                    return false;
                }

                parameters[segment.Value] = value;
            }
            else if (!string.Equals(segment.Value, value, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }
}
