using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;
using System.Threading.Tasks;
using RatLimiter.Lambda.RateLimiting.Models;

namespace RatLimiter.Lambda.Tests;

public class FunctionTest
{
    [Fact]
    public async Task TestRatLimiterFunctionHandler_HappyPath()
    {
        var request = new APIGatewayProxyRequest();
        var context = new TestLambdaContext();

        var body = new CheckRequest()
        {
            ApiKey = "RAT-ABC-123",
            Cost = 10
        };

        request.Body = JsonSerializer.Serialize(body);

        var decision = new RateLimitDecision() { Limit = 100, Allowed = true, RemainingTokens = 100 };

        var expectedResponse = new APIGatewayProxyResponse
        {
            Body = JsonSerializer.Serialize(decision),
            StatusCode = 200,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };

        var function = new Function();
        var response = await function.FunctionHandler(request, context);

        Console.WriteLine("Lambda Response: \n" + response.Body);
        Console.WriteLine("Expected Response: \n" + expectedResponse.Body);

        Assert.Equal(expectedResponse.Body, response.Body);
        Assert.Equal(expectedResponse.Headers, response.Headers);
        Assert.Equal(expectedResponse.StatusCode, response.StatusCode);
    }
}