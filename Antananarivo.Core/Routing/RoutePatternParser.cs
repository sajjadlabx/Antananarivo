namespace Antananarivo.Core.Routing;

/// <summary>
/// Parses a route pattern such as "/users/{id}/posts/{postId}"
/// into segments, validating parameter syntax.
/// </summary>
public static class RoutePatternParser
{
    public static IReadOnlyList<RouteSegment> Parse(string path)
    {
        var segments = new List<RouteSegment>();

        if (path == "/")
        {
            return segments;
        }

        foreach (var raw in path.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            var isParameter = raw.StartsWith('{') && raw.EndsWith('}');

            if (isParameter)
            {
                var name = raw[1..^1];

                if (name.Length == 0)
                {
                    throw new ArgumentException(
                        $"Route parameter name must not be empty: {path}");
                }

                if (name.Contains('{') || name.Contains('}'))
                {
                    throw new ArgumentException(
                        $"Invalid route parameter syntax: {raw} in {path}");
                }

                segments.Add(new RouteSegment(name, isParameter: true));
            }
            else
            {
                if (raw.Contains('{') || raw.Contains('}'))
                {
                    throw new ArgumentException(
                        $"Invalid route pattern: {raw} in {path}. " +
                        "Use '{{name}}' for a parameter.");
                }

                segments.Add(new RouteSegment(raw, isParameter: false));
            }
        }

        return segments;
    }

    /// <summary>
    /// The structural shape used for conflict detection, where every
    /// parameter segment is normalized to '{}'.
    /// </summary>
    public static string GetStructure(IReadOnlyList<RouteSegment> segments)
    {
        if (segments.Count == 0)
        {
            return "/";
        }

        return "/" + string.Join(
            "/",
            segments.Select(s => s.IsParameter ? "{}" : s.Value));
    }
}
