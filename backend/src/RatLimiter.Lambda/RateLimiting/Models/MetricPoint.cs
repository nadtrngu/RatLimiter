namespace RatLimiter.Lambda.RateLimiting.Models;
public class MetricPoint
{
    public long Throttled { get; set; }
    public long Allowed { get; set; }
}
