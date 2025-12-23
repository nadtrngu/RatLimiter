using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;

namespace RatLimiter.Lambda.RateLimiting;
public static class Helpers
{
    private static readonly IDictionary<string, string> DefaultHeaders =
        new Dictionary<string, string>
        {
            { "Content-Type", "application/json" },
            { "Access-Control-Allow-Origin", "http://localhost:5174" }, // or "*" for now
            { "Access-Control-Allow-Headers", "Content-Type,X-Admin-Token" },
            { "Access-Control-Allow-Methods", "GET,POST,PUT,OPTIONS" }
        };

    public static APIGatewayProxyResponse GetCorsPreflight()
    {
        return new APIGatewayProxyResponse
        {
            StatusCode = 204,
            Body = "",
            Headers = new Dictionary<string, string>(DefaultHeaders)
        };
    }
    
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
