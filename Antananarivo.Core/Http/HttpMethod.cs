namespace Antananarivo.Core.Http;

public readonly struct HttpMethod : IEquatable<HttpMethod>
{
    public static readonly HttpMethod Get = new("GET");
    public static readonly HttpMethod Post = new("POST");
    public static readonly HttpMethod Put = new("PUT");
    public static readonly HttpMethod Delete = new("DELETE");
    public static readonly HttpMethod Head = new("HEAD");
    public static readonly HttpMethod Options = new("OPTIONS");
    public static readonly HttpMethod Patch = new("PATCH");

    private readonly string _value;

    private HttpMethod(string value)
    {
        _value = value;
    }

    public override string ToString() => _value;

    public static implicit operator string(HttpMethod method) => method._value;
    public static implicit operator HttpMethod(string value)
    {
        return value switch
        {
            "GET" => Get,
            "POST" => Post,
            "PUT" => Put,
            "DELETE" => Delete,
            "HEAD" => Head,
            "OPTIONS" => Options,
            "PATCH" => Patch,
            _ => throw new FormatException(
                $"Unsupported HTTP method: {value}")
        };
    }

    public bool Equals(HttpMethod other) => _value == other._value;
    public override bool Equals(object? obj) => obj is HttpMethod other && Equals(other);
    public override int GetHashCode() => _value.GetHashCode();

    public static bool operator ==(HttpMethod left, HttpMethod right) => left.Equals(right);
    public static bool operator !=(HttpMethod left, HttpMethod right) => !left.Equals(right);
}