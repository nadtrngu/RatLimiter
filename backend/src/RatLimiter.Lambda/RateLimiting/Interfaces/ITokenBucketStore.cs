using RatLimiter.Lambda.RateLimiting.Models;
using StackExchange.Redis;

namespace RatLimiter.Lambda.RateLimiting.Interfaces;
public interface ITokenBucketStore
{
    Task SaveAsync(string apiKey, TokenBucketState tokenBucketState, TokenBucketConfig? bucketConfig = null);
    Task<TokenBucketState?> GetAsync(string apiKey);
    Task SaveNewKey(string apiKey);
    Task<IEnumerable<string>> GetAllAsync();
    Task<TokenBucketConfig?> GetBucketConfigAsync(string apiKey);
    Task<RedisValue[]> GetTokenBucketConfigAsync(string apiKey);
    Task UpdateBucketLimitAsync(string apiKey, LimitUpdateRequest updateLimitRequest, TokenBucketState existing);
    Task<TokenBucketState?> GetBucketStateAsync(string apiKey);
}
