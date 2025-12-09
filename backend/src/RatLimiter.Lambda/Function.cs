using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace RatLimiter.Lambda;

public class CheckRequest
{
    public string ApiKey { get; set; }
    public int Cost { get; set; } = 1;
}

public class CheckResponse
{
    public bool Allowed { get; set; }
    public int RemainingTokens { get; set; }
    public int Limit { get; set; }
    public int ResetInSeconds { get; set; }
}

public class Function
{
    public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var checkResponse = new CheckResponse
        {
            Allowed = true,
            RemainingTokens = 99,
            Limit = 100,
            ResetInSeconds = 10
        };

        var json = JsonSerializer.Serialize(checkResponse);

        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = json,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            }
        };
    }
}