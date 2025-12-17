using RatLimiter.Lambda.RateLimiting.Interfaces;
using RatLimiter.Lambda.RateLimiting.Models;
using StackExchange.Redis;

namespace RatLimiter.Lambda.RateLimiting.Services;
public class RedisTokenBucketStore : ITokenBucketStore
{

    private readonly ConnectionMultiplexer redis;

    public RedisTokenBucketStore(ConnectionMultiplexer multiplexer)
    {
        redis = multiplexer;
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

    public async Task<TokenBucketConfig?> GetBucketConfigAsync(string apiKey)
    {
        var key = $"rat:config:{apiKey}";

        var db = redis.GetDatabase();
        var allKeys = await db.HashGetAllAsync(key);
        if (allKeys.Length == 0) return null;

        var bucket = allKeys.ToDictionary(e => (string)e.Name, e => e.Value);

        if (!bucket.TryGetValue("Name", out var name) ||
           !bucket.TryGetValue("Status", out var status) ||
           !Enum.TryParse(status, true, out Status statusParsed) ||
           !bucket.TryGetValue("Algorithm", out var algorithm) ||
           !Enum.TryParse(algorithm, true, out Algorithm algoParsed) ||
           !bucket.TryGetValue("RefillRate", out var refillRate) ||
           !bucket.TryGetValue("Capacity", out var capacity) ||
           !bucket.TryGetValue("CreatedAt", out var createdAt) ||
           !bucket.TryGetValue("UpdatedAt", out var updatedAt)) return null;

        bucket.TryGetValue("Description", out var description);

        return new TokenBucketConfig()
        {
            Capacity = (int)capacity,
            Algorithm = algoParsed,
            Description = description,
            Name = name,
            Status = statusParsed,
            CreatedAt = (long)createdAt,
            UpdatedAt = (long)updatedAt,
            RefillRate = (int)refillRate
        };
    }

    public async Task<TokenBucketState?> GetBucketStateAsync(string apiKey)
    {
        var key = $"rat:bucket:{apiKey}";

        var db = redis.GetDatabase();
        var allKeys = await db.HashGetAllAsync(key);
        if (allKeys.Length == 0) return null;

        var bucket = allKeys.ToDictionary(e => (string)e.Name, e => e.Value);

        if (!bucket.TryGetValue("RefillRate", out var refillRate) ||
           !bucket.TryGetValue("Capacity", out var capacity) ||
           !bucket.TryGetValue("LastRefill", out var lastRefill) ||
           !bucket.TryGetValue("NumberOfTokens", out var numberOfTokens)) return null;

        return new TokenBucketState()
        {
            Capacity = (int)capacity,
            LastRefill = (int)lastRefill,
            NumberOfTokens = (int)numberOfTokens,
            RefillRate = (int)refillRate
        };
    }

    public async Task<IEnumerable<string>> GetAllAsync()
    {
        var key = "rat:api-keys";
        var db = redis.GetDatabase();
        var allKeys = await db.SetMembersAsync(key);

        if (allKeys.Length == 0) return Enumerable.Empty<string>();
        return allKeys.ToStringArray();
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

    public async Task<RedisValue[]> GetTokenBucketConfigAsync(string apiKey)
    {
        var configKey = $"rat:config:{apiKey}";
        var db = redis.GetDatabase();

        RedisValue[] fields = [new RedisValue("Name"), new RedisValue("Status"), new RedisValue("Algorithm"), new RedisValue("Capacity"), new RedisValue("RefillRate"), new RedisValue("CreatedAt"), new RedisValue("Description")];
        return await db.HashGetAsync(configKey, fields);
    }

    public async Task UpdateBucketLimitAsync(string apiKey, LimitUpdateRequest updateLimitRequest, TokenBucketState existing)
    {
        var bucketKey = $"rat:bucket:{apiKey}";
        var configKey = $"rat:config:{apiKey}";

        var db = redis.GetDatabase();

        var numberOfTokens = Math.Min(updateLimitRequest.Capacity, existing.NumberOfTokens);

        await db.HashSetAsync(bucketKey, [new("Capacity", updateLimitRequest.Capacity), new("RefillRate", updateLimitRequest.RefillRate), new("NumberOfTokens", numberOfTokens)]);
        await db.HashSetAsync(configKey, [new("Capacity", updateLimitRequest.Capacity), new("RefillRate", updateLimitRequest.RefillRate),
                                          new("Algorithm", updateLimitRequest.Algorithm.ToString()), new("UpdatedAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds())]);
    }
}
