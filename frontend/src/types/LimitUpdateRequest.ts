export interface LimitUpdateRequest {
  capacity: number;
  refillRate: number;
  algorithm?: 'TokenBucket';
}