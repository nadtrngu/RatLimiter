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
        var allKeys = await _keyService.GetAllKeysAsync();

        if (allKeys == null) return Helpers.GetResponseObj(200, new Dictionary<string,  object>());

        var keyList = allKeys.ToList();
        var tasks = keyList.Select(async k => (Key: k, Values: await _keyService.GetTokenBucketConfigAsync(k)));
        var pairs = await Task.WhenAll(tasks);
        var dict = pairs.ToDictionary(p => p.Key, p => MapTokenBucketConfig(p.Values));

        return Helpers.GetResponseObj(200, dict);
    }

    public async Task<APIGatewayProxyResponse> GetKeyDetailsAsync(APIGatewayProxyRequest request)
    {
        if (request.PathParameters == null || !request.PathParameters.TryGetValue("key", out var apiKey))
           return Helpers.GetResponseObj(400, new Dictionary<string, string>() { { "message", "Bad request - Invalid or missing arguments." } });

        var bucketInfo = await _keyService.GetTokenBucketConfigAsync(apiKey);

        if (bucketInfo == null) return Helpers.GetResponseObj(404, new Dictionary<string, string>() { { "message", "Api key not found." } });

        var mappedData = MapTokenBucketConfig(bucketInfo);

        if (mappedData == null) return Helpers.GetResponseObj(400, new Dictionary<string, string>() { { "message", "Bad request - Invalid or missing arguments." } });

        return Helpers.GetResponseObj(200, mappedData);
    }

    private static BucketConfigDTO? MapTokenBucketConfig(RedisValue[] values)
    {
        if (values == null || values.Length < 6) return null;

        var name       = values[0];
        var status     = values[1];
        var algorithm  = values[2];
        var capacity   = values[3];
        var refillRate = values[4];
        var createdAt  = values[5];
        var description = values.Length > 6 ? values[6] : RedisValue.Null;

        if (name.IsNullOrEmpty ||
            status.IsNullOrEmpty ||
            !Enum.TryParse(status, true, out Status statusEnum) ||
            algorithm.IsNullOrEmpty ||
            !Enum.TryParse(algorithm, true, out Algorithm algorithmEnum) ||
            capacity.IsNullOrEmpty ||
            refillRate.IsNullOrEmpty ||
            createdAt.IsNullOrEmpty) return null;

        return new BucketConfigDTO()
        {
            Name = (string)name!,
            Algorithm = Enum.GetName(algorithmEnum) ?? algorithmEnum.ToString(),
            Capacity = (int)capacity,
            CreatedAt = DateTimeOffset.FromUnixTimeSeconds((long)createdAt),
            RefillRate = (int)refillRate,
            Status = Enum.GetName(statusEnum) ?? statusEnum.ToString(),
            Description = description.IsNull ? string.Empty : (string)description!
        };
    }
}

