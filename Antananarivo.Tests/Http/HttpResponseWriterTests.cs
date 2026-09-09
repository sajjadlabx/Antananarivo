using System.Text;
using Antananarivo.Core.Http;

using HttpStatusCode = Antananarivo.Core.Http.HttpStatusCode;

namespace Antananarivo.Tests.Http;

public class HttpResponseWriterTests
{
    private static async Task<string> WriteToStringAsync(HttpResponse response)
    {
        var writer = new HttpResponseWriter();
        using var stream = new MemoryStream();
        await writer.WriteAsync(stream, response);
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    [Fact]
    public async Task Write_StatusLine_200OK()
    {
        var response = new HttpResponse
        {
            StatusCode = (HttpStatusCode)200,
            Body = "Hello"
        };

        string raw = await WriteToStringAsync(response);

        Assert.StartsWith("HTTP/1.1 200 OK\r\n", raw);
    }

    [Fact]
    public async Task Write_StatusLine_404NotFound()
    {
        var response = new HttpResponse
        {
            StatusCode = (HttpStatusCode)404,
            Body = "Page not found."
        };

        string raw = await WriteToStringAsync(response);

        Assert.StartsWith("HTTP/1.1 404 Not Found\r\n", raw);
    }

    [Fact]
    public async Task Write_Utf8Body_ContentLengthEqualsByteCount()
    {
        string body = "héllo 🌍";
        int expectedByteCount = Encoding.UTF8.GetByteCount(body);

        // Guard: byte count differs from char count for this body,
        // so the test actually proves bytes (not chars) are used.
        Assert.NotEqual(body.Length, expectedByteCount);

        var response = new HttpResponse
        {
            StatusCode = (HttpStatusCode)200,
            Body = body
        };

        string raw = await WriteToStringAsync(response);

        Assert.Contains($"Content-Length: {expectedByteCount}\r\n", raw);
    }

    [Fact]
    public async Task Write_EmptyBody_ContentLengthZero()
    {
        var response = new HttpResponse
        {
            StatusCode = (HttpStatusCode)200,
            Body = string.Empty
        };

        string raw = await WriteToStringAsync(response);

        Assert.Contains("Content-Length: 0\r\n", raw);
        Assert.EndsWith("\r\n\r\n", raw);
    }

    [Fact]
    public async Task Write_MultipleCustomHeaders_Preserved()
    {
        var response = new HttpResponse
        {
            StatusCode = (HttpStatusCode)200,
            Body = "Hello"
        };
        response.Headers["Content-Type"] = "text/plain; charset=utf-8";
        response.Headers["X-Custom-A"] = "A";
        response.Headers["X-Custom-B"] = "B";

        string raw = await WriteToStringAsync(response);

        Assert.Contains("Content-Type: text/plain; charset=utf-8\r\n", raw);
        Assert.Contains("X-Custom-A: A\r\n", raw);
        Assert.Contains("X-Custom-B: B\r\n", raw);
    }

    [Fact]
    public async Task Write_CrlfFormatting_CorrectSeparator()
    {
        var response = new HttpResponse
        {
            StatusCode = (HttpStatusCode)200,
            Body = "Hello"
        };
        response.Headers["Content-Type"] = "text/plain";

        string raw = await WriteToStringAsync(response);

        // No bare LF characters; every line ends with CRLF.
        string withoutCrlf = raw.Replace("\r\n", string.Empty);
        Assert.DoesNotContain("\n", withoutCrlf);
        Assert.DoesNotContain("\r", withoutCrlf);

        // Exactly one blank line separates headers from body.
        Assert.Contains("\r\n\r\n", raw);
    }

    [Fact]
    public async Task Write_BodySerialization_ExactBodyAfterHeaders()
    {
        string body = "{\"key\":\"val\"}";
        var response = new HttpResponse
        {
            StatusCode = (HttpStatusCode)200,
            Body = body
        };

        string raw = await WriteToStringAsync(response);

        int separatorIndex = raw.IndexOf("\r\n\r\n", StringComparison.Ordinal);
        Assert.True(separatorIndex >= 0);

        string actualBody = raw[(separatorIndex + 4)..];
        Assert.Equal(body, actualBody);
    }

    [Fact]
    public async Task Write_AutomaticallyGeneratesSingleContentLength()
    {
        var response = new HttpResponse
        {
            StatusCode = (HttpStatusCode)200,
            Body = "Hello"
        };

        string raw = await WriteToStringAsync(response);

        int count = CountOccurrences(raw, "Content-Length:");
        Assert.Equal(1, count);
        Assert.Contains($"Content-Length: {Encoding.UTF8.GetByteCount("Hello")}\r\n", raw);
    }

    [Fact]
    public async Task Write_ConflictingUserContentLength_IsOverriddenWithSingleCorrectValue()
    {
        var response = new HttpResponse
        {
            StatusCode = (HttpStatusCode)200,
            Body = "Hello"
        };
        response.Headers["Content-Length"] = "9999";

        string raw = await WriteToStringAsync(response);

        int count = CountOccurrences(raw, "Content-Length:");
        Assert.Equal(1, count);
        Assert.DoesNotContain("Content-Length: 9999", raw);
        Assert.Contains($"Content-Length: {Encoding.UTF8.GetByteCount("Hello")}\r\n", raw);
    }

    private static int CountOccurrences(string text, string value)
    {
        int count = 0;
        int index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }
        return count;
    }
}
