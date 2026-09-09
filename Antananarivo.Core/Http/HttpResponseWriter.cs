using System.Text;


namespace Antananarivo.Core.Http;


public sealed class HttpResponseWriter
{
    public async Task WriteAsync(
        Stream stream,
        HttpResponse response,
        CancellationToken cancellationToken = default)
    {
        var encoding = Encoding.UTF8;
        var responseBuilder = new StringBuilder();


        responseBuilder.Append(
            $"HTTP/1.1 {response.StatusCode.Code} {response.StatusCode.ReasonPhrase}\r\n");


        foreach (var header in response.Headers)
        {
            if (string.Equals(
                header.Key,
                "Content-Length",
                StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            responseBuilder.Append(
                $"{header.Key}: {header.Value}\r\n");
        }


        byte[] bodyBytes = encoding.GetBytes(response.Body);


        string contentLengthLine =
            $"Content-Length: {bodyBytes.Length}\r\n";

        responseBuilder.Append(contentLengthLine);


        responseBuilder.Append("\r\n");
        responseBuilder.Append(response.Body);


        byte[] data = encoding.GetBytes(responseBuilder.ToString());

        await stream.WriteAsync(
            data,
            cancellationToken);
    }
}