using System.Net;
using System.Net.Sockets;

namespace Antananarivo.Core.Networking;

public sealed class TcpServer
{
    private readonly TcpListener _listener;

    public TcpServer(IPAddress address, int port)
    {
        _listener = new TcpListener(address, port);
    }

    public async Task StartAsync(
        CancellationToken cancellationToken = default)
    {
        _listener.Start();

        Console.WriteLine("Antananarivo server started.");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient client;

                try
                {
                    client = await _listener.AcceptTcpClientAsync(
                        cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                Console.WriteLine("Client connected.");

                _ = HandleClientAsync(
                    client,
                    cancellationToken);
            }
        }
        finally
        {
            _listener.Stop();

            Console.WriteLine("Antananarivo server stopped.");
        }
    }

    private async Task HandleClientAsync(
        TcpClient client,
        CancellationToken cancellationToken)
    {
        using (client)
        {
            var connection = new TcpConnection(client);

            try
            {
                await connection.ReceiveAsync(
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Connection cancelled.");
            }
            catch (IOException)
            {
                Console.WriteLine("Connection closed.");
            }
            finally
            {
                connection.Close();
            }
        }
    }
}