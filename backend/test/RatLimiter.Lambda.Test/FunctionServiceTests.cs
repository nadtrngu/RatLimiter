using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Moq;
using RatLimiter.Lambda.RateLimiting.Interfaces;
using RatLimiter.Lambda.RateLimiting.Models;
using RatLimiter.Lambda.RateLimiting.Services;
using StackExchange.Redis;
using Xunit;

namespace RatLimiter.Lambda.Tests;

public class FunctionServiceTests
{
    readonly string apiKey = "TEST-KEY";
    private readonly Mock<IRateLimiter> limiterMock;
    private readonly Mock<IApiKeyService> apiKeyServiceMock;
    private readonly FunctionService functionService;

    public FunctionServiceTests()
    {
        limiterMock = new Mock<IRateLimiter>();
        apiKeyServiceMock = new Mock<IApiKeyService>();

        functionService = new(limiterMock.Object, apiKeyServiceMock.Object);
    }

    [Fact]
    public async Task CheckAsync_Returns200_WhenAllowed()
    {
        // Arrange
        limiterMock
            .Setup(l => l.Check(apiKey, 1))
            .ReturnsAsync(new RateLimitDecision
            {
                Allowed = true,
                Limit = 100,
                RemainingTokens = 99,
                ResetInSeconds = 0
            });

        var request = new APIGatewayProxyRequest
        {
            Body = JsonSerializer.Serialize(new CheckRequest
            {
                ApiKey = apiKey,
                Cost = 1
            })
        };

        // Act
        var response = await functionService.CheckAsync(request);

        // Assert
        Assert.Equal(200, response.StatusCode);

        var decision = JsonSerializer.Deserialize<RateLimitDecision>(response.Body);
        Assert.NotNull(decision);
        Assert.True(decision!.Allowed);
        Assert.Equal(99, decision.RemainingTokens);
        Assert.Equal(100, decision.Limit);

        limiterMock.Verify(l => l.Check(apiKey, 1), Times.Once);
    }

    [Fact]
    public async Task CheckAsync_Returns400_WhenBodyInvalid()
    {
        var request = new APIGatewayProxyRequest { Body = "{}" };

        var response = await functionService.CheckAsync(request);

        Assert.Equal(400, response.StatusCode);
    }

    [Fact]
    public async Task CheckAsync_Returns429_WhenDenied()
    {
        // Arrange
        limiterMock
            .Setup(l => l.Check(apiKey, 1))
            .ReturnsAsync(new RateLimitDecision
            {
                Allowed = false,
                Limit = 100,
                RemainingTokens = 0,
                ResetInSeconds = 30
            });

        var body = JsonSerializer.Serialize(new CheckRequest
        {
            ApiKey = apiKey,
            Cost = 1
        });

        var request = new APIGatewayProxyRequest
        {
            Body = body
        };

        // Act
        var response = await functionService.CheckAsync(request);

        // Assert
        Assert.Equal(429, response.StatusCode);

        var decision = JsonSerializer.Deserialize<RateLimitDecision>(response.Body);
        Assert.NotNull(decision);
        Assert.False(decision!.Allowed);
        Assert.Equal(0, decision.RemainingTokens);
        Assert.Equal(100, decision.Limit);
        Assert.Equal(30, decision.ResetInSeconds);

        limiterMock.Verify(l => l.Check(apiKey, 1), Times.Once);
    }

    [Fact]
    public async Task CreateNewApiKeyAsync_Returns201_WhenNewKeyCreated()
    {
        apiKeyServiceMock.Setup(
            x => x.CreateAsync(It.IsAny<string>(), It.IsAny<Status>(), It.IsAny<string>(),
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Algorithm>())).ReturnsAsync(apiKey);

        var requestObj = new NewApiKeyRequest
        {
            Capacity = 10,
            Name = "test",
            RefillRate = 2
        };

        var body = JsonSerializer.Serialize(requestObj);
        var request = new APIGatewayProxyRequest
        {
            Body = body
        };

        var response = await functionService.CreateNewApiKeyAsync(request);

        // Assert
        Assert.Equal(201, response.StatusCode);

        var newApiKey = JsonSerializer.Deserialize<Dictionary<string, string>>(response.Body);
        Assert.NotNull(newApiKey);
        Assert.Equal(newApiKey["ApiKey"], apiKey);
    }

    [Fact]
    public async Task CreateNewApiKeyAsync_Returns400_WhenBodyIsEmpty()
    {
        apiKeyServiceMock.Setup(
            x => x.CreateAsync(It.IsAny<string>(), It.IsAny<Status>(), It.IsAny<string>(),
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Algorithm>())).ReturnsAsync(apiKey);

        var request = new APIGatewayProxyRequest
        {
            Body = "{ }"
        };

        var response = await functionService.CreateNewApiKeyAsync(request);

        // Assert
        Assert.Equal(400, response.StatusCode);
        apiKeyServiceMock.Verify(
            x => x.CreateAsync(It.IsAny<string>(), It.IsAny<Status>(), It.IsAny<string>(),
                               It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Algorithm>()), Times.Never);
    }

    [Fact]
    public async Task GetAllKeysAsync_Returns200_WhenThereAreNoKeys()
    {
        apiKeyServiceMock.Setup(
            x => x.GetAllKeysAsync()).ReturnsAsync(Array.Empty<string>);

        var request = new APIGatewayProxyRequest();

        var response = await functionService.GetAllKeysAsync(request);

        // Assert
        Assert.Equal(200, response.StatusCode);

        var apiKeys = JsonSerializer.Deserialize<Dictionary<string, string>>(response.Body);
        Assert.NotNull(apiKeys);
        Assert.Empty(apiKeys);
    }

    [Fact]
    public async Task GetAllKeysAsync_Returns200_WhenKeysExist()
    {
        var createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        apiKeyServiceMock
            .Setup(x => x.GetAllKeysAsync())
            .ReturnsAsync([apiKey]);
        apiKeyServiceMock
            .Setup(x => x.GetTokenBucketConfigAsync(It.IsAny<string>()))
            .ReturnsAsync([
                new RedisValue("Name"),
                new RedisValue("0"),
                new RedisValue("0"),
                new RedisValue("100"),
                new RedisValue("2"),
                new RedisValue(createdAt),
                new RedisValue("")
            ]);

        var request = new APIGatewayProxyRequest();

        var response = await functionService.GetAllKeysAsync(request);

        // Assert
        Assert.Equal(200, response.StatusCode);

        var apiKeys = JsonSerializer.Deserialize<Dictionary<string, BucketConfigDTO>>(response.Body);
        Assert.NotEmpty(apiKeys);
        Assert.True(apiKeys.TryGetValue(apiKey, out var config));
        Assert.Equal(Status.ACTIVE.ToString(), config.Status);
    }

    [Fact]
    public async Task GetKeyDetailsAsync_Returns200_WhenKeyExist()
    {
        var createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        apiKeyServiceMock
            .Setup(s => s.GetTokenBucketConfigAsync(It.Is<string>(k => k == apiKey)))
            .ReturnsAsync(
            [
                    new RedisValue("MyKeyName"),        // Name
                    new RedisValue("ACTIVE"),           // Status (as string, matches your enum)
                    new RedisValue("TokenBucket"),      // Algorithm (string name)
                    new RedisValue("100"),              // Capacity
                    new RedisValue("2"),                // RefillRate
                    new RedisValue(createdAt),          // CreatedAt (long)
                    new RedisValue("")                  // Description
            ]);

        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>() { { "key", apiKey } }
        };

        var response = await functionService.GetKeyDetailsAsync(request);

        // Assert
        Assert.Equal(200, response.StatusCode);

        var keyConfig = JsonSerializer.Deserialize<BucketConfigDTO>(response.Body);
        Assert.NotNull(keyConfig);
        Assert.Equal(Status.ACTIVE.ToString(), keyConfig.Status);
    }

    [Fact]
    public async Task GetKeyDetailsAsync_Returns404_WhenKeyIsMissing()
    {
        var createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        apiKeyServiceMock
            .Setup(s => s.GetTokenBucketConfigAsync(It.Is<string>(k => k == apiKey)))
            .ReturnsAsync(
            [
                new RedisValue("MyKeyName"),        // Name
                new RedisValue("ACTIVE"),           // Status (as string, matches your enum)
                new RedisValue("TokenBucket"),      // Algorithm (string name)
                new RedisValue("100"),              // Capacity
                new RedisValue("2"),                // RefillRate
                new RedisValue(createdAt),          // CreatedAt (long)
                new RedisValue("")                  // Description
            ]);

        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>() { { "key", "ABC" } }
        };

        var response = await functionService.GetKeyDetailsAsync(request);

        // Assert
        Assert.Equal(404, response.StatusCode);
    }

    [Fact]
    public async Task GetKeyDetailsAsync_Returns400_WhenDataIsMissing()
    {
        apiKeyServiceMock
            .Setup(s => s.GetTokenBucketConfigAsync(It.IsAny<string>()))
            .ReturnsAsync(
            [
                new RedisValue("MyKeyName"),        // Name
                new RedisValue("ACTIVE"),           // Status (as string, matches your enum)
                new RedisValue("TokenBucket"),      // Algorithm (string name)
                new RedisValue("100"),              // Capacity
                new RedisValue("2"),                // RefillRate
            ]);

        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>() { { "key", apiKey } }
        };

        var response = await functionService.GetKeyDetailsAsync(request);

        // Assert
        Assert.Equal(400, response.StatusCode);
    }

    [Fact]
    public async Task UpdateKeyLimitAsync_Returns204_WhenDataUpdatedSuccessfully()
    {
        apiKeyServiceMock.Setup(x => x.UpdateKeyLimitAsync(It.IsAny<string>(), It.IsAny<LimitUpdateRequest>())).Returns(Task.CompletedTask);

        var requestBody = new LimitUpdateRequest()
        {
            Capacity = 20,
            RefillRate = 1
        };

        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>() { { "key", apiKey } },
            Body = JsonSerializer.Serialize(requestBody)
        };

        var response = await functionService.UpdateKeyLimitAsync(request);

        // Assert
        Assert.Equal(204, response.StatusCode);
    }

    [Fact]
    public async Task UpdateKeyLimitAsync_Returns400_WhenBodyIsMissing()
    {
        apiKeyServiceMock.Setup(x => x.UpdateKeyLimitAsync(It.IsAny<string>(), It.IsAny<LimitUpdateRequest>())).Returns(Task.CompletedTask);

        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>() { { "key", apiKey } },
            Body = "{ }"
        };

        var response = await functionService.UpdateKeyLimitAsync(request);

        // Assert
        Assert.Equal(400, response.StatusCode);
    }

    [Fact]
    public async Task UpdateKeyLimitAsync_Returns400_WhenDataIsInvalid()
    {
        apiKeyServiceMock.Setup(x => x.UpdateKeyLimitAsync(It.IsAny<string>(), It.IsAny<LimitUpdateRequest>())).Returns(Task.CompletedTask);

        var requestBody = new LimitUpdateRequest()
        {
            Capacity = -20,
            RefillRate = 1
        };

        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>() { { "key", apiKey } },
            Body = JsonSerializer.Serialize(requestBody)
        };

        var response = await functionService.UpdateKeyLimitAsync(request);

        // Assert
        Assert.Equal(400, response.StatusCode);
    }
}
