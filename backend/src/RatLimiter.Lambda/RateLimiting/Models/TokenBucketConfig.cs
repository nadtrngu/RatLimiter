namespace RatLimiter.Lambda.RateLimiting.Models;

public class TokenBucketConfig
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public Status Status { get; set; }
    public Algorithm Algorithm { get; set; }
    public int RefillRate { get; set; }
    public int Capacity { get; set; }
    public long CreatedAt { get; set; }
    public long UpdatedAt { get;set; }
}
