using Amazon.Lambda.APIGatewayEvents;

namespace RatLimiter.Lambda.RateLimiting.Interfaces;
public interface IFunctionService
{
    Task<APIGatewayProxyResponse> CheckAsync(APIGatewayProxyRequest request);
    Task<APIGatewayProxyResponse> CreateNewApiKeyAsync(APIGatewayProxyRequest request);
    Task<APIGatewayProxyResponse> GetAllKeysAsync(APIGatewayProxyRequest request);
}

