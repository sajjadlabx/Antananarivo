using Antananarivo.Core.Http;
using Antananarivo.Core.Routing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using HttpStatusCode = Antananarivo.Core.Http.HttpStatusCode;

namespace Antananarivo.Core.Networking;

public sealed class TcpConnection
{
    private readonly TcpClient _client;
    private readonly Router _router;
    private readonly HttpParser _parser = new();
    private readonly HttpResponseWriter _writer = new();

    public EndPoint? RemoteEndPoint =>
        _client.Client.RemoteEndPoint;

    public TcpConnection(TcpClient client, Router router)
    {
        _client = client;
        _router = router ?? throw new ArgumentNullException(nameof(router));
    }

    public async Task ReceiveAsync(
        CancellationToken cancellationToken = default)
    {
        using NetworkStream stream = _client.GetStream();

        byte[] buffer = new byte[4096];

        int bytesRead = await stream.ReadAsync(
            buffer,
            cancellationToken);

        if (bytesRead == 0)
        {
            Console.WriteLine("Client disconnected.");
            return;
        }

        string rawRequest = Encoding.UTF8.GetString(
            buffer,
            0,
            bytesRead);

        Console.WriteLine("Raw HTTP Request:");
        Console.WriteLine(rawRequest);

        HttpRequest request;

        try
        {
            request = _parser.Parse(rawRequest);
        }
        catch (FormatException)
        {
            Console.WriteLine("Malformed HTTP request.");

            await WriteResponseAsync(
                stream,
                CreateErrorResponse(400),
                cancellationToken);

            return;
        }

        Console.WriteLine("Parsed HTTP Request:");
        Console.WriteLine($"Method: {request.Method}");
        Console.WriteLine($"Path: {request.Path}");
        Console.WriteLine($"Version: {request.Version}");

        var response = ResolveResponse(request);

        await WriteResponseAsync(
            stream,
            response,
            cancellationToken);

        Console.WriteLine("Response sent.");
    }

    private HttpResponse ResolveResponse(HttpRequest request)
    {
        var match = _router.Match(request);

        if (match.IsMatch)
        {
            return match.Route!.Handler(request);
        }

        return match.Status == RouteMatchStatus.MethodMismatch
            ? CreateErrorResponse(405)
            : CreateErrorResponse(404);
    }

    private static HttpResponse CreateErrorResponse(int statusCode)
    {
        var code = (HttpStatusCode)statusCode;

        var response = new HttpResponse
        {
            StatusCode = code,
            Body = $"{code.Code} {code.ReasonPhrase}"
        };

        response.Headers["Content-Type"] =
            "text/plain; charset=utf-8";

        return response;
    }

    private async Task WriteResponseAsync(
        NetworkStream stream,
        HttpResponse response,
        CancellationToken cancellationToken)
    {
        await _writer.WriteAsync(
            stream,
            response,
            cancellationToken);
    }

    public void Close()
    {
        _client.Close();
    }
}
