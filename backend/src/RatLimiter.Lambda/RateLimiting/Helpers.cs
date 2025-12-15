using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;

namespace RatLimiter.Lambda.RateLimiting;
public static class Helpers
{
    public static APIGatewayProxyResponse GetResponseObj(int status, object bodyObj)
    {
        return new APIGatewayProxyResponse
        {
            StatusCode = status,
            Body = JsonSerializer.Serialize(bodyObj),
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            }
        };
    }
}
