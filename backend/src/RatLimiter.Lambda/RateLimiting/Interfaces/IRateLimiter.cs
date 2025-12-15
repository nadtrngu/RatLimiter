using RatLimiter.Lambda.RateLimiting.Models;

namespace RatLimiter.Lambda.RateLimiting.Interfaces;

public interface IRateLimiter
{
    Task<RateLimitDecision> Check(string apiKey, int cost = 1);
}