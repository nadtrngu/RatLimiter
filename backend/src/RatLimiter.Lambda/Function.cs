using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using RatLimiter.Lambda.RateLimiting.Models;
using RatLimiter.Lambda.RateLimiting.Services;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace RatLimiter.Lambda;

public class CheckRequest
{
    public string ApiKey { get; set; }
    public int Cost { get; set; } = 1;
}

public class NewApiKeyRequest
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public Status Status { get; set; } = Status.ACTIVE;
    public Algorithm Algorithm { get; set; } = Algorithm.TokenBucket;
    public int RefillRate { get; set; } = 5;
    public int Capacity { get; set; } = 100;
}

public class Function
{
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            var rateLimiter = Bootstrap.RateLimiter;
            var keyService = Bootstrap.ApiKeyService;

            switch (request.Path)
            {
                case "/v1/api-keys":
                    if (request.HttpMethod == "POST")
                    {
                        if (request?.Headers == null || !request.Headers.TryGetValue("X-Admin-Token", out var adminHeader) || adminHeader != Environment.GetEnvironmentVariable("ADMIN_TOKEN"))
                            return new APIGatewayProxyResponse
                            {
                                StatusCode = 401,
                                Body = "{\"message\":\"Unauthorized - Missing Admin Token.\"}",
                                Headers = new Dictionary<string, string>
                                {
                                    { "Content-Type", "application/json" }
                                }
                            };

                        var newApiKeyRequest = JsonSerializer.Deserialize<NewApiKeyRequest>(request.Body);

                        if (newApiKeyRequest is null || string.IsNullOrEmpty(newApiKeyRequest.Name))
                        {
                            return new APIGatewayProxyResponse
                            {
                                StatusCode = 400,
                                Body = "{\"message\":\"Bad request - Invalid or missing arguments.\"}",
                                Headers = new Dictionary<string, string>
                                {
                                    { "Content-Type", "application/json" }
                                }
                            };
                        }

                        var apiKey = await keyService.CreateAsync(
                            newApiKeyRequest.Name, newApiKeyRequest.Status,
                            newApiKeyRequest.Description, newApiKeyRequest.Capacity,
                            newApiKeyRequest.RefillRate, newApiKeyRequest.Algorithm);

                        return new APIGatewayProxyResponse
                        {
                            StatusCode = 201,
                            Body = JsonSerializer.Serialize(new Dictionary<string, string>() { { "ApiKey", apiKey} }),
                            Headers = new Dictionary<string, string>
                            {
                                { "Content-Type", "application/json" }
                            }
                        };
                    }

                    return new APIGatewayProxyResponse
                    {
                        StatusCode = 405,
                        Body = "{\"message\":\"Method not allowed.\"}",
                        Headers = new Dictionary<string, string>
                        {
                            { "Content-Type", "application/json" }
                        }
                    };
                case "/v1/check":
                    var checkRequest = JsonSerializer.Deserialize<CheckRequest>(request.Body);

                    if (checkRequest is null || string.IsNullOrEmpty(checkRequest.ApiKey) || checkRequest.Cost <= 0)
                    {
                        return new APIGatewayProxyResponse
                        {
                            StatusCode = 400,
                            Body = "{\"message\":\"Bad request - Invalid or missing arguments.\"}",
                            Headers = new Dictionary<string, string>
                            {
                                { "Content-Type", "application/json" }
                            }
                        };
                    }

                    var decision = await rateLimiter.Check(checkRequest.ApiKey, checkRequest.Cost);

                    return new APIGatewayProxyResponse
                    {
                        StatusCode = decision.Allowed ? 200 : 429,
                        Body = JsonSerializer.Serialize(decision),
                        Headers = new Dictionary<string, string>
                        {
                            { "Content-Type", "application/json" }
                        }
                    };

                default:
                    return new APIGatewayProxyResponse
                    {
                        StatusCode = 404,
                        Body = "{\"message\":\"Not Found.\"}",
                        Headers = new Dictionary<string, string>
                        {
                            { "Content-Type", "application/json" }
                        }
                    };
            }
        }
        catch (UnauthorizedAccessException)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 401,
                Body = "{\"message\":\"Unauthorized - Invalid ApiKey.\"}",
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }
        catch (Exception ex)
        {
            context.Logger.LogLine(ex.ToString());

            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = "{\"message\":\"Server Error.\"}",
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }
    }
}