using Antananarivo.Core.Http;
using Antananarivo.Core.Routing;
using HttpMethod = Antananarivo.Core.Http.HttpMethod;

namespace Antananarivo.Tests.Routing;

public class RouteParameterTests
{
    private static HttpRequest CreateRequest(HttpMethod method, string path) =>
        new() { Method = method, Path = path };

    [Fact]
    public void Match_SingleParameter_ExtractsValue()
    {
        var router = new Router();
        router.MapGet("/users/{id}", _ => new HttpResponse());

        var request = CreateRequest(HttpMethod.Get, "/users/42");
        var result = router.Match(request);

        Assert.True(result.IsMatch);
        Assert.Equal("42", request.RouteParameters["id"]);
    }

    [Fact]
    public void Match_ParameterAcceptsNonNumericValue()
    {
        var router = new Router();
        router.MapGet("/users/{id}", _ => new HttpResponse());

        var request = CreateRequest(HttpMethod.Get, "/users/abc");
        var result = router.Match(request);

        Assert.True(result.IsMatch);
        Assert.Equal("abc", request.RouteParameters["id"]);
    }

    [Fact]
    public void Match_MultipleParameters_ExtractsAllValues()
    {
        var router = new Router();
        router.MapGet("/users/{userId}/posts/{postId}", _ => new HttpResponse());

        var request = CreateRequest(HttpMethod.Get, "/users/42/posts/10");
        var result = router.Match(request);

        Assert.True(result.IsMatch);
        Assert.Equal(2, request.RouteParameters.Count);
        Assert.Equal("42", request.RouteParameters["userId"]);
        Assert.Equal("10", request.RouteParameters["postId"]);
    }

    [Fact]
    public void Match_StaticAndParameterSegments_Combined()
    {
        var router = new Router();
        router.MapGet("/users/{userId}/posts/{postId}", _ => new HttpResponse());

        var request = CreateRequest(HttpMethod.Get, "/users/42/posts/10");
        var result = router.Match(request);

        Assert.True(result.IsMatch);
        Assert.Equal("42", request.RouteParameters["userId"]);
        Assert.Equal("10", request.RouteParameters["postId"]);
    }

    [Fact]
    public void Match_ParameterDoesNotConsumeMultipleSegments()
    {
        var router = new Router();
        router.MapGet("/users/{id}", _ => new HttpResponse());

        var result = router.Match(CreateRequest(HttpMethod.Get, "/users/42/profile"));

        Assert.False(result.IsMatch);
        Assert.Equal(RouteMatchStatus.NotFound, result.Status);
    }

    [Fact]
    public void Match_EmptyParameterSegment_DoesNotMatch()
    {
        var router = new Router();
        router.MapGet("/users/{id}", _ => new HttpResponse());

        var result = router.Match(CreateRequest(HttpMethod.Get, "/users/"));

        Assert.False(result.IsMatch);
        Assert.Equal(RouteMatchStatus.NotFound, result.Status);
    }

    [Fact]
    public void Match_ParameterRoute_RequiresExactStaticSegments()
    {
        var router = new Router();
        router.MapGet("/users/{id}", _ => new HttpResponse());

        var result = router.Match(CreateRequest(HttpMethod.Get, "/admin/42"));

        Assert.False(result.IsMatch);
        Assert.Equal(RouteMatchStatus.NotFound, result.Status);
    }

    [Fact]
    public void Match_ExtraSegments_DoNotMatch()
    {
        var router = new Router();
        router.MapGet("/users/{id}", _ => new HttpResponse());

        var result = router.Match(CreateRequest(HttpMethod.Get, "/users/42/extra"));

        Assert.False(result.IsMatch);
    }

    [Fact]
    public void Match_MissingSegments_DoNotMatch()
    {
        var router = new Router();
        router.MapGet("/users/{userId}/posts/{postId}", _ => new HttpResponse());

        var result = router.Match(CreateRequest(HttpMethod.Get, "/users/42"));

        Assert.False(result.IsMatch);
    }

    [Fact]
    public void Match_QueryStringExcludedFromParameter()
    {
        var router = new Router();
        router.MapGet("/users/{id}", _ => new HttpResponse());

        var request = new HttpRequest
        {
            Method = HttpMethod.Get,
            Path = "/users/42",
            QueryString = "?active=true"
        };
        var result = router.Match(request);

        Assert.True(result.IsMatch);
        Assert.Equal("42", request.RouteParameters["id"]);
    }

    [Fact]
    public void Match_MethodMismatch_ParameterRoute()
    {
        var router = new Router();
        router.MapGet("/users/{id}", _ => new HttpResponse());

        var result = router.Match(CreateRequest(HttpMethod.Post, "/users/42"));

        Assert.False(result.IsMatch);
        Assert.Equal(RouteMatchStatus.MethodMismatch, result.Status);
    }

    [Fact]
    public void Match_SeparatelyRegisteredPostRoute_MatchesPost()
    {
        var router = new Router();
        router.MapGet("/users/{id}", _ => new HttpResponse());
        router.MapPost("/users/{id}", _ => new HttpResponse());

        var getRequest = CreateRequest(HttpMethod.Get, "/users/42");
        var postRequest = CreateRequest(HttpMethod.Post, "/users/42");

        Assert.True(router.Match(getRequest).IsMatch);
        Assert.Equal("42", getRequest.RouteParameters["id"]);
        Assert.True(router.Match(postRequest).IsMatch);
        Assert.Equal("42", postRequest.RouteParameters["id"]);
    }

    [Fact]
    public void Match_RootRoute_StillMatches()
    {
        var router = new Router();
        router.MapGet("/", _ => new HttpResponse());
        router.MapGet("/users/{id}", _ => new HttpResponse());

        var result = router.Match(CreateRequest(HttpMethod.Get, "/"));

        Assert.True(result.IsMatch);
    }

    [Fact]
    public void Match_ExistingExactRoute_StillMatches()
    {
        var router = new Router();
        router.MapGet("/users", _ => new HttpResponse());
        router.MapGet("/users/{id}", _ => new HttpResponse());

        var result = router.Match(CreateRequest(HttpMethod.Get, "/users"));

        Assert.True(result.IsMatch);
    }

    [Fact]
    public void Match_StaticRoutePreferredOverParameterRoute()
    {
        var router = new Router();
        router.MapGet("/users/{id}", _ => new HttpResponse { Body = "parameter" });
        router.MapGet("/users/list", _ => new HttpResponse { Body = "static" });

        var request = CreateRequest(HttpMethod.Get, "/users/list");
        var result = router.Match(request);

        Assert.True(result.IsMatch);
        Assert.Equal("static", result.Route!.Handler(request).Body);
        Assert.Empty(request.RouteParameters);
    }

    [Fact]
    public void Match_StaticRouteRegisteredFirst_ParameterRouteStillWorks()
    {
        var router = new Router();
        router.MapGet("/users/list", _ => new HttpResponse { Body = "static" });
        router.MapGet("/users/{id}", _ => new HttpResponse { Body = "parameter" });

        var listRequest = CreateRequest(HttpMethod.Get, "/users/list");
        var idRequest = CreateRequest(HttpMethod.Get, "/users/42");

        var listResult = router.Match(listRequest);
        var idResult = router.Match(idRequest);

        Assert.True(listResult.IsMatch);
        Assert.Equal("static", listResult.Route!.Handler(listRequest).Body);
        Assert.True(idResult.IsMatch);
        Assert.Equal("42", idRequest.RouteParameters["id"]);
    }

    [Fact]
    public void Handler_ReceivesRouteParameters()
    {
        string? capturedId = null;
        var router = new Router();
        router.MapGet("/users/{id}", request =>
        {
            capturedId = request.RouteParameters["id"];
            return new HttpResponse();
        });

        var request = CreateRequest(HttpMethod.Get, "/users/42");
        var result = router.Match(request);
        result.Route!.Handler(request);

        Assert.True(result.IsMatch);
        Assert.Equal("42", capturedId);
    }

    [Fact]
    public void Match_UnmatchedRoute_LeavesParametersEmpty()
    {
        var router = new Router();
        router.MapGet("/users/{id}", _ => new HttpResponse());

        var request = CreateRequest(HttpMethod.Get, "/users/42/extra");
        router.Match(request);

        Assert.Empty(request.RouteParameters);
    }

    [Fact]
    public void Map_ConflictingParameterNames_Throws()
    {
        var router = new Router();
        router.MapGet("/users/{id}", _ => new HttpResponse());

        Assert.Throws<ArgumentException>(() =>
            router.MapGet("/users/{userId}", _ => new HttpResponse()));
    }

    [Fact]
    public void Map_ConflictingParameterNames_DifferentMethod_IsAllowed()
    {
        var router = new Router();
        router.MapGet("/users/{id}", _ => new HttpResponse());

        router.MapPost("/users/{userId}", _ => new HttpResponse());

        Assert.Equal(2, router.Routes.Count);
    }

    [Fact]
    public void Map_DuplicateParameterizedRoute_Throws()
    {
        var router = new Router();
        router.MapGet("/users/{id}", _ => new HttpResponse());

        Assert.Throws<ArgumentException>(() =>
            router.MapGet("/users/{id}", _ => new HttpResponse()));
    }

    [Fact]
    public void Map_StaticAndParameterRoutes_Coexist()
    {
        var router = new Router();

        router.MapGet("/users/list", _ => new HttpResponse());
        router.MapGet("/users/{id}", _ => new HttpResponse());

        Assert.Equal(2, router.Routes.Count);
    }

    [Fact]
    public void Map_EmptyParameterName_Throws()
    {
        var router = new Router();

        Assert.Throws<ArgumentException>(() =>
            router.MapGet("/users/{}", _ => new HttpResponse()));
    }

    [Fact]
    public void Map_NestedBraces_Throw()
    {
        var router = new Router();

        Assert.Throws<ArgumentException>(() =>
            router.MapGet("/users/{{id}}", _ => new HttpResponse()));
    }

    [Fact]
    public void Map_MismatchedBraces_Throw()
    {
        var router = new Router();

        Assert.Throws<ArgumentException>(() =>
            router.MapGet("/users/{id", _ => new HttpResponse()));
    }

    [Fact]
    public void Map_ParameterNotWholeSegment_Throws()
    {
        var router = new Router();

        Assert.Throws<ArgumentException>(() =>
            router.MapGet("/users/user{id}", _ => new HttpResponse()));
    }
}
