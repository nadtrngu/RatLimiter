using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using RatLimiter.Lambda.RateLimiting.Interfaces;
using RatLimiter.Lambda.RateLimiting.Models;

namespace RatLimiter.Lambda.RateLimiting.Services;
public class FunctionService : IFunctionService
{
    private readonly IRateLimiter _rateLimiter;
    private readonly IApiKeyService _keyService;

    public FunctionService(IRateLimiter rateLimiter, IApiKeyService apiKeyService)
    {
        _keyService = apiKeyService;
        _rateLimiter = rateLimiter;
    }

    public async Task<APIGatewayProxyResponse> CheckAsync(APIGatewayProxyRequest request)
    {
        var checkRequest = JsonSerializer.Deserialize<CheckRequest>(request.Body);

        if (checkRequest is null || string.IsNullOrEmpty(checkRequest.ApiKey) || checkRequest.Cost <= 0)
        {
            return Helpers.GetResponseObj(400, new Dictionary<string, string>() { { "message", "Bad request - Invalid or missing arguments." } });
        }

        var decision = await _rateLimiter.Check(checkRequest.ApiKey, checkRequest.Cost);

        return Helpers.GetResponseObj(decision.Allowed ? 200 : 429, decision);
    }

    public async Task<APIGatewayProxyResponse> CreateNewApiKeyAsync(APIGatewayProxyRequest request)
    {
        var newApiKeyRequest = JsonSerializer.Deserialize<NewApiKeyRequest>(request.Body);

        if (newApiKeyRequest is null || string.IsNullOrEmpty(newApiKeyRequest.Name))
        {
            return Helpers.GetResponseObj(400, new Dictionary<string, string>() { { "message", "Bad request - Invalid or missing arguments." } });
        }

        var apiKey = await _keyService.CreateAsync(
            newApiKeyRequest.Name, newApiKeyRequest.Status,
            newApiKeyRequest.Description, newApiKeyRequest.Capacity,
            newApiKeyRequest.RefillRate, newApiKeyRequest.Algorithm);

        return Helpers.GetResponseObj(201, new Dictionary<string, string>() { { "ApiKey", apiKey } });
    }
}

