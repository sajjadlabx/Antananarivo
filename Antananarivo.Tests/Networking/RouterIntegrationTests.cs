using System.Net;
using System.Net.Sockets;
using System.Text;
using Antananarivo.Core.Http;
using Antananarivo.Core.Networking;
using Antananarivo.Core.Routing;

namespace Antananarivo.Tests.Networking;

public class RouterIntegrationTests
{
    private static int GetAvailablePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static async Task<string> SendRequestAsync(
        int port,
        string request,
        CancellationToken cancellationToken)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, port, cancellationToken);

        using NetworkStream stream = client.GetStream();
        byte[] requestBytes = Encoding.UTF8.GetBytes(request);
        await stream.WriteAsync(requestBytes, cancellationToken);

        byte[] buffer = new byte[8192];
        int bytesRead = await stream.ReadAsync(buffer, cancellationToken);
        return Encoding.UTF8.GetString(buffer, 0, bytesRead);
    }

    private static async Task<string> StartServerAndSendAsync(
        Router router,
        string request)
    {
        int port = GetAvailablePort();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var server = new TcpServer(IPAddress.Loopback, port, router);

        var serverTask = server.StartAsync(cts.Token);

        string response = await SendRequestAsync(port, request, cts.Token);

        await cts.CancelAsync();

        try
        {
            await serverTask;
        }
        catch (OperationCanceledException)
        {
        }

        return response;
    }

    [Fact]
    public async Task TcpRequest_ReachesRouter_ReturnsHandlerResponse()
    {
        var router = new Router().MapGet("/hello", _ => new HttpResponse
        {
            StatusCode = 200,
            Body = "Hello from the router!"
        });

        string response = await StartServerAndSendAsync(
            router,
            "GET /hello HTTP/1.1\r\nHost: localhost\r\n\r\n");

        Assert.Contains("HTTP/1.1 200 OK", response);
        Assert.Contains("Hello from the router!", response);
    }

    [Fact]
    public async Task PostRoute_WorksThroughTcp()
    {
        var router = new Router().MapPost("/users", _ => new HttpResponse
        {
            StatusCode = 201,
            Body = "User created"
        });

        string response = await StartServerAndSendAsync(
            router,
            "POST /users HTTP/1.1\r\nHost: localhost\r\nContent-Length: 0\r\n\r\n");

        Assert.Contains("HTTP/1.1 201 Created", response);
        Assert.Contains("User created", response);
    }

    [Fact]
    public async Task MethodMismatch_Returns405()
    {
        var router = new Router().MapGet("/hello", _ => new HttpResponse
        {
            StatusCode = 200,
            Body = "Hello"
        });

        string response = await StartServerAndSendAsync(
            router,
            "POST /hello HTTP/1.1\r\nHost: localhost\r\nContent-Length: 0\r\n\r\n");

        Assert.Contains("HTTP/1.1 405 Method Not Allowed", response);
    }

    [Fact]
    public async Task UnmatchedRoute_Returns404()
    {
        var router = new Router().MapGet("/", _ => new HttpResponse
        {
            StatusCode = 200,
            Body = "root"
        });

        string response = await StartServerAndSendAsync(
            router,
            "GET /nonexistent HTTP/1.1\r\nHost: localhost\r\n\r\n");

        Assert.Contains("HTTP/1.1 404 Not Found", response);
    }

    [Fact]
    public async Task ParameterizedRoute_WorksThroughTcp()
    {
        var router = new Router().MapGet("/users/{id}", request => new HttpResponse
        {
            StatusCode = 200,
            Body = $"User {request.RouteParameters["id"]}"
        });

        string response = await StartServerAndSendAsync(
            router,
            "GET /users/42 HTTP/1.1\r\nHost: localhost\r\n\r\n");

        Assert.Contains("HTTP/1.1 200 OK", response);
        Assert.Contains("User 42", response);
    }

    [Fact]
    public async Task RouteParameter_ExtractionThroughTcp()
    {
        string? capturedId = null;
        var router = new Router().MapGet("/users/{id}", request =>
        {
            capturedId = request.RouteParameters["id"];
            return new HttpResponse { StatusCode = 200, Body = "ok" };
        });

        await StartServerAndSendAsync(
            router,
            "GET /users/42 HTTP/1.1\r\nHost: localhost\r\n\r\n");

        Assert.Equal("42", capturedId);
    }

    [Fact]
    public async Task QueryString_SurvivesRouting()
    {
        string? capturedQuery = null;
        string? capturedId = null;
        var router = new Router().MapGet("/users/{id}", request =>
        {
            capturedId = request.RouteParameters["id"];
            capturedQuery = request.QueryString;
            return new HttpResponse { StatusCode = 200, Body = "ok" };
        });

        await StartServerAndSendAsync(
            router,
            "GET /users/42?active=true HTTP/1.1\r\nHost: localhost\r\n\r\n");

        Assert.Equal("42", capturedId);
        Assert.Equal("?active=true", capturedQuery);
    }

    [Fact]
    public async Task MalformedRequest_Returns400()
    {
        var router = new Router().MapGet("/", _ => new HttpResponse
        {
            StatusCode = 200,
            Body = "root"
        });

        string response = await StartServerAndSendAsync(
            router,
            "GARBAGE\r\n\r\n");

        Assert.Contains("HTTP/1.1 400 Bad Request", response);
    }

    [Fact]
    public async Task Response_IncludesContentLengthAndBody()
    {
        var router = new Router().MapGet("/data", _ => new HttpResponse
        {
            StatusCode = 200,
            Body = "payload"
        });

        string response = await StartServerAndSendAsync(
            router,
            "GET /data HTTP/1.1\r\nHost: localhost\r\n\r\n");

        Assert.Contains("HTTP/1.1 200 OK", response);
        Assert.Contains("Content-Length:", response);
        Assert.Contains("payload", response);
    }

    [Fact]
    public async Task MultipleSequentialRequests_EachRoutedCorrectly()
    {
        int port = GetAvailablePort();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var router = new Router()
            .MapGet("/hello", _ => new HttpResponse { StatusCode = 200, Body = "hello route" })
            .MapPost("/users", _ => new HttpResponse { StatusCode = 201, Body = "created" })
            .MapGet("/users/{id}", request => new HttpResponse
            {
                StatusCode = 200,
                Body = $"user {request.RouteParameters["id"]}"
            });

        var server = new TcpServer(IPAddress.Loopback, port, router);
        var serverTask = server.StartAsync(cts.Token);

        string getResponse = await SendRequestAsync(
            port,
            "GET /hello HTTP/1.1\r\nHost: localhost\r\n\r\n",
            cts.Token);
        string postResponse = await SendRequestAsync(
            port,
            "POST /users HTTP/1.1\r\nHost: localhost\r\nContent-Length: 0\r\n\r\n",
            cts.Token);
        string paramResponse = await SendRequestAsync(
            port,
            "GET /users/7 HTTP/1.1\r\nHost: localhost\r\n\r\n",
            cts.Token);
        string notFoundResponse = await SendRequestAsync(
            port,
            "GET /missing HTTP/1.1\r\nHost: localhost\r\n\r\n",
            cts.Token);

        Assert.Contains("hello route", getResponse);
        Assert.Contains("HTTP/1.1 201 Created", postResponse);
        Assert.Contains("user 7", paramResponse);
        Assert.Contains("HTTP/1.1 404 Not Found", notFoundResponse);

        await cts.CancelAsync();

        try
        {
            await serverTask;
        }
        catch (OperationCanceledException)
        {
        }
    }

    [Fact]
    public async Task MultipleConcurrentConnections_AllRouted()
    {
        int port = GetAvailablePort();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var router = new Router().MapGet("/", _ => new HttpResponse
        {
            StatusCode = 200,
            Body = "concurrent root"
        });

        var server = new TcpServer(IPAddress.Loopback, port, router);
        var serverTask = server.StartAsync(cts.Token);

        var tasks = Enumerable.Range(0, 5).Select(_ => Task.Run(async () =>
        {
            string response = await SendRequestAsync(
                port,
                "GET / HTTP/1.1\r\nHost: localhost\r\n\r\n",
                cts.Token);
            Assert.Contains("concurrent root", response);
        }, cts.Token));

        await Task.WhenAll(tasks);

        await cts.CancelAsync();

        try
        {
            await serverTask;
        }
        catch (OperationCanceledException)
        {
        }
    }

    [Fact]
    public async Task RootRoute_ReturnsHtmlThroughTcp()
    {
        var router = new Router().MapGet("/", _ => new HttpResponse
        {
            StatusCode = 200,
            Body = "<h1>Hello from Antananarivo!</h1>"
        });

        string response = await StartServerAndSendAsync(
            router,
            "GET / HTTP/1.1\r\nHost: localhost\r\n\r\n");

        Assert.Contains("HTTP/1.1 200 OK", response);
        Assert.Contains("<h1>Hello from Antananarivo!</h1>", response);
    }
}
