using RatLimiter.Lambda.RateLimiting.Models;

namespace RatLimiter.Lambda.RateLimiting.Interfaces;
public interface ITokenBucketStore
{
    Task SaveAsync(string apiKey, TokenBucketState tokenBucketState, TokenBucketConfig? bucketConfig = null);
    Task<TokenBucketState?> GetAsync(string apiKey);
    Task SaveNewKey(string apiKey);
}
