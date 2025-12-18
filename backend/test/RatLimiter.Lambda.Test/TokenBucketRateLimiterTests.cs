using System;
using System.Threading.Tasks;
using Moq;
using RatLimiter.Lambda.RateLimiting.Interfaces;
using RatLimiter.Lambda.RateLimiting.Models;
using RatLimiter.Lambda.RateLimiting.Services;
using Xunit;

namespace RatLimiter.Lambda.Tests;

public class TokenBucketRateLimiterTests
{
    readonly string apiKey = "TEST-KEY";

    [Fact]
    public async Task Check_Allows_WhenEnoughTokens()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var storeMock = new Mock<ITokenBucketStore>();

        storeMock
            .Setup(s => s.GetAsync(apiKey))
            .ReturnsAsync(new TokenBucketState
            {
                Capacity = 100,
                NumberOfTokens = 50,
                RefillRate = 5,
                LastRefill = now
            });

        // SaveAsync should be called with the updated state
        TokenBucketState savedState = null;
        storeMock
            .Setup(s => s.SaveAsync(apiKey, It.IsAny<TokenBucketState>(), null))
            .Callback<string, TokenBucketState, TokenBucketConfig>((_, state, _) => savedState = state)
            .Returns(Task.CompletedTask);

        var limiter = new TokenBucketRateLimiter(storeMock.Object);

        // Act
        var result = await limiter.Check(apiKey, cost: 10);

        // Assert
        Assert.True(result.Allowed);
        Assert.Equal(100, result.Limit);
        Assert.Equal(45, result.RemainingTokens);

        Assert.NotNull(savedState);
        Assert.Equal(45, savedState!.NumberOfTokens);

        storeMock.Verify(s => s.GetAsync(apiKey), Times.Once);
        storeMock.Verify(s => s.SaveAsync(apiKey, It.IsAny<TokenBucketState>(), null), Times.Once);
    }

    [Fact]
    public async Task Check_Denies_WhenNotEnoughTokens()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var storeMock = new Mock<ITokenBucketStore>();

        storeMock
            .Setup(s => s.GetAsync(apiKey))
            .ReturnsAsync(new TokenBucketState
            {
                Capacity = 100,
                NumberOfTokens = 2,
                RefillRate = 2,
                LastRefill = now
            });

        // If tokens don't change, SaveAsync should NOT be called
        var limiter = new TokenBucketRateLimiter(storeMock.Object);

        // Act
        var result = await limiter.Check(apiKey, cost: 5);

        // Assert
        Assert.False(result.Allowed);
        Assert.Equal(100, result.Limit);
        Assert.Equal(2, result.RemainingTokens);

        Assert.InRange(result.ResetInSeconds, 2, 3);

        storeMock.Verify(s => s.SaveAsync(apiKey, It.IsAny<TokenBucketState>(), null), Times.Never);
    }

    [Fact]
    public async Task Check_ThrowsUnauthorized_ForUnknownKey()
    {
        var storeMock = new Mock<ITokenBucketStore>();

        storeMock
            .Setup(s => s.GetAsync("NONE"))
            .ReturnsAsync((TokenBucketState)null);

        var limiter = new TokenBucketRateLimiter(storeMock.Object);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => limiter.Check("NONE"));
    }
}
