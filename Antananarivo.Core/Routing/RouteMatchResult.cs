using HttpMethod = Antananarivo.Core.Http.HttpMethod;

namespace Antananarivo.Core.Routing;

public sealed class RouteMatchResult
{
    private RouteMatchResult(RouteMatchStatus status, Route? route)
    {
        Status = status;
        Route = route;
    }

    public RouteMatchStatus Status { get; }

    public Route? Route { get; }

    public bool IsMatch => Status == RouteMatchStatus.Matched;

    public static RouteMatchResult Matched(Route route) => new(RouteMatchStatus.Matched, route);

    public static RouteMatchResult NotFound() => new(RouteMatchStatus.NotFound, null);

    public static RouteMatchResult MethodMismatch() => new(RouteMatchStatus.MethodMismatch, null);
}

public enum RouteMatchStatus
{
    NotFound,
    Matched,
    MethodMismatch
}
