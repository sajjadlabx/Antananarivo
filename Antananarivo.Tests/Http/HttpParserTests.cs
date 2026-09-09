using Antananarivo.Core.Http;

namespace Antananarivo.Tests.Http;

public class HttpParserTests
{
    [Fact]
    public void Parse_ValidGetRequest_ReturnsCorrectRequest()
    {
        // Arrange
        var rawRequest =
            "GET / HTTP/1.1\r\n" +
            "Host: localhost:8080\r\n" +
            "User-Agent: TestClient\r\n" +
            "\r\n";

        var parser = new HttpParser();

        // Act
        var request = parser.Parse(rawRequest);

        // Assert
        Assert.Equal("GET", request.Method);
        Assert.Equal("/", request.Path);
        Assert.Equal("HTTP/1.1", request.Version);
    }
}