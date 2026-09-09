namespace Antananarivo.Core.Http;

public sealed class HttpResponse
{
    public HttpStatusCode StatusCode { get; init; }

    public Dictionary<string, string> Headers { get; } = new();

    public string Body { get; init; } = string.Empty;
}