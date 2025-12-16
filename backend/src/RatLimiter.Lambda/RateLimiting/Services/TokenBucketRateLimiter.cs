using RatLimiter.Lambda.RateLimiting.Interfaces;
using RatLimiter.Lambda.RateLimiting.Models;

namespace RatLimiter.Lambda.RateLimiting.Services;
public class TokenBucketRateLimiter : IRateLimiter
{
    private readonly ITokenBucketStore _store;

    public TokenBucketRateLimiter(ITokenBucketStore store)
    {
        _store = store;
    }

    public async Task<RateLimitDecision> Check(string apiKey, int cost = 1)
    {
        bool didChange = false;
        TokenBucketState? state = null;
        try
        {
            state = await _store.GetAsync(apiKey) ?? throw new UnauthorizedAccessException();

            DateTimeOffset now = DateTimeOffset.UtcNow;
            didChange = RefillTokens(state, now);

            if (state.NumberOfTokens < cost)
            {
                var deficit = cost - state.NumberOfTokens;
                var seconds = state.RefillRate > 0 ? ((double)deficit / state.RefillRate) : -1; // If refill rate is set to 0, we let the user know that they won't have tokens reset.
                var resetInSeconds = (int)Math.Ceiling(seconds);

                return new RateLimitDecision()
                {
                    Allowed = false,
                    Limit = state.Capacity,
                    RemainingTokens = state.NumberOfTokens,
                    ResetInSeconds = resetInSeconds
                };
            }

            state.NumberOfTokens -= cost;
            didChange = true;
            return new RateLimitDecision() { Allowed = true, Limit = state.Capacity, RemainingTokens = state.NumberOfTokens };
        }
        finally
        {
            if (state != null && didChange)
                await _store.SaveAsync(apiKey, state);
        }
    }

    private static bool RefillTokens(TokenBucketState state, DateTimeOffset now)
    {
        if (state.Capacity == state.NumberOfTokens) return false;

        var elapsedSeconds = Math.Max(0, now.ToUnixTimeSeconds() - state.LastRefill);
        var tokensToRefill = (int)(elapsedSeconds * state.RefillRate);

        if (tokensToRefill > 0)
        {
            state.NumberOfTokens = Math.Min(state.Capacity, tokensToRefill + state.NumberOfTokens);
            state.LastRefill = now.ToUnixTimeSeconds();

            return true;
        }

        return false;
    }

}
