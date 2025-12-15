using RatLimiter.Lambda.RateLimiting.Interfaces;

namespace RatLimiter.Lambda.RateLimiting.Services;
public static class Bootstrap
{
    private static readonly RedisTokenBucketStore store = new();

    private static readonly Lazy<IRateLimiter> _rateLimiter = new(() =>
    {
        return new TokenBucketRateLimiter(store);
    });

    private static readonly Lazy<IApiKeyService> _apiKeyService = new(() =>
    {
        return new ApiKeyService(store);
    });
    public static IRateLimiter RateLimiter => _rateLimiter.Value;
    public static IApiKeyService ApiKeyService => _apiKeyService.Value;
}

