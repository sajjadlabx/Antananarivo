namespace Antananarivo.Core.Http;

public readonly struct HttpStatusCode : IEquatable<HttpStatusCode>
{
    private readonly int _code;
    private readonly string _reasonPhrase;

    private HttpStatusCode(int code, string reasonPhrase)
    {
        _code = code;
        _reasonPhrase = reasonPhrase;
    }

    public int Code => _code;

    public string ReasonPhrase => _reasonPhrase;

    public override string ToString() => _reasonPhrase;

    public static implicit operator int(HttpStatusCode statusCode) => statusCode._code;
    public static implicit operator HttpStatusCode(int code)
    {
        return code switch
        {
            100 => new HttpStatusCode(100, "Continue"),
            200 => new HttpStatusCode(200, "OK"),
            201 => new HttpStatusCode(201, "Created"),
            202 => new HttpStatusCode(202, "Accepted"),
            204 => new HttpStatusCode(204, "No Content"),
            301 => new HttpStatusCode(301, "Moved Permanently"),
            302 => new HttpStatusCode(302, "Found"),
            304 => new HttpStatusCode(304, "Not Modified"),
            400 => new HttpStatusCode(400, "Bad Request"),
            401 => new HttpStatusCode(401, "Unauthorized"),
            403 => new HttpStatusCode(403, "Forbidden"),
            404 => new HttpStatusCode(404, "Not Found"),
            405 => new HttpStatusCode(405, "Method Not Allowed"),
            408 => new HttpStatusCode(408, "Request Timeout"),
            409 => new HttpStatusCode(409, "Conflict"),
            413 => new HttpStatusCode(413, "Content Too Large"),
            415 => new HttpStatusCode(415, "Unsupported Media Type"),
            422 => new HttpStatusCode(422, "Unprocessable Content"),
            429 => new HttpStatusCode(429, "Too Many Requests"),
            500 => new HttpStatusCode(500, "Internal Server Error"),
            501 => new HttpStatusCode(501, "Not Implemented"),
            502 => new HttpStatusCode(502, "Bad Gateway"),
            503 => new HttpStatusCode(503, "Service Unavailable"),
            504 => new HttpStatusCode(504, "Gateway Timeout"),
            _ => throw new FormatException(
                $"Unsupported HTTP status code: {code}")
        };
    }

    public bool Equals(HttpStatusCode other) => _code == other._code && _reasonPhrase == other._reasonPhrase;
    public override bool Equals(object? obj) => obj is HttpStatusCode other && Equals(other);
    public override int GetHashCode() => _code.GetHashCode();

    public static bool operator ==(HttpStatusCode left, HttpStatusCode right) => left.Equals(right);
    public static bool operator !=(HttpStatusCode left, HttpStatusCode right) => !left.Equals(right);
}