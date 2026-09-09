using System.Text;

namespace Antananarivo.Core.Http;

public sealed class HttpResponseWriter
{
    public async Task WriteAsync(
        Stream stream,
        HttpResponse response,
        CancellationToken cancellationToken = default)
    {
        var responseBuilder = new StringBuilder();

        responseBuilder.Append(
            $"HTTP/1.1 {response.StatusCode.Code} {response.StatusCode.ReasonPhrase}\r\n");

        foreach (var header in response.Headers)
        {
            responseBuilder.Append(
                $"{header.Key}: {header.Value}\r\n");
        }

        responseBuilder.Append("\r\n");
        responseBuilder.Append(response.Body);

        byte[] data = Encoding.UTF8.GetBytes(
            responseBuilder.ToString());

        await stream.WriteAsync(
            data,
            cancellationToken);
    }
}