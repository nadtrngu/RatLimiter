using RatLimiter.Lambda.RateLimiting.Interfaces;
using RatLimiter.Lambda.RateLimiting.Models;
using StackExchange.Redis;

namespace RatLimiter.Lambda.RateLimiting.Services;
public class RedisTokenBucketStore : ITokenBucketStore
{

    private readonly ConnectionMultiplexer redis;

    public RedisTokenBucketStore()
    {
        string redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
        if (string.IsNullOrEmpty(redisConnectionString))
            throw new ArgumentNullException(nameof(redisConnectionString));

        redis = ConnectionMultiplexer.Connect(redisConnectionString);
    }

    public async Task<TokenBucketState?> GetAsync(string apiKey)
    {
        var key = $"rat:bucket:{apiKey}";

        var db = redis.GetDatabase();
        var allKeys = await db.HashGetAllAsync(key);
        if (allKeys.Length == 0) return null;

        var bucket = allKeys.ToDictionary(e => (string)e.Name, e => e.Value);

        if (!bucket.TryGetValue("Capacity", out var capacity) ||
           !bucket.TryGetValue("LastRefill", out var lastRefill) ||
           !bucket.TryGetValue("NumberOfTokens", out var numberOfTokens) ||
           !bucket.TryGetValue("RefillRate", out var refillRate)) return null;

        return new TokenBucketState()
        {
            Capacity = (int)capacity,
            LastRefill = (long)lastRefill,
            NumberOfTokens = (int)numberOfTokens,
            RefillRate = (int)refillRate
        };
    }

    public async Task SaveAsync(string apiKey, TokenBucketState tokenBucketState, TokenBucketConfig? bucketConfig = null)
    {
        var bucketKey = $"rat:bucket:{apiKey}";
        var configKey = $"rat:config:{apiKey}";

        var db = redis.GetDatabase();

        await db.HashSetAsync(bucketKey, [new("Capacity", tokenBucketState.Capacity), new("RefillRate", tokenBucketState.RefillRate), new("LastRefill", tokenBucketState.LastRefill), new("NumberOfTokens", tokenBucketState.NumberOfTokens)]);
        
        if (bucketConfig != null)
        {
            await db.HashSetAsync(configKey, [new("Name", bucketConfig.Name),new("Description", bucketConfig.Description), new("Status", bucketConfig.Status.ToString()),
                                            new("Algorithm", bucketConfig.Algorithm.ToString()), new("RefillRate", tokenBucketState.RefillRate),
                                            new("Capacity", tokenBucketState.Capacity), new("CreatedAt", bucketConfig.CreatedAt), new("UpdatedAt", bucketConfig.UpdatedAt)]);
        }
    }

    public async Task SaveNewKey(string apiKey)
    {
        var apiKeysStorage = "rat:api-keys";
        var db = redis.GetDatabase();
        await db.SetAddAsync(apiKeysStorage, apiKey);
    }
}
