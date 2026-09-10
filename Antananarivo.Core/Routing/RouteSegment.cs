namespace Antananarivo.Core.Routing;

public sealed class RouteSegment
{
    public RouteSegment(string value, bool isParameter)
    {
        Value = value;
        IsParameter = isParameter;
    }

    public string Value { get; }

    public bool IsParameter { get; }
}
