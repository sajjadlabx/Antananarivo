using System.Net;
using Antananarivo.Core.Networking;

using var cancellationTokenSource = new CancellationTokenSource();

Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;

    Console.WriteLine("Shutdown requested...");

    cancellationTokenSource.Cancel();
};

var server = new TcpServer(
    IPAddress.Loopback,
    8080);

await server.StartAsync(
    cancellationTokenSource.Token);