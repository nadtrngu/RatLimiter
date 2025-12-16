using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using RatLimiter.Lambda.RateLimiting.Interfaces;
using RatLimiter.Lambda.RateLimiting.Models;
using StackExchange.Redis;

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

    public async Task<APIGatewayProxyResponse> GetAllKeysAsync(APIGatewayProxyRequest request)
    {
        Dictionary<string, TokenBucketConfig> KeysMetadata = new();
        var allKeys = await _keyService.GetAllKeysAsync();

        if (allKeys == null) return Helpers.GetResponseObj(200, new Dictionary<string,  object>());

        var keyList = allKeys.ToList();
        var tasks = keyList.Select(async k => (Key: k, Values: await _keyService.GetTokenBucketConfigAsync(k)));
        var pairs = await Task.WhenAll(tasks);
        var dict = pairs.ToDictionary(p => p.Key, p => MapTokenBucketConfig(p.Values));

        return Helpers.GetResponseObj(200, dict);
    }

    private static BucketConfigDTO? MapTokenBucketConfig(RedisValue[] redisValue)
    {
        if (redisValue.Length < 6) return null;
        var name = redisValue[0];
        var status = redisValue[1];
        var algorithm = redisValue[2];
        var capacity = redisValue[3];
        var refillRate = redisValue[4];
        var createdAt = redisValue[5];

        if (string.IsNullOrEmpty(name) ||
            string.IsNullOrEmpty(status) ||
            !Enum.TryParse(status, true, out Status statusEnum) ||
            string.IsNullOrEmpty(algorithm) ||
            !Enum.TryParse(algorithm, true, out Algorithm algorithmEnum) ||
            string.IsNullOrEmpty(capacity) ||
            string.IsNullOrEmpty(refillRate) ||
            string.IsNullOrEmpty(createdAt)) return null;

        return new BucketConfigDTO()
        {
            Name = name,
            Algorithm = Enum.GetName(algorithmEnum),
            Capacity = (int)capacity,
            CreatedAt = DateTimeOffset.FromUnixTimeSeconds((long)createdAt),
            RefillRate = (int)refillRate,
            Status = Enum.GetName(statusEnum)
        };
    }
}

