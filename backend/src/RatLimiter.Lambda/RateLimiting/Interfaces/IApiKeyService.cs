using RatLimiter.Lambda.RateLimiting.Models;

namespace RatLimiter.Lambda.RateLimiting.Interfaces;
public interface IApiKeyService
{
    Task<string> CreateAsync(string name, Status status, string? description = null, int capacity = 100, int refillRate = 5, Algorithm algorithm = Algorithm.TokenBucket);
}