using RatLimiter.Lambda.RateLimiting.Models;
using StackExchange.Redis;

namespace RatLimiter.Lambda.RateLimiting.Interfaces;
public interface IApiKeyService
{
    Task<string> CreateAsync(string name, Status status, string? description = null, int capacity = 100, int refillRate = 5, Algorithm algorithm = Algorithm.TokenBucket);
    Task<IEnumerable<string>> GetAllKeysAsync();
    Task<RedisValue[]> GetTokenBucketConfigAsync(string apiKey);
    Task UpdateKeyLimitAsync(string apiKey, LimitUpdateRequest updateLimitRequest);
}