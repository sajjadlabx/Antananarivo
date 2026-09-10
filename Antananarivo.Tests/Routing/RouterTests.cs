using Antananarivo.Core.Http;
using Antananarivo.Core.Routing;
using HttpMethod = Antananarivo.Core.Http.HttpMethod;

namespace Antananarivo.Tests.Routing;

public class RouterTests
{
    private static HttpRequest CreateRequest(HttpMethod method, string path) =>
        new() { Method = method, Path = path };

    [Fact]
    public void MapGet_RegistersRoute()
    {
        var router = new Router();

        router.MapGet("/users", _ => new HttpResponse());

        var route = Assert.Single(router.Routes);
        Assert.Equal(HttpMethod.Get, route.Method);
        Assert.Equal("/users", route.Path);
    }

    [Fact]
    public void MapGet_ReturnsRouterForChaining()
    {
        var router = new Router();

        var result = router.MapGet("/", _ => new HttpResponse());

        Assert.Same(router, result);
    }

    [Fact]
    public void MapPost_RegistersRoute()
    {
        var router = new Router();

        router.MapPost("/users", _ => new HttpResponse());

        var route = Assert.Single(router.Routes);
        Assert.Equal(HttpMethod.Post, route.Method);
        Assert.Equal("/users", route.Path);
    }

    [Fact]
    public void MapPut_RegistersRoute()
    {
        var router = new Router();

        router.MapPut("/users", _ => new HttpResponse());

        var route = Assert.Single(router.Routes);
        Assert.Equal(HttpMethod.Put, route.Method);
        Assert.Equal("/users", route.Path);
    }

    [Fact]
    public void MapDelete_RegistersRoute()
    {
        var router = new Router();

        router.MapDelete("/users", _ => new HttpResponse());

        var route = Assert.Single(router.Routes);
        Assert.Equal(HttpMethod.Delete, route.Method);
        Assert.Equal("/users", route.Path);
    }

    [Fact]
    public void Map_MultipleRoutes_AllRegistered()
    {
        var router = new Router();

        router.MapGet("/", _ => new HttpResponse());
        router.MapGet("/users", _ => new HttpResponse());
        router.MapPost("/users", _ => new HttpResponse());
        router.MapPut("/users", _ => new HttpResponse());
        router.MapDelete("/users", _ => new HttpResponse());

        Assert.Equal(5, router.Routes.Count);
    }

    [Fact]
    public void Map_NullHandler_ThrowsArgumentNullException()
    {
        var router = new Router();

        Assert.Throws<ArgumentNullException>(() => router.MapGet("/users", null!));
    }

    [Fact]
    public void Map_EmptyPath_ThrowsArgumentException()
    {
        var router = new Router();

        Assert.Throws<ArgumentException>(() => router.MapGet("", _ => new HttpResponse()));
    }

    [Fact]
    public void Map_PathWithoutLeadingSlash_ThrowsArgumentException()
    {
        var router = new Router();

        Assert.Throws<ArgumentException>(() => router.MapGet("users", _ => new HttpResponse()));
    }

    [Fact]
    public void Map_DuplicateRoute_ThrowsArgumentException()
    {
        var router = new Router();
        router.MapGet("/users", _ => new HttpResponse());

        Assert.Throws<ArgumentException>(() =>
            router.MapGet("/users", _ => new HttpResponse()));
    }

    [Fact]
    public void Map_SamePathDifferentMethod_IsAllowed()
    {
        var router = new Router();

        router.MapGet("/users", _ => new HttpResponse());
        router.MapPost("/users", _ => new HttpResponse());

        Assert.Equal(2, router.Routes.Count);
    }

    [Fact]
    public void Match_GetRoute_Matches()
    {
        var router = new Router();
        router.MapGet("/hello", _ => new HttpResponse());

        var result = router.Match(CreateRequest(HttpMethod.Get, "/hello"));

        Assert.True(result.IsMatch);
        Assert.NotNull(result.Route);
        Assert.Equal(HttpMethod.Get, result.Route.Method);
        Assert.Equal("/hello", result.Route.Path);
    }

    [Fact]
    public void Match_PostRoute_Matches()
    {
        var router = new Router();
        router.MapPost("/users", _ => new HttpResponse());

        var result = router.Match(CreateRequest(HttpMethod.Post, "/users"));

        Assert.True(result.IsMatch);
    }

    [Fact]
    public void Match_PutRoute_Matches()
    {
        var router = new Router();
        router.MapPut("/users", _ => new HttpResponse());

        var result = router.Match(CreateRequest(HttpMethod.Put, "/users"));

        Assert.True(result.IsMatch);
    }

    [Fact]
    public void Match_DeleteRoute_Matches()
    {
        var router = new Router();
        router.MapDelete("/users", _ => new HttpResponse());

        var result = router.Match(CreateRequest(HttpMethod.Delete, "/users"));

        Assert.True(result.IsMatch);
    }

    [Fact]
    public void Match_MethodMismatch_ReturnsMethodMismatch()
    {
        var router = new Router();
        router.MapGet("/hello", _ => new HttpResponse());

        var result = router.Match(CreateRequest(HttpMethod.Post, "/hello"));

        Assert.False(result.IsMatch);
        Assert.Equal(RouteMatchStatus.MethodMismatch, result.Status);
    }

    [Fact]
    public void Match_PathMismatch_ReturnsNotFound()
    {
        var router = new Router();
        router.MapGet("/hello", _ => new HttpResponse());

        var result = router.Match(CreateRequest(HttpMethod.Get, "/world"));

        Assert.False(result.IsMatch);
        Assert.Equal(RouteMatchStatus.NotFound, result.Status);
    }

    [Fact]
    public void Match_NoRoutes_ReturnsNotFound()
    {
        var router = new Router();

        var result = router.Match(CreateRequest(HttpMethod.Get, "/"));

        Assert.False(result.IsMatch);
        Assert.Equal(RouteMatchStatus.NotFound, result.Status);
    }

    [Fact]
    public void Match_MultipleRoutes_FindsCorrectHandler()
    {
        var router = new Router();
        router.MapGet("/", _ => new HttpResponse { Body = "root" });
        router.MapGet("/users", _ => new HttpResponse { Body = "users" });
        router.MapPost("/users", _ => new HttpResponse { Body = "created" });

        var getResult = router.Match(CreateRequest(HttpMethod.Get, "/users"));
        var postResult = router.Match(CreateRequest(HttpMethod.Post, "/users"));
        var rootResult = router.Match(CreateRequest(HttpMethod.Get, "/"));

        Assert.True(getResult.IsMatch);
        Assert.Equal("users", getResult.Route!.Handler(null!).Body);
        Assert.True(postResult.IsMatch);
        Assert.Equal("created", postResult.Route!.Handler(null!).Body);
        Assert.True(rootResult.IsMatch);
        Assert.Equal("root", rootResult.Route!.Handler(null!).Body);
    }

    [Fact]
    public void Match_SamePathDifferentMethods_EachMatchesOwnMethod()
    {
        var router = new Router();
        router.MapGet("/users", _ => new HttpResponse { Body = "get" });
        router.MapPost("/users", _ => new HttpResponse { Body = "post" });
        router.MapPut("/users", _ => new HttpResponse { Body = "put" });
        router.MapDelete("/users", _ => new HttpResponse { Body = "delete" });

        Assert.True(router.Match(CreateRequest(HttpMethod.Get, "/users")).IsMatch);
        Assert.True(router.Match(CreateRequest(HttpMethod.Post, "/users")).IsMatch);
        Assert.True(router.Match(CreateRequest(HttpMethod.Put, "/users")).IsMatch);
        Assert.True(router.Match(CreateRequest(HttpMethod.Delete, "/users")).IsMatch);
        Assert.Equal(
            RouteMatchStatus.MethodMismatch,
            router.Match(CreateRequest(HttpMethod.Patch, "/users")).Status);
    }

    [Fact]
    public void Match_RootPath_Matches()
    {
        var router = new Router();
        router.MapGet("/", _ => new HttpResponse());

        var result = router.Match(CreateRequest(HttpMethod.Get, "/"));

        Assert.True(result.IsMatch);
    }

    [Fact]
    public void Handler_IsInvokedWithRequest()
    {
        HttpRequest? received = null;
        var router = new Router();
        router.MapGet("/hello", request =>
        {
            received = request;
            return new HttpResponse();
        });

        var incoming = CreateRequest(HttpMethod.Get, "/hello");
        var result = router.Match(incoming);

        Assert.True(result.IsMatch);
        var response = result.Route!.Handler(incoming);
        Assert.Same(incoming, received);
        Assert.NotNull(response);
    }

    [Fact]
    public void Handler_ReturnsResponse()
    {
        var router = new Router();
        router.MapGet("/hello", _ => new HttpResponse
        {
            StatusCode = 200,
            Body = "Hello from Antananarivo!"
        });

        var result = router.Match(CreateRequest(HttpMethod.Get, "/hello"));
        var response = result.Route!.Handler(CreateRequest(HttpMethod.Get, "/hello"));

        Assert.True(result.IsMatch);
        Assert.Equal(200, response.StatusCode.Code);
        Assert.Equal("Hello from Antananarivo!", response.Body);
    }
}
