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

    [Fact]
    public void Parse_HeaderValueContainingColon_PreservesValue()
    {
        var rawRequest =
            "GET / HTTP/1.1\r\n" +
            "Authorization: Basic abc:def\r\n" +
            "\r\n";

        var parser = new HttpParser();
        var request = parser.Parse(rawRequest);

        Assert.Equal("Basic abc:def", request.Headers["Authorization"]);
    }

    [Fact]
    public void Parse_EmptyQueryString_ReturnsQuestionMarkOnly()
    {
        var rawRequest =
            "GET /hello? HTTP/1.1\r\n" +
            "Host: localhost\r\n" +
            "\r\n";

        var parser = new HttpParser();
        var request = parser.Parse(rawRequest);

        Assert.Equal("/hello", request.Path);
        Assert.Equal("?", request.QueryString);
    }

    [Fact]
    public void Parse_BodyDoesNotIncludeSeparatorCrlf()
    {
        var rawRequest =
            "POST /hello HTTP/1.1\r\n" +
            "Host: localhost\r\n" +
            "Content-Length: 5\r\n" +
            "\r\n" +
            "hello\r\n";

        var parser = new HttpParser();
        var request = parser.Parse(rawRequest);

        Assert.Equal("hello", request.Body);
    }

    [Fact]
    public void Parse_MultipleHeaders_ReturnsAllHeaders()
    {
        var rawRequest =
            "GET / HTTP/1.1\r\n" +
            "Host: localhost\r\n" +
            "Content-Type: text/plain\r\n" +
            "Content-Length: 5\r\n" +
            "\r\n";

        var parser = new HttpParser();
        var request = parser.Parse(rawRequest);

        Assert.Equal(3, request.Headers.Count);
        Assert.Equal("localhost", request.Headers["Host"]);
        Assert.Equal("text/plain", request.Headers["Content-Type"]);
        Assert.Equal("5", request.Headers["Content-Length"]);
    }

    [Fact]
    public void Parse_WhitespaceAroundHeaderValues_TrimsWhitespace()
    {
        var rawRequest =
            "GET / HTTP/1.1\r\n" +
            "  Host  :   localhost  \r\n" +
            "\r\n";

        var parser = new HttpParser();
        var request = parser.Parse(rawRequest);

        Assert.Equal("Host", request.Headers.Keys.First());
        Assert.Equal("localhost", request.Headers["Host"]);
    }

    [Fact]
    public void Parse_EmptyRequest_ThrowsFormatException()
    {
        var parser = new HttpParser();

        Assert.Throws<FormatException>(() => parser.Parse(""));
    }

    [Fact]
    public void Parse_NullRequest_ThrowsFormatException()
    {
        var parser = new HttpParser();

        Assert.Throws<FormatException>(() => parser.Parse(null!));
    }

    [Fact]
    public void Parse_MissingVersion_ThrowsFormatException()
    {
        var rawRequest =
            "GET /path\r\n" +
            "\r\n";

        var parser = new HttpParser();

        Assert.Throws<FormatException>(() => parser.Parse(rawRequest));
    }

    [Fact]
    public void Parse_PostMethod_ReturnsPostMethod()
    {
        var rawRequest =
            "POST /submit HTTP/1.1\r\n" +
            "Host: localhost\r\n" +
            "\r\n";

        var parser = new HttpParser();
        var request = parser.Parse(rawRequest);

        Assert.Equal("POST", request.Method);
    }

    [Fact]
    public void Parse_MultiLineBody_ReturnsFullBody()
    {
        var rawRequest =
            "POST /data HTTP/1.1\r\n" +
            "Host: localhost\r\n" +
            "\r\n" +
            "line1\r\n" +
            "line2\r\n" +
            "line3";

        var parser = new HttpParser();
        var request = parser.Parse(rawRequest);

        Assert.Equal("line1\r\nline2\r\nline3", request.Body);
    }

    [Fact]
    public void Parse_QueryStringWithMultipleParams_PreservesAll()
    {
        var rawRequest =
            "GET /search?q=test&page=1&limit=10 HTTP/1.1\r\n" +
            "Host: localhost\r\n" +
            "\r\n";

        var parser = new HttpParser();
        var request = parser.Parse(rawRequest);

        Assert.Equal("/search", request.Path);
        Assert.Equal("?q=test&page=1&limit=10", request.QueryString);
    }
}
