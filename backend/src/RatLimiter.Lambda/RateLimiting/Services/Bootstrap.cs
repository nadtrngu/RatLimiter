using RatLimiter.Lambda.RateLimiting.Interfaces;
using RatLimiter.Lambda.RateLimiting.Services;
using StackExchange.Redis;

public static class Bootstrap
{
    private static readonly Lazy<ConnectionMultiplexer> _multiplexer = new(() =>
    {
        var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
        if (string.IsNullOrEmpty(redisConnectionString))
            throw new ArgumentNullException(nameof(redisConnectionString));

        return ConnectionMultiplexer.Connect(redisConnectionString);
    });

    public static ConnectionMultiplexer ConnectionMultiplexer => _multiplexer.Value;

    private static readonly Lazy<RedisTokenBucketStore> _store =
        new(() => new RedisTokenBucketStore(ConnectionMultiplexer));

    private static readonly Lazy<IRateLimiter> _rateLimiter =
        new(() => new TokenBucketRateLimiter(_store.Value));

    private static readonly Lazy<IApiKeyService> _apiKeyService =
        new(() => new ApiKeyService(_store.Value));

    public static IRateLimiter RateLimiter => _rateLimiter.Value;
    public static IApiKeyService ApiKeyService => _apiKeyService.Value;
}
