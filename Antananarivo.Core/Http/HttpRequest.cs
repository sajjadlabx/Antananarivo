namespace Antananarivo.Core.Http;

public sealed class HttpRequest
{
    public string Method { get; init; } = string.Empty;

    public string Path { get; init; } = string.Empty;

    public string Version { get; init; } = string.Empty;

    public Dictionary<string, string> Headers { get; } = new();
}