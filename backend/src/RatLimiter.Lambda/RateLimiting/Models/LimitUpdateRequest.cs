namespace RatLimiter.Lambda.RateLimiting.Models;

public class LimitUpdateRequest
{
    public Algorithm Algorithm { get; set; }
    public int RefillRate { get; set; }
    public int Capacity { get; set; }
}
