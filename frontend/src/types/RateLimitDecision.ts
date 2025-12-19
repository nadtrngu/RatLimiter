export interface RateLimitDecision {
  allowed: boolean;
  remainingTokens: number;
  limit: number;
  resetInSeconds: number;
}