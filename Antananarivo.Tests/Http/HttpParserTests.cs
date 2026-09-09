using Antananarivo.Core.Http;

namespace Antananarivo.Tests.Http;

public class HttpParserTests
{
    [Fact]
    public void Parse_ValidGetRequest_ReturnsCorrectRequest()
    {
        var rawRequest =
            "GET / HTTP/1.1\r\n" +
            "Host: localhost:8080\r\n" +
            "User-Agent: TestClient\r\n" +
            "\r\n";

        var parser = new HttpParser();
        var request = parser.Parse(rawRequest);

        Assert.Equal("GET", request.Method);
        Assert.Equal("/", request.Path);
        Assert.Equal("HTTP/1.1", request.Version);
    }

    [Fact]
    public void Parse_RequestWithHeaders_ReturnsHeaders()
    {
        var rawRequest =
            "GET /hello HTTP/1.1\r\n" +
            "Host: localhost:8080\r\n" +
            "User-Agent: TestClient\r\n" +
            "\r\n";

        var parser = new HttpParser();
        var request = parser.Parse(rawRequest);

        Assert.Equal("localhost:8080", request.Headers["Host"]);
        Assert.Equal("TestClient", request.Headers["User-Agent"]);
    }

    [Fact]
    public void Parse_InvalidRequestLine_ThrowsFormatException()
    {
        var rawRequest =
            "GET /\r\n" +
            "Host: localhost:8080\r\n" +
            "\r\n";

        var parser = new HttpParser();

        Assert.Throws<FormatException>(
            () => parser.Parse(rawRequest));
    }

    [Fact]
    public void Parse_PathWithQueryString_SplitsPathAndQuery()
    {
        var rawRequest =
            "GET /products?page=2&sort=name HTTP/1.1\r\n" +
            "Host: localhost\r\n" +
            "\r\n";

        var parser = new HttpParser();
        var request = parser.Parse(rawRequest);

        Assert.Equal("/products", request.Path);
        Assert.Equal("?page=2&sort=name", request.QueryString);
    }

    [Fact]
    public void Parse_PathWithoutQueryString_HasEmptyQueryString()
    {
        var rawRequest =
            "GET / HTTP/1.1\r\n" +
            "Host: localhost\r\n" +
            "\r\n";

        var parser = new HttpParser();
        var request = parser.Parse(rawRequest);

        Assert.Equal("/", request.Path);
        Assert.Equal(string.Empty, request.QueryString);
    }

    [Fact]
    public void Parse_RequestWithBody_CapturesBody()
    {
        var rawRequest =
            "POST /data HTTP/1.1\r\n" +
            "Host: localhost\r\n" +
            "Content-Length: 13\r\n" +
            "\r\n" +
            "{\"key\":\"val\"}";

        var parser = new HttpParser();
        var request = parser.Parse(rawRequest);

        Assert.Equal("POST", request.Method);
        Assert.Equal("{\"key\":\"val\"}", request.Body);
    }

    [Fact]
    public void Parse_RequestWithoutBody_HasEmptyBody()
    {
        var rawRequest =
            "GET / HTTP/1.1\r\n" +
            "Host: localhost\r\n" +
            "\r\n";

        var parser = new HttpParser();
        var request = parser.Parse(rawRequest);

        Assert.Equal(string.Empty, request.Body);
    }
}