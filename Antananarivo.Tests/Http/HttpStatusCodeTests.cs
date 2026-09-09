using Antananarivo.Core.Http;

using HttpMethod = Antananarivo.Core.Http.HttpMethod;

namespace Antananarivo.Tests.Http;

public class HttpStatusCodeTests
{
    [Fact]
    public void StatusCode_200_OK_HasCorrectCodeAndReason()
    {
        var code200 = (HttpStatusCode)200;
        Assert.Equal(200, code200.Code);
        Assert.Equal("OK", code200.ReasonPhrase);
    }

    [Fact]
    public void StatusCode_201_Created_HasCorrectCodeAndReason()
    {
        var code201 = (HttpStatusCode)201;
        Assert.Equal(201, code201.Code);
        Assert.Equal("Created", code201.ReasonPhrase);
    }

    [Fact]
    public void StatusCode_202_Accepted_HasCorrectCodeAndReason()
    {
        var code202 = (HttpStatusCode)202;
        Assert.Equal(202, code202.Code);
        Assert.Equal("Accepted", code202.ReasonPhrase);
    }

    [Fact]
    public void StatusCode_204_NoContent_HasCorrectCodeAndReason()
    {
        var code204 = (HttpStatusCode)204;
        Assert.Equal(204, code204.Code);
        Assert.Equal("No Content", code204.ReasonPhrase);
    }

    [Fact]
    public void StatusCode_301_MovedPermanently_HasCorrectCodeAndReason()
    {
        var code301 = (HttpStatusCode)301;
        Assert.Equal(301, code301.Code);
        Assert.Equal("Moved Permanently", code301.ReasonPhrase);
    }

    [Fact]
    public void StatusCode_302_Found_HasCorrectCodeAndReason()
    {
        var code302 = (HttpStatusCode)302;
        Assert.Equal(302, code302.Code);
        Assert.Equal("Found", code302.ReasonPhrase);
    }

    [Fact]
    public void StatusCode_304_NotModified_HasCorrectCodeAndReason()
    {
        var code304 = (HttpStatusCode)304;
        Assert.Equal(304, code304.Code);
        Assert.Equal("Not Modified", code304.ReasonPhrase);
    }

    [Fact]
    public void StatusCode_400_BadRequest_HasCorrectCodeAndReason()
    {
        var code400 = (HttpStatusCode)400;
        Assert.Equal(400, code400.Code);
        Assert.Equal("Bad Request", code400.ReasonPhrase);
    }

    [Fact]
    public void StatusCode_401_Unauthorized_HasCorrectCodeAndReason()
    {
        var code401 = (HttpStatusCode)401;
        Assert.Equal(401, code401.Code);
        Assert.Equal("Unauthorized", code401.ReasonPhrase);
    }

    [Fact]
    public void StatusCode_403_Forbidden_HasCorrectCodeAndReason()
    {
        var code403 = (HttpStatusCode)403;
        Assert.Equal(403, code403.Code);
        Assert.Equal("Forbidden", code403.ReasonPhrase);
    }

    [Fact]
    public void StatusCode_404_NotFound_HasCorrectCodeAndReason()
    {
        var code404 = (HttpStatusCode)404;
        Assert.Equal(404, code404.Code);
        Assert.Equal("Not Found", code404.ReasonPhrase);
    }

    [Fact]
    public void StatusCode_405_MethodNotAllowed_HasCorrectCodeAndReason()
    {
        var code405 = (HttpStatusCode)405;
        Assert.Equal(405, code405.Code);
        Assert.Equal("Method Not Allowed", code405.ReasonPhrase);
    }

    [Fact]
    public void StatusCode_408_RequestTimeout_HasCorrectCodeAndReason()
    {
        var code408 = (HttpStatusCode)408;
        Assert.Equal(408, code408.Code);
        Assert.Equal("Request Timeout", code408.ReasonPhrase);
    }

    [Fact]
    public void StatusCode_409_Conflict_HasCorrectCodeAndReason()
    {
        var code409 = (HttpStatusCode)409;
        Assert.Equal(409, code409.Code);
        Assert.Equal("Conflict", code409.ReasonPhrase);
    }

    [Fact]
    public void StatusCode_413_ContentTooLarge_HasCorrectCodeAndReason()
    {
        var code413 = (HttpStatusCode)413;
        Assert.Equal(413, code413.Code);
        Assert.Equal("Content Too Large", code413.ReasonPhrase);
    }

    [Fact]
    public void StatusCode_415_UnsupportedMediaType_HasCorrectCodeAndReason()
    {
        var code415 = (HttpStatusCode)415;
        Assert.Equal(415, code415.Code);
        Assert.Equal("Unsupported Media Type", code415.ReasonPhrase);
    }

    [Fact]
    public void StatusCode_422_UnprocessableContent_HasCorrectCodeAndReason()
    {
        var code422 = (HttpStatusCode)422;
        Assert.Equal(422, code422.Code);
        Assert.Equal("Unprocessable Content", code422.ReasonPhrase);
    }

    [Fact]
    public void StatusCode_429_TooManyRequests_HasCorrectCodeAndReason()
    {
        var code429 = (HttpStatusCode)429;
        Assert.Equal(429, code429.Code);
        Assert.Equal("Too Many Requests", code429.ReasonPhrase);
    }

    [Fact]
    public void StatusCode_500_InternalServerError_HasCorrectCodeAndReason()
    {
        var code500 = (HttpStatusCode)500;
        Assert.Equal(500, code500.Code);
        Assert.Equal("Internal Server Error", code500.ReasonPhrase);
    }

    [Fact]
    public void StatusCode_501_NotImplemented_HasCorrectCodeAndReason()
    {
        var code501 = (HttpStatusCode)501;
        Assert.Equal(501, code501.Code);
        Assert.Equal("Not Implemented", code501.ReasonPhrase);
    }

    [Fact]
    public void StatusCode_502_BadGateway_HasCorrectCodeAndReason()
    {
        var code502 = (HttpStatusCode)502;
        Assert.Equal(502, code502.Code);
        Assert.Equal("Bad Gateway", code502.ReasonPhrase);
    }

    [Fact]
    public void StatusCode_503_ServiceUnavailable_HasCorrectCodeAndReason()
    {
        var code503 = (HttpStatusCode)503;
        Assert.Equal(503, code503.Code);
        Assert.Equal("Service Unavailable", code503.ReasonPhrase);
    }

    [Fact]
    public void StatusCode_504_GatewayTimeout_HasCorrectCodeAndReason()
    {
        var code504 = (HttpStatusCode)504;
        Assert.Equal(504, code504.Code);
        Assert.Equal("Gateway Timeout", code504.ReasonPhrase);
    }

    [Fact]
    public void StatusCode_100_Continue_HasCorrectCodeAndReason()
    {
        var code100 = (HttpStatusCode)100;
        Assert.Equal(100, code100.Code);
        Assert.Equal("Continue", code100.ReasonPhrase);
    }

    [Fact]
    public void StatusCode_ImplicitOperator_IntToHttpStatusCode()
    {
        var code = (HttpStatusCode)200;
        Assert.Equal(200, code.Code);
        Assert.Equal("OK", code.ReasonPhrase);
    }

    [Fact]
    public void StatusCode_ImplicitOperator_HttpStatusCodeToInt()
    {
        var code = (HttpStatusCode)200;
        Assert.True(code == 200);
    }

    [Fact]
    public void StatusCode_EqualityOperator()
    {
        var code1 = (HttpStatusCode)200;
        var code2 = (HttpStatusCode)200;
        var code3 = (HttpStatusCode)404;

        Assert.True(code1 == code2);
        Assert.False(code1 == code3);
        Assert.True(code1 != code3);
    }

    [Fact]
    public void StatusCode_UnsupportedStatusCode_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => (HttpStatusCode)599);
    }

    [Fact]
    public void HttpResponse_UsesHttpStatusCode()
    {
        var response = new HttpResponse
        {
            StatusCode = (HttpStatusCode)200,
            Body = "Hello"
        };

        Assert.Equal(200, response.StatusCode.Code);
        Assert.Equal("OK", response.StatusCode.ReasonPhrase);
    }

    [Fact]
    public void HttpStatusCode_GetReturnsGetMethodTest()
    {
        // This test verifies the HttpStatusCode type works alongside HttpMethod
        var code200 = (HttpStatusCode)200;
        var methodGet = HttpMethod.Get;

        Assert.Equal("GET", methodGet.ToString());
        Assert.Equal(200, code200.Code);
    }
}