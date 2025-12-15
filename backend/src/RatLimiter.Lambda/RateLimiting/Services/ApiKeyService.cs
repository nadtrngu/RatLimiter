using System.Security.Cryptography;
using RatLimiter.Lambda.RateLimiting.Interfaces;
using RatLimiter.Lambda.RateLimiting.Models;

namespace RatLimiter.Lambda.RateLimiting.Services;
public class ApiKeyService : IApiKeyService
{

    private readonly ITokenBucketStore _store;

    public ApiKeyService(ITokenBucketStore store)
    {
        _store = store;
    }
    public async Task<string> CreateAsync(string name, Status status, string? description = null, int capacity = 100, int refillRate = 5, Algorithm algorithm = Algorithm.TokenBucket)
    {
        var apiKey = GetRandomString();
        var bucketConfig = new TokenBucketConfig()
        {
            Name = name,
            Description = description,
            Algorithm = algorithm,
            Capacity = capacity,
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            RefillRate = refillRate,
            Status = status
        };

        var bucketState = new TokenBucketState()
        {
            Capacity = capacity,
            RefillRate = refillRate,
            LastRefill = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            NumberOfTokens = capacity
        };

        await _store.SaveAsync(apiKey, bucketState, bucketConfig);
        await _store.SaveNewKey(apiKey);
        return apiKey;
    }

    private static string GetRandomString()
    {
        string allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        int stringLength = 20;
        return RandomNumberGenerator.GetString(allowedChars, stringLength);
    }
}