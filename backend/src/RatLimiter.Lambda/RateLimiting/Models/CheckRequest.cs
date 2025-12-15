namespace RatLimiter.Lambda.RateLimiting.Models;
public class CheckRequest
{
    public string ApiKey { get; set; }
    public int Cost { get; set; } = 1;
}
