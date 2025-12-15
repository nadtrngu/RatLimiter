namespace RatLimiter.Lambda.RateLimiting.Models;
public class NewApiKeyRequest
{
    public string Name { get; set; }
    public string? Description { get; set; } = string.Empty;
    public Status Status { get; set; } = Status.ACTIVE;
    public Algorithm Algorithm { get; set; } = Algorithm.TokenBucket;
    public int RefillRate { get; set; } = 5;
    public int Capacity { get; set; } = 100;
}
