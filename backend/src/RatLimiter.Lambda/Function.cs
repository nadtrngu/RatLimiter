using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using RatLimiter.Lambda.RateLimiting;
using RatLimiter.Lambda.RateLimiting.Interfaces;
using RatLimiter.Lambda.RateLimiting.Services;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace RatLimiter.Lambda;

public class Function
{
    private readonly IFunctionService _functionService;

    public Function()
    {
        _functionService = new FunctionService(Bootstrap.RateLimiter, Bootstrap.ApiKeyService);
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            var checkHeader = RequireAdminOrNull(request);
            if (checkHeader != null)
                return checkHeader;

            switch (request.Path)
            {
                case "/v1/api-keys":

                    if (request.HttpMethod == "POST")
                    {
                        return await _functionService.CreateNewApiKeyAsync(request);
                    }

                    return Helpers.GetResponseObj(405, new Dictionary<string, string>() { { "message", "Method not allowed." } });

                case "/v1/check":
                    return await _functionService.CheckAsync(request);

                default:
                    return Helpers.GetResponseObj(404, new Dictionary<string, string>() { { "message", "Not Found." } });
            }
        }
        catch (UnauthorizedAccessException)
        {
            return Helpers.GetResponseObj(401, new Dictionary<string, string>() { { "message", "Unauthorized - Invalid ApiKey." } });
        }
        catch (Exception ex)
        {
            context.Logger.LogLine(ex.ToString());

            return Helpers.GetResponseObj(500, new Dictionary<string, string>() { { "message", "Server Error." } });
        }
    }

    private static APIGatewayProxyResponse? RequireAdminOrNull(APIGatewayProxyRequest request)
    {
        if (request.Path == "/v1/check") return null;

        if (string.IsNullOrEmpty(request.Body))
            return Helpers.GetResponseObj(400, new Dictionary<string, string>() { { "message", "Bad request - Invalid or missing arguments." } });

        if (request?.Headers == null || !request.Headers.TryGetValue("X-Admin-Token", out var adminHeader) || adminHeader != Environment.GetEnvironmentVariable("ADMIN_TOKEN"))
            return Helpers.GetResponseObj(401, new Dictionary<string, string>() { { "message", "Unauthorized - Missing Admin Token." } });

        return null;
    }
}