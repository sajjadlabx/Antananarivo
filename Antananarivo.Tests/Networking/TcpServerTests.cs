using System.Net;
using System.Net.Sockets;
using System.Text;
using Antananarivo.Core.Networking;

namespace Antananarivo.Tests.Networking;

public class TcpServerTests
{
    private static int GetAvailablePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    [Fact]
    public async Task Server_AcceptsConnection_SendsValidHttpResponse()
    {
        int port = GetAvailablePort();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var server = new TcpServer(IPAddress.Loopback, port);

        var serverTask = server.StartAsync(cts.Token);

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, port, cts.Token);

        using NetworkStream stream = client.GetStream();
        string request = "GET / HTTP/1.1\r\nHost: localhost\r\n\r\n";
        byte[] requestBytes = Encoding.UTF8.GetBytes(request);
        await stream.WriteAsync(requestBytes, cts.Token);

        byte[] buffer = new byte[4096];
        int bytesRead = await stream.ReadAsync(buffer, cts.Token);
        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        Assert.Contains("HTTP/1.1 200 OK", response);
        Assert.Contains("Hello from Antananarivo!", response);
        Assert.Contains("Content-Length:", response);

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
    public async Task Server_CanBeCancelled_ShutsDownCleanly()
    {
        int port = GetAvailablePort();
        using var cts = new CancellationTokenSource();
        var server = new TcpServer(IPAddress.Loopback, port);

        var serverTask = server.StartAsync(cts.Token);

        await Task.Delay(100, cts.Token);

        await cts.CancelAsync();

        await serverTask;

        Assert.True(cts.IsCancellationRequested);
    }

    [Fact]
    public async Task Server_MultipleConcurrentClients_AllReceiveResponses()
    {
        int port = GetAvailablePort();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var server = new TcpServer(IPAddress.Loopback, port);

        var serverTask = server.StartAsync(cts.Token);

        int clientCount = 5;
        var tasks = new List<Task>();

        for (int i = 0; i < clientCount; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                using var client = new TcpClient();
                await client.ConnectAsync(IPAddress.Loopback, port, cts.Token);

                using NetworkStream stream = client.GetStream();
                string request = "GET / HTTP/1.1\r\nHost: localhost\r\n\r\n";
                byte[] requestBytes = Encoding.UTF8.GetBytes(request);
                await stream.WriteAsync(requestBytes, cts.Token);

                byte[] buffer = new byte[4096];
                int bytesRead = await stream.ReadAsync(buffer, cts.Token);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Assert.Contains("HTTP/1.1 200 OK", response);
            }, cts.Token));
        }

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
}
