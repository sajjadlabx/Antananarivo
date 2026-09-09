using Antananarivo.Core.Http;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Antananarivo.Core.Networking;

public sealed class TcpConnection
{
    public EndPoint? RemoteEndPoint =>
    _client.Client.RemoteEndPoint;

    private readonly TcpClient _client;

    public TcpConnection(TcpClient client)
    {
        _client = client;
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

        var parser = new HttpParser();

        HttpRequest request = parser.Parse(rawRequest);

        Console.WriteLine("Parsed HTTP Request:");
        Console.WriteLine($"Method: {request.Method}");
        Console.WriteLine($"Path: {request.Path}");
        Console.WriteLine($"Version: {request.Version}");

        var response = new HttpResponse
        {
            StatusCode = 200,
            Body = """
                   <!DOCTYPE html>
                   <html>
                   <head>
                       <meta charset="utf-8">
                       <title>Antananarivo</title>
                   </head>
                   <body>
                       <h1>Hello from Antananarivo!</h1>
                       <p>My web server is working.</p>
                   </body>
                   </html>
                   """
        };

        response.Headers["Content-Type"] =
            "text/html; charset=utf-8";

        var writer = new HttpResponseWriter();

        await writer.WriteAsync(
            stream,
            response,
            cancellationToken);

        Console.WriteLine("Response sent.");
    }

    public void Close()
    {
        _client.Close();
    }
}