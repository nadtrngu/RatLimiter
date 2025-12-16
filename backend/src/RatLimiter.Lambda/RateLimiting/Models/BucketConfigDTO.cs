namespace RatLimiter.Lambda.RateLimiting.Models;
public class BucketConfigDTO
{
    public string Name { get; set; }
    public string Algorithm { get; set; }
    public int Capacity { get; set; }
    public string Status { get; set; }
    public int RefillRate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

