using System.Net;
using Antananarivo.Core.Http;
using Antananarivo.Core.Networking;
using Antananarivo.Core.Routing;
using HttpStatusCode = Antananarivo.Core.Http.HttpStatusCode;

var router = new Router();

router.MapGet("/", _ => new HttpResponse
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
               <p>Try <a href="/users/42">/users/42</a>.</p>
           </body>
           </html>
           """
});

router.MapGet("/users/{id}", request =>
{
    var id = request.RouteParameters["id"];

    return new HttpResponse
    {
        StatusCode = 200,
        Body = $"""
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset="utf-8">
                    <title>Antananarivo</title>
                </head>
                <body>
                    <h1>User {id}</h1>
                    <p>You requested user {id}.</p>
                </body>
                </html>
                """
    };
});

router.MapGet("/health", _ => new HttpResponse
{
    StatusCode = 200,
    Body = "Healthy"
});

using var cancellationTokenSource = new CancellationTokenSource();

Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;

    Console.WriteLine("Shutdown requested...");

    cancellationTokenSource.Cancel();
};

var server = new TcpServer(
    IPAddress.Loopback,
    8080,
    router);

await server.StartAsync(
    cancellationTokenSource.Token);
