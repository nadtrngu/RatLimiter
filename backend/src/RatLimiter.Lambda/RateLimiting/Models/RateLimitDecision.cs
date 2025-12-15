namespace RatLimiter.Lambda.RateLimiting.Models;

public class RateLimitDecision
{
    public bool Allowed { get; set; }
    public int RemainingTokens { get; set; }
    public int Limit { get; set; }
    public int ResetInSeconds { get; set; } = 0;
}