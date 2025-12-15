namespace RatLimiter.Lambda.RateLimiting.Models;
public class TokenBucketState
{
    public int Capacity { get; set; }
    public int RefillRate { get; set; }
    public long LastRefill { get; set; }
    public int NumberOfTokens { get; set; }
}